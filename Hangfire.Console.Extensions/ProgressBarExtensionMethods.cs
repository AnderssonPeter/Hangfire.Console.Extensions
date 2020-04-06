using System;
using Hangfire.Console.Progress;

namespace Hangfire.Console.Extensions
{
    public static class ProgressBarExtensionMethods
    {
        public static IProgress<T> AsIProgress<T>(this IProgressBar progressBar, Func<T, double> convert)
        {
            return new ProgressConverter<T>(progressBar, convert);
        }

        private class ProgressConverter<T> : IProgress<T>
        {
            private readonly IProgressBar progressBar;
            private readonly Func<T, double> convert;
            private double lastValue = -1;

            public ProgressConverter(IProgressBar progressBar, Func<T, double> convert)
            {
                this.progressBar = progressBar;
                this.convert = convert;
            }

            public void Report(T value)
            {
                var newValue = convert(value);
                if (newValue - lastValue > 0.01)
                {
                    progressBar.SetValue(newValue);
                    lastValue = newValue;
                }
            }
        }
    }
}
