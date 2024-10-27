using System.Runtime.InteropServices;

namespace Airgeadlamh.YoutubeUploader
{
    internal class Program {
        public static string stream_name = "";
        public static string streamer_name = "";
        public static string stream_link = "";
        public static string mp3_image = "";
        public static string make_mp3 = "";
        public static string upload_to_youtube = "";
        public static string stream_file = "";
        public static string mp4_path = "";
        public static string mp3_path = "";
        public static bool verbose = false;
        public static bool os_windows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public static bool os_linux = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        static string[] stream_list = Directory.GetFiles("streams");
        private static string out_dir = "";
        static void Main(string[] args)
        {
            
            #region Selecting stream
            Functions.create_basedir();
            
            for (var i = 0; i < stream_list.Count(); i++)
            {
                Console.WriteLine($"{i}: {Path.GetFileName(stream_list[i])}");
            }
            #endregion
            Console.WriteLine("A: Create new stream file");
            
            Console.WriteLine();
            Console.Write("Select the stream file: ");
            #pragma warning disable CS8604 // Possible null reference argument.
            string selected = Console.ReadLine();
            int selectednumber;
            bool isNumber = int.TryParse(selected, out selectednumber);
            if (isNumber)
            {
                process_stream(selectednumber);
            }
            else {
                switch (selected.ToUpper())
                {
                    case "A":
                        create_stream();
                        break;
                }
            }
            
        }

        private static void create_stream(){
            Console.WriteLine("Filename:");
            string fname = Console.ReadLine();
            string filepath = Path.Join("stream_files", fname);
            Console.WriteLine("Stream name:");
            string name = Console.ReadLine();
            File.AppendAllText(filepath, $"stream_name;{name}" + Environment.NewLine);
            Console.WriteLine("Stream link:");
            string link = Console.ReadLine();
            File.AppendAllText(filepath, $"stream_link;{link}" + Environment.NewLine);
            Console.WriteLine("Streamer name:");
            string streamername = Console.ReadLine();
            File.AppendAllText(filepath, $"streamer_name;{streamername}" + Environment.NewLine);
            Console.WriteLine("Stream filename:");
            string filename = Console.ReadLine();
            File.AppendAllText(filepath, $"stream_file;{filename}" + Environment.NewLine);
            File.AppendAllText(filepath, "#Stream_Info#" + Environment.NewLine);
            Console.WriteLine("Stream file created!");
        }

        private static void process_stream(int selectednumber){
            while (true) {
                string[] stream_info = File.ReadAllText(stream_list[selectednumber]).Split("#Stream_Info#")[0].Split("\n");
                stream_name = stream_info[0].Split(";")[1];
                stream_link = stream_info[1].Split(";")[1];
                streamer_name = stream_info[2].Split(";")[1];
                stream_file = Path.Join("stream_files", stream_info[3].Split(";")[1]);
                
                //if (upload_to_youtube == "yes" && !File.Exists("client_secrets.json"))
                //{
                //    Console.WriteLine("You need the client_secrets.json from the Youtube API to do uploads!");
                //    upload_to_youtube = "no";
                //}
                
                
                
                out_dir = Path.Join("output", stream_name);
                Directory.CreateDirectory(out_dir);
                Directory.CreateDirectory(Path.Join(out_dir, "mp4"));

                Console.WriteLine($"\n====================================\nStream name: {stream_name} \nStream Link: {stream_link} \nStreamer: {streamer_name} \nStream file: {Path.GetFileName(stream_file)} \n====================================\n");

                string[] song_data = File.ReadAllText(stream_list[selectednumber]).Split("#Stream_Info#")[1].Split("\n");
                Console.WriteLine("Songs in this stream:");
                for (int i = 0; i < song_data.Length; i++)
                {
                    if (song_data[i].Contains(';'))
                    {
                        string[] song = song_data[i].Split(";");
                        Console.WriteLine($"{i}: {song[0]}");
                    }
                }
                Console.WriteLine("\nA: Add song\nP: Process all Songs\nR: Reprocess all songs\nU: Upload all songs\nM: Make all MP3\nQ: Quit");
                string selected = Console.ReadLine();
                int snumber;
                bool isNumber = int.TryParse(selected, out snumber);
                if (isNumber)
                {
                    string[] song = song_data[snumber].Split(";");
                    Console.WriteLine($"\nSelected Song: \"{song[0]}\"\nU: Upload\nR: Reprocess\nM: Make MP3\nP: Play Song (mp4)\nP3: Play Song (mp3)\nQ: Quit");
                    string subselected = Console.ReadLine();
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
                            Functions.shell_run($"mpv {Path.Join("output", stream_name, "mp4") + song[0] + ".mp4"}", verbose);
                            break;
                        case "P3":
                            Functions.shell_run($"mpv {Path.Join("output", "mp3", streamer_name) + song[0] + ".mp3"}", verbose);
                            break;
                    }
                    
                }
                else {
                    switch (selected.ToUpper())
                    {
                        case "A":
                            Console.WriteLine("\nSong name:");
                            string n = Console.ReadLine().Replace(" ", "");
                            Console.WriteLine("Start time:");
                            string s = Console.ReadLine().Replace(" ", "");
                            Console.WriteLine("End time:");
                            string e = Console.ReadLine().Replace(" ", "");
                            File.AppendAllText(stream_list[selectednumber], $"{n};{s};{e}" + Environment.NewLine);
                            break;
                        case "P":
                            process_all(song_data);
                            break;
                        case "U":
                            process_all(song_data, false, false, true);
                            break;
                        case "R":
                            process_all(song_data, true);
                            break;
                        case "M":
                            process_all(song_data, false, true);
                            break;
                        case "Q":
                            Environment.Exit(0);
                            break;
                    }
                    
                }
            }
        }

