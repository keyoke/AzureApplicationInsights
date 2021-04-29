using System;
using System.Collections.Generic;
using System.Net;

namespace privatetestrunner.shared.testruns
{
    public class TestRuns
    {
        public TestRuns() {
            this.PingTests = new List<PingTest>();
        }

        public IList<PingTest> PingTests { get; set; }
    }
}