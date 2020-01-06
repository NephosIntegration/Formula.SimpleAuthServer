using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using IdentityServer4.Models;
using IdentityServer4.Test;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Formula.SimpleAuthServer
{
    public class AuthServerConfigLoader : ISimpleAuthServerConfig
    {
        protected AuthServerConfigDefinition instance = null;

        public AuthServerConfigLoader LoadFromFile(String fileName)
        {
            var json = File.ReadAllText(fileName);
            this.instance = JsonSerializer.Deserialize<AuthServerConfigDefinition>(json);
            return this;
        }

        public AuthServerConfigLoader SaveToFile(String fileName)
        {
            if (this.InstanceValid())
            {
                var json = JsonSerializer.Serialize(this.instance);
                var fileStream = File.Open(fileName, FileMode.Append, FileAccess.Write);
                var fileWriter = new StreamWriter(fileStream);
                fileWriter.Write(json);
                fileWriter.Flush();
                fileWriter.Close();
            }

            return this;
        }

        protected Boolean InstanceValid(Boolean throwIfNot = true)
        {
            Boolean output = false;

            if (this.instance == null)
            {
                if (throwIfNot)
                {
                    throw new Exception("Auth Server Configuration not found");
                }
            }
            else
            {
                output = true;
            }

            return output;
        }

        public IEnumerable<ApiResource> GetApiResources()
        {
            var output = new List<ApiResource>();

            if (this.InstanceValid())
            {
                foreach(var i in this.instance.ApiResources)
                {
                    output.Add(new ApiResource(i.Name, i.DisplayName));
                }
            }

            return output;
        }

        public IEnumerable<Client> GetClients()
        {
            var output = new List<Client>();

            if (this.InstanceValid())
            {
                foreach(var i in this.instance.ClientDetails)
                {

                    var clientTypeEnum = i.ClientType.ToClientTypeFromString();

                    var clientDetails = new SimpleClientDetails() {
                        ClientId = i.ClientId,
                        Secret = i.Secret,
                        ClientName = i.ClientName,
                        BaseUri = i.BaseUri,
                        RedirectUris = i.RedirectUris,
                        LogoutRedirectUri = i.LogoutRedirectUri,
                        AllowedScopes = i.AllowedScopes,
                    };

                    switch(clientTypeEnum)
                    {
                        case ClientType.BrowserClient:
                            output.Add(new BrowserClient(clientDetails));
                            break;
                        case ClientType.InteractiveClient:
                            output.Add(new InteractiveClient(clientDetails));
                            break;
                        case ClientType.NonInteractiveClient:
                            output.Add(new NonInteractiveClient(clientDetails));
                            break;
                        default:
                            throw new Exception("Unknown ClientType");
                    }
                }
            }

            return output;
        }

        public IEnumerable<IdentityResource> GetIdentityResources()
        {
            var output = new List<IdentityResource>();

            if (this.InstanceValid())
            {
                foreach(var i in this.instance.IdentityResources)
                {
                    switch (i.ToResourceTypeFromString())
                    {
                        case AuthServerConfigResourceType.OpenId:
                            output.Add(new IdentityResources.OpenId());
                            break;
                        case AuthServerConfigResourceType.Profile:
                            output.Add(new IdentityResources.Profile());
                            break;
                        case AuthServerConfigResourceType.Email:
                            output.Add(new IdentityResources.Email());
                            break;
                        case AuthServerConfigResourceType.Phone:
                            output.Add(new IdentityResources.Phone());
                            break;
                        case AuthServerConfigResourceType.Address:
                            output.Add(new IdentityResources.Address());
                            break;
                        default:
                            throw new Exception("Unknown Identity Resource Type");
                    }
                }
            }

            return output;
        }

        public List<TestUser> GetTestUsers()
        {
            var output = new List<TestUser>();

            if (this.InstanceValid())
            {
                foreach(var i in this.instance.AuthServerUsers)
                {
                    var newUser = new TestUser();
                    newUser.SubjectId = i.SubjectId;
                    newUser.Username = i.Username;
                    newUser.Password = i.Password;

                    foreach(var claim in i.Claims)
                    {
                        //new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                        newUser.Claims.Add(new Claim(claim.ClaimType, claim.ClaimValue, claim.ValueType));
                    }

                    output.Add(newUser);
                }
            }

            return output;
        }
    }
}
