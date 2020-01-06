using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Formula.SimpleAuthServer
{
    public class AuthServerUser
    {
        public AuthServerUser()
        {
            this.Claims = new List<AuthServerClaimDetails>();
        }

        public String SubjectId { get; set; }
        public String Username { get; set; }
        public String Password { get; set; }
        public List<AuthServerClaimDetails> Claims { get; set; }
    }
}