using System;
using System.Collections.Generic;

namespace privatetestrunner.shared.options
{
    public class TestRunOptions
    {
        public const string TestRun = "TestRun";
        public String InstrumentationKey { get; set; }
        public String EndpointAddress { get; set; }
        public String Location { get; set; }
        public String AzureStorageConnectionString { get; set; }
        public List<PingTest> PingTests { get; set; }
    }
}