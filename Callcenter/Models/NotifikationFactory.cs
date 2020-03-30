using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Callcenter.DBConnection;
using Callcenter.Models.Identity;

namespace Callcenter.Models
{
    internal class NotifikationFactory
    {
        private readonly Database database;
        public NotifikationFactory(Database database)
        {
            this.database = database;
        }

        internal void Send(Notifikation notifikation, ApplicationUser user, Entry entry)
        {
            Console.WriteLine($"Notifikation Gesendet: {user.NormalizedUserName} Telefonnummer: {entry.phone}");
        }
    }
}
