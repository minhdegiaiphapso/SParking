using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Green.Devices.Dal
{
    public class SfactorsCardreaderLog
    {
        private static string GetPreferenceDirectory()
        {
            var documents = AppDomain.CurrentDomain.BaseDirectory;
            var folder = Path.Combine(documents, "SfactorsCardreaderLog");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return folder;
        }
        private static string GetFileName(DateTime date)
        {
            var folder = GetPreferenceDirectory();
            return Path.Combine(folder, date.ToString("yyyyMMdd") + ".conf");
        }
        private static int mylock = 0;
        public static void Log(CardInfoLog myLog)
        {
            if (mylock==0)
            {
                mylock = 1;
                List<CardInfoLog> currentLog;
                var filePath = GetFileName(DateTime.Now);
                if (File.Exists(filePath))
                {
                    var data = File.ReadAllText(filePath);
                    currentLog = JsonConvert.DeserializeObject<List<CardInfoLog>>(data);
                }
                else
                {
                    currentLog = new List<CardInfoLog>();
                }
                currentLog.Add(myLog);
                var datasave = JsonConvert.SerializeObject(currentLog, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(filePath, datasave);
                mylock = 0;
            }
        }
    }
    public class CardInfoLog
    {
        [JsonProperty("CardType")]
        public string CardType { get; set; }
        [JsonProperty("CardId")]
        public string CardId { get; set; }
        [JsonProperty("TimeReceived")]
        public string TimeReceived { get; set; }
        [JsonProperty("Message")]
        public string Message { get; set; }
    }
}
