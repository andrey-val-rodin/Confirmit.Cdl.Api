using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.Tools.Database
{
    /// <summary>
    /// Helper to periodically cleanup expired documents
    /// </summary>
    public class Cleanup
    {
        private CancellationTokenSource _source;
        private readonly TimeSpan _cleanupInterval;
        private readonly TimeSpan _expirationPeriod;
        private readonly ILogger<Cleanup> _logger;

        [UsedImplicitly]
        public Cleanup(ILogger<Cleanup> logger, IOptions<CleanupConfig> config) : this(
            TimeSpan.FromMinutes(config.Value.CleanupIntervalInMinutes),
            TimeSpan.FromDays(config.Value.ExpirationPeriodInDays),
            logger)
        {
        }

        public Cleanup(TimeSpan cleanupInterval, TimeSpan expirationPeriod,
            ILogger<Cleanup> logger)
        {
            if (cleanupInterval.Ticks < 1)
                throw new ArgumentOutOfRangeException($"{nameof(cleanupInterval)} must be more than zero");

            _cleanupInterval = cleanupInterval;
            _expirationPeriod = expirationPeriod;
            _logger = logger;
        }

        public void Start()
        {
            Start(CancellationToken.None);
        }

        public void Start(CancellationToken cancellationToken)
        {
            if (_source != null)
                throw new InvalidOperationException("Already started. Call Stop first.");

            _source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            Task.Factory.StartNew(() => StartInternalAsync(_source.Token), cancellationToken);
        }

        public void Stop()
        {
            if (_source == null)
                throw new InvalidOperationException("Not started. Call Start first.");

            _source.Cancel();
            _source = null;
        }

        private async Task StartInternalAsync(CancellationToken cancellationToken)
        {
            var cleaner = new QueryProvider();
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("CancellationRequested. Exiting.");
                    break;
                }

                try
                {
                    await Task.Delay(_cleanupInterval, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    _logger.LogInformation("TaskCanceledException. Exiting.");
                    break;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Task.Delay exception. Exiting.");
                    break;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("CancellationRequested. Exiting.");
                    break;
                }

                try
                {
                    _logger.LogInformation("Deleting outdated documents");
                    var deletedCount = await cleaner.CleanupAsync(_expirationPeriod);
                    _logger.LogInformation($"Database cleanup: success. Outdated documents deleted: {deletedCount}");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Database cleanup: failed.");
                }
            }
        }
    }
}