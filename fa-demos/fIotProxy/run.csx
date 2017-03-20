#r "Newtonsoft.Json"
using System;
using System.Net;
using System.Text;
using System.Configuration;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;

public static async Task Run(string iotMessage, TraceWriter log)
{
    log.Info($"C# Event Hub trigger function processed a message: {iotMessage}");
    var _url = "https://login.microsoftonline.com/snetga.onmicrosoft.com/oauth2/token";
    string token = string.Empty;
    using (WebClient client = new WebClient())
    {
        try
        {
            var resultBytest = client.UploadValues(new Uri(_url)
            , "POST"
            , new System.Collections.Specialized.NameValueCollection {
                {"grant_type","client_credentials" },
                {"resource", ConfigurationManager.AppSettings["id:client_id"]},
                {"client_id", ConfigurationManager.AppSettings["id:client_id"]},
                {"client_secret", ConfigurationManager.AppSettings["id:client_secret"]}
            });
            var resultString = Encoding.Default.GetString(resultBytest);
            dynamic result = JsonConvert.DeserializeObject(resultString);
            token = result.access_token;
            Console.WriteLine(resultString);
        }
        catch (WebException webEx)
        {
            Console.WriteLine("Hmm {0}", webEx.Message);
        }
    }
    var hubConnection = new HubConnection("https://iot-fn-signalr.azurewebsites.net/");
    string authHeader = string.Format("Bearer {0}", token);
    hubConnection.Headers.Add("Authorization", authHeader);
    IHubProxy stockTickerHubProxy = hubConnection.CreateHubProxy("IotHub");
    await hubConnection.Start();
    try
    {
        RelayState state = JsonConvert.DeserializeObject<RelayState>(iotMessage);
        await stockTickerHubProxy.Invoke("UpdateStatus", state);
    }
    catch (Exception e)
    {
        await stockTickerHubProxy.Invoke("UpdateStatus",
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