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
                Console.WriteLine("Params: <Project|Solution path> <Target directory>");
            else
            {
                BackupPackages(args[0], args[1]);
            }        
        }
        
        static void BackupPackages(string projectPath, string targetDirectory)
        {
            Console.WriteLine("Project|Solution: " + projectPath);
            Console.WriteLine("Target directory: " + targetDirectory);
            if (!File.Exists(projectPath))
            {
                Console.WriteLine("Project file not found. ");
                return;
            }
            
            var outputData = ExecuteProcessReadingOutput("dotnet", 
                "list \""+projectPath+"\" package --format json --include-transitive");

            Console.ForegroundColor = ConsoleColor.White;
            NugetPackagesJson? packages = JsonSerializer.Deserialize<NugetPackagesJson>(outputData);
            if (packages != null)
                foreach (var project in packages.projects)
                {
                    Console.WriteLine();
                    Console.WriteLine("Backup project packages: " + Path.GetFileName(project.path));
                    BackupProjectPackages(project, targetDirectory);
                }
        }

        private static void BackupProjectPackages(Project project, string targetDirectory)
        {
            List<TopLevelPackage> topLevelPackages = project.frameworks[0].topLevelPackages;
            if (topLevelPackages != null)
                foreach (var package in topLevelPackages)
                {
                    BackupPackage(package.id, package.resolvedVersion, targetDirectory);
                }

            List<TransitivePackage> transitivePackages = project.frameworks[0].transitivePackages;
            if (transitivePackages != null)
                foreach (var package in transitivePackages)
                {
                    BackupPackage(package.id, package.resolvedVersion, targetDirectory);
                }
        }

        /// <summary>
        /// Выполняет указанный исполняемый файл с указанными аргументами, а результат вывода в стандартный вывод
        /// приложения возвращает результатом функции.
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

        private static void BackupPackage(string packageName, string packageVersion, string targetDir)
        {
            Console.Write(packageName + ":" + packageVersion);
            ExecuteProcessReadingOutput("nuget.exe", 
                $"install {packageName} -Version {packageVersion} -o "+"\""+targetDir+"\"");
            Console.WriteLine(" - done");
        }
    }
}
