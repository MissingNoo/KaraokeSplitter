using System.Runtime.InteropServices;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Airgeadlamh.YoutubeUploader
{
    public class Stream
    {
        public ObjectId _id = ObjectId.GenerateNewId();
        required public string Name {get;set;}
        required public string Streamer {get;set;}
        required public string Link {get;set;}
        required public string File {get;set;}
        required public List<SongEntry> Songs {get;set;}
        required public Boolean Member {get;set;}
    }

    public class SongEntry
    {
        public ObjectId _id = ObjectId.GenerateNewId();
        required public string Name {get;set;}
        required public string Start {get;set;}
        required public string End {get;set;}
        public string? Type {get;set;}
    }
    internal class Program {
        public static string mp3_image = "";
        public static string make_mp3 = "";
        public static string upload_to_youtube = "";
        public static string mp4_path = "";
        public static string mp3_path = "";
        public static bool verbose = false;
        public static Stream stream = new Stream
        {
            File = "",
            Link = "",
            Name = "",
            Songs = [],
            Streamer = "",
            Member = false
        };
        /*public static Stream stream = new Stream
        {
            Name = stream.Name,
            Streamer = stream.Streamer,
            Link = stream.Link,
            Songs = new List<SongEntry>()
        };*/
        public static bool os_windows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public static bool os_linux = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        static List<Stream>? stream_list;
        private static string out_dir = "";
        static void Main(string[] args)
        {
            //Mongo.add_upload();
            if (args.Length > 0 && args[0] == "list")
            {
                Console.WriteLine("Checking existing videos");
                Console.WriteLine("============================");
                try
                {
                    new MyUploads().Run().Wait();
                }
                catch (AggregateException ex)
                {
                    foreach (var e in ex.InnerExceptions)
                    {
                    Console.WriteLine("Error: " + e.Message);
                    }
                }
            }
            while (true)
            {
                #region Selecting stream
                Directory.CreateDirectory("stream_files");
                stream_list = Mongo.db.GetCollection<Stream>("Streams").Find(Builders<Stream>.Filter.Empty).ToList();
                int _i = 0;
                foreach (var item in stream_list)
                {
                    Console.WriteLine($"{++_i}: {item.Name}");
                }
                #endregion
                Console.WriteLine("A: Create new stream file");
                
                Console.WriteLine();
                Console.Write("Select the stream file: ");
                string selected = Console.ReadLine() ?? "";
                int selectednumber;
                bool isNumber = int.TryParse(selected, out selectednumber);
                if (isNumber)
                {
                    
                    process_stream(selectednumber - 1);
                }
                else {
                    switch (selected.ToUpper())
                    {
                        case "A":
                            create_stream();
                            //stream_list = Directory.GetFiles("streams");
                            break;
                    }
                }
            }
        }

        private static void create_stream(){
            Console.WriteLine("Stream name:");
            string name = Console.ReadLine() ?? "";
            Console.WriteLine("Stream link:");
            string link = Console.ReadLine() ?? "";
            Console.WriteLine("Streamer name:");
            string streamername = Console.ReadLine() ?? "";
            Console.WriteLine("Stream filename:");
            string filename = Console.ReadLine() ?? "";
            Console.WriteLine("Member only(s/n):");
            string mb = Console.ReadLine() ?? "";
            Boolean m = mb.ToUpper() == "S" ? true : false;
            Stream stream = new Stream
            {
                File = filename,
                Name = name,
                Streamer = streamername,
                Link = link,
                Songs = new List<SongEntry>(),
                Member = m
            };
            Mongo.add_stream(stream);
        }

        private static void process_stream(int selectednumber){
            while (true) {
                Console.Clear();
                var db = Mongo.db;
                var streams = db.GetCollection<Stream>("Streams");
                #pragma warning disable CS8602 //This is never null because the options come from the list
                var filter = Builders<Stream>.Filter.Eq(s => s.Name, stream_list[selectednumber].Name);
                #pragma warning restore CS8602 // Dereference of a possibly null reference.
                stream = streams.Find(filter).FirstOrDefault();
                var songs_col = db.GetCollection<SongEntry>(stream.Name);
                stream.Songs = songs_col.Find(Builders<SongEntry>.Filter.Empty).ToList();
                //if (upload_to_youtube == "yes" && !File.Exists("client_secrets.json"))
                //{
                //    Console.WriteLine("You need the client_secrets.json from the Youtube API to do uploads!");
                //    upload_to_youtube = "no";
                //}
                
                
                
                out_dir = Path.Join("output", stream.Name);
                Directory.CreateDirectory(out_dir);
                Directory.CreateDirectory(Path.Join(out_dir, "mp4"));

                Console.WriteLine($"\n====================================\nStream name: {stream.Name} \nStream Link: {stream.Link} \nStreamer: {stream.Streamer} \nStream file: {Path.GetFileName(stream.File)}\nMember only: {stream.Member} \n====================================\n");
                int i = 0;
                foreach (var song in stream.Songs)
                {
                    Console.WriteLine($"{++i}: {song.Name} - ({song.Start} - {song.End})");
                }
                Console.WriteLine("\nA: Add song\nP: Process all Songs\nR: Reprocess all songs\nU: Upload all songs\nM: Make all MP3\nD: Add date to title of already uploaded ones\nML: Make list for youtube\nQ: Quit");
                string selected = Console.ReadLine() ?? "";
                int snumber;
                bool isNumber = int.TryParse(selected, out snumber);
                
                if (isNumber)
                {
                    var song = stream.Songs[snumber - 1];
                    Console.WriteLine($"\nSelected Song: \"{song.Name}\"\nU: Update moments\nD: Delete\nUP: Upload\nR: Reprocess\nM: Make MP3\nP: Play Song (mp4)\nQ: Quit");
                    string subselected = Console.ReadLine() ?? "";
                    var collection = db.GetCollection<SongEntry>(stream.Name);
                    var song_filter = Builders<SongEntry>.Filter.Eq("Name", song.Name);
                    switch (subselected.ToUpper())
                    {
                        case "U":
                            Console.WriteLine($"\nStart time {song.Start}:");
                            string s = Console.ReadLine() ?? "".Replace(" ", "");
                            if (s != "")
                            {
                                var updateResult = collection.UpdateOne(song_filter, Builders<SongEntry>.Update.Set("Start", s));
                            }
                            Console.WriteLine($"\nEnd time {song.End}:");
                            s = Console.ReadLine() ?? "".Replace(" ", "");
                            if (s != "")
                            {
                                var updateResult = collection.UpdateOne(song_filter, Builders<SongEntry>.Update.Set("End", s));
                            }                            
                            break;
                        case "D":
                            collection.DeleteOne(song_filter);
                            break;
                        case "UP":
                            process_song(song, false, false, 0, true);
                            break;
                        case "R":
                            process_song(song, true);
                            break;
                        case "M":
                            process_song(song, true, true, snumber, false);
                            break;
                        case "P":
                            Functions.shell_run($"mpv {Path.Join("output", stream.Name, "mp4", song.Type, song.Name).Replace(" ", "\\ ") + ".mp4"}", verbose);
                            break;
                        case "P3":
                            Functions.shell_run($"mpv {Path.Join("output", "mp3", stream.Streamer, song.Name).Replace(" ", "\\ ") + ".mp3"}", verbose);
                            break;
                    }
                    
                }
                else {
                    switch (selected.ToUpper())
                    {
                        case "A":
                            Console.WriteLine("\nStart time:");
                            string s = Console.ReadLine() ?? "".Replace(" ", "");
                            Console.WriteLine("End time:");
                            string e = Console.ReadLine() ?? "".Replace(" ", "");
                            Console.WriteLine("Name:");
                            string n = Console.ReadLine() ?? "";
                            Console.WriteLine("Type(s:Song/n:Noise):");
                            string t = Console.ReadLine() ?? "";
                            switch (t.ToUpper())
                            {
                                default:
                                    t = "Song";
                                    break;
                                case "S":
                                    t = "Song";
                                    break;
                                case "N":
                                    t = "Noise";
                                    break;
                            }
                            
                            var col = db.GetCollection<SongEntry>(stream.Name);
                            SongEntry song = new SongEntry {
                                Start = s,
                                End = e,
                                Name = n,
                                Type  = t
                            };
                            col.InsertOne(song);
                            break;
                        case "P":
                            process_all();
                            break;
                        case "U":
                            process_all(false, false, true);
                            break;
                        case "R":
                            process_all(true);
                            break;
                        case "M":
                            process_all(false, true);
                            break;
                        case "D":
                            Console.WriteLine("Insert date: ");
                            string day = Console.ReadLine() ?? "";
                            string[] uploaded_list = File.ReadAllText("upload_list.txt").Split("\n");
                            foreach (var _song in stream.Songs)
                            {
                                string title = _song.Name;
                                foreach (var upload in uploaded_list)
                                {
                                    if (upload == $"[{stream.Streamer}] {title}")
                                    {
                                        Console.WriteLine($"{title} - ({day})");
                                        break;
                                    }
                                }
                            }
                            Console.WriteLine("Press any key to continue");
                            Console.Read();
                            break;
                        case "ML":
                            i = 0;
                            Console.WriteLine("Songs in this video:");
                            foreach (var _song in stream.Songs)
                            {
                                if (_song.Type == "Song")
                                {
                                    Console.WriteLine($"{++i}: {_song.Name} - {_song.Start}");
                                }
                            }
                            Console.WriteLine("\nOthers:");
                            foreach (var _song in stream.Songs)
                            {
                                if (_song.Type == "Noise")
                                {
                                    Console.WriteLine($"{_song.Name} - {_song.Start}");
                                }
                            }
                            Console.ReadLine();
                            break;
                        case "Q":
                            Environment.Exit(0);
                            break;
                    }
                    
                }
            
            }
        }

        private static void process_song(SongEntry song, bool force = false, bool mp3 = false, int cur_song = 0, bool upload = false) {
            if (song.Type == null)
            {
                song.Type = "Music";
            }
            mp4_path = Path.Join(out_dir, "mp4", song.Type, song.Name) + ".mp4";
            Directory.CreateDirectory(Path.Join(out_dir, "mp4", song.Type));
            mp3_path = Path.Join("output", "mp3", stream.Streamer, song.Type, song.Name) + ".mp3";
            Console.WriteLine();
            if (!File.Exists(mp4_path) || force)
            {
                Console.WriteLine($"Cutting \"{song.Name}\" from stream, {song.Start} to {song.End}... ");
                if (song.Type == "Song")
                {
                    Functions.shell_run($"/usr/bin/ffmpeg -y -ss \"{song.Start}\" -to \"{song.End}\" -i stream_files/\'{stream.File}\' -c copy \'{mp4_path}\'", verbose);
                } else if (song.Type == "Noise")
                {
                    Functions.shell_run($"/usr/bin/ffmpeg -y -ss \"{song.Start}\" -to \"{song.End}\" -i stream_files/\'{stream.File}\' \'{mp4_path}\'", verbose);
                }
                
            }
            else
            {
                Console.WriteLine($"\"{song.Name}\" already split");
            }
            if (mp3)
            {
                if (!File.Exists(mp3_path) || force)
                {
                    Directory.CreateDirectory(Path.Join("output", "mp3", stream.Streamer, song.Type));
                    Console.WriteLine($"Converting \"{song.Name}\" to mp3... ");
                    Functions.shell_run($"/usr/bin/ffmpeg -y -i \'{mp4_path}\' \'{mp3_path}\'", verbose);
                    var tfile = TagLib.File.Create(mp3_path);
                    tfile.Tag.Title = song.Name;
                    tfile.Tag.Album = stream.Name;
                    tfile.Tag.Performers = [stream.Streamer];
                    tfile.Tag.Track = (uint)cur_song;
                    tfile.Save();
                }
                else
                {
                    Console.WriteLine($"\"{song.Name}\" already converted to mp3");
                }
            }
            if (upload)
            {
                song_upload(song);
            }
        }
        
        private static void process_all(bool force = false, bool mp3 = false, bool uploadsong = false) {
            int cur_song = 1;
            
            foreach (var song in stream.Songs)
            {
                process_song(song, force, mp3, cur_song, uploadsong);
                cur_song++;
            }
        }

        private static void song_upload(SongEntry song) {
            if (stream.Member)
            {
                Console.WriteLine("Can't upload content from member only streams!");
                Console.ReadLine();
            } else {
                string title = $"[{stream.Streamer}] {song.Name}";
                bool already_uploaded = false;
                if (File.Exists("upload_list.txt"))
                {
                    string[] uploaded_list = File.ReadAllText("upload_list.txt").Split("\n");
                    foreach (var upload in uploaded_list)
                    {
                        if (upload == title)
                        {
                            already_uploaded = true;
                        }
                    }
                }
                
                if (!already_uploaded)
                {
                    UploadVideo.video_title = title;
                    UploadVideo.upload();
                }
                else
                {
                    Console.WriteLine($"\"[{stream.Streamer}] {song.Name}\" Already uploaded to Youtube!\n");
                }
            }
        }
    }
}