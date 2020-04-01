using Callcenter.Models;
using Callcenter.Models.Identity;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDbGenericRepository;
using MongoDbGenericRepository.Attributes;
using MongoDbGenericRepository.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Callcenter.DBConnection
{
    //see
    //https://github.com/alexandre-spieser/mongodb-generic-repository/blob/master/MongoDbGenericRepository/MongoDbContext.cs
    //https://github.com/alexandre-spieser/AspNetCore.Identity.MongoDbCore
    internal partial class Database
    {

        //IMongoClient IMongoDbContext.Client => mongoClient;
        private readonly IMongoCollection<ApplicationUser> users;
        internal Task<IAsyncCursor<ApplicationUser>> FindUserForPlz(string suche, bool zipreverse)
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
            return users.FindAsync(filter);
        }
        internal Task<IAsyncCursor<Entry>> FindRequestForUsers(ApplicationUser user)
        {
            if (user == null || user.zips == null || user.zips.Count == 0)
            {
                return requests.FindAsync(e => false);
            }
            else
            {
                return requests.FindAsync($"{{ \"zips\": {inreg(user.zips.ToArray())}}}");
            }
        }

        internal Task<IAsyncCursor<Entry>> FindRequestForZips(List<string> zips)
        {
            if (zips == null || zips.Count == 0)
            {
                return requests.FindAsync(e => false);
            }
            else
            {
                return requests.FindAsync($"{{ \"zips\": {inreg(zips.ToArray())}}}");
            }
        }

        internal void SendRequestToUser(string connectionid, List<string> zips)
        {
            
            new Task(() => _hubContext.Clients.Client(connectionid).SendAsync("item", FindRequestForZips(zips).Result).Wait()).Start();
        }
        //IMongoDatabase IMongoDbContext.Database => mongoDatabase;
        ///// <summary>
        ///// Drops a collection, use very carefully.
        ///// </summary>
        ///// <typeparam name="TDocument">The type representing a Document.</typeparam>
        //public virtual void DropCollection<TDocument>(string partitionKey = null)
        //{
        //    mongoDatabase.DropCollection(GetCollectionName<TDocument>(partitionKey));
        //}
        //void IMongoDbContext.DropCollection<TDocument>(string partitionKey)
        //{
        //    throw new NotImplementedException();
        //}
        ///// <summary>
        ///// Sets the Guid representation of the MongoDB Driver.
        ///// </summary>
        ///// <param name="guidRepresentation">The new value of the GuidRepresentation</param>
        //public virtual void SetGuidRepresentation(MongoDB.Bson.GuidRepresentation guidRepresentation)
        //{
        //    MongoDefaults.GuidRepresentation = guidRepresentation;
        //}
        //IMongoCollection<TDocument> IMongoDbContext.GetCollection<TDocument>(string partitionKey)
        //{
        //    return mongoDatabase.GetCollection<TDocument>(GetCollectionName<TDocument>(partitionKey));
        //}
        ///// <summary>
        ///// Given the document type and the partition key, returns the name of the collection it belongs to.
        ///// </summary>
        ///// <typeparam name="TDocument">The type representing a Document.</typeparam>
        ///// <param name="partitionKey">The value of the partition key.</param>
        ///// <returns>The name of the collection.</returns>
        //private string GetCollectionName<TDocument>(string partitionKey)
        //{
        //    var collectionName = GetAttributeCollectionName<TDocument>() ?? Pluralize<TDocument>();
        //    if (string.IsNullOrEmpty(partitionKey))
        //    {
        //        return collectionName;
        //    }
        //    return $"{partitionKey}-{collectionName}";
        //}
        ///// <summary>
        ///// Extracts the CollectionName attribute from the entity type, if any.
        ///// </summary>
        ///// <typeparam name="TDocument">The type representing a Document.</typeparam>
        ///// <returns>The name of the collection in which the TDocument is stored.</returns>
        //private string GetAttributeCollectionName<TDocument>()
        //{
        //    return (typeof(TDocument).GetTypeInfo()
        //                             .GetCustomAttributes(typeof(CollectionNameAttribute))
        //                             .FirstOrDefault() as CollectionNameAttribute)?.Name;
        //}
        ///// <summary>
        ///// Very naively pluralizes a TDocument type name.
        ///// </summary>
        ///// <typeparam name="TDocument">The type representing a Document.</typeparam>
        ///// <returns>The pluralized document name.</returns>
        //protected virtual string Pluralize<TDocument>()
        //{
        //    return (typeof(TDocument).Name.Pluralize()).Camelize();
        //}
    }
}
