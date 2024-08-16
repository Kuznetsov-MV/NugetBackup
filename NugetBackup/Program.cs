using System.Diagnostics;

namespace NugetBackup 
{
    internal class Program
    {
        static void Main(string[] args)
        {
             Console.WriteLine("Nuget packages backup utility");
             if (args.Length != 2)
                 Console.WriteLine("Params: <Project path> <Target directory>");
             else
                 BackupPackages(args[0], args[1]);        
        }
        
        static void BackupPackages(string projectPath, string targetDirectory)
        {
            Console.WriteLine("Project: " + projectPath);
            Console.WriteLine("Target directory: " + targetDirectory);
            
            var outputFile = "res";
            ExecuteProcessReadingOutput("dotnet", "list \""+projectPath+"\" package --format json --include-transitive", outputFile);
        }

        /// <summary>
        /// Выполняет указанный исполняемый файл с указанными аргументами, а результат вывода в стандартный выход
        /// перенаправляет в указанный файл.
        /// </summary>
        /// <param name="processFileName"></param>
        /// <param name="processArguments"></param>
        /// <param name="outputFile"></param>
        public static void ExecuteProcessReadingOutput(string processFileName, string processArguments,
            string outputFile)
        {
            var outputStream = new StreamWriter(outputFile);
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = processFileName;
                process.StartInfo.Arguments = processArguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!String.IsNullOrEmpty(e.Data))
                    {
                        outputStream.WriteLine(e.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.WaitForExit(); 
                process.Close();
            }
            finally
            {
                outputStream.Close();
            }
        }
    }
}
