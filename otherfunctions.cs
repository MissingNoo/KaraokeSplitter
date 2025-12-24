namespace Airgeadlamh.YoutubeUploader
{
    internal class Functions {
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
    }
}