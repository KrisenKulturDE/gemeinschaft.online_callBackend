using Callcenter.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace Callcenter.DBConnection
{
    internal partial class Database
    {
        private readonly IMongoCollection<Entry> requests;
        //public List<Entry> GetAll() => collection.Find(e => true).SortBy(e => e.timestamp).ToList();
        /// <summary>
        /// Gibt alle Einträge Sortiert zurück
        /// </summary>
        /// <param name="skip">Erstes Element</param>
        /// <param name="limit">Anzahl der Elemente</param>
        /// <returns></returns>
        public List<Entry> GetAll(int skip, int limit)
        {
            var list = requests.Find(e => !e.finishts.HasValue).Skip(skip).Limit(limit).ToList();
            list.Sort(Entry.Compare);
            return list;
        }


        //public List<Entry> GetNoZip() => collection.Find(e => e.zip == "00000").SortBy(e => e.timestamp).ToList();
        /// <summary>
        /// Gibt alle einträge zurück, welche keine PLZ Besitzen
        /// </summary>
        /// <returns></returns>
        public List<Entry> GetNoZip()
        {
            var list = requests.Find(e => !e.finishts.HasValue && e.zip == "00000").ToList();
            list.Sort(Entry.Compare);
            return list;
        }




        //internal void Remove(ObjectId id) => requests.DeleteOne(e => e.id == id);
        /// <summary>
        /// Löscht einen Telefon Anruf
        /// </summary>
        /// <param name="id"></param>
        internal void Remove(ObjectId id) => Remove(Find(id));
        internal void Remove(Entry entry)
        {
            entry.finishts = DateTime.Now;
            Replace(entry);
        }
        /// <summary>
        /// SPeichert einen Neuene Anruf in der Datenbank
        /// </summary>
        /// <param name="entry"></param>
        internal void Add(Entry entry)
        {
            if (entry.id == null)
            {
                entry.id = MongoDB.Bson.ObjectId.GenerateNewId();
            }
            requests.InsertOne(entry);
        }
        /// <summary>
        /// Findet einen Telefonanruf
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal Entry Find(ObjectId id) => requests.Find(e => e.id == id).SingleOrDefault();

        /// <summary>
        /// Anzahl aller einträge in der datenkbank
        /// </summary>
        /// <returns></returns>
        public long CountAll() => requests.Find(e => true).CountDocuments();

        /// Anzahl aller Einträge welche keine plz besitzen
        public long CountNoZip() => requests.Find(e => e.zip == "00000").CountDocuments();
        /// <summary>
        /// Anzahl aller Anrufe in den letzten 60 Minuten
        /// </summary>
        /// <returns></returns>
        internal long CountCallHour() => requests.Find(e => e.timestamp > DateTime.Now.Subtract(TimeSpan.FromMinutes(60))).CountDocuments();
        /// <summary>
        /// Anzahl im Frontend Bearbeiteten Einträge in der Letzten Stunde
        /// </summary>
        /// <returns></returns>
        internal long CountEditHour() => requests.Find(e => e.modifyts.HasValue && e.modifyts > DateTime.Now.Subtract(TimeSpan.FromMinutes(60))).CountDocuments();
        /// <summary>
        /// Anzahl aller Anrufe in den letzten 24 Stunden
        /// </summary>
        /// <returns></returns>
        internal long CountCallDay() => requests.Find(e => e.timestamp > DateTime.Now.Subtract(TimeSpan.FromMinutes(1440))).CountDocuments();
        /// <summary>
        /// Anzahl im Frontend Bearbeiteten Einträge in der Letzten 24 Stunden
        /// </summary>
        /// <returns></returns>
        internal long CountEditDay() => requests.Find(e => e.modifyts.HasValue && e.modifyts > DateTime.Now.Subtract(TimeSpan.FromMinutes(1440))).CountDocuments();
        /// <summary>
        /// Ersetzt Einen Telefonanruf in der datenbank
        /// </summary>
        /// <param name="entry"></param>
        internal void Replace(Entry entry) => requests.ReplaceOne(e => e.id == entry.id, entry);
    }
}
