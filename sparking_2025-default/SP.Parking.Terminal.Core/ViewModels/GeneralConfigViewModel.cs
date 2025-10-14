using SP.Parking.Terminal.Core.Services;
using Cirrious.MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Green.Devices.Dal;
using Cirrious.MvvmCross.Plugins.Messenger;
using System.IO;
using System.Text.RegularExpressions;

namespace SP.Parking.Terminal.Core.ViewModels
{
    public class GeneralConfigViewModel : BaseViewModel
    {
        private string _serverResultMessage;
        private int _serverMessageLevel;
        private string _clientResultMessage;
        private int _clientMessageLevel;
        private string _primaryIP;
        private string _secondaryIP;
        private string _anprIP;
        private string _storagePath;
		private string _storageANPR;
		private string _terminalName;
        private int _actualSections;
        private Models.Terminal _terminal;

        private IUserPreferenceService _userPreferenceService;
        private IServer _server;
        private IHostSettings _hostSettings;

        private bool _checking = false;
        private int _numChecked = 0;
        public const string msgInvalidIP1 = "checkserver.invalid_ip_1";
        public const string msgInvalidIP2 = "checkserver.invalid_ip_2";
        public const string msgFailIP1 = "checkserver.fail_ip_1";
        public const string msgFailIP2 = "checkserver.fail_ip_2";
        public const string msgServerSuccess = "checkserver.success";
        public const string msgClientSuccess = "checkclient.success";
        public const string msgSaved = "message.saved";
        public const string msgChecking = "checkserver.checking";
        public const string msgInvalidStoragePath = "checkclient.invalid_storage_path";
        public const string msgInvalidTerminalName = "checkclient.invalid_terminal_name";

        public string ServerResultMessage
		{
            get { return _serverResultMessage; }
			set
			{
                if (_serverResultMessage == value) return;
                _serverResultMessage = value;
                RaisePropertyChanged(() => ServerResultMessage);
			}
		}

        public int ServerMessageLevel
        {
            get { return _serverMessageLevel; }
            set
            {
                if (_serverMessageLevel == value) return;
                _serverMessageLevel = value;
                RaisePropertyChanged(() => ServerMessageLevel);
            }
        }

        public string PrimaryIP
        {
            get { return _primaryIP; }
            set
            {
                if (_primaryIP == value) return;
                _primaryIP = value;
                RaisePropertyChanged(() => PrimaryIP);
            }
        }

        public string SecondaryIP
        {
            get { return _secondaryIP; }
            set
            {
                if (_secondaryIP == value) return;
                _secondaryIP = value;
                RaisePropertyChanged(() => SecondaryIP);
            }
        }

        public string AnprIP
        {
            get { return _anprIP; }
            set
            {
                if (_anprIP == value) return;
                _anprIP = value;
                RaisePropertyChanged(() => AnprIP);
            }
        }

		public int ActualSections
		{
			get { return _actualSections; }
			set
			{
				if (_actualSections == value) return;
				_actualSections = value;
				RaisePropertyChanged(() => ActualSections);
			}
		}

		public string ClientResultMessage
        {
            get { return _clientResultMessage; }
            set
            {
                if (_clientResultMessage == value) return;
                _clientResultMessage = value;
                RaisePropertyChanged(() => ClientResultMessage);
            }
        }

        public int ClientMessageLevel
        {
            get { return _clientMessageLevel; }
            set
            {
                if (_clientMessageLevel == value) return;
                _clientMessageLevel = value;
                RaisePropertyChanged(() => ClientMessageLevel);
            }
        }

        public string StoragePath
        {
            get { return _storagePath; }
            set 
            {
                if (_storagePath == value) return;
                _storagePath = value;
                RaisePropertyChanged(() => StoragePath);
            }
        }
		public string StorageANPR
		{
			get { return _storageANPR; }
			set
			{
				if (_storageANPR == value) return;
				_storageANPR = value;
				RaisePropertyChanged(() => StorageANPR);
			}
		}
		public string TerminalName
        {
            get { return _terminalName; }
            set
            {
                if (_terminalName == value) return;
                _terminalName = value;
                RaisePropertyChanged(() => TerminalName);
            }
        }

