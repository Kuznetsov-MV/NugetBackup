using System.Diagnostics;
using System.Text.Json;

namespace NugetBackup 
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Nuget packages backup utility");
            if (args.Length != 2)
             Console.WriteLine("Params: <Project path> <Target directory>");
            else
            {
                BackupPackages(args[0], args[1]);
            }        
        }
        
        static void BackupPackages(string projectPath, string targetDirectory)
        {
            Console.WriteLine("Project: " + projectPath);
            Console.WriteLine("Target directory: " + targetDirectory);
            
            var outputData = ExecuteProcessReadingOutput("dotnet", 
                "list \""+projectPath+"\" package --format json --include-transitive");

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Used packages:");
            var packages = JsonSerializer.Deserialize<NugetPackagesJson>(outputData);
            foreach (var package in packages.projects[0].frameworks[0].topLevelPackages)
            {
                Console.WriteLine(package.id + ":" + package.resolvedVersion);
            }
            foreach (var package in packages.projects[0].frameworks[0].transitivePackages)
            {
                Console.WriteLine(package.id + ":" + package.resolvedVersion);
            }
        }

        /// <summary>
        /// Выполняет указанный исполняемый файл с указанными аргументами, а результат вывода в стандартный вывод
        /// возвращает результатом функции.
        /// </summary>
        /// <param name="processFileName"></param>
        /// <param name="processArguments"></param>
        public static string ExecuteProcessReadingOutput(string processFileName, string processArguments)
        {
            string outputString = "";
            
            Process process = new Process();
            process.StartInfo.FileName = processFileName;
            process.StartInfo.Arguments = processArguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.OutputDataReceived += (sender, e) =>
            {
                if (!String.IsNullOrEmpty(e.Data)) outputString += e.Data;
            };

            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit(); 
            process.Close();
            
            return outputString;
        }
    }
}
