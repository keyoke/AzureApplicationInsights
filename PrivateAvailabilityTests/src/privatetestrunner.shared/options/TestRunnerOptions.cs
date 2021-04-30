using privatetestrunner.shared.testruns;
using System;
using System.Collections.Generic;

namespace privatetestrunner.shared.options
{
    public class TestRunnerOptions
    {
        public const string TestRunner = "TestRunner";
        public String InstrumentationKey { get; set; }
        public String EndpointAddress { get; set; }
        public String Location { get; set; }
        public String StorageContainerEndpoint { get; set; }
        public String StorageBlobName { get; set; }
        public TestRuns TestRuns { get; set; }

    }
}