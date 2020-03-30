using Callcenter.Models;
using Callcenter.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Callcenter.Controllers
{
    internal partial class SignalRHub : Hub
    {

        public SignInResult Login(string userName, string password)
        {
            ApplicationUser user = userManager.FindByNameAsync(userName).Result;
            if (user == null)
                throw new KeyNotFoundException("Username not found");
            SignInResult result = siginmanager.CheckPasswordSignInAsync(user, password, true).Result;
            if (result.Succeeded)
            {
                database.SendRequestToUsers(Context.ConnectionId , user);
            }
            return result;
        }

        public async Task<IdentityResult> SetPlzToUser(string userid, string zip)
        {
            ApplicationUser user = await userManager.FindByIdAsync(userid);
            List<string> zips = ApplicationUser.ParseZips(zip);
            user.zips = zips;
            return await userManager.UpdateAsync(user);
        }
        public IQueryable<ApplicationUser> GetAllUsers()
        {
            return userManager.Users;
        }
        public async Task<IEnumerable<EntryTransport>> GetMyEntrys() => await GetMyEntrys(await GetUser());
        private async Task<IEnumerable<EntryTransport>> GetMyEntrys(ApplicationUser user)
        {
            var entrys = await database.FindRequestForUsers(user);
            return entrys.ToEnumerable<Entry>().Select(entry => entry.TrasportModel);
        }
        public Task<IdentityResult> RegisterUser(string userName, string password, string email)
        {
            var user = new ApplicationUser(userName, email);
            return userManager.CreateAsync(user, password);
        }
        public async void Logout()
        {
            await siginmanager.SignOutAsync();
        }
        private void ThrowIfNoAccessToZip(string zip)
        {
            if (siginmanager.Context.User == null)
            {
                throw new UnauthorizedAccessException("No Access to the Entry by Zip");
            }
        }
        public async Task<ApplicationUser> GetUser()
        {
            var user = siginmanager.Context.User.Identity?.Name;
            if (user == null)
                return null;
            return await userManager.FindByNameAsync(user);
        }

    }
}
