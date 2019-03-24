using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Logging.SignalR
{
    public class SignalRLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private readonly IOptionsMonitor<SignalRLoggerOptions> _options;
        private readonly ConcurrentDictionary<string, SignalRLogger> _loggers;

        private readonly HubConnection _connection;

        private IDisposable _optionsReloadToken;
        private IExternalScopeProvider _scopeProvider = NullExternalScopeProvider.Instance;

        public SignalRLoggerProvider(IOptionsMonitor<SignalRLoggerOptions> options)
        {
            _options = options;
            _loggers = new ConcurrentDictionary<string, SignalRLogger>();

            _connection = new HubConnectionBuilder()
                .WithUrl(_options.CurrentValue.ConnectionUrl)
                .ConfigureLogging(logging => 
                {
                    logging.SetMinimumLevel(LogLevel.Debug);
                    logging.AddDebug();
                })
                .Build();
            
            Task.Run(async () => await _connection.StartAsync());

            ReloadLoggerOptions(options.CurrentValue);
        }

        private void ReloadLoggerOptions(SignalRLoggerOptions options)
        {
            foreach (var logger in _loggers)
            {
                logger.Value.Options = options;
            }

            _optionsReloadToken = _options.OnChange(ReloadLoggerOptions);
        }

        /// <inheritdoc />
        public ILogger CreateLogger(string name)
        {
            return _loggers.GetOrAdd(name, loggerName => new SignalRLogger(name)
            {
                Options = _options.CurrentValue,
                ScopeProvider = _scopeProvider,
                Connection = _connection
            });
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _optionsReloadToken?.Dispose();
        }

        /// <inheritdoc />
        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;

            foreach (var logger in _loggers)
            {
                logger.Value.ScopeProvider = _scopeProvider;
            }

        }
    }
}