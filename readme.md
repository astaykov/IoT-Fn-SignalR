# Azure IoT Hub based real-time web dashboards

This is a quick start solution for everyone who wants to quickly experiement with Azure IoT and Web.

> **Warning**
> 
> There is neither limiting nor throttling of the events coming from IoT Hub!
> If there are too many events coming out from your IoT Hub, you may face issues
> with your web!

> **Disclaimer**
>
> For the sake of simplicity, everything is unauthenticated!

## Really simple architecture
All we have is
* Azure IoT Hub (and devices which send data to the hub)
* Azure Function with Event Hub trigger
* Web App with SignalR hub
* A common POCO (plain old CLR object)

![simple diagram](https://github.com/astaykov/IoT-Fn-SignalR/blob/master/diagram.png?raw=true "Data flow diagram")

## Azure IoT Hub tricks
In order to have an Azure Function to listen to IoT Hub, we need to get the Event Hub compatible endpoint for our IoT Hub.
We do this from the **Endpoints** setting of the IoT Hub and chose the **Messaging** endpoint.

Then we have to take the shared key. We can use the "service" shared access policy for our function.

At the end, we construct our EventHub compatible connection string as follows:

> Endpoint=[event-hub-compatible-endpoint];SharedAccessKeyName=service;SharedAccessKey=the-key-with-the-equals-sign-at-the-end

The result is something similar to:

> Endpoint=sb://iothub-ns-myhub-25435-f23a23b4c7.servicebus.windows.net/;SharedAccessKeyName=service;SharedAccessKey=aB0rp1rNpURBjdq2RzcOu46qMuP91Kpc0HJ0Qw=

## Web App with SignalR

There is nothing more trivial than writing an ASP.NET Web App and wiring up a SignalR Hub. There are tons of sample on the Internet
Still, here, the most important parts are:

### The common Object model
This model is shared between WebApp backend, JavaScript on the client, devices that send data to IoT Hub and the Azure Function

```csharp
    public class RelayState
    {
        public string DeviceId { get; set; }
        public bool RelayOneState { get; set; }
        public bool RelayTwoState { get; set; }
    }
```

### The SignalR Hub implementation
Not really a rocket science

```csharp
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
```

### HTML JavaScript for client side real-time updates
Again, nothing fancy, we have defined one DIV container with one unsorted list for the updates:

	<div class="container" style="width: auto; height: 200px; overflow: auto;">
		<ul id="discussion"></ul>
	</div>

And our JavaScript (after including all the neccessery files) looks like that:
```javascript
    <script>
        $(function () {
            // Reference the auto-generated proxy for the hub.
            var iot = $.connection.iotHub;
            // Create a function that the hub can call back to display messages.
            iot.client.updateStatus = function (status) {
                // Add the message to the page.
                $('#discussion').append('<li><strong>' + htmlEncode(status.DeviceId)
                    + '</strong>: RelayOne: ' + htmlEncode(status.RelayOneState) + ' | RelayTwo: ' + htmlEncode(status.RelayTwoState) + '</li>');
            };
            $.connection.hub.start().done(function () {});
        });
        // This optional function html-encodes messages for display in the page.
        function htmlEncode(value) {
            var encodedValue = $('<div />').text(value).html();
            return encodedValue;
        }
    </script>
```
## Azure Function
The final and the plumbing component of our solution. An azure function which automatically triggers when there is a message in our IoT Hub.

It is probably a less known fact, that SignalR has actually a .NET Client. So we connect to SignalR hub from any .NET application, not only from a WebApp/JavaScript.
We will use that client to connect to the SignalR Hub from our Azure Function. So, we have to add a project dependency on the SignalR client and the Azure Functions
infrastructure will automatically resolve that and download the NuGet package. This is done in a *project.json* file:
```
{
	"frameworks": {
		"net46":{
			"dependencies": {
				"Microsoft.AspNet.SignalR.Client": "2.2.1"
			}
		}
	}
}
```
Once we have our dependencies listed, let's write our function. Note that Newtonsoft.Json package is already added to the Azure Functions infrastructure and we do not
explicitly need to advertise this in project.json, but we need to include it with the **#r** directive.
Here is the code of our Function. As simple as:

```csharp
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
    public string DeviceId { get; set; }
    public bool RelayOneState { get; set; }
    public bool RelayTwoState { get; set; }
}
```

## Confusion disclaimer

> I am pretty sure you are already confused about the double (or triple) meaning of the word **hub**.
> Indeed there are a lot of **Hubs** and please try to not confuse one with the other.
> There is IoT Hub (an Azure Service), there is Event Hub (another Azure Service) and there is 
> SignalR Hub (programm code which lives in our web app).