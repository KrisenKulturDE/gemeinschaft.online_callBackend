using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Callcenter.Config;
using Callcenter.Controllers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using System.Text;
using Callcenter.Models;

namespace Callcenter.DBConnection
{
    internal partial class Database
    {
        private readonly IMongoCollection<Notifikation> notifications;
        private readonly NotifikationFactory notifikationFactory;

        public Database(IOptions<MongoDbConf> options, IHubContext<SignalRHub> hubContext)
        {
            var mongoDbConf = options.Value;
            MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(mongoDbConf.Connection));
            settings.SslSettings = new SslSettings { EnabledSslProtocols = SslProtocols.Tls12 };
            var mongoClient = new MongoClient(settings);

            var database = mongoClient.GetDatabase(mongoDbConf.DbName);

            requests = database.GetCollection<Entry>("requests");
            //captchas = database.GetCollection<Captcha>("captcha");
            observer = database.GetCollection<Observer>("observer");
            notifications = database.GetCollection<Notifikation>("notifications");
            CreateIndexOptions<Notifikation> notificationIndexoptions = new CreateIndexOptions<Notifikation>();
            notificationIndexoptions.Unique = true;
            var notificationIndex = new CreateIndexModel<Notifikation>(Builders<Notifikation>.IndexKeys.Combine(
                Builders<Notifikation>.IndexKeys.Ascending(n => n.entry),
                Builders<Notifikation>.IndexKeys.Ascending(n => n.organisation)                
                ), notificationIndexoptions);
            notificationIndex.Options.Unique = true;
            notifications.Indexes.CreateOne(notificationIndex);
            _hubContext = hubContext;
            notifikationFactory = new NotifikationFactory(this);
            Listen();

        }

     


        private static string inreg(string inpt)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{$in: [");
            for (int i = 0; i < inpt.Length; i++)
            {
                sb.Append('\'');
                sb.Append(inpt.Substring(0, inpt.Length - i));
                sb.Append("', ");
            }
            sb.Append("]}");
            return sb.ToString();
        }


        //public static Random random = new Random();
        private IHubContext<SignalRHub> _hubContext;

    }
}