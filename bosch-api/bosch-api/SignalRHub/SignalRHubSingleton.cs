using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bosch_api.Helper
{
    public static class SignalRHubConnection
    {
        private static HubConnection hubConnection = null;
        //static string hubUrl = "https://localhost:5001/boschapihub";
        //static string hubUrl = "https://bosch-api.azurewebsites.net/boschapihub";
        private static object obj=new object();

        public static HubConnection GetInstance(string hubUrl)
        {
            if(hubConnection==null)
            {
                lock(obj)
                {
                    hubConnection=new HubConnectionBuilder()
                        .WithUrl(hubUrl)
                        .Build();

                    hubConnection.StartAsync().Wait();
                }
            }

            return hubConnection;
        }

    }
}
