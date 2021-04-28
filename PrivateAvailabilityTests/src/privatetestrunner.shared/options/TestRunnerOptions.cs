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
        public String StorageComtainerEndpoint { get; set; }
        public String StorageBlobName { get; set; }

    }
}