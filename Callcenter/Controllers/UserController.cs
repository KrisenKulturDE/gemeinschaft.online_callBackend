using Callcenter.Models;
using Callcenter.Models.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Callcenter.Controllers
{
    //https://jasonwatmore.com/post/2019/10/11/aspnet-core-3-jwt-authentication-tutorial-with-example-api
    //https://docs.microsoft.com/de-de/aspnet/core/signalr/authn-and-authz?view=aspnetcore-3.1
    internal partial class SignalRHub : Hub
    {

        public async Task<TokenWith<SignInResult>> Login(string userName, string password)
        {
            ApplicationUser user = await userManager.FindByNameAsync(userName);
            if (user == null)
                throw new KeyNotFoundException("Username not found");
            SignInResult result = await siginmanager.CheckPasswordSignInAsync(user, password, true);
            TokenWith<SignInResult> ret;
            if (result.Succeeded)
            {
                //database.SendRequestToUser(Context.ConnectionId , user.zips);
                ret = await TokenWith<SignInResult>(result, user);
            }
            else
            {
                ret = await Task<TokenWith<SignInResult>>.FromResult(new TokenWith<SignInResult>(result, null));
            }
            return ret;
        }
        [Authorize]
        public async Task<TokenWith<DateTime>> RenewToken()
        {
            return await TokenWith<DateTime>(DateTime.Now.AddMinutes(JwtManager.ExpireMinutes), await GetUser());
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
        public async Task<ApplicationUser> Logout()
        {
            var user = await GetUser();
            await siginmanager.SignOutAsync();
            return await Task<ApplicationUser>.FromResult(user);
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
            return await userManager.FindByEmailAsync(user);
        }
        private async Task<TokenWith<T>> TokenWith<T>(T data, ApplicationUser user)
        {
            DateTime now = DateTime.UtcNow;
            IdentityUserToken<Guid> token = new IdentityUserToken<Guid>()
            {
                UserId = user.Id,
                Name = user.Email,
                Value = now.ToString()
            };
            //Task t = new Task(()=>user.Tokens.RemoveAll(t => now.AddMinutes(JwtManager.ExpireMinutes) < now));
            //t.Start();
            //user.AddUserToken<IdentityUserToken<Guid>>(token);
            //user.AddUserToken(token);
            //t.Wait();
            //await userManager.UpdateAsync(user);
            TokenWith<T> result = new TokenWith<T>(data, jwtManager.GenerateToken(token, now));
            return await Task<TokenWith<T>>.FromResult(result);
        }
    }
}
