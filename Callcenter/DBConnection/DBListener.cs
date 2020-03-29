﻿using Callcenter.Models;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Text;

namespace Callcenter.DBConnection
{
    internal partial class Database
    {

        /// <summary>
        /// Listener Für Änderungen in der Entry Datenbank
        /// Sorgt mit hilfe von SignalR dafür, das die Frontends aktualsiert werden.
        /// Prüft ob es eine Organisation gibt, welche eine benachrichtigung z.b. über email aboniert hat und versendert die.
        /// Es ist auch möglich sich auf die PLZ 00000 zu Registrieren.
        /// </summary>
        private async void Listen()
        {
            var options = new ChangeStreamOptions { FullDocument = ChangeStreamFullDocumentOption.UpdateLookup };
            var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<Entry>>()
                .Match("{ operationType: { $in: [ 'insert','replace', 'update' ] }}")
                .Project("{ fullDocument: 1 }");

            using var cursor = requests.Watch(pipeline, options);
            await cursor.ForEachAsync(change =>
            {
                Entry entry = BsonSerializer.Deserialize<Entry>((BsonDocument)change.Elements.ToList()[1].Value);
                if (entry.zip.Equals("00000"))
                {
                    var send = entry.TrasportModel;
                    _hubContext.Clients.All.SendAsync("ItemChange", send);
                    foreach (Observer organisation in observer.Find("{ \"zips\": {$in: [ '00000', ]}}").ToList<Observer>())
                    {
                        var notifikation = new Notifikation()
                        {
                            entry = entry.id.ToString(),
                            organisation = organisation.id.ToString(),
                            timestamp = DateTime.Now
                        };
                        if (TryAddNotifkation(notifikation))
                        {
                            notifikationFactory.Send(notifikation, organisation, entry);
                        }
                    }
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < entry.zip.Length; i++)
                    {
                        sb.Append('\'');
                        sb.Append(entry.zip.Substring(0, entry.zip.Length - i));
                        sb.Append("', ");
                    }
                    foreach (Observer organisation in observer.Find($"{{ \"zips\": {{$in: [ {sb.ToString()} ]}}}}").ToList<Observer>())
                    {
                        var notifikation = new Notifikation()
                        {
                            entry = entry.id.ToString(),
                            organisation = organisation.id.ToString(),
                            timestamp = DateTime.Now
                        };
                        if (TryAddNotifkation(notifikation))
                        {
                            notifikationFactory.Send(notifikation, organisation, entry);
                        }
                    }
                }
            });
        }


        /// <summary>
        /// Versucht einen Neuen Eintrag in die Datenbank zu Speichern.
        /// Die Software ist darauf ausgelegt auf mehreren Servern gleichzeitig zu laufen
        /// in der Methode Listen werden Alle instanzen dieser Software auf einmal informiert.
        /// die Datenbank hat einen Unique Index auf die relevanten Felder (siehe Konstruktor)
        /// Die Schnellste Instanz wird den Eintrag einfügen können die restlichen bekommen eine Exeption
        /// Bei Exception wird false zurück gegeben und bei erfolg true. So wird die Benachrichtigung nur vom Schnellsten Server versendet.
        /// </summary>
        public bool TryAddNotifkation(Notifikation notifikation)
        {
            try
            {
                if (notifikation.id == null)
                {
                    notifikation.id = new ObjectId();
                }
                notifikation.timestamp = DateTime.Now;
                notifications.InsertOne(notifikation);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
