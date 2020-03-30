using Callcenter.DBConnection;
using Callcenter.Models;
using Callcenter.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Callcenter.Controllers
{
    internal partial class SignalRHub : Hub
    {
        private readonly Database database;
        private readonly SignInManager<ApplicationUser> siginmanager;
        private readonly UserManager<ApplicationUser> userManager;
        private static long ConnectionCount;
        public SignalRHub(Database database, SignInManager<ApplicationUser> siginmanager, UserManager<ApplicationUser> userManager)
        {
            this.database = database;
            this.siginmanager = siginmanager;
            this.userManager = userManager;
        }
        public override Task OnConnectedAsync()
        {
            Interlocked.Increment(ref ConnectionCount);
            return base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception exception)
        {
            Interlocked.Decrement(ref ConnectionCount);
            return base.OnDisconnectedAsync(exception);
        }
        internal static EntryRequest ParseRequest(string request)
        {
            if (int.TryParse(request, out int v))
            {
                return (EntryRequest)v;
            }
            foreach (EntryRequest er in (EntryRequest[])Enum.GetValues(typeof(EntryRequest)))
            {
                if (request.ToLower().Equals(er.ToString().Trim().ToLower()))
                    return er;
            }
            throw new FormatException($"kann \"{request}\" nicht nach EntryRequest umwandeln");
        }
        public async Task scaletest(string eingang)
        {
            await Clients.All.SendAsync("scalerespone", new ScaleResponse(eingang ,ConnectionCount));
        }
    }
    public class ScaleResponse
    {
        public DateTime timestamp { get; set; }
        public string response { get; set; }
        public long countconnections { get; set; }
        public ScaleResponse(string Response, long Count)
        {
            this.timestamp = DateTime.Now;
            this.response = Response;
            this.countconnections = Count;
        }
    }
}
