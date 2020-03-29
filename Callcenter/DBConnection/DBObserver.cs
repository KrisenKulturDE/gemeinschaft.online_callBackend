using Callcenter.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Callcenter.DBConnection
{
    internal partial class Database
    {
        private readonly IMongoCollection<Observer> observer;
        /// <summary>
        /// Eine Bearbeitete Organisation Finden und Ersetzen
        /// </summary>
        /// <param name="entry"></param>
        internal void UpdateObserver(Observer entry)
        {
            entry.Verify();
            observer.ReplaceOne(o => o.id == entry.id, entry);
        }

        /// <summary>
        /// Findet eine Orgnisation über die id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal Observer FindObserver(string id) => FindObserver(new ObjectId(id));
        internal Observer FindObserver(ObjectId id) => observer.Find(i => i.id == id).SingleOrDefault();

        /// <summary>
        /// Fügt eine Organisation in die Datenbank hinzu
        /// </summary>
        /// <param name="entry"></param>
        internal void AddObserver(Observer entry)
        {
            entry.Verify();
            observer.InsertOne(entry);
        }

        /// <summary>
        /// Gibt alle Organisationen Zurück
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<Observer> GetObserver() => observer.Find(o => true).ToEnumerable<Observer>();
        internal IEnumerable<Observer> FindObserver(string suche, bool zipreverse)
        {
            string filter;
            if (zipreverse)
            {
                filter = $"{{$or: [ {{ \"zips\": {{'$regex': '{suche}'}}}},{{ \"name\": {{'$regex': '{suche}'}}}},{{ \"ansprechpartner\": {{'$regex': '{suche}'}}}},{{ \"email\": {{'$regex': '{suche}'}}}}  ]}}";
            }
            else
            {
                filter = $"{{$or: [ {{ \"zips\": {inreg(suche)}}},{{ \"name\": {{'$regex': '{suche}'}}}},{{ \"ansprechpartner\": {{'$regex': '{suche}'}}}},{{ \"email\": {{'$regex': '{suche}'}}}}  ]}}";
            }
            return observer.Find(filter).ToEnumerable<Observer>();
        }












    }
}
