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
using SP.Parking.Terminal.Core.Models;
using Cirrious.CrossCore;
using System.Threading;
using Green.Devices.Dal.Siemens;

namespace SP.Parking.Terminal.Core.ViewModels
{
    public class LoginSuccessMessage : MvxMessage
    {
        public LoginSuccessMessage(object sender) : base(sender) { }
    }

    //public class LogoutSuccessMessage : MvxMessage
    //{
    //    public LogoutSuccessMessage(object sender) : base(sender) { }
    //}

    public class LoginViewModel : BaseViewModel
    {
        IRunModeManager _modeManager;
        private IMvxMessenger _messenger;
        private IUserServiceLocator _userServiceLocator;
        private IResourceLocatorService _resourceLocator;
        //private IUserService _userService;
        private IUserPreferenceService _preferenceService;

        private string _username;
        private string _password;
		private string _resultMessage;

        public const string msgCardReaderDisconnect = "error.cardreader_disconnect";
        public const string msgInvalid = "login.invalid";
        public const string msgServerError = "error.server_error";
        public const string msgServerDisconnect = "error.server_disconnect";

		public string ResultMessage
		{
			get { return _resultMessage; }
			set
			{
				if (_resultMessage == value)
					return;

				_resultMessage = value;
				RaisePropertyChanged(() => ResultMessage);
                RaisePropertyChanged(() => HasError);
			}
		}

        public bool HasError
        {
            get { return _resultMessage != null; }
        }

        public string Username
        {
            get { return _username; }
            set
            {
                if (_username == value) return;
                _username = value;
                RaisePropertyChanged(() => Username);
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                if (_password == value) return;
                _password = value;
                RaisePropertyChanged(() => Password);
            }
        }

        private List<Section> _sections;

        public LoginViewModel(IViewModelServiceLocator services
            , IUserServiceLocator userServiceLocator, IResourceLocatorService resourceLocator, IUserPreferenceService userPrefService)
            : base(services)
        {
            this._userServiceLocator = userServiceLocator;
            this._resourceLocator = resourceLocator;
            this._messenger = services.Messenger;
            _modeManager = services.ModeManager;
            _preferenceService = userPrefService;
            //Username = "support";
            //Password = "gp142536";
        }
        private int GetLane()
        {
            switch (this.Section.Id)
            {
                case SectionPosition.Lane1:
                    return 1;
                case SectionPosition.Lane2:
                    return 2;
                case SectionPosition.Lane3:
                    return 3;
                case SectionPosition.Lane4:
                    return 4;
            }
            return 0;
        }
        public void Init(ParameterKey key)
        {
            // Attach card reader callback
            Section = Services.Parameter.Retrieve<ISection>(key);
            if (this.Section != null && this.Section.BarrierBySiemensControl != null && !string.IsNullOrEmpty(this.Section.BarrierBySiemensControl.IP) && this.Section.BarrierBySiemensControl.TypeIn != LogoTypeIn4.None)
            {
                Port4 mydevice = Port4.GetInstance();
                var Lane = GetLane();
                mydevice.AddCommandIn(new SiemenInfo
                {
                    TcpIp = this.Section.BarrierBySiemensControl.IP,
                    TypeIn = this.Section.BarrierBySiemensControl.TypeIn,
                    Lane = Lane
                });
            }
            _sections = _preferenceService.SystemSettings.GetAllSections(Section.DisplayedPosition);
            foreach (var sec in _sections)
            {
                
                if (sec.ModWinsCards != null)
                {
                    if (sec.ShouldBeDisplayed &&!CurrentListCardReader.StartGreenCardReader(sec.ModWinsCards, OnGreenCarReaderReceived, null) )
                        ResultMessage = msgCardReaderDisconnect;
                    else
                        ResultMessage = null;
                }
                //sec.SetupCardReader();
                //if (!sec.StartCardReader(OnRFIDCardReceived, null) && sec.ShouldBeDisplayed)
                //    ResultMessage = msgCardReaderDisconnect;
                //else
                //    ResultMessage = null;
            }

            //_userService = _userServiceLocator.GetUserService(Section.Id);
        }
        private void OnGreenCarReaderReceived(object sender, GreenCardReaderEventArgs e)
        {
            if (e == null || string.IsNullOrEmpty(e.CardID))
            {
                ResultMessage = msgInvalid;
            }
            else
            {
                ResultMessage = null;
                StatusChanged(ProgressStatus.Started);
                LoginForAllSectionInSamePosition(e.CardID);
            }
        }
        public override void Start()
        {
            base.Start();

            DoLoginForTesting();
        }

