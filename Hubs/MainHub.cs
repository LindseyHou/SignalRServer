using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Net;
using System.IO;
using System.Collections.Generic;

namespace SignalRServer.Hubs
{
    public class MainHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("OnConnectedAsync", "Hello world");
        }
        public string GetData(string methodName, string groupName = "")
        {
            HttpClient client = new HttpClient();
            var response = client.GetAsync("http://127.0.0.1:8001/data?methodName=" + methodName + "&groupName=" + groupName).Result;
            var responseString = response.Content.ReadAsStringAsync().Result;
            return responseString;
        }
        public string GetDatas(string methodName, List<string> groupNames)
        {
            HttpClient client = new HttpClient();
            var url = "http://127.0.0.1:8001/datas??methodName=" + methodName;
            for (int i = 0; i < groupNames.Count; i++)
            {
                url += "&groupNames=" + groupNames[i];
            }
            var response = client.GetAsync(url).Result;
            var responseString = response.Content.ReadAsStringAsync().Result;
            return responseString;
        }

        public string PostBuildInfo(string postDataStr)
        {
            HttpClient client = new HttpClient();
            var content = new StringContent(postDataStr);

            var response = client.PostAsync("http://127.0.0.1:8000/api/v1/data/buildinfo", content).Result;

            var responseString = response.Content.ReadAsStringAsync().Result;
            return responseString;
        }
        public async Task AddToGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("ReceiveMessage", Context.ConnectionId + " Join Group: " + groupName);
        }
        public async Task RemoveFromGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("ReceiveMessage", Context.ConnectionId + " Quit Group: " + groupName);
        }
    }
}
