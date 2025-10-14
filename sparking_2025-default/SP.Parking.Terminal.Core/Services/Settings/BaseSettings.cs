using Newtonsoft.Json;
using SP.Parking.Terminal.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SP.Parking.Terminal.Core.Utilities;

namespace SP.Parking.Terminal.Core.Services
{
    public class BaseSettingsData
    {
        /// <summary>
        /// Gets or sets checksum value of server synchronizing data
        /// </summary>
        public string SyncDataChecksum { get; set; }

        /// <summary>
        /// Gets or sets checksum of all data
        /// </summary>
        public string Checksum { get; set; }
    }

    public interface IBaseSettings
    {
        /// <summary>
        /// Gets or sets checksum of all data
        /// </summary>
        string Checksum { get; set; }

        /// <summary>
        /// Gets or sets checksum value of server synchronizing data
        /// </summary>
        string SyncDataChecksum { get; set; }

        /// <summary>
        /// Save settings
        /// </summary>
        void Save();

        /// <summary>
        /// Forces save settings
        /// </summary>
        void ForceSave();

        /// <summary>
        /// Loads settings
        /// </summary>
        void Load();

        /// <summary>
        /// Get flag indicates if configuration file already existed
        /// </summary>
        bool HasLocal { get; }

        /// <summary>
        /// Calculate checksum of current data
        /// </summary>
        /// <returns></returns>
        string CalculateChecksum();
        void MarkChanged();
    }

    public abstract class BaseSettings<T> : IBaseSettings where T : BaseSettingsData
    {
        protected ArgumentParameter _argParams;

        public const string SettingRootPath = "UserPrefs";

        public bool HasLocal { get; private set; }

        public bool IsChanged { get; private set; }

        public string SyncDataChecksum
        {
            get { return _data.SyncDataChecksum; }
            set
            {
                if (_data.SyncDataChecksum != value)
                {
                    _data.SyncDataChecksum = value; //MarkChanged(); 
                }
            }
        }

        public string Checksum
        {
            get { return _data.Checksum; }
            set
            {
                if (_data.Checksum != value)
                {
                    _data.Checksum = value; //MarkChanged();
                }
            }
        }

        protected T _data;
        private string _savedFilePath;
        private string _savedBackupFilePath;
        protected virtual string GetSettingName()
        {
            return this.GetType().Name;
        }

        public BaseSettings()
        {
            Load();
        }

        //public BaseSettings(IRunModeManager modeManager)
        //{
        //    _modeManager = modeManager;
        //    Load();
        //}

        public BaseSettings(ArgumentParameter argParams = null)
        {
            _argParams = argParams;
            Load();
        }

        public void MarkChanged()
        {
            IsChanged = true;
        }

        public static string GetPreferenceDirectory()
        {
            var documents = @"C:\ProgramData";
            var folder = Path.Combine(documents, @"SP\Parking");
            folder = Path.Combine(folder, SettingRootPath);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            return folder;
        }
		private bool theFirst = true;
		private void startAnpr(string anprHost)
		{
			if (theFirst && System.IO.Directory.Exists(anprHost))
			{
				var anprV8 = AnprV8Engine.Instance;
				anprV8.Start(anprHost);
				theFirst = false;
			}
		}
		public void Load()
        {
            try
            {
                var folder = GetPreferenceDirectory();
                
                _savedFilePath = Path.Combine(folder, GetSettingName() + ".conf");
                string subFolder = Path.Combine(folder, "backup");
                if (!Directory.Exists(subFolder))
                    Directory.CreateDirectory(subFolder);
                _savedBackupFilePath = Path.Combine(subFolder, GetSettingName() + ".conf");
                if (File.Exists(_savedFilePath))
                {
                    var data = File.ReadAllText(_savedFilePath);

                    _data = JsonConvert.DeserializeObject<T>(data, JsonSettings.Value);
					if (_data is HostSettingsData)
					{
						var hs = _data as HostSettingsData;
						startAnpr(hs.StorageANPR);
					}
					if (_data == null)
                    {
                        LocalLogService.Log(new Exception("Deserialize settings fail", new Exception(Environment.StackTrace)));
                        LoadBackup();
                    }
                    HasLocal = true;
                }
                else
                {
                    _data = Activator.CreateInstance<T>();
                    HasLocal = false;
                }
            }
            catch (Exception ex)
            {
                LocalLogService.Log(ex);
            }
        }

        public void LoadBackup()
        {
            try
            {
                var folder = GetPreferenceDirectory();
                string subFolder = Path.Combine(folder, "backup");
                string backupFilePath = Path.Combine(subFolder, GetSettingName() + ".conf");
                if (File.Exists(backupFilePath))
                {
                    var data = File.ReadAllText(backupFilePath);
                    _data = JsonConvert.DeserializeObject<T>(data, JsonSettings.Value);
					if (_data is HostSettingsData)
					{
						var hs = _data as HostSettingsData;
						startAnpr(hs.StorageANPR);
					}
					if (_data == null)
                    {
                        LocalLogService.Log(new Exception("Deserialize backup settings fail", new Exception(Environment.StackTrace)));
                        throw new Exception("Load settings fail", new Exception(Environment.StackTrace));
                    }
                    else
                    {
                        File.Copy(backupFilePath, _savedFilePath, true);
                        IsChanged = true;
                    }
                }
            }
            catch(Exception ex)
            {
                LocalLogService.Log(ex);
                throw new Exception("Load settings fail", new Exception(Environment.StackTrace));
            }
        }

        public static void CreateBackup()
        {
            string folder = GetPreferenceDirectory();
            string subFolder = Path.Combine(folder, "backup");
            if (!Directory.Exists(subFolder))
                Directory.CreateDirectory(subFolder);

            string[] files = Directory.GetFiles(folder);
            foreach (var item in files)
            {
                string path = Path.Combine(subFolder, Path.GetFileName(item));
                File.Copy(item, path, true);
            }
        }

        public void Save()
        {
            if (!IsChanged) return;
              ForceSave();
        }

        public static Lazy<JsonSerializerSettings> JsonSettings = new Lazy<JsonSerializerSettings>(() => {
            var settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.All;
            return settings;
        });

        public virtual void ForceSave()
        {
            try
            {
                string data;
                if (typeof(T) == typeof(HostSettingsData))
                    data = JsonConvert.SerializeObject(_data);
                else
                    data = JsonConvert.SerializeObject(_data, Formatting.Indented, JsonSettings.Value);

                File.WriteAllText(_savedFilePath, data);
                File.WriteAllText(_savedBackupFilePath, data);
                IsChanged = false;
                HasLocal = true;
            }
            catch (Exception ex)
            {
                
            }
        }

        public string CalculateChecksum()
        {
            string syncDataChecksum = _data.SyncDataChecksum;
            string checksum = _data.Checksum;
            _data.SyncDataChecksum = null;
            _data.Checksum = null;
            string dumpString;
            if (typeof(T) == typeof(HostSettingsData))
                dumpString = JsonConvert.SerializeObject(_data);
            else
                dumpString = JsonConvert.SerializeObject(_data, Formatting.Indented, JsonSettings.Value);
            _data.SyncDataChecksum = syncDataChecksum;
            _data.Checksum = checksum;
            if(dumpString == null)
                return null;
            byte[] result = null;
            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                result = hash.ComputeHash(enc.GetBytes(dumpString));
            }
            return Convert.ToBase64String(result);
        }
    }
}
