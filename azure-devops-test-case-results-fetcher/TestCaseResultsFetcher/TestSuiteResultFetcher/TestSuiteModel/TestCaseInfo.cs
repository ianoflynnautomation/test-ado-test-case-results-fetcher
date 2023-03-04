using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestCaseResultsFetcher.TestSuiteResultFetcher.TestSuiteModel
{
    internal class TestCaseInfo
    {
        public TestCaseReference TestCaseReference { get; set; }

        public Results Results { get; set; }

        public Configuration Configuration { get; set; }
    }
}