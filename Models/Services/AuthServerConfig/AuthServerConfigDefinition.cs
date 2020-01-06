using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Formula.SimpleAuthServer
{
    public enum AuthServerConfigResourceType 
    {
        OpenId = 1,
        Profile = 2,
        Email = 3,
        Phone = 4,
        Address = 5,
    }

    public static class AuthServerConfigResourceTypeExtensions
    {
        public static String ToStringFromResourceType(this AuthServerConfigResourceType type)
        {
            return Enum.GetName(typeof(AuthServerConfigResourceType), type);
        }

        public static AuthServerConfigResourceType ToResourceTypeFromString(this String type)
        {
            AuthServerConfigResourceType output = AuthServerConfigResourceType.OpenId;
            if (!Enum.TryParse(type, out output))
            {
                throw new Exception(type + " is not a valid AuthServerConfigResourceType");
            }

            return output;
        }
    }

    public class AuthServerConfigDefinition
    {
        public AuthServerConfigDefinition()
        {
            this.IdentityResources = new List<String>();
            this.ClientDetails = new List<AuthServerClientDetails>();
            this.ApiResources = new List<AuthServerApiResource>();
            this.AuthServerUsers = new List<AuthServerUser>();
        }

        public List<String> IdentityResources { get; set; }
        public List<AuthServerClientDetails> ClientDetails { get; set; }
        public List<AuthServerApiResource> ApiResources { get; set; }
        public List<AuthServerUser> AuthServerUsers { get; set; }
    }
}