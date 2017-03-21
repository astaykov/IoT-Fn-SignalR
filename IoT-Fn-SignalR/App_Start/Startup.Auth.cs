using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.ActiveDirectory;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace IoT_Fn_SignalR
{
    public partial class Startup
    {
        // App config settings
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AadInstance"];
        private static string tenant = ConfigurationManager.AppSettings["ida:Tenant"];
        private static string redirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];

        // B2C policy identifiers
        public static string SignUpInPolicyId = ConfigurationManager.AppSettings["ida:SignUpInPolicyId"];

        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            // Configure OpenID Connect middleware for each policy
            app.UseOpenIdConnectAuthentication(CreateOptionsFromPolicy(SignUpInPolicyId));

            app.UseWindowsAzureActiveDirectoryBearerAuthentication(
               new WindowsAzureActiveDirectoryBearerAuthenticationOptions
               {
                   
                   Tenant = ConfigurationManager.AppSettings["ida:Tenant"],
                   // This piece is optional - it is used for displaying the user's name in the navigation bar.
                   TokenValidationParameters = new System.IdentityModel.Tokens.TokenValidationParameters
                   {
                       ValidAudience = clientId
                   },
               });
        }

        // Used for avoiding yellow-screen-of-death
        private Task AuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            notification.HandleResponse();
            if (notification.Exception.Message == "access_denied")
            {
                notification.Response.Redirect("/");
            }
            else
            {
                notification.Response.Redirect("/Home/Error?message=" + notification.Exception.Message);
            }

            return Task.FromResult(0);
        }

        private OpenIdConnectAuthenticationOptions CreateOptionsFromPolicy(string policy)
        {
            return new OpenIdConnectAuthenticationOptions
            {
                // For each policy, give OWIN the policy-specific metadata address, and
                // set the authentication type to the id of the policy
                MetadataAddress = String.Format(aadInstance, tenant, policy),
                AuthenticationType = policy,

                // These are standard OpenID Connect parameters, with values pulled from web.config
                ClientId = clientId,
                RedirectUri = redirectUri,
                PostLogoutRedirectUri = redirectUri,
                
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    AuthenticationFailed = AuthenticationFailed,
                },
                Scope = "openid",
                ResponseType = "id_token",

                // This piece is optional - it is used for displaying the user's name in the navigation bar.
                TokenValidationParameters = new System.IdentityModel.Tokens.TokenValidationParameters
                {
                    NameClaimType = "name",
                },
            };
        }
    }
}