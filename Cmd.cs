using System.Diagnostics;

namespace MvSymLn
{
    static class Cmd
    {
        public static int ExecuteExe(string strCommand, string strCommandParameters = "")
        {
            return Execute(false, strCommand, strCommandParameters);
        }

        public static int ExecuteByCmd(string strCommand, string strCommandParameters = "")
        {
            return Execute(true, strCommand, strCommandParameters);
        }

        public static int Execute(bool useCMD, string strCommand, string strCommandParameters = "")
        {
            Process pProcess = new Process();

            if (useCMD)
            {
                pProcess.StartInfo.FileName = "cmd.exe";
                pProcess.StartInfo.Arguments = "/c " + strCommand + " " + strCommandParameters;
            }
            else
            {
                pProcess.StartInfo.FileName = strCommand;
                pProcess.StartInfo.Arguments = strCommandParameters;
            }

            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.CreateNoWindow = false;

            //Set output of program to be written to process output stream
            //pProcess.StartInfo.RedirectStandardOutput = false;

            //Optional
            //pProcess.StartInfo.WorkingDirectory = strWorkingDirectory;

            //Start the process
            pProcess.Start();


            //Get program output
            //string strOutput = pProcess.StandardOutput.ReadToEnd();

            //Wait for process to finish
            pProcess.WaitForExit();

            return pProcess.ExitCode;
        }
    }
}
