using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Linq;
using System.Management;

namespace Chino_chan.Modules
{
    public class CPUInfo
    {
        private ManagementObjectSearcher Searcher { get; set; }

        public string Name { get; private set; }
        public string Socket { get; private set; }
        public string Description { get; private set; }
        public uint Speed { get; private set; }
        public uint L2 { get; private set; }
        public uint L3 { get; private set; }
        public uint Cores { get; private set; }
        public uint Threads { get; private set; }
        public float Percentage
        {
            get
            {
                if (Searcher == null)
                {
                    var Counter = new PerformanceCounter
                    {
                        CategoryName = "Processor",
                        CounterName = "% Processor Time",
                        InstanceName = "_Total"
                    };

                    return Counter.NextValue();
                }
                else
                {
                    var CPUPref = Searcher.Get().Cast<ManagementBaseObject>().FirstOrDefault();
                    return (float)CPUPref["PercentProcessorTime"];
                }
            }
        }

        public CPUInfo(ManagementBaseObject Info)
        {
            Socket = (string)Info["SocketDesignation"];
            Name = ((string)Info["Name"]).Replace("  ", " ");
            Description = (string)Info["Caption"];
            Speed = (uint)Info["MaxClockSpeed"];
            L2 = (uint)Info["L2CacheSize"];
            L3 = (uint)Info["L3CacheSize"];
            Cores = (uint)Info["NumberOfCores"];
            Threads = (uint)Info["NumberOfLogicalProcessors"];

            Searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PerfFormattedData_PerfOS_Processor");
        }
        public CPUInfo()
        {
            var CPUKey = "HARDWARE\\DESCRIPTION\\System\\CentralProcessor";
            var CPU0Key = "HKEY_LOCAL_MACHINE" + CPUKey + "\\0";
            Name = (string)Registry.GetValue(CPU0Key, "ProcessorNameString", "N/A");
            Socket = "N/A";
            Description = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");
            Speed = (uint)Registry.GetValue(CPU0Key, "~Mhz", 0);
            L2 = 0;
            L3 = 0;
            Cores = (uint)RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(CPUKey).SubKeyCount;
            Threads = (uint)RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(CPUKey).SubKeyCount;
        }
    }
    public class OsInfo
    {
        public string Name { get; private set; }
        public string Version { get; private set; }
        public string Architecture { get; private set; }

        public OsInfo(ManagementBaseObject Info)
        {
            Name = ((string)Info["Caption"]).Trim();
            Version = (string)Info["Version"];
            Architecture = (string)Info["OSArchitecture"];
        }
    }
    public class MemInfo
    {
        private PerformanceCounter Counter { get; set; }

        public ulong TotalMemory { get; private set; }
        public float CurrentMemory
        {
            get
            {
                return Counter.NextValue();
            }
        }

        public MemInfo(ManagementBaseObject Info)
        {
            TotalMemory = (ulong)Info["TotalPhysicalMemory"];
            Counter = new PerformanceCounter("Memory", "Available MBytes", true);
        }
    }
    public class VideoCardInfo
    {
        public string Name { get; private set; }
        public uint RAM { get; private set; }

        public VideoCardInfo(ManagementBaseObject Info)
        {
            Name = (string)Info["Name"];
            RAM = (uint)Info["AdapterRAM"];
        }
    }

    public class SysInfo
    {
        private bool _Loaded = false;
        public bool Loaded
        {
            get
            {
                return _Loaded;
            }
        }

        public CPUInfo CPU { get; private set; }
        public OsInfo OS { get; private set; }
        public MemInfo MemInfo { get; private set; }
        public VideoCardInfo VideoCardInfo { get; private set; }

        public SysInfo() { }

        public void Load()
        {
            var Watch = new Stopwatch();
            try
            {
                Watch.Start();
                var Searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");

                CPU = new CPUInfo(Searcher.Get().Cast<ManagementBaseObject>().FirstOrDefault());
                Global.Logger.Log(ConsoleColor.Cyan, LogType.WMI, null, "Loading Processor data took " + Watch.ElapsedMilliseconds + "ms");

                Watch.Restart();
                Searcher.Query = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
                OS = new OsInfo(Searcher.Get().Cast<ManagementBaseObject>().FirstOrDefault());
                Global.Logger.Log(ConsoleColor.Cyan, LogType.WMI, null, "Loading Operating System data took " + Watch.ElapsedMilliseconds + "ms");

                Watch.Restart();
                Searcher.Query = new ObjectQuery("SELECT * FROM Win32_ComputerSystem");
                MemInfo = new MemInfo(Searcher.Get().Cast<ManagementBaseObject>().FirstOrDefault());
                Global.Logger.Log(ConsoleColor.Cyan, LogType.WMI, null, "Loading Computer System data took " + Watch.ElapsedMilliseconds + "ms");

                Watch.Restart();
                Searcher.Query = new ObjectQuery("SELECT * FROM Win32_VideoController");
                VideoCardInfo = new VideoCardInfo(Searcher.Get().Cast<ManagementBaseObject>().FirstOrDefault());
                Global.Logger.Log(ConsoleColor.Cyan, LogType.WMI, null, "Loading Video card data took " + Watch.ElapsedMilliseconds + "ms");

                _Loaded = true;
            }
            catch
            {
                Global.Logger.Log(ConsoleColor.Red, LogType.WMI, null, "WMI is not available!");
            }
            if (!_Loaded)
            {
                Watch.Reset();

            }
            Watch.Stop();
        }
    }
}
