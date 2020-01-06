using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Formula.SimpleAuthServer
{
    public enum ClientType
    {
        BrowserClient = 1,
        InteractiveClient = 2,
        NonInteractiveClient = 3,
    }
    public static class ClientTypeExtensions
    {
        public static String ToStringFromClientType(this ClientType type)
        {
            return Enum.GetName(typeof(ClientType), type);
        }

        public static ClientType ToClientTypeFromString(this String type)
        {
            ClientType output = ClientType.BrowserClient;
            if (!Enum.TryParse(type, out output))
            {
                throw new Exception(type + " is not a valid ClientType");
            }
            return output;
        }
    }

    public class AuthServerClientDetails : SimpleClientDetails
    {
        public String ClientType { get; set; }
    }
}