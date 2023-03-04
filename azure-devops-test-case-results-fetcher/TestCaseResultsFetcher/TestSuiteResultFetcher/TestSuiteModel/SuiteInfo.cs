using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestCaseResultsFetcher.TestSuiteResultFetcher.TestSuiteModel
{
    internal class SuiteInfo
    {
        public List<TestCaseInfo> Value { get; set; } = new();
    }
}