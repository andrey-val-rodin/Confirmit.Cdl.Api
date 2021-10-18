using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

namespace Confirmit.Cdl.Api.xIntegrationTests.Framework
{
    public class LoggerStub<T> : ILogger<T>
    {
        public readonly ConcurrentBag<Tuple<LogLevel, string>> LogEntries =
            new ConcurrentBag<Tuple<LogLevel, string>>();

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (formatter != null)
                LogEntries.Add(Tuple.Create(logLevel, formatter(state, exception)));
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }
}
