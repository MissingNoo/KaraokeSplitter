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
    }

    public class SongEntry
    {
        public ObjectId _id = ObjectId.GenerateNewId();
        required public string Name {get;set;}
        required public string Start {get;set;}
        required public string End {get;set;}
    }
    internal class Program {
        public static string mp3_image = "";
        public static string make_mp3 = "";
        public static string upload_to_youtube = "";
        public static string stream_file = "";
        public static string mp4_path = "";
        public static string mp3_path = "";
        public static bool verbose = false;
        public static Stream stream = new Stream
        {
            File = "",
            Link = "",
            Name = "",
            Songs = [],
            Streamer = ""
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
            Console.WriteLine("Filename:");
            string fname = Console.ReadLine() ?? "";
            string filepath = Path.Join("streams", fname);
            Console.WriteLine("Stream name:");
            string name = Console.ReadLine() ?? "";
            File.AppendAllText(filepath, $"stream.Name;{name}" + Environment.NewLine);
            Console.WriteLine("Stream link:");
            string link = Console.ReadLine() ?? "";
            File.AppendAllText(filepath, $"stream.Link;{link}" + Environment.NewLine);
            Console.WriteLine("Streamer name:");
            string streamername = Console.ReadLine() ?? "";
            File.AppendAllText(filepath, $"stream.Streamer;{streamername}" + Environment.NewLine);
            Console.WriteLine("Stream filename:");
            string filename = Console.ReadLine() ?? "";
            File.AppendAllText(filepath, $"stream_file;{filename}" + Environment.NewLine);
            File.AppendAllText(filepath, "#Stream_Info#" + Environment.NewLine);
            Console.WriteLine("Stream file created!");
            Stream stream = new Stream
            {
                File = filename,
                Name = name,
                Streamer = streamername,
                Link = link,
                Songs = new List<SongEntry>()
            };
            Mongo.add_stream(stream);
        }

        private static void process_stream(int selectednumber){
            while (true) {
                Console.Clear();
                var db = Mongo.db;
                var streams = db.GetCollection<Stream>("Streams");
                var filter = Builders<Stream>.Filter.Eq(s => s.Name, "teste");
                stream = (Stream)streams.Find(filter).FirstOrDefault();
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

                Console.WriteLine($"\n====================================\nStream name: {stream.Name} \nStream Link: {stream.Link} \nStreamer: {stream.Streamer} \nStream file: {Path.GetFileName(stream_file)} \n====================================\n");
                int i = 0;
                foreach (var song in stream.Songs)
                {
                    Console.WriteLine($"{++i}: {song.Name}");
                }
                Console.WriteLine("\nA: Add song\nP: Process all Songs\nR: Reprocess all songs\nU: Upload all songs\nM: Make all MP3\nD: Add date to title of already uploaded ones\nQ: Quit");
                string selected = Console.ReadLine() ?? "";
                int snumber;
                bool isNumber = int.TryParse(selected, out snumber);
                
                if (isNumber)
                {
                    var song = stream.Songs[snumber - 1];
                    Console.WriteLine($"\nSelected Song: \"{song.Name}\"\nU: Upload\nR: Reprocess\nM: Make MP3\nP: Play Song (mp4)\nP3: Play Song (mp3)\nQ: Quit");
                    string subselected = Console.ReadLine() ?? "";
                    switch (subselected.ToUpper())
                    {
                        case "U":
                            process_song(song, false, false, 0, true);
                            break;
                        case "R":
                            process_song(song, true);
                            break;
                        case "M":
                            process_song(song, true, true, snumber, false);
                            break;
                        case "P":
                            Functions.shell_run($"mpv {Path.Join("output", stream.Name, "mp4") + song.Name + ".mp4"}", verbose);
                            break;
                        case "P3":
                            Functions.shell_run($"mpv {Path.Join("output", "mp3", stream.Streamer) + song.Name + ".mp3"}", verbose);
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
                            Console.WriteLine("Song name:");
                            string n = Console.ReadLine() ?? "";
                            var col = db.GetCollection<SongEntry>(stream.Name);
                            SongEntry song = new SongEntry {
                                Start = s,
                                End = e,
                                Name = n
                            };
                            col.InsertOne(song);
                            //File.AppendAllText(stream_list[selectednumber], $"{n};{s};{e}" + Environment.NewLine);
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
                        case "Q":
                            Environment.Exit(0);
                            break;
                    }
                    
                }
            
            }
        }

        private static void process_song(SongEntry song, bool force = false, bool mp3 = false, int cur_song = 0, bool upload = false) {
            mp4_path = Path.Join(out_dir, "mp4", song.Name) + ".mp4";
            mp3_path = Path.Join("output", "mp3", stream.Streamer, song.Name) + ".mp3";
            Console.WriteLine();
            if (!File.Exists(mp4_path) || force)
            {
                Console.WriteLine($"Cutting \"{song.Name}\" from stream, {song.Start} to {song.End}... ");
                Functions.shell_run($"/usr/bin/ffmpeg -y -ss \"{song.Start}\" -to \"{song.End}\" -i \'{stream_file}\' -c copy \'{mp4_path}\'", verbose);
            }
            else
            {
                Console.WriteLine($"\"{song.Name}\" already split");
            }
            if (mp3)
            {
                if (!File.Exists(mp3_path) || force)
                {
                    Directory.CreateDirectory(Path.Join("output", "mp3", stream.Streamer));
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
            if (!File.Exists(stream_file))
            {
                Console.WriteLine("Stream file not found, exiting.");
                return;
            }
            foreach (var song in stream.Songs)
            {
                process_song(song, force, mp3, cur_song, uploadsong);
                cur_song++;
            }
        }

        private static void song_upload(SongEntry song) {
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