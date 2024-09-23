using System;
using System.Diagnostics;

namespace Microsoft.Azure.Functions.Worker.Sdk.Generators.PrecompiledFunctionMetadataProviderGenerator
{
    [DebuggerDisplay("{Info}")]
    public sealed record RetryModel
    {
        public string Code { get; }
        public string Info { get; }

        private RetryModel(
            string code,
            string info)
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

            var info = $"ExponentialBackoff {maxRetryCount}, {minimumInterval} to {maximumInterval}";

            return new RetryModel(code, info);
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

            var info = $"FixedDelay {maxRetryCount}, {delayInterval}";

            return new RetryModel(code, info);
        }

        private static string ToCode(TimeSpan value)
        {
            return $"new global::System.TimeSpan({value.Days}, {value.Hours}, {value.Minutes}, {value.Seconds}, {value.Milliseconds})";
        }
    }
}
