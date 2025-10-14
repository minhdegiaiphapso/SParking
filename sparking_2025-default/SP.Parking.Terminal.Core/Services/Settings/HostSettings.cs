using SP.Parking.Terminal.Core.Models;
using SP.Parking.Terminal.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Services
{
    public class HostSettingsData : BaseSettingsData
    {
        public string PrimaryServerIP { get; set; }
        public string SecondaryServerIP { get; set; }
        public string AnprIP { get; set; }
        public string LogServerIP { get; set; }

        string _storagePath = "C:\\";
        public string StoragePath
        {
            get { return _storagePath; }
            set { _storagePath = value; }
        }
		string _storageAnpr = "C:\\";
		public string StorageANPR
		{
			get { return _storageAnpr; }
			set { _storageAnpr = value; }
		}
		public Models.Terminal Terminal { get; set; }
        public string ParkingName { get; set; }
        public int ActualSecsions {  get; set; }
    }

    public interface IHostSettings : IBaseSettings
    {
        string PrimaryServerIP { get; set; }
        string SecondaryServerIP { get; set; }
        string AnprIP { get; set; }
        string LogServerIP { get; set; }
		string StorageANPR { get; set; }
		string StoragePath { get; set; }
        Models.Terminal Terminal { get; set; }
        string ParkingName { get; set; }
        int ActualSections { get; set; }
    }

    public class HostSettings : BaseSettings<HostSettingsData>, IHostSettings
    {
        public HostSettings(ArgumentParameter argParams = null)
            : base(argParams)
        {
            argParams = new ArgumentParameter() { Mode = RunMode.Testing };
            if (argParams != null && argParams.Mode == RunMode.Testing && argParams.Host != null && argParams.Host.Length > 0)
            {
                PrimaryServerIP = argParams.Host[0];
                if (argParams.Host.Length > 1)
                    SecondaryServerIP = argParams.Host[1];
            }
        }

        public string PrimaryServerIP
        {
            get
            {
                return _data.PrimaryServerIP;
            }
            set
            {
                if (_data.PrimaryServerIP != value)
                {
                    _data.PrimaryServerIP = value;
                    //MarkChanged();
                }
            }
        }

        public string SecondaryServerIP
        {
            get
            {
                return _data.SecondaryServerIP;
            }
            set
            {
                if (_data.SecondaryServerIP != value)
                {
                    _data.SecondaryServerIP = value;
                    //MarkChanged();
                }
            }
        }

        public string AnprIP
        {
            get { return _data.AnprIP; }
            set
            {
                if(_data.AnprIP != value)
                {
                    _data.AnprIP = value;
                    //MarkChanged();
                }
            }
        }

        public string LogServerIP
        {
            get
            {
                return _data.LogServerIP;
            }
            set
            {
                if (_data.LogServerIP != value)
                {
                    _data.LogServerIP = value;
                    //MarkChanged();
                }
            }
        }

        public string StoragePath
        {
            get
            {
                return _data.StoragePath;
            }
            set
            {
                if (_data.StoragePath != value)
                {
                    _data.StoragePath = value;
                    //MarkChanged();
                }
            }
        }
		public string StorageANPR
		{
			get
			{
				return _data.StorageANPR;
			}
			set
			{
				if (_data.StorageANPR != value)
				{
					_data.StorageANPR = value;
					startAnpr(value);
					//MarkChanged();
				}
			}
		}
		public Models.Terminal Terminal
        {
            get
            {
                return _data.Terminal;
            }
            set
            {
                //MarkChanged();
                _data.Terminal = value;
            }
        }
        public int ActualSections { 
            get
            {
                return _data.ActualSecsions;
            }
            set
            {
                _data.ActualSecsions = value;
            }
        }
        public string ParkingName
        {
            get { return _data.ParkingName; }
            set
            {
                if (_data.ParkingName != value)
                {
                    _data.ParkingName = value;
                    //MarkChanged();
                }
            }
        }

        protected override string GetSettingName()
        {
            if (_argParams != null && _argParams.Mode == RunMode.Testing)
                return this.GetType().Name + "Test";
            else
                return base.GetSettingName();
        }
		private void startAnpr(string path)
		{
			if (System.IO.Directory.Exists(path))
			{
				var anprV8 = AnprV8Engine.Instance;
				anprV8.Start(path);
			}
		}
	}
}
