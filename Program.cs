﻿namespace Airgeadlamh.YoutubeUploader
{
    internal class Program {
        public static string stream_name = "";
        public static string streamer_name = "";
        public static string stream_link = "";
        public static string mp3_image = "";
        public static string upload_to_youtube = "";
        public static string stream_file = "";
        public static string mp4_path = "";
        public static bool verbose = false;
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
            stream_file = stream_info[5].Split(";")[1];;
            
            string out_dir = Path.Join("output", stream_name);
            Directory.CreateDirectory(out_dir);
            Directory.CreateDirectory(Path.Join(out_dir, "mp4"));
            #endregion

            Console.WriteLine($"\n====================================\nStream name: {stream_name} \nStream Link: {stream_link} \nStreamer: {streamer_name} \nStream file: {stream_file} \nUploading to youtube: {upload_to_youtube} \n====================================\n");
            
            #region Processing
            string[] song_data = File.ReadAllText(stream_list[selected]).Split("#Stream_Info#")[1].Split("\n");
            foreach (var item in song_data)
            {
                if (item.Contains(';'))
                {
                    string[] song = item.Split(";");
                    mp4_path = Path.Join(out_dir, "mp4", song[0]) + ".mp4";
                    if (!File.Exists(mp4_path))
                    {
                        Functions.shell_run($"/usr/bin/ffmpeg -y -ss \"{song[1]}\" -to \"{song[2]}\" -i \'{stream_file}\' -c copy \'{mp4_path}\'", verbose);
                    }
                    string[] uploaded_list = File.ReadAllText("upload_list.txt").Split("\n");
                    bool already_uploaded = false;
                    foreach (var upload in uploaded_list)
                    {
                        if (upload == $"[{streamer_name}] {song[0]}")
                        {
                            already_uploaded = true;
                        }
                    }
                    if (upload_to_youtube == "yes")
                    {
                        if (!already_uploaded)
                        {
                            UploadVideo.video_title = $"[{streamer_name}] {song[0]}";
                            UploadVideo.upload();
                            File.AppendAllText("upload_list.txt", UploadVideo.video_title);
                        }
                        else
                        {
                            Console.WriteLine($"[{streamer_name}] {song[0]} Already uploaded!");
                        }
                        
                    }
                }
            }
            #endregion
        }
    }
}