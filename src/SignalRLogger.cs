using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging.Abstractions.Internal;


namespace Microsoft.Extensions.Logging.SignalR
{
    public class SignalRLogger : ILogger
    {
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalRLogger"/> class.
        /// </summary>
        /// <param name="name">The name of the logger.</param>
        public SignalRLogger(string name)
        {
            _name = name;
        }

        internal SignalRLoggerOptions Options { get; set; }

        internal IExternalScopeProvider ScopeProvider { get; set; }

        internal HubConnection Connection { get; set; }

        public IDisposable BeginScope<TState>(TState state)
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            // If the filter is null, everything is enabled
            // unless the debugger is not attached
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var message = formatter(state, exception);
            var properties = new Dictionary<string, string> { { nameof(message), message } };

            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            message = $"{ logLevel }: {message}";

            if (exception != null)
            {
                message += Environment.NewLine + Environment.NewLine + exception;
            }

            try
            {
                Task.Run(async () => await Connection.InvokeAsync("SendMessage", GetLogLevelString(logLevel), message));
            }
            catch (Exception e)
            {
               
            }
            
        }

        private static string GetLogLevelString(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    return "Trace";
                case LogLevel.Debug:
                    return "Debug";
                case LogLevel.Information:
                    return "Information";
                case LogLevel.Warning:
                    return "Warning";
                case LogLevel.Error:
                    return "Error";
                case LogLevel.Critical:
                    return "Critical";
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }
    }
}
