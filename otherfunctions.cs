namespace Airgeadlamh.YoutubeUploader
{
    internal class Functions {
        public static string shell_run(String command, bool verbose = false) {
            //Console.WriteLine(command);
            string result = "";
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
                result += proc.StandardOutput.ReadToEnd();
                result += proc.StandardError.ReadToEnd();
                proc.WaitForExit();
            }
            return result;
        }

        public static int get_video_frames(string path)
        {
            int result = 0;
            result = Int32.Parse(shell_run("ffprobe -v error -select_streams v:0 -count_packets -show_entries stream=nb_read_packets -of csv=p=0 " + $"\'{path}\'"));
            return result;
        }
    }
}