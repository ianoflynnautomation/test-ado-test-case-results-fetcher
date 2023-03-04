using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestCaseResultsFetcher.TestSuiteResultFetcher.TestSuiteModel
{
    internal class Results
    {
        public LastResultsDetails LastResultsDetails { get; set; }

        public int LastResultsId { get; set; }

        public string LasrRunBuildNumber { get; set; }

        public string State { get; set; }

        public string LastResultsState { get; set; }

        public string Outcome { get; set; }

        public int LastTestRunId { get; set; }


    }
}