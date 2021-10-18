using Confirmit.Cdl.Api.Services;
using Confirmit.MessageBroker.Consume.Sdk;
using Confirmit.NetCore.Common;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Confirmit.Cdl.Api.xIntegrationTests.Framework
{
    public static class MessageBusAssert
    {
        private static readonly ConfirmitRabbitMQConnectionFactory Factory = new ConfirmitRabbitMQConnectionFactory();

        public static async Task EventReceivedAsync<TMessage, TArrange, TAct>(
            [NotNull] string exchangeName,
            [NotNull] string topic,
            TimeSpan timeout,
            TArrange arrange,
            [NotNull] Func<TArrange, Task<TAct>> action,
            [NotNull] Func<Message<TMessage>, TArrange, TAct, bool> customAssert)
        where TMessage : class
        {
            if (exchangeName == null) throw new ArgumentNullException(nameof(exchangeName));
            if (topic == null) throw new ArgumentNullException(nameof(topic));
            if (customAssert == null) throw new ArgumentNullException(nameof(customAssert));
            if (action == null) throw new ArgumentNullException(nameof(action));

            RabbitMQConsumptionOptions<TMessage> CreateTestOptions()
            {
                return new RabbitMQConsumptionOptions<TMessage>
                {
                    RoutingKey = topic,

                    ExchangeName = exchangeName,
                    DurableExchange = true,
                    AutoDeleteExchange = false,

                    QueueName = $"{exchangeName}_test_{Guid.NewGuid()}",
                    DurableQueue = false,
                    AutoDeleteQueue = true
                };
            }

            var cancellationSource = new CancellationTokenSource();
            var services = new ServiceCollection();
            services.AddScoped<IConfirmitScopeContext>(sp => new ConfirmitScopeContextStub());
            var serviceProvider = services.BuildServiceProvider(false);
            using var scope = serviceProvider.CreateScope();
            using var connection = Factory.CreateConnection();
            var handler = new TestMessageHandler<TMessage, TArrange, TAct>(
                arrange,
                action,
                customAssert,
                timeout,
                cancellationSource);

            using var consumption = new RabbitMQConsumption<TMessage>(
                scope.ServiceProvider,
                connection,
                new NullLogger<RabbitMQConsumption<TMessage>>(),
                new OptionsWrapper<RabbitMQConsumptionOptions<TMessage>>(CreateTestOptions()),
                handler,
                null);
            consumption.Start(cancellationSource.Token);
            await handler.ActAsync();
            handler.Wait(timeout);
        }
    }

    public class TestMessageHandler<TMessage, TArrange, TAct> : IMessageHandler<TMessage>
        where TMessage : class
    {
        private readonly ManualResetEvent _assertCompleteEvent = new ManualResetEvent(false);
        private readonly ManualResetEvent _actionCompleteEvent = new ManualResetEvent(false);
        private readonly TArrange _arrange;
        private readonly Func<Message<TMessage>, TArrange, TAct, bool> _assert;
        private readonly TimeSpan _timeout;
        private readonly Func<TArrange, Task<TAct>> _action;
        private readonly CancellationTokenSource _cancellationSource;
        private TAct _actionResult;
        private readonly ConcurrentBag<Exception> _exceptions = new ConcurrentBag<Exception>();

        public TestMessageHandler(TArrange arrange,
            Func<TArrange, Task<TAct>> action,
            Func<Message<TMessage>, TArrange, TAct, bool> assert,
            TimeSpan timeout,
            CancellationTokenSource cancellationSource)
        {
            _arrange = arrange;
            _assert = assert;
            _timeout = timeout;
            _action = action;
            _cancellationSource = cancellationSource;
        }

        public Task HandleMessage(
            Message<TMessage> message,
            IServiceScope scope,
            CancellationToken cancellationToken)
        {
            try
            {
                if (_assertCompleteEvent.WaitOne(0))
                {
                    return Task.CompletedTask;
                }
                if (!_actionCompleteEvent.WaitOne(_timeout))
                {
                    Assert.True(false, $"Failed due to Action timeout expired: {_timeout}");
                }

                if (_assert(message, _arrange, _actionResult))
                {
                    _assertCompleteEvent.Set();
                }
            }
            catch (AggregateException a)
            {
                if (a.InnerExceptions != null)
                {
                    foreach (var ie in a.InnerExceptions)
                    {
                        _exceptions.Add(ie);
                    }
                }
                _assertCompleteEvent.Set();
            }
            catch (Exception e)
            {
                _exceptions.Add(e);
                _assertCompleteEvent.Set();
            }
            return Task.CompletedTask;
        }

        public async Task ActAsync()
        {
            try
            {
                _actionResult = await _action(_arrange);

            }
            finally
            {
                _actionCompleteEvent.Set();
            }
        }

        public void Wait(TimeSpan timeout)
        {
            if (!_assertCompleteEvent.WaitOne(timeout))
            {
                _cancellationSource.Cancel();
                Assert.True(false, $"Failed due to timeout expired: {timeout}");
            }
            _cancellationSource.Cancel();
            if (_exceptions.IsEmpty) return;
            if (_exceptions.Count == 1)
            {
                throw _exceptions.First();
            }
            var msg = _exceptions.Aggregate("test", (s, e) => s + e.Message + "\n");
            throw new AggregateException(msg, _exceptions);
        }
    }

    public static class EventAssert
    {
        private static void All(params Action[] actions)
        {
            var exceptions = new List<Exception>();
            foreach (var action in actions)
            {
                try
                {
                    action();
                }
                catch (AggregateException a)
                {
                    if (a.InnerExceptions != null)
                    {
                        foreach (var ie in a.InnerExceptions)
                        {
                            exceptions.Add(ie);
                        }
                    }
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }

            if (!exceptions.Any()) return;
            if (exceptions.Count == 1)
            {
                throw exceptions.First();
            }
            var msg = exceptions.Aggregate("test", (s, e) => s + e.Message + "\n");
            throw new AggregateException(msg, exceptions);
        }

        public static void AreEqual(DocumentEvent expected, DocumentEvent actual)
        {
            if (expected == null && actual == null) return;
            if (expected == null || actual == null) Assert.True(false, "one of objects is null");
            All(
                () => Assert.True(expected.CompanyId == actual.CompanyId, nameof(actual.CompanyId)),
                () => Assert.True(expected.UserId == actual.UserId, nameof(actual.UserId)),
                () => Assert.True(expected.DocumentUrn == actual.DocumentUrn, nameof(actual.DocumentUrn)),
                () => Assert.True(expected.PublicRevisionUrn == actual.PublicRevisionUrn, nameof(actual.PublicRevisionUrn))
            );
        }

        public static void AreEqual(RevisionEvent expected, RevisionEvent actual)
        {
            if (expected == null && actual == null) return;
            if (expected == null || actual == null) Assert.True(false, "one of objects is null");
            All(
                () => Assert.True(expected.CompanyId == actual.CompanyId, nameof(actual.CompanyId)),
                () => Assert.True(expected.UserId == actual.UserId, nameof(actual.UserId)),
                () => Assert.True(expected.RevisionUrn == actual.RevisionUrn, nameof(actual.RevisionUrn))
            );
        }

    }
}