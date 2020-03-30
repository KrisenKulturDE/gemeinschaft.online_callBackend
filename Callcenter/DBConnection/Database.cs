using MongoDB.Driver;
using System.Security.Authentication;
using Callcenter.Config;
using Callcenter.Controllers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using System.Text;
using Callcenter.Models;
using Callcenter.Models.Identity;
using System;
using System.Threading.Tasks;

namespace Callcenter.DBConnection
{
    internal partial class Database
    {
        private readonly IMongoCollection<Notifikation> notifications;
        private readonly NotifikationFactory notifikationFactory;
        private readonly IMongoClient mongoClient;
        private readonly IMongoDatabase mongoDatabase;
        public Database(IOptions<MongoDbConf> options, IHubContext<SignalRHub> hubContext)
        {
            var mongoDbConf = options.Value;
            MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(mongoDbConf.Connection));
            settings.SslSettings = new SslSettings { EnabledSslProtocols = SslProtocols.Tls12 };
            mongoClient = new MongoClient(settings);

            mongoDatabase = mongoClient.GetDatabase(mongoDbConf.DbName);

            requests = mongoDatabase.GetCollection<Entry>("requests");
            //captchas = database.GetCollection<Captcha>("captcha");
            users = mongoDatabase.GetCollection<ApplicationUser>("Users");
            notifications = mongoDatabase.GetCollection<Notifikation>("notifications");
            CreateIndexOptions<Notifikation> notificationIndexoptions = new CreateIndexOptions<Notifikation>();
            notificationIndexoptions.Unique = true;
            var notificationIndex = new CreateIndexModel<Notifikation>(Builders<Notifikation>.IndexKeys.Combine(
                Builders<Notifikation>.IndexKeys.Ascending(n => n.entry),
                Builders<Notifikation>.IndexKeys.Ascending(n => n.user)                
                ), notificationIndexoptions);
            notificationIndex.Options.Unique = true;
            notifications.Indexes.CreateOne(notificationIndex);
            _hubContext = hubContext;
            notifikationFactory = new NotifikationFactory(this);
            Listen();

        }



        private static string inreg(params string[] input)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{$in: [");
            foreach (string inpt in input)
            {
                for (int i = 0; i < inpt.Length; i++)
                {
                    sb.Append('\'');
                    sb.Append(inpt.Substring(0, inpt.Length - i));
                    sb.Append("', ");
                }
            }
            sb.Append("]}");
            return sb.ToString();
        }


        //public static Random random = new Random();
        private IHubContext<SignalRHub> _hubContext;

    }
}