using System.Diagnostics;
using System.Text.Json;
using CommandLineParser.Arguments;

namespace NugetBackup 
{
    internal class Program
    {
        private static SwitchArgument _includeTransitivePackagesArgument;
        private static SwitchArgument _keepNupkgFilesOnlyArgument;
        private static string _projectPath;
        private static string _targetDirectory;

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Nuget packages backup utility");
            
            if (ParseArguments(args)) 
                BackupPackages(_projectPath, _targetDirectory, _keepNupkgFilesOnlyArgument.Value, _includeTransitivePackagesArgument.Value);
        }
        

        private static bool ParseArguments(string[] args)
        {
            CommandLineParser.CommandLineParser parser = new CommandLineParser.CommandLineParser();
            
            _includeTransitivePackagesArgument = new SwitchArgument('i', "includetp", "Include transitive packages", true);
            _keepNupkgFilesOnlyArgument = new SwitchArgument('k', "keeppo", "Keep .nupkg-files only", true);
            parser.Arguments.Add(_includeTransitivePackagesArgument);
            parser.Arguments.Add(_keepNupkgFilesOnlyArgument);
            
            parser.ParseCommandLine(args);
            if (args.Length < 3 || !parser.ParsingSucceeded)
            {
                parser.ShowUsage();
                Console.WriteLine("nugetbackup.exe  <Project|Solution path> <Target directory> options");
                Console.ForegroundColor = ConsoleColor.White;
                return false;
            }

            _projectPath = parser.AdditionalArgumentsSettings.AdditionalArguments[0];
            _targetDirectory = parser.AdditionalArgumentsSettings.AdditionalArguments[1];
            return true;
        }

        static void BackupPackages(string projectPath, string targetDirectory, bool keepNupkgFilesOnly,
            bool includeTransitivePackages)
        {
            Console.WriteLine("Project|Solution: " + projectPath);
            Console.WriteLine("Target directory: " + targetDirectory);
            if (!File.Exists(projectPath))
            {
                Console.WriteLine("Project file not found. ");
                return;
            }

            var arguments = "list \""+projectPath+"\" package --format json";
            if (includeTransitivePackages) arguments += " --include-transitive";
            var outputData = ExecuteProcessReadingOutput("dotnet", arguments);

            Console.ForegroundColor = ConsoleColor.White;
            NugetPackagesJson? packages = JsonSerializer.Deserialize<NugetPackagesJson>(outputData);
            if (packages != null)
            {
                foreach (var project in packages.projects)
                {
                    Console.WriteLine();
                    Console.WriteLine("Backup project packages: " + Path.GetFileName(project.path));
                    BackupProjectPackages(project, targetDirectory);
                }
                
                if (keepNupkgFilesOnly) 
                    KeepNupkgFileOnly(targetDirectory);
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

        private static void BackupPackage(string packageName, string packageVersion, string targetDir)
        {
            Console.Write(packageName + ":" + packageVersion);
            ExecuteProcessReadingOutput("nuget.exe", 
                $"install {packageName} -Version {packageVersion} -o "+"\""+targetDir+"\"");
            Console.WriteLine(" - done");
        }

        /// <summary>
        /// Перемещает все файлы пакетов из подкаталогов непосредственно в targetDir, а затем удаляет сами подкаталоги
        /// с установленными пакетами.
        /// из targetDir. 
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="packageVersion"></param>
        /// <param name="targetDir"></param>
        private static void KeepNupkgFileOnly(string targetDir)
        {
            DirectoryInfo targetDirInfo = new DirectoryInfo(targetDir);
            foreach (DirectoryInfo dir in targetDirInfo.EnumerateDirectories())
            {
                string oldPath = Path.Combine(targetDir, dir.FullName, Path.Combine(dir.FullName, dir.Name+".nupkg"));
                string newPath = Path.Combine(targetDir, dir.Name+".nupkg");
                File.Move(oldPath, newPath, true);
                dir.Delete(true); 
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
    }
}
