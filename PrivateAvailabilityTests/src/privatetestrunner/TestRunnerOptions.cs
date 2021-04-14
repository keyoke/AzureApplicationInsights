using System;
using System.Collections.Generic;
using System.Net;

namespace privatetestrunner
{
    public class TestRunOptions
    {
        public const string TestRun = "TestRun";
        public String InstrumentationKey { get; set; }
        public String EndpointAddress { get; set; }
        public String Location { get; set; }
        public List<PingTest> PingTests { get; set; }
        public List<MultiStepTest> MultiStepTests { get; set; }
    }

    public class PingTest
    {
        public String Name { get; set; }
        public String Url { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public Int32 Timeout { get; set; }
        public Boolean ParseDependentRequests { get; set; }
    }

    public class MultiStepTest
    {
        public String Name { get; set; }
    }
}