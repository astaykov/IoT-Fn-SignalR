using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalRConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            DoWhatYouNeed();
            Console.Write("Press any kay to exit");
            Console.ReadKey();
        }

        private static async void DoWhatYouNeed()
        {
            var hubConnection = new HubConnection("http://iot-fn-signalr.azurewebsites.net/");
            IHubProxy stockTickerHubProxy = hubConnection.CreateHubProxy("IotHub");
            await hubConnection.Start();
            for (int i = 0; i < 100; i++)
            {
                await stockTickerHubProxy.Invoke("UpdateStatus",
                    new RelayState
                    {
                        DeviceId = "ConsoleApp",
                        RelayOneState = false,
                        RelayTwoState = true
                    });
                await Task.Delay(1000);
            }
        }
    }

    public class RelayState
    {
        //{"DeviceId":"esp8266relay", "RelayOneState":false, "RelayTwoState":false}
        public string DeviceId { get; set; }
        public bool RelayOneState { get; set; }
        public bool RelayTwoState { get; set; }
    }
}
