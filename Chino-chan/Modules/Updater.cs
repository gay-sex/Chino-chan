using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Google.Apis.Download;

namespace Chino_chan.Modules
{
    public class Updater
    {
        MediaDownloader Downloader;
        
        private string ProjectName
        {
            get
            {
                return Global.Settings.GithubLink.Substring(Global.Settings.GithubLink.LastIndexOf("/"));
            }
        }
        
        public Updater()
        {
            Downloader = new MediaDownloader(Global.GoogleDrive)
            {
                ChunkSize = 512 * 1024
            };
        }
        
        public bool Update()
        {
            var Downloaded = DownloadUpdate();
            if (Downloaded)
            {
                var Script = RunScript();

                return Script;
            }
            return false;
        }

        public bool DownloadUpdate()
        {
            Global.Logger.Log(ConsoleColor.Cyan, LogType.Updater, null, "Updating..");

            if (Directory.Exists("Update") && Directory.EnumerateFiles("Update").Count() != 0)
            {
                try
                {
                    Directory.Delete("Update", true);
                }
                catch (IOException)
                {
                    Global.Logger.Log(ConsoleColor.Red, LogType.Updater, "Download", "Something is using the Update folder! Please consider removing it manually!");
                    return false;
                }
            }

            Directory.CreateDirectory("Update");

            var Files = Global.GoogleDrive.Files.List().Execute().Items;

            var Packages = Files.Where(p =>
            {
                return p.OriginalFilename == "Chino-chan.exe";
            }).OrderBy(s =>
            {
                return s.CreatedDate;
            });
            
            if (Packages.Count() == 0)
            {
                Global.Logger.Log(ConsoleColor.Red, LogType.GoogleDrive, null, "Couldn't find Chino-chan.exe! Please upload to update!");
                return false;
            }

            var ChinoChan = Packages.First();

            var ChinoChanPath = "Update\\Chino-chan.exe";

            Global.Logger.Log(ConsoleColor.Cyan, LogType.Updater, null, "Downloading update..");

            var Progress = Downloader.Download(ChinoChan.DownloadUrl, new FileStream(ChinoChanPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read));
            
            if (Progress.Status != DownloadStatus.Completed)
            {
                Global.Logger.Log(ConsoleColor.Red, LogType.GoogleDrive, null, "Couldn't download the latest Chino-chan.exe: " + Progress.Exception.ToString());
                return false;
            }
            Global.Logger.Log(ConsoleColor.Cyan, LogType.Updater, null, "Downloaded!");

            return true;
        }
        public bool RunScript()
        {
            if (!File.Exists("Update\\Chino-chan.exe"))
            {
                Global.Logger.Log(ConsoleColor.Red, LogType.Updater, null, "I couldn't find any updates!");
                return false;
            }

            #region Script
            var Script =
@"$Count = (Get-Process | Where-Object {$_.ProcessName -like ""Chino-chan""}).Count
If ($Count -gt 0) {
	Wait-Process -Name Chino-chan
    Start-Sleep -s 1
}
If (Test-Path Update\\Chino-chan.exe) {
	Remove-Item -Path Chino-chan.exe
	Move-Item -Path Update\\Chino-chan.exe -Destination Chino-chan.exe
    Remove-Item -Path Update
}
If (Test-Path Chino-chan.exe) {
	Start-Process Chino-chan.exe
}
Remove-Item -LiteralPath $MyInvocation.MyCommand.Path -Force";

            #endregion

            File.WriteAllText("PostScript.ps1", Script);

            var PolicyChange = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe",
                    Arguments = "Set-ExecutionPolicy Unrestricted",
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };
            PolicyChange.Start();
            PolicyChange.WaitForExit();
            
            var PostScript = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe",
                    Arguments = ".\\PostScript.ps1",
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };
            PostScript.Start();

            return true;
        }
    }
}