        private static void process_song(string[] song, bool force = false, bool mp3 = false, int cur_song = 0, bool upload = false) {
            mp4_path = Path.Join(out_dir, "mp4", song[0]) + ".mp4";
            mp3_path = Path.Join("output", "mp3", streamer_name, song[0]) + ".mp3";
            Console.WriteLine();
            if (!File.Exists(mp4_path) || force)
            {
                Console.WriteLine($"Cutting \"{song[0]}\" from stream, {song[1]} to {song[2]}... ");
                Functions.shell_run($"/usr/bin/ffmpeg -y -ss \"{song[1]}\" -to \"{song[2]}\" -i \'{stream_file}\' -c copy \'{mp4_path}\'", verbose);
            }
            else
            {
                Console.WriteLine($"\"{song[0]}\" already split");
            }
            if (mp3)
            {
                if (!File.Exists(mp3_path) || force)
                {
                    Directory.CreateDirectory(Path.Join("output", "mp3", streamer_name));
                    Console.WriteLine($"Converting \"{song[0]}\" to mp3... ");
                    Functions.shell_run($"/usr/bin/ffmpeg -y -i \'{mp4_path}\' \'{mp3_path}\'", verbose);
                    var tfile = TagLib.File.Create(mp3_path);
                    tfile.Tag.Title = song[0];
                    tfile.Tag.Album = stream_name;
                    tfile.Tag.Performers = [streamer_name];
                    tfile.Tag.Track = (uint)cur_song;
                    tfile.Save();
                }
                else
                {
                    Console.WriteLine($"\"{song[0]}\" already converted to mp3");
                }
            }
            if (upload)
            {
                song_upload(song);
            }
        }
        
        private static void process_all(string[] song_data, bool force = false, bool mp3 = false, bool uploadsong = false) {
            int cur_song = 1;
            if (!File.Exists(stream_file))
            {
                Console.WriteLine("Stream file not found, exiting.");
                return;
            }
            foreach (var item in song_data)
            {
                if (item.Contains(';'))
                {
                    string[] song = item.Split(";");
                    process_song(song, force, mp3, cur_song, uploadsong);
                    cur_song++;
                }
            }
        }

        private static void song_upload(string[] song) {
            string title = $"[{streamer_name}] {song[0]}";
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
                Console.WriteLine($"\"[{streamer_name}] {song[0]}\" Already uploaded to Youtube!\n");
            }
        }
    }
}