        private void DoLoginForTesting()
        {
            if (_modeManager.ArgumentParams.Mode == RunMode.Testing)
            {
                Mvx.Resolve<IWebApiTestingServer>().RegisterLane(Section, lane => {
                    if (lane != null && lane.Enabled)
                    {
                        this.Section.Direction = lane.Direction;
                        Username = "admin";
                        Password = "nopass";
                        LoginCommand.Execute(null);
                    }
                });
            }
        }

		private MvxCommand _loginCommand;
		public ICommand LoginCommand {
			get {
				_loginCommand = _loginCommand ?? new MvxCommand(() =>
				{
                    if (Username == null || Password == null || Username.Length == 0 || Password.Length == 0)
                    {
                        ResultMessage = msgInvalid;
                    }
                    else
                    {
                        ResultMessage = null;
                        StatusChanged(ProgressStatus.Started);
                        //_userService.Login(Username, Password, Section.Lane.Id, OnLoginResultReceived);
                        LoginForAllSectionInSamePosition();
                    }
				});

				return _loginCommand;
			}
		}

        private void LoginForAllSectionInSamePosition(string cardId = "")
        {
            ManualResetEvent resetEvent = new ManualResetEvent(false);
            int numberOfThreads = _sections.Count;
            bool isSuccess = false;
            Exception exception = null;
            foreach (var sec in _sections)
            {
                if (!string.IsNullOrEmpty(cardId))
                {
                    sec.UserService.Login(cardId, sec.Lane.Id, ex => {
                        if (ex == null)
                            isSuccess = true;
                        else
                            exception = ex;
                        Interlocked.Decrement(ref numberOfThreads);
                        if (numberOfThreads == 0)
                            resetEvent.Set();
                        ClearTotalAmount(sec);
                    });
                }
                else
                {
                    sec.UserService.Login(Username, Password, sec.Lane.Id, ex => {
                        if (ex == null)
                        {
                            isSuccess = true;
                            ClearTotalAmount(sec);
                        }
                        else
                            exception = ex;
                        Interlocked.Decrement(ref numberOfThreads);
                        if (numberOfThreads == 0)
                            resetEvent.Set();
                    });
                }
            }
            resetEvent.WaitOne(3000);
            OnLoginResultReceived(isSuccess ? null : exception);
        }


        /*** 18-07-2016 Clear So luong tien thu duoc cua 1 ca lam viec ***/
        private void ClearTotalAmount(Section section)
        {
            section.TotalAmount = 0;
            //_preferenceService.SystemSettings.UpdateSection(section);
           // _preferenceService.SystemSettings.Save();

        }
        private void OnRFIDCardReceived(object sender, CardReaderEventArgs e)
        {
            if (e == null || e.CardID == null)
            {
                ResultMessage = msgInvalid;
            }
            else
            {
                ResultMessage = null;
                StatusChanged(ProgressStatus.Started);
               // _userService.Login(e.CardID, Section.Lane.Id, OnLoginResultReceived);
                LoginForAllSectionInSamePosition(e.CardID);
            }
        }

        private void OnLoginResultReceived(Exception exception)
        {
            StatusChanged(ProgressStatus.Ended);
            if (exception == null)
            {
                LoginSuccess();
            }
            else if (exception is LoginInvalidException)
            {
                ResultMessage = msgInvalid;
            }
            else if (exception is ServerErrorException)
            {
                ResultMessage = msgServerError;
            }
            else if (exception is ServerDisconnectException)
            {
                ResultMessage = msgServerDisconnect;
            }
        }

        int flag = 0;
        private void LoginSuccess()
        {
            lock (this)
            {
                Interlocked.Increment(ref flag);
                if (flag > 1)
                    return;
            }
            // Detach card reader callback
            //this.Section.StopCardReader(OnRFIDCardReceived, null);
            //this.Section.UserService = _userService;

            // Request LaneViewModelFactory to show corresponding LaneViewModel type
            ResultMessage = null;
            if (_messenger.HasSubscriptionsFor<LoginSuccessMessage>())
            {
                _messenger.Publish(new LoginSuccessMessage(this));
            }
            Close();
            //BaseSettings<BaseSettingsData>.CreateBackup();
        }

        public override void Close()
        {
            base.Close();
            if (this.Section.ModWinsCards != null)
            {
                CurrentListCardReader.StoptGreenCardReader(this.Section.ModWinsCards, OnGreenCarReaderReceived, null);
            }
            //this.Section.StopCardReader(OnRFIDCardReceived, null);
        }

        public override void Unloaded()
        {
            base.Unloaded();

            foreach (var item in _sections)
            {
                //item.StopCardReader(OnRFIDCardReceived, null);
                if (item.ModWinsCards != null)
                {
                    CurrentListCardReader.StoptGreenCardReader(item.ModWinsCards, OnGreenCarReaderReceived, null);
                }
            }
        }
    }
}
