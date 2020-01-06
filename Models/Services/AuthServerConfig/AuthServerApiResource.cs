using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Formula.SimpleAuthServer
{
    public class AuthServerApiResource
    {
        public String Name { get; set; }
        public String DisplayName { get; set; }
        public List<String> ClaimTypes { get; set; }
    }
}