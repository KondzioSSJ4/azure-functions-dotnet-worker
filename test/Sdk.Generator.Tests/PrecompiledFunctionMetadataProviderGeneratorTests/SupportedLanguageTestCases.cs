using System.Collections;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;

namespace Microsoft.Azure.Functions.SdkGeneratorTests.PrecompiledFunctionMetadataProviderGeneratorTests
{
    public class SupportedLanguageTestCases : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            static object[] Test(LanguageVersion version)
                => new object[] { version };

            //yield return Test(LanguageVersion.CSharp7_3);
            //yield return Test(LanguageVersion.CSharp8);
            //yield return Test(LanguageVersion.CSharp9);
            yield return Test(LanguageVersion.CSharp10);
            //yield return Test(LanguageVersion.CSharp11);
            //yield return Test(LanguageVersion.Latest);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
