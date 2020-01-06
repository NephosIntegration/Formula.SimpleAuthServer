using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Formula.SimpleAuthServer
{
    public class AuthServerClaimDetails
    {
        public AuthServerClaimDetails()
        {

        }
        
        public AuthServerClaimDetails(String claimType, String claimValue, String valueType = ClaimValueTypes.String)
        {
            this.ClaimType = claimType;
            this.ClaimValue = claimValue;
            this.ValueType = valueType;
        }

        public String ClaimType { get; set; }
        public String ClaimValue { get; set; }
        public String ValueType { get; set; }
    }
}