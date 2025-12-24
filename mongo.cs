using MongoDB.Driver;
using MongoDB.Bson;
using System.Data.Common;
namespace Airgeadlamh.YoutubeUploader
{
    public class Mongo
    {
        static string connectionString = Environment.GetEnvironmentVariable("MONGODB_URI") ?? "mongodb://localhost:27017/";
        public static MongoClient client = new MongoClient(connectionString);
        public static IMongoDatabase db = client.GetDatabase("KaraokeSplitter");
        public static void add_stream(Stream stream){
            var col = db.GetCollection<Stream>("Streams");
            col.InsertOne(stream);
        }
        public static void AddSongEntry(SongEntry song)
        {
            var stream = Program.stream;
            var collection = db.GetCollection<SongEntry>(stream.Name);
            collection.InsertOne(song);
            /*var filter = Builders<SongEntry>.Filter.Eq("_id", songEntry.Id);
            var updateResult = collection.UpdateOne(filter, new UpdateDefinition<MongoDBSongEntry>
            {
                $set = songEntry.ToBsonDocument()
            });*/
        }
    }
}
