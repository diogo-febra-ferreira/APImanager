using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestAPI.Models
{
    public class Module : Application
    {
        public int Parent { get; set; }

        public List<Data> Data { get; set; }
    }
}