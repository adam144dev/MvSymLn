using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvSymLn
{
    public class RealocateTask
    {
        public string Source { get; }
        public string Destination { get; }

        public RealocateTask(string SrcPath, string DstPath)
        {
            Source = SrcPath;
            Destination = DstPath;
        }

        public override string ToString()
        {
            return Source + " -> " + Destination;
        }
    }

    class MvSymLn
    {
        List<RealocateTask> Tasks;

        private int _Err = -1;
        public int Err { get { return _Err; } }

        public MvSymLn(List<RealocateTask> tasks)
        {
                 Tasks = tasks;
        }

        // returns true on SUCCESS or false and then 0 based index of failure in Err property, in case of success cleared (-1)
        public bool Realocate()
        {
            _Err = -1;  // clear Err

            for (int i = 0; i < Tasks.Count(); i++)
            {
                if (!TaskRealocate(Tasks[i]))
                {
                    _Err = i;
                    return false;
                }
            }

            return true;
        }

        protected bool TaskRealocate(RealocateTask task)
        {
            Console.WriteLine("Task:\t{0}", task);

            if (!TaskCopy(task))
                return false;

            if (!TaskDelete(task))
                return false;

            if (!TaskMkSymLn(task))
                return false;

            return true;
        }

        private bool TaskCopy(RealocateTask task)
        {
            const string cmd = "XCOPY";
            const string flags = @"/q /i /s /e /k /r /h";   // /f   // /o /x   // requires admin     // !?!
            string cmdParams = BetterPath(task.Source) + " " + BetterPath(task.Destination) + " " + flags;

            return ExecuteCommand(cmd, cmdParams, XcopyExitCode);
         }

        static private string XcopyExitCode(int exitCode)
        {
            Dictionary<int, string> xcopyExitCodes = new Dictionary<int, string>
            {
                { 0, "Files were copied without error." },
                { 1, "No files were found to copy." },
                { 2, "The user pressed CTRL+C to terminate xcopy." },
                { 4, "Initialization error occurred.There is not enough memory or disk space, or you entered an invalid drive name or invalid syntax on the command line." },
                { 5, "Disk write error occurred." }
            };

            string txt;
            if (xcopyExitCodes.TryGetValue(exitCode, out txt))
                return txt;
            else
                return "";  // Win32ErrMsg(exitCode);   // try Windows System (from Exception)
        }

        private bool TaskDelete(RealocateTask task)
        {
            // RMDIR [/S] [/Q] [drive:]path
            const string cmd = "RMDIR";
            const string flags = @"/s /q";
            string cmdParams = flags + " " + BetterPath(task.Source);

            // !?! rmdir nie zwraca bledu !?!
            return ExecuteCommand(cmd, cmdParams, Win32ErrMsg);
        }

        private bool TaskMkSymLn(RealocateTask task)
        {
            // MKLINK [[/D] | [/H] | [/J]] Link Target
            const string cmd = "MKLINK";
            const string flags = @"/j";
            string cmdParams = flags + " " + BetterPath(task.Source) + " " + BetterPath(task.Destination);

            return ExecuteCommand(cmd, cmdParams, Win32ErrMsg);
        }

        private bool ExecuteCommand(string cmd, string cmdParams, Func<int, string> errorCodeToMessage)
        {
            Console.WriteLine("\t{0}", cmd);
            try
            {
                int ExitCode = Cmd.ExecuteByCmd(cmd, cmdParams);
                if (ExitCode == 0)
                {
                    Console.WriteLine(FormatMessageOk());
                    return true;
                }
                else
                {
                    Console.WriteLine(FormatMessageError(cmd, ExitCode, errorCodeToMessage(ExitCode)));
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(FormatMessageError(cmd, e.HResult, e.Message));
                return false; ;
            }
        }

        static protected string FormatMessageOk()
        {
            return string.Format("\t=> OK");
        }

        static protected string FormatMessageError(string cmd, int errCode, string errMessage)
        {
            return string.Format("\t=> ERROR \"{0}\" ({1} \"{2}\")", cmd, errCode, errMessage);
        }
        
        // DOS Codes seems to be sublist of WinErr
        //  http://stanislavs.org/helppc/dos_error_codes.html
        //  https://www.rpi.edu/dept/cis/software/g77-mingw32/include/winerror.h
        static private string Win32ErrMsg(int errCode)
        {
            var e = new System.ComponentModel.Win32Exception(errCode);
            return e.Message;
        }

        private string BetterPath(string s)
        {
            return string.Format("\"{0}\"", s);
          }
    }
}

//  https://www.microsoft.com/resources/documentation/windows/xp/all/proddocs/en-us/xcopy.mspx?mfr=true
//  https://support.microsoft.com/pl-pl/kb/289483

//  xcopy Source [Destination] flags:  
//  /q : Suppresses the display of xcopy messages.
//  /f : Displays source and destination file names while copying. 
//  /i : If Source is a directory or contains wildcards and Destination does not exist, xcopy assumes destination specifies a directory name and creates a new directory. Then, xcopy copies all specified files into the new directory. By default, xcopy prompts you to specify whether Destination is a file or a directory. 
//  /s : Copies directories and subdirectories, unless they are empty.If you omit /s, xcopy works within a single directory.
//  /e : Copies all subdirectories, even if they are empty.Use /e with the /s and /t command-line options.
//  /k : Copies files and retains the read-only attribute on destination files if present on the source files.By default, xcopy removes the read-only attribute.
//  /r : Copies read-only files.
//  /h : Copies files with hidden and system file attributes. By default, xcopy does not copy hidden or system files. 
//  requires admin:
//  /o : Copies file ownership and discretionary access control list(DACL) information.
//  /x : Copies file audit settings and system access control list(SACL) information(implies /o). 

//  Exit code   Description
//      0       Files were copied without error.
//      1       No files were found to copy.
//      2       The user pressed CTRL+C to terminate xcopy.
//      4       Initialization error occurred.There is not enough memory or disk space, or you entered an invalid drive name or invalid syntax on the command line.
//      5       Disk write error occurred.