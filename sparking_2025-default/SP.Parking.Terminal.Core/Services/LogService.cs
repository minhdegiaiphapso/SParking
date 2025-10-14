using NLog;
using RestSharp;
using SP.Parking.Terminal.Core.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using log4net;
using NLog.Config;
using NLog.Targets;
using System.Reflection;
using System.Diagnostics;
using SP.Parking.Terminal.Core.Models;

public static class LocalLogService
{
    static string _version = string.Empty;
    static string _folderPath = string.Empty;
    public static Exception LastException { get; set; }

    static LocalLogService()
    {
        try
        {
            _version = GetVersion();
            var path = Directory.GetCurrentDirectory();
            _folderPath = Path.Combine(path, "logs");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        catch { }
    }

    public static void Log(Exception exception)
    {
        try
        {
            var now = TimeMapInfo.Current.LocalTime;
            string formatLog = string.Format("{0} {1} {2} \n", now.ToString("yyyy-MM-dd HH:mm:ss"), _version, exception.ToString());
            //string formatLog = string.Format("{0} {1} {2} \n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), _version, exception.ToString());
            WriteFile(formatLog);
            LastException = exception;
        }
        catch { }
    }

    private static void WriteFile(string content)
    {
        var now = TimeMapInfo.Current.LocalTime;
        string filePath = Path.Combine(_folderPath, now.ToString("yyyy-MM-dd") + ".log");
        //string filePath = Path.Combine(_folderPath, DateTime.Now.ToString("yyyy-MM-dd") + ".log");
        File.AppendAllText(filePath, content);
    }

    public static string GetVersion()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
        string version = fileVersionInfo.ProductVersion;
        return version;
    }
}