        public GeneralConfigViewModel(IViewModelServiceLocator services, IUserPreferenceService userPreferenceService, IServer server)
            : base(services)
        {
            _userPreferenceService = userPreferenceService;
            _server = server;
            _hostSettings = _userPreferenceService.HostSettings;
        }

        public void Init(ParameterKey key)
        {
        }

        public override void Start()
        {
            base.Start();
            PrimaryIP = _hostSettings.PrimaryServerIP;
            SecondaryIP = _hostSettings.SecondaryServerIP;
            AnprIP = _hostSettings.AnprIP;
            StoragePath = _hostSettings.StoragePath;
			StorageANPR = _hostSettings.StorageANPR;
            ActualSections = _hostSettings.ActualSections;
			_terminal = _hostSettings.Terminal;
            if (_terminal == null)
            {
                _terminal = new Models.Terminal();
                _terminal.Name = Environment.MachineName;
                //_terminal.TerminalId = _terminal.Name;
                if (string.IsNullOrEmpty(_terminal.TerminalId))
                    _terminal.TerminalId = Guid.NewGuid().ToString();
                _terminal.Status = Models.TerminalStatus.Enable;
                _hostSettings.Terminal = _terminal;
            }
            TerminalName = _terminal.Name;
        }

		private MvxCommand _checkServerConfigCommand;
		public ICommand CheckServerConfigCommand {
			get {
                _checkServerConfigCommand = _checkServerConfigCommand ?? new MvxCommand(() =>
				{
                    if (_checking) return;
                    if (!IsValidIPString(PrimaryIP))
                    {
                        SetServerMessage(msgInvalidIP1);
                        return;
                    }
                    if(!IsValidIPString(SecondaryIP))
                    {
                        SetServerMessage(msgInvalidIP2);
                        return;
                    }
                    _numChecked = 0;
                    _checking = true;
                    SetServerMessage(msgChecking);
                    StatusChanged(ProgressStatus.Started);
                    _server.CheckHealthServer(PrimaryIP, OnCheckHealthResultReceived);
				});

                return _checkServerConfigCommand;
			}
		}

        private MvxCommand _checkClientConfigCommand;
        public ICommand CheckClientConfigCommand
        {
            get
            {
                _checkClientConfigCommand = _checkClientConfigCommand ?? new MvxCommand(() =>
                {
                    if (_checking) return;
                    if (!IsValidStoragePath(StoragePath))
                    {
                        SetClientMessage(msgInvalidStoragePath);
                        return;
                    }
                    if(TerminalName == null || TerminalName.Length == 0)
                    {
                        SetClientMessage(msgInvalidTerminalName);
                        return;
                    }
                    SetClientMessage(msgClientSuccess);
                });

                return _checkClientConfigCommand;
            }
        }

        private bool IsValidIPString(string ip)
        {
            //var regex = new Regex(@"\d{1,3}[.]\d{1,3}[.]\d{1,3}[.]\d{1,3}[:]\d{1,5}");
            //var match = regex.Match(ip);
            //if(match.Success)
            //{
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}

            if (ip == null) return false;
            string[] octs = ip.Split('.');
            if (octs.Length != 4) return false;
            int idx = 0;
            foreach(string oct in octs)
            {
                string s = oct;
                int o = 0;
                if (idx == 3 && s.Contains(':'))
                {
                    string[] eles = s.Split(':');
                    if (eles.Length != 2) return false;
                    if (!int.TryParse(eles[1], out o))
                        return false;
                    s = eles[0];
                }
                if (int.TryParse(s, out o))
                {
                    if (o > 255 || o < 0) return false;
                }
                else
                {
                    return false;
                }
                idx++;
            }
            return true;
        }

