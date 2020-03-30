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
using System.Threading.Tasks;

namespace Callcenter.Controllers
{
    internal partial class SignalRHub : Hub
    {
        private readonly Database database;
        private readonly SignInManager<ApplicationUser> siginmanager;
        private readonly UserManager<ApplicationUser> userManager;
        public SignalRHub(Database database, SignInManager<ApplicationUser> siginmanager, UserManager<ApplicationUser> userManager)
        {
            this.database = database;
            this.siginmanager = siginmanager;
            this.userManager = userManager;
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
    }
}
