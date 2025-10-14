using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Utilities
{
    public static class OtherUtilities
    {
        public static string GetVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fileVersionInfo.ProductVersion;
            return version;
        }

        public static string GetLastGroupNumber(string word)
        {
            return GetResult(word, @"(\d+)(?!.*\d)");
        }

        public static string GetLastNonDigitWordCharacter(string word)
        {
            return GetResult(word, @"([^a-zA-Z0-9]+)$");
        }

        public static string GetPort(string str)
        {
            return GetResult(str, @"(?<=:)\d+");
        }

        public static string GetResult(string str, string regexStr)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;

            var regex = new Regex(regexStr);
            var match = regex.Match(str);
            if (match.Success)
            {
                return match.Groups[0].Value;
            }
            return string.Empty;
        }

        public static string RemoveLastNonDigitWordChar(string word)
        {
            if (string.IsNullOrEmpty(word)) return string.Empty;

            string chars = GetLastNonDigitWordCharacter(word);
            return  word.Remove(word.LastIndexOf(chars));
        }
    }
}