        private bool IsValidStoragePath(string path)
        {
            try
            {
                string testfile = path;
                char lastChar = testfile[testfile.Length - 1];
                if (lastChar != '\\')
                    testfile += '\\';
                testfile += "test";
                File.WriteAllText(testfile, "test");
                File.Delete(testfile);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void OnCheckHealthResultReceived(Exception exception)
        {
            _numChecked++;
            if (exception != null)
            {
                if(_numChecked == 1)
                    SetServerMessage(msgFailIP1);
                else if(_numChecked == 2)
                    SetServerMessage(msgFailIP2);
            }
            else
            {
                if(_numChecked == 1)
                {
                    _server.CheckHealthServer(SecondaryIP, OnCheckHealthResultReceived);
                    return;
                }
                else if (_numChecked == 2)
                {
                    SetServerMessage(msgServerSuccess);
                }
            }
            StatusChanged(ProgressStatus.Ended);
            _checking = false;
        }

        private MvxCommand _saveServerConfigCommand;
        public ICommand SaveServerConfigCommand
        {
            get
            {
                _saveServerConfigCommand = _saveServerConfigCommand ?? new MvxCommand(() =>
                {
                    if (_checking) return;
                    if (!IsValidIPString(PrimaryIP))
                    {
                        SetServerMessage(msgInvalidIP1);
                        return;
                    }
                    if (!IsValidIPString(SecondaryIP))
                    {
                        SetServerMessage(msgInvalidIP2);
                        return;
                    }
                    _hostSettings.PrimaryServerIP = _primaryIP;
                    _hostSettings.SecondaryServerIP = _secondaryIP;
                    _hostSettings.AnprIP = _anprIP;
                    _hostSettings.MarkChanged();
                    _hostSettings.Save();
                    SetServerMessage(msgSaved);
                });

                return _saveServerConfigCommand;
            }
        }

        private MvxCommand _saveClientConfigCommand;
        public ICommand SaveClientConfigCommand
        {
            get
            {
                _saveClientConfigCommand = _saveClientConfigCommand ?? new MvxCommand(() =>
                {
                    if (_checking) return;
                    if(_terminalName == null || _terminalName.Length == 0)
                    {
                        SetClientMessage(msgInvalidTerminalName);
                        return;
                    }
                    if (!IsValidStoragePath(StoragePath))
                    {
                        SetClientMessage(msgInvalidStoragePath);
                        return;
                    }
                    _hostSettings.StoragePath = _storagePath;
					_hostSettings.StorageANPR = _storageANPR;
                    _hostSettings.ActualSections = _actualSections;
					if (_terminalName != _terminal.Name)
                    {
                        _terminal.Name = _terminalName;
                        //_terminal.TerminalId = _terminalName;
                        if (string.IsNullOrEmpty(_terminal.TerminalId))
                            _terminal.TerminalId = Guid.NewGuid().ToString();
                        _hostSettings.Terminal = _terminal;
                    }
                    _hostSettings.MarkChanged();
                    _hostSettings.Save();
                    SetClientMessage(msgSaved);
                });

                return _saveClientConfigCommand;
            }
        }

        private void SetServerMessage(string messageId)
        {
            switch(messageId)
            {
                case msgServerSuccess:
                    ServerMessageLevel = 1;
                    break;
                case msgSaved:
                    ServerMessageLevel = 2;
                    break;
                default:
                    ServerMessageLevel = 3;
                    break;
            }
            ServerResultMessage = messageId;
        }

        private void SetClientMessage(string messageId)
        {
            switch (messageId)
            {
                case msgClientSuccess:
                    ClientMessageLevel = 1;
                    break;
                case msgSaved:
                    ClientMessageLevel = 2;
                    break;
                default:
                    ClientMessageLevel = 3;
                    break;
            }
            ClientResultMessage = messageId;
        }
    }
}
