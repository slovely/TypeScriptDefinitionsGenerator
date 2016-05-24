using System;
using System.Globalization;
using Microsoft.AspNet.SignalR;

namespace SampleWebsite.Models.Hubs
{
    public class TimeHub : Hub
    {
        public string CurrentServerTime()
        {
            return DateTime.Now.ToString(CultureInfo.CurrentCulture);
        }
    }
}