using System;
using System.IO;
using System.Collections.Generic;


// blędy:
// !?! rmdir NI kasuje wsztystkiego i potem nie zwraca bledu !?!
// ReadAllLines !!! zostawia puste linie -. tbd !!!
// funkcje dostepu do plikow no stare xcopy chyba nie obslugują długich scieżek


namespace MvSymLn
{
    class Program
    {
        private static string SrcPath;
        private static string DstPath;
        private static string TasksFile;

        private static bool ParseArgs(string[] args)
        {
            if (args.Length < 3)
            {
                //Console.WriteLine("Adam Gros");
                Console.WriteLine("Usage:");
                Console.WriteLine("{0} SrcPath DstPath TasksFile", Environment.CommandLine);
                Console.WriteLine("SrcPath, DstPath - directories");
                Console.WriteLine("TasksFile - contains subdirectories of SrcPath");

                return false;
            }

            SrcPath = args[0];
            DstPath = args[1];
            TasksFile = args[2];

            return true;
        }


        private static bool ExpandPaths()
        {
            try
            {
                SrcPath = Path.GetFullPath(SrcPath);
                DstPath = Path.GetFullPath(DstPath);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception:\n {0}", e.Message);
                return false;
            }

            return true;
        }
        
        // assures: full/absolute & tests if exists as directories and are available
        private static bool CheckPaths()
        {
            try
            {
                SrcPath = Path.GetFullPath(SrcPath);
                DstPath = Path.GetFullPath(DstPath);

                if (!Directory.Exists(SrcPath))
                {
                    Console.WriteLine("Source not available: {0}", SrcPath);
                    return false;
                }
                if (!Directory.Exists(DstPath))
                {
                    Console.WriteLine("Destination not available: {0}", DstPath);
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception:\n {0}", e.Message);
                return false;
            }

            return true;
        }

        private static string[] ReadTasksFile(string tasksFile)
        {
            try
            {
                return File.ReadAllLines(tasksFile);  
                // !!! zostawia puste linie -. tbd !!!
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception:\n {0}", e.Message);
                return null;
            }
        }

        private static List<RealocateTask> PrepareTasks(string srcPath, string dstPath, string[] tasksList)
        {
            List<RealocateTask> tasks = new List<RealocateTask>();

            foreach (string item in tasksList)
            {
                try
                {
                    tasks.Add(new RealocateTask(Path.Combine(srcPath, item), Path.Combine(dstPath, item)));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception:\n {0}", e.Message);
                    return null;
                }
            }

            return tasks;
        }

        // verifies: source directories are available / target directories does not exist
        private static bool CheckTasks(List<RealocateTask> tasks)
        {
            foreach (RealocateTask task in tasks)
            {
                // !? no exceptions
                try
                {
                    if (!Directory.Exists(task.Source))
                    {
                        Console.WriteLine("Source directory not available: {0}", task.Source);
                        return false;
                    }
                    if (Directory.Exists(task.Destination))
                    {
                        Console.WriteLine("Destination directory already exists: {0}", task.Destination);
                        return false;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception:\n {0}", e.Message);
                    return false;
                }
            }

            return true;
        }

        public static List<RealocateTask> GenerateTasks()
        {
            string [] fileTasks = ReadTasksFile(TasksFile);
            if (fileTasks == null)
                return null;

            List<RealocateTask> tasks = PrepareTasks(SrcPath, DstPath, fileTasks);
            if (tasks == null)
                return null;

            if (!CheckTasks(tasks))
                return null;

            return tasks;
        }

        static int Main(string[] args)
        {
            if (!ParseArgs(args))
                return 1;

            if (!CheckPaths())
                return 2;

            Console.WriteLine("{0}", Environment.CommandLine);
            Console.WriteLine("SrcPath: {0}", SrcPath);
            Console.WriteLine("DstPath: {0}", DstPath);
            Console.WriteLine("TasksFile: {0}", Path.GetFullPath(TasksFile)); //??

            List<RealocateTask> tasks = GenerateTasks();
            if (tasks == null)
                return 3;

            MvSymLn msl = new MvSymLn(tasks);
            if (!msl.Realocate())
                return 4;

//Console.ReadKey();  // !!! usunąc !!!
            return 0;
        }

    }
}
