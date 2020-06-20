using System.Diagnostics;
using System.IO;


namespace inverter.service.util 
{
    public static class Command {

        static object SpinLock = new object();

        public static string SendCommand(string exec, string cmd)
        {
            lock (SpinLock) {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = exec;
                    process.StartInfo.Arguments = cmd;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.Start();

                    // Synchronously read the standard output of the spawned process. 
                    StreamReader reader = process.StandardOutput;
                    string output = reader.ReadToEnd();
                    process.WaitForExit();

                    // Write the redirected output to this application's window.
                    return output;

                }
            }
        }
    }
}