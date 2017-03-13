using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IoT_Fn_SignalR.Models
{
    public class RelayState
    {
        //{"DeviceId":"esp8266relay", "RelayOneState":false, "RelayTwoState":false}
        public string DeviceId { get; set; }
        public bool RelayOneState { get; set; }
        public bool RelayTwoState { get; set; }
    }
}