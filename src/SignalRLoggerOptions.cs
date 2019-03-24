using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.Logging
{
    public class SignalRLoggerOptions
    {
        public bool IncludeScopes { get; set; }

        public string ConnectionUrl { get; set; }
    }
}
