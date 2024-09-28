using System;
using System.Diagnostics;

namespace Microsoft.Azure.Functions.Worker.Sdk.Generators.MetadataGenerator
{
    [DebuggerDisplay("{Info}")]
    public sealed record RetryModel
    {
        public string Code { get; }
        internal RetryInfo Info { get; }

        private RetryModel(
            string code,
            RetryInfo info)
        {
            Code = code;
            Info = info;
        }

        public static RetryModel AsExponentialBackoff(
            int maxRetryCount,
            TimeSpan minimumInterval,
            TimeSpan maximumInterval)
        {
            var code = $$"""
                    new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.DefaultRetryOptions()
                    {
                        MaxRetryCount = {{maxRetryCount}},
                        MinimumInterval = {{ToCode(minimumInterval)}},
                        MaximumInterval = {{ToCode(maximumInterval)}}
                    }
                    """;

            return new RetryModel(code, new RetryInfo
            {
                MaxRetryCount = maxRetryCount,
                MinimumInterval = minimumInterval,
                MaximumInterval = maximumInterval
            });
        }

        public static RetryModel AsFixedDelay(
            int maxRetryCount,
            TimeSpan delayInterval)
        {
            var code = $$"""
                    new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.DefaultRetryOptions()
                    {
                        MaxRetryCount = {{maxRetryCount}},
                        DelayInterval = {{ToCode(delayInterval)}}
                    }
                    """;

            return new RetryModel(code, new RetryInfo()
            {
                MaxRetryCount = maxRetryCount,
                DelayInterval = delayInterval
            });
        }

        private static string ToCode(TimeSpan value)
        {
            return $"new global::System.TimeSpan({value.Days}, {value.Hours}, {value.Minutes}, {value.Seconds}, {value.Milliseconds})";
        }

        public enum RetryStrategy
        {
            FixedDelay = 1,
            ExponentialBackoff
        }

        public class RetryInfo
        {
            public int MaxRetryCount { get; set; }
            public TimeSpan? DelayInterval { get; set; }
            public TimeSpan? MinimumInterval { get; set; }
            public TimeSpan? MaximumInterval { get; set; }

            public RetryStrategy? Strategy => DelayInterval is null ? RetryStrategy.ExponentialBackoff : RetryStrategy.FixedDelay;

            public override string ToString()
            {
                return Strategy switch
                {
                    RetryStrategy.FixedDelay => $"FixedDelay {MaxRetryCount}, {DelayInterval}",
                    RetryStrategy.ExponentialBackoff => $"ExponentialBackoff {MaxRetryCount}, {MinimumInterval} to {MaximumInterval}",

                    _ => throw new NotSupportedException($"Invalid retry {Strategy}")
                };

            }
        }
    }
}
