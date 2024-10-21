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
        static void Main(string[] args)
        {
            
            #region Selecting stream
            Functions.create_basedir();
            string[] stream_list = Directory.GetFiles("streams");
            for (var i = 0; i < stream_list.Count(); i++)
            {
                Console.WriteLine($"{i}: {Path.GetFileName(stream_list[i])} ");
            }
            #endregion
            
            #region Grabbing stream data from file
            Console.WriteLine();
            Console.Write("Select the stream file: ");
            #pragma warning disable CS8604 // Possible null reference argument.
            int selected = int.Parse(Console.ReadLine());
            string[] stream_info = File.ReadAllText(stream_list[selected]).Split("#Stream_Info#")[0].Split("\n");
            stream_name = stream_info[0].Split(";")[1];
            stream_link = stream_info[1].Split(";")[1];
            streamer_name = stream_info[2].Split(";")[1];
            mp3_image = stream_info[3].Split(";")[1];
            upload_to_youtube = stream_info[4].Split(";")[1];
            if (upload_to_youtube == "yes" && !File.Exists("client_secrets.json"))
            {
                Console.WriteLine("You need the client_secrets.json from the Youtube API to do uploads!");
                upload_to_youtube = "no";
            }
            stream_file = stream_info[5].Split(";")[1];;
            make_mp3 = stream_info[6].Split(";")[1];;
            
            string out_dir = Path.Join("output", stream_name);
            Directory.CreateDirectory(out_dir);
            Directory.CreateDirectory(Path.Join(out_dir, "mp4"));
            
            #endregion

            Console.WriteLine($"\n====================================\nStream name: {stream_name} \nStream Link: {stream_link} \nStreamer: {streamer_name} \nStream file: {stream_file} \nMaking mp3: {make_mp3} \nUploading to youtube: {upload_to_youtube} \n====================================\n");
            
            #region Processing
            string[] song_data = File.ReadAllText(stream_list[selected]).Split("#Stream_Info#")[1].Split("\n");
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
                    mp4_path = Path.Join(out_dir, "mp4", song[0]) + ".mp4";
                    mp3_path = Path.Join("output", "mp3", streamer_name, song[0]) + ".mp3";
                    if (!File.Exists(mp4_path))
                    {
                        Console.WriteLine($"Cutting \"{song[0]}\" from stream, {song[1]} to {song[2]}... ");
                        Functions.shell_run($"/usr/bin/ffmpeg -y -ss \"{song[1]}\" -to \"{song[2]}\" -i \'{stream_file}\' -c copy \'{mp4_path}\'", verbose);
                    }
                    else
                    {
                        Console.WriteLine($"\"{song[0]}\" already split");
                    }

                    if (make_mp3 == "yes")
                    {
                        if (!File.Exists(mp3_path))
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

                    if (upload_to_youtube == "yes")
                    {
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
                            File.AppendAllText("upload_list.txt", title);
                        }
                        else
                        {
                            Console.WriteLine($"\"[{streamer_name}] {song[0]}\" Already uploaded to Youtube!");
                        }
                        
                    }
                    cur_song++;
                }
            }
            #endregion
        }
    }
}