using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;

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
            var CPUKey = "HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0\\";
            Name = (string)Registry.LocalMachine.GetValue(CPUKey + "ProcessorNameString", "N/A");
            Socket = "N/A";
            Description = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");
            Speed = Convert.ToUInt32(Registry.LocalMachine.GetValue(CPUKey + "~MHz", 0));
            L2 = 0;
            L3 = 0;
            Cores = (uint)RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(CPUKey).SubKeyCount;
            Threads = (uint)RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(CPUKey).SubKeyCount;

            Searcher = null;
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
        public OsInfo()
        {
            Name = (string)Registry.LocalMachine.GetValue("SOFTWARE\\MICROSOFT\\Windows NT\\CurrentVersion\\ProductName", "");
            Version = (string)Registry.LocalMachine.GetValue("SOFTWARE\\MICROSOFT\\Windows NT\\CurrentVersion\\CompositionEditionID", "")
                    + (string)Registry.LocalMachine.GetValue("SOFTWARE\\MICROSOFT\\Windows NT\\CurrentVersion\\ReleaseId", "");
            Architecture = Environment.Is64BitOperatingSystem == true ? "x64" : "x86";
        }
    }
    public class MemInfo
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
            public MEMORYSTATUSEX()
            {
                dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }


        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

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
        public MemInfo()
        {
            MEMORYSTATUSEX MemInfo = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(MemInfo))
            {
                TotalMemory = MemInfo.ullTotalPhys;
            }
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
        public VideoCardInfo()
        {
            Name = "N/A";
            RAM = 0;
        }
    }

    public class SysInfo
    {
        public CPUInfo CPU { get; private set; }
        public OsInfo OS { get; private set; }
        public MemInfo MemInfo { get; private set; }
        public VideoCardInfo VideoCardInfo { get; private set; }

        public SysInfo() { }

        public void Load()
        {
            var Watch = new Stopwatch();
            Watch.Start();

            try
            {
                var Searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
                CPU = new CPUInfo(Searcher.Get().Cast<ManagementBaseObject>().First());
            }
            catch
            {
                CPU = new CPUInfo();
            }
            Global.Logger.Log(ConsoleColor.Cyan, LogType.WMI, null, "Loading Processor data took " + Watch.ElapsedMilliseconds + "ms");
            Watch.Restart();

            try
            {
                var Searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
                OS = new OsInfo(Searcher.Get().Cast<ManagementBaseObject>().FirstOrDefault());
            }
            catch
            {
                OS = new OsInfo();
            }
            Global.Logger.Log(ConsoleColor.Cyan, LogType.WMI, null, "Loading Operating System data took " + Watch.ElapsedMilliseconds + "ms");
            Watch.Restart();

            try
            {
                var Searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
                MemInfo = new MemInfo(Searcher.Get().Cast<ManagementBaseObject>().FirstOrDefault());
            }
            catch
            {
                MemInfo = new MemInfo();
            }
            Global.Logger.Log(ConsoleColor.Cyan, LogType.WMI, null, "Loading Memory data took " + Watch.ElapsedMilliseconds + "ms");
            Watch.Restart();

            try
            {
                var Searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
                VideoCardInfo = new VideoCardInfo(Searcher.Get().Cast<ManagementBaseObject>().FirstOrDefault());
            }
            catch
            {
                VideoCardInfo = new VideoCardInfo();
            }
            Global.Logger.Log(ConsoleColor.Cyan, LogType.WMI, null, "Loading Video card data took " + Watch.ElapsedMilliseconds + "ms");
            Watch.Stop();
            Watch = null;
        }
    }
}
