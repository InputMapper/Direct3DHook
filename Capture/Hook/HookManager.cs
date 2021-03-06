﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using EasyHook;

namespace Direct3DHookLib.Hook
{
    public class HookManager
    {
        internal static List<int> HookedProcesses = new List<int>();

        /*
         * Please note that we have obtained this information with system privileges.
         * So if you get client requests with a process ID don't try to open the process
         * as this will fail in some cases. Just search the ID in the following list and
         * extract information that is already there...
         * 
         * Of course you can change the way this list is implemented and the information
         * it contains but you should keep the code semantic.
         */
        internal static List<ProcessInfo> ProcessList = new List<ProcessInfo>();
        private static List<int> ActivePIDList = new List<int>();

        public static void AddHookedProcess(int processId)
        {
            lock (HookedProcesses)
            {
                HookedProcesses.Add(processId);
            }
        }

        public static void RemoveHookedProcess(int processId)
        {
            lock (HookedProcesses)
            {
                HookedProcesses.Remove(processId);
            }
        }

        public static bool IsHooked(int processId)
        {
            lock (HookedProcesses)
            {
                return HookedProcesses.Contains(processId);
            }
        }

        public static ProcessInfo[] EnumProcesses()
        {
            var result = new List<ProcessInfo>();
            var procList = Process.GetProcesses();

            for (var i = 0; i < procList.Length; i++)
            {
                var proc = procList[i];

                try
                {
                    var info = new ProcessInfo();

                    info.FileName = proc.MainModule.FileName;
                    info.Id = proc.Id;
                    info.Is64Bit = RemoteHooking.IsX64Process(proc.Id);
                    info.User = RemoteHooking.GetProcessIdentity(proc.Id).Name;

                    result.Add(info);
                }
                catch
                {
                }
            }

            return result.ToArray();
        }

        [Serializable]
        public class ProcessInfo
        {
            public string FileName;
            public int Id;
            public bool Is64Bit;
            public string User;
        }
    }
}