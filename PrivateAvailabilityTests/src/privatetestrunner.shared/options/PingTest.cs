using System;
using System.Net;

namespace privatetestrunner.shared.options
{
    public class PingTest
    {
        public String Name { get; set; }
        public String Url { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public Int32 Timeout { get; set; }
        public Boolean ParseDependentRequests { get; set; }
    }
}