using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Callcenter.Models;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using System.Security.Authentication;
using System.Text;
using System.Runtime.Serialization.Json;
using System.IO;

namespace Callcenter.Controllers
{
    internal partial class SignalRHub
    {

        /// <summary>
        /// Markiert einen Eintrag als nicht mehr bearbeitet.
        /// </summary>
        public Task FreeEntry(string id)
        {
            Task t = new Task(() =>
            {
                Entry entry = database.Find(new ObjectId(id));
                if (entry != null)
                {
                    entry.marked = false;
                    database.Replace(entry);
                }
            });
            t.Start();
            return t;
        }
        /// <summary>
        /// Markiert einen Eintrag als bearbeitet.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<EntryFill> MarkEntry(string id)
        {
            Task<EntryFill> t = new Task<EntryFill>(() =>
            {

                Entry entry = database.Find(new ObjectId(id));
                if (entry != null)
                {
                    entry.marked = true;
                    database.Replace(entry);
                }
                return entry.TrasportModel;
            });
            t.Start();
            return t;
        }
        /// <summary>
        /// Speichert, eine neuen eintrag, wird ein verwendeter eintrag gespiechert, wird ein neuer mit altem timestamp erzeugt.
        /// </summary>
        /// <returns></returns>
        public Task<EntryFill> AddOrModifyEntry(string id, string phone, string zip, string request)
        {
            Task<EntryFill> t = new Task<EntryFill>(() =>
            {
                Entry entry = entry = new Entry()
                {
                    timestamp = DateTime.Now,
                };
                try
                {
                    if (String.IsNullOrWhiteSpace(zip))
                    {
                        zip = "00000";
                    }
                    if (!(String.IsNullOrWhiteSpace(id) || id.Equals("000000000000000000000000")))
                    {
                        var oldvalue = database.Find(new ObjectId(id));
                        entry = new Entry()
                        {
                            timestamp = oldvalue.timestamp,
                        };
                    }
                    entry.modifyts = DateTime.Now;
                    entry.phone = phone;
                    entry.zip = zip;
                    entry.request = ParseRequest(request);
                    entry.Validate();
                    database.Add(entry);
                    return entry.TrasportModel;
                }
                catch (Exception e)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Fehler: ");
                    DataContractJsonSerializer dcjs = new DataContractJsonSerializer(entry.GetType());
                    using (MemoryStream ms = new MemoryStream())
                    {
                        dcjs.WriteObject(ms, entry);
                        sb.AppendLine(Encoding.Default.GetString(ms.ToArray()));
                    };
                    sb.AppendLine(e.ToString());
                    Console.WriteLine(sb.ToString());
                    throw e;
                }
            });
            t.Start();
            return t;
        }

        /// <summary>
        /// Löscht einen Enitrag aus der Datenbank
        /// </summary>
        /// <returns></returns>
        public Task DeleteEntry(string id)
        {
            Task t = new Task(() =>
            {
                Entry entry = database.Find(new ObjectId(id));
                if (entry == null)
                {
                    throw new KeyNotFoundException("Id ist ungültig");
                }
                database.Remove(entry);
                Clients.Caller.SendAsync("delete", id);
            });
            t.Start();
            return t;
        }
        /// <summary>
        /// Gibt Alle einträge zurück
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public IEnumerable<EntryFill> GetAllEntrys(int skip, int limit) => database.GetAll(skip, limit).Select(e => e.TrasportModel);
        public long CountNoZip() => database.CountNoZip();
        public long CountCallHour() => database.CountCallHour();
        public long CountEditHour() => database.CountEditHour();
        public long CountCallDay() => database.CountCallDay();
        public long CountEditDay() => database.CountEditDay();
    }
}
