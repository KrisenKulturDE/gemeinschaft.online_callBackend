using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Callcenter.Models.Identity
{
    [CollectionName("Users")]
    public class ApplicationUser : MongoIdentityUser<Guid>
    {
        public ApplicationUser() : base()
        { }

        public ApplicationUser(string userName, string email) : base(userName, email)
        {
        }
        public bool isAdmin { get; set; }
        public List<string> zips { get; set; }
        public List<EntryRequest> notifyrequest { get; set; }




        internal static List<string> ParseZips(string zip)
        {
            List<string> ret = new List<string>();
            foreach (string i in zip.Split('\n'))
            {
                var str = i.Trim();
                if (!String.IsNullOrWhiteSpace(str) && str.Length <= 5 && int.TryParse(str, out int plz))
                {
                    ret.Add(plz.ToString());
                }
            }
            return ret;
        }
        public string NotifyRequestString()
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (EntryRequest er in notifyrequest)
            {
                if (!first)
                {
                    sb.Append(", ");
                }
                sb.Append(er.ToString());
                first = false;
            }
            return sb.ToString();
        }
    }
}
