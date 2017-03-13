using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using IoT_Fn_SignalR.Models;

namespace IoT_Fn_SignalR.Hubs
{
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
}