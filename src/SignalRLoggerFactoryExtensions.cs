using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.SignalR;

namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// Extension methods for the <see cref="ILoggerFactory"/> class.
    /// </summary>
    public static class SignalRLoggerFactoryExtensions
    {
        public static ILoggingBuilder AddSignalR(this ILoggingBuilder builder, Action<SignalRLoggerOptions> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, SignalRLoggerProvider>());
            builder.Services.Configure(configure);

            return builder;
        }
    }
}
