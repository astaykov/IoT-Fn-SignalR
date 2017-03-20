using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using IoT_Fn_SignalR.Models;
using Microsoft.AspNet.SignalR.Hubs;
using System.Security.Principal;
using Microsoft.Owin.Security;
using System.Threading.Tasks;

namespace IoT_Fn_SignalR.Hubs
{
    [HeadersAuth]
    public class IotHub : Hub
    {
        public void Hello()
        {
            Clients.All.hello();
        }

        public void UpdateStatus(RelayState state)
        {
            Clients.All.updateStatus(state);
        }
    }



    public class HeadersAuthAttribute : AuthorizeAttribute
    {
        public override bool AuthorizeHubConnection(HubDescriptor hubDescriptor, IRequest request)
        {
            bool authResult = base.AuthorizeHubConnection(hubDescriptor, request);

            //var token = request.QueryString.Get("Bearer");
            //var authenticationTicket = Startup.AuthServerOptions.AccessTokenFormat.Unprotect(token);

            //if (authenticationTicket == null || authenticationTicket.Identity == null || !authenticationTicket.Identity.IsAuthenticated)
            //{
            //    return false;
            //}

            //request.Environment["server.User"] = new ClaimsPrincipal(authenticationTicket.Identity);
            //request.Environment["server.Username"] = authenticationTicket.Identity.Name;
            //request.GetHttpContext().User = new ClaimsPrincipal(authenticationTicket.Identity);

            if (!authResult)
            {
                var owinAuthMan = request.GetHttpContext().GetOwinContext().Authentication;
                var authTypes = owinAuthMan.GetAuthenticationTypes();

                AuthenticateResult aResult = default(AuthenticateResult);

                foreach (var auth in authTypes)
                {
                    aResult = owinAuthMan.AuthenticateAsync(auth.AuthenticationType).Result;
                    if (aResult != null && aResult.Identity.IsAuthenticated)
                    {
                        authResult = true;
                        break;
                    }
                }

                if (!string.IsNullOrWhiteSpace(request.Headers["Authorization"]))
                {
                    authResult = true;
                }
            }
            return authResult;

        }

        public override bool AuthorizeHubMethodInvocation(IHubIncomingInvokerContext hubIncomingInvokerContext, bool appliesToMethod)
        {
            bool userAuthenticated = hubIncomingInvokerContext.Hub.Context.User.Identity.IsAuthenticated;
            return base.AuthorizeHubMethodInvocation(hubIncomingInvokerContext, appliesToMethod);
        }

        //public override bool AuthorizeHubConnection(HubDescriptor hubDescriptor, IRequest request)
        //{
        //    if (string.IsNullOrEmpty(request.Headers[UserIdHeader]))
        //    {
        //        return false;
        //    }

        //    return true;
        //}
    }
}

