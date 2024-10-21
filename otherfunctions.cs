namespace Airgeadlamh.YoutubeUploader
{
    internal class Functions {
        public static String example_stream = @"stream_name;ROCKIN KARAOKE RELAY 2024
stream_link;https://www.youtube.com/watch?v=xfifdKrvgI4
streamer_name;Lumin Tsukiboshi
mp3_image;lumin.png
upload_to_youtube;yes
stream_file;stream.mkv
make_mp3;yes
#Stream_Info#
Yuzurenai Negai - Magic Knight Rayearth;9:55;13:58
Diamonds - Princess Princess;14:13;18:20
Haru-Spring - Hysteric Blue;18:34;22:20
In My Dream - Sasaki Sayaka;22:45;27:15
Unravel - Tokyo Ghoul;27:16;31:04
Funny Bunny - The Pillows;31:06;34:24
Sekai ga Owaru Made wa - WANDS;34:30;39:30";
        public static void shell_run(String command, bool verbose) {
            //Console.WriteLine(command);
            //string result = "";
            using (System.Diagnostics.Process proc = new System.Diagnostics.Process())
            {
                proc.StartInfo.FileName = "/bin/bash";
                proc.StartInfo.Arguments = "-c \" " + command + " \"";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.Start();
                if (verbose)
                {
                    Console.WriteLine(proc.StandardOutput.ReadToEnd());
                    Console.WriteLine(proc.StandardError.ReadToEnd());
                }
                //result += proc.StandardOutput.ReadToEnd();
                //result += proc.StandardError.ReadToEnd();
                proc.WaitForExit();
            }
            //return result;
        }

        public static void create_basedir() {
            Directory.CreateDirectory("streams");
            Directory.CreateDirectory("stream_files");
            using (StreamWriter outputFile = new StreamWriter("streams/stream_example.txt"))
            {
                String[] lines = example_stream.Split("\n");
                foreach (string line in lines)
                    outputFile.WriteLine(line);
            }
        }
    }
}