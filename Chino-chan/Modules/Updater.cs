//using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Chino_chan.Modules
{
    public class Updater
    {
        WebClient Client;

        CSharpCodeProvider Provider;

        private string ProjectName
        {
            get
            {
                return Global.Settings.GithubLink.Substring(Global.Settings.GithubLink.LastIndexOf("/"));
            }
        }

        
        public Updater()
        {
            Provider = new CSharpCodeProvider();
            Client = new WebClient();
        }

        /*
        public void Update()
        {
            if (ProcessUpdate())
            {
                Global.Logger.Log(ConsoleColor.DarkYellow, LogType.Updater, null, "Update successful! Restarting...");
                Global.StopAsync().GetAwaiter().GetResult();
                Environment.Exit(exitCode: 0);
            }
            else
            {
                Global.Logger.Log(ConsoleColor.Red, LogType.Updater, null, "Couldn't update!");
            }
        }
        */

        public bool Update()
        {
            if (Directory.Exists("GitUpdate"))
            {
                Global.Logger.Log(ConsoleColor.Red, LogType.Updater, "Download", "Please remove GitUpdate folder manually, because probably the last update didn't complete, and try again!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Global.Settings.GithubLink))
            {
                Global.Logger.Log(ConsoleColor.Red, LogType.Updater, "Github", "Please provide the link of the repository to the Settings.json in the Data folder! (GithubLink property)");
                return false;
            }
            Directory.CreateDirectory("GitUpdate");

            Client.DownloadFile(new Uri(Global.Settings.GithubLink + "/archive/" + Global.Settings.GithubBranch + ".zip"), "GitUpdate\\archive.zip");

            if (!File.Exists("GitUpdate\\archive.zip"))
            {
                Global.Logger.Log(ConsoleColor.Red, LogType.Updater, "Github", "Couldn't download the archive of the repository!");
                Directory.Delete("GitUpdate", true);
                return false;
            }

            Global.Logger.Log(ConsoleColor.DarkYellow, LogType.Updater, "Github", "Repository downloaded!");

            ZipFile.ExtractToDirectory("GitUpdate\\archive.zip", "GitUpdate");

            Global.Logger.Log(ConsoleColor.DarkYellow, LogType.Updater, "Github", "Repository extracted!");

            File.Delete("GitUpdate\\archive.zip");

            if (!Compile())
            {
                Global.Logger.Log(ConsoleColor.Red, LogType.Updater, "Compile", "Couldn't compile!");
                Directory.Delete("GitUpdate", true);
                return false;
            }

            File.WriteAllLines("post.ps1", new string[4]
            {
                "Wait-Process -Name Chino-chan -Timeout 300",
                "Remove-Item -Path Chino-chan.exe",
                "Copy-Item -Path GitUpdate\\Compiled\\Chino-chan.exe -Destination Chino-chan.exe",
                "Start-Process Chino-chan.exe"
            });
            
            Process.Start("powershell.exe Set-ExecutionPolicy Unrestricted -Scope CurrentUser").WaitForExit();

            Process.Start(new ProcessStartInfo()
            {
                UseShellExecute = true,
                FileName = "powershell.exe",
                Arguments = "post.ps1"
            });

            return true;
        }

        private bool Compile()
        {
            Global.Logger.Log(ConsoleColor.DarkYellow, LogType.Updater, "Updater", "Compiling...");
            var ProjectFileMatches = Directory.EnumerateFiles("GitUpdate", "*.csproj", SearchOption.AllDirectories);
            if (ProjectFileMatches.Count() != 1)
            {
                Global.Logger.Log(ConsoleColor.Red, LogType.Updater, "Compiler", "Couldn't find the project file!");
                return false;
            }
            var Project = File.ReadAllText(ProjectFileMatches.First());

            var Deps = ProcessDependencyFiles(Project);
            if (Deps == null)
            {
                return false;
            }
            
            var Result = Provider.CompileAssemblyFromFile(new CompilerParameters(Deps)
            {
                OutputAssembly = "GitUpdate\\Compiled\\Chino-chan.exe",
                GenerateExecutable = true,
                MainClass = "Entrance.cs",
                TreatWarningsAsErrors = false,
                GenerateInMemory = false,
                WarningLevel = 4,
            }, ProcessSourceFiles(Project));
            
            if (Result.Errors.Count > 0)
            {
                foreach (CompilerError Error in Result.Errors)
                {
                    Global.Logger.Log(ConsoleColor.Red, LogType.Updater, "Compile Error",
                        "Line number " + Error.Line +
                        ", Error Number: " + Error.ErrorNumber +
                        ", '" + Error.ErrorText + "; in " + Error.FileName);
                }
                return false;
            }

            Global.Logger.Log(ConsoleColor.DarkYellow, LogType.Updater, "Updater", "Compiled!");
            return true;
        }


        private string[] ProcessSourceFiles(string Project)
        {
            var SearchCompile = new Regex("<Compile Include=\"(.*?)\" \\/>");
            var Files = new List<string>();
            foreach (Match Match in SearchCompile.Matches(Project))
            {
                Files.Add("GitUpdate\\" + ProjectName + "-" + Global.Settings.GithubBranch + 
                    "\\" + ProjectName + "\\" + Match.Groups[1].Value);
            }
            return Files.ToArray();
        }
        private string[] ProcessDependencyFiles(string Project)
        {
            var SearchCompile = new Regex("<Reference Include=\"(.*?)\" \\/>");
            var DependencyNames = new List<string>();
            foreach (Match Match in SearchCompile.Matches(Project))
            {
                DependencyNames.Add(Match.Groups[1].Value);
            }
            var Assemblies = "";
            if (Directory.Exists((Assemblies = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
                + "\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.2")))
            {
                // Has nothing to do
            }
            else if (Directory.Exists((Assemblies = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
                + "\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.2")))
            {
                // Has nothing to do
            }
            else
            {
                Global.Logger.Log(ConsoleColor.Red, LogType.Updater, null, "Please consider installing .Net Framework 4.6.2!");
                return null;
            }

            var Dlls = Directory.EnumerateFiles(Assemblies, "*.dll");
            var Dependencies = Dlls.Where(t => DependencyNames.Contains(Path.GetFileNameWithoutExtension(t))).ToList();
            Dependencies.AddRange(Directory.EnumerateFiles(Environment.CurrentDirectory, "*.dll"));
            return Dependencies.ToArray();
        }
    }
}
