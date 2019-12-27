using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Formula.SimpleAuthServer
{
    public class RedirectUrlDetails
    {
        public RedirectUrlDetails()
        {

        }
        
        public RedirectUrlDetails(String url)
        {
            this.Url = url;
        }

        public String Url { get; set; }
        public bool IsLocal { get; set; } = false;
    }
}