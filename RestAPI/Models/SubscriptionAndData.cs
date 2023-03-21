using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestAPI.Models
{
    public class SubscriptionAndData : Module
    {
        public string Event { get; set; }
        public string Endpoint { get; set; }

        public string Content { get; set; }
    }
}