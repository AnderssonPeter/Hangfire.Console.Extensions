using System;
using System.IO;
using Hangfire.Server;
using Serilog.Events;

namespace Hangfire.Console.Extensions.Serilog
{
    internal class PerformContextValue : ScalarValue
    {
        public PerformContextValue(object value) : base(value)
        {
        }

        // The context attached to this property value
        public PerformContext PerformContext { get; set; }
        /// <inheritdoc />
        public override void Render(TextWriter output, string format = null, IFormatProvider formatProvider = null)
        {
            // How the value will be rendered in Json output, etc.
            // Not important for the function of this code..
            output.Write(PerformContext.BackgroundJob.Id);
        }
    }
}
