#r "Newtonsoft.Json"
using System;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;

public static async void Run(string iotMessage, TraceWriter log)
{
    log.Info($"C# Event Hub trigger function processed a message: {iotMessage}");
    var srHubConnection = new HubConnection("http://iot-fn-signalr.azurewebsites.net/");
    IHubProxy srHubProxy = srHubConnection.CreateHubProxy("IotHub");
    await srHubConnection.Start();
    try
    {
        RelayState state = JsonConvert.DeserializeObject<RelayState>(iotMessage);
        await srHubProxy.Invoke("UpdateStatus", state);
    }
    catch (Exception e)
    {
        await srHubProxy.Invoke("UpdateStatus",
        new RelayState
        {
            DeviceId = e.Message,
            RelayOneState = false,
            RelayTwoState = false
        });
    }
}

public class RelayState
{
    //{"DeviceId":"esp8266relay", "RelayOneState":false, "RelayTwoState":false}
    public string DeviceId { get; set; }
    public bool RelayOneState { get; set; }
    public bool RelayTwoState { get; set; }
}