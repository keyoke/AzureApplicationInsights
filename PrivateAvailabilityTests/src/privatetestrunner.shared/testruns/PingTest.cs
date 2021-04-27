using System;
using System.Collections.Generic;
using System.Net;

namespace privatetestrunner.shared.testruns
{
    public class PingTest
    {
        public String Name { get; set; }
        public String Url { get; set; }
        public Int32 StatusCode { get; set; }
        public Int32 Timeout { get; set; }
        public Boolean ParseDependentRequests { get; set; }
    }
}