using Cirrious.CrossCore;
using SP.Parking.Terminal.Core.Models;
using SP.Parking.Terminal.Core.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Wpf.Utility
{
    public class CrashDump
    {
        private static object obj = new object();
        internal enum MINIDUMP_TYPE
        {
            MiniDumpNormal = 0x00000000,
            MiniDumpWithDataSegs = 0x00000001,
            MiniDumpWithFullMemory = 0x00000002,
            MiniDumpWithHandleData = 0x00000004,
            MiniDumpFilterMemory = 0x00000008,
            MiniDumpScanMemory = 0x00000010,
            MiniDumpWithUnloadedModules = 0x00000020,
            MiniDumpWithIndirectlyReferencedMemory = 0x00000040,
            MiniDumpFilterModulePaths = 0x00000080,
            MiniDumpWithProcessThreadData = 0x00000100,
            MiniDumpWithPrivateReadWriteMemory = 0x00000200,
            MiniDumpWithoutOptionalData = 0x00000400,
            MiniDumpWithFullMemoryInfo = 0x00000800,
            MiniDumpWithThreadInfo = 0x00001000,
            MiniDumpWithCodeSegs = 0x00002000
        }
        [DllImport("dbghelp.dll")]
        static extern bool MiniDumpWriteDump(
            IntPtr hProcess,
            Int32 ProcessId,
            IntPtr hFile,
            MINIDUMP_TYPE DumpType,
            IntPtr ExceptionParam,
            IntPtr UserStreamParam,
            IntPtr CallackParam);

        public CrashDump() { }

        public static void MiniDumpToFile()
        {
            lock (obj)
            {
                try
                {
                    var now = TimeMapInfo.Current.LocalTime;
                    FileStream fsToDump = null;
                    string path = Path.Combine(GetPath(), now.ToString("HHmmss") + ".dmp");
                    //string path = Path.Combine(GetPath(), DateTime.Now.ToString("HHmmss") + ".dmp");
                    if (File.Exists(path))
                        fsToDump = File.Open(path, FileMode.Append);
                    else
                        fsToDump = File.Create(path);
                    Process thisProcess = Process.GetCurrentProcess();
                    
                    MiniDumpWriteDump(thisProcess.Handle, thisProcess.Id, fsToDump.SafeFileHandle.DangerousGetHandle(), MINIDUMP_TYPE.MiniDumpNormal, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                    fsToDump.Close();
                }
                catch { }
            }
        }

        private static string GetPath()
        {
            string path = Directory.GetCurrentDirectory();
            var now = TimeMapInfo.Current.LocalTime;
            path = Path.Combine(path, "logs", "crash", now.ToString("yyyy-MM-dd"));
            //path = Path.Combine(path, "logs", "crash", DateTime.Now.ToString("yyyy-MM-dd"));
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }
    }
}
