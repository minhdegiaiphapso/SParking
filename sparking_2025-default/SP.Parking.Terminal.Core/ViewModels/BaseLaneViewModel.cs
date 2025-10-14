using SP.Parking.Terminal.Core.Services;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Green.Devices.Dal;
using System.Windows.Input;
using Cirrious.CrossCore;
using SP.Parking.Terminal.Core.Models;
using System.Windows.Threading;
using Green.Devices.Dal.CardControler;
using Serilog;
using System.Windows.Controls;

namespace SP.Parking.Terminal.Core.ViewModels
{
    public class KeyPressedMessage : MvxMessage
    {
        public object Sender { get; set; }
        public KeyEventArgs KeyEventArgs { get; set; }
        public KeyPressedMessage(object sender, KeyEventArgs args)
            : base(sender)
        {
            Sender = sender;
            KeyEventArgs = args;
        }
    }

    public class CloseChildMessage : MvxMessage
    {
        public SectionPosition SectionId { get; set; }

        public CloseChildMessage(object sender, SectionPosition pos)
            : base(sender)
        {
            SectionId = pos;
        }
    }

    public class ClosePopupMessage : MvxMessage
    {
        public ClosePopupMessage(object sender)
            : base(sender)
        {

        }
    }

    public class ShowChildMessage : MvxMessage
    {
        public SectionPosition SectionId { get; set; }

        public Type ChildTypeViewModel { get; set; }

        public object Params { get; set; }

        //public ShowChildMessage(object sender, SectionPosition pos, Type type)
        //    : base(sender)
        //{
        //    SectionId = pos;
        //    ChildTypeViewModel = type;
        //}
        public ShowChildMessage(object sender, SectionPosition pos, Type type, object param = null)
            : base(sender)
        {
            SectionId = pos;
            ChildTypeViewModel = type;
            Params = param;
        }
    }

    public class ChangeLaneMessage : MvxMessage
    {
        public SectionPosition SectionId { get; set; }

        public ChangeLaneMessage(object sender, SectionPosition pos)
            : base(sender)
        {
            SectionId = pos;
        }
    }
	public class DetectedTag
	{
		public string EPC { get; set; }
		public DateTime DetectedAt { get; set; }
		public double? Rssi { get; set; }
        public bool Avlaible {  get; set; }
	}

	public class BaseLaneViewModel : BaseViewModel
    {
        protected ITestingService _testingService;

        IMvxMessenger _messenger;

        //protected IKeyService _keyService;

        protected IRunModeManager _modeManager;

        protected IStorageService _storageService;

        protected ILogService _logService;

        protected IResourceLocatorService _resourceService;

        protected IALPRService _alprService;
		private IOptionsSettings _settings;
		protected IServer _server;
        protected List<FarCards> FarCards;
        protected List<BlackNumber> BlackNumbers;
        protected string CurtentBlackNum;
        protected IUserPreferenceService _userPreferenceService;

        protected IUserServiceLocator _userServiceLocator;

        protected IUserService _userService;

        protected MvxSubscriptionToken _keyPressedToken;


        protected MvxSubscriptionToken _closePopupToken;

        protected bool _notEditPlateNumberYet = true;

        protected LaneDirection _laneDirection;
        protected ILogger _logger;

        #region Properties
        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (_isBusy == value)
                    return;
                _isBusy = value;
                RaisePropertyChanged(() => IsBusy);
            }
        }
        private ApmsUser _user;
        public ApmsUser User
        {
            get { return _user; }
            set { _user = value; RaisePropertyChanged(() => UserNameAndId); }
        }

        public string UserNameAndId
        {
            get
            {
                if (_user != null)
                    return _user.DisplayName + " - " + _user.StaffID;
                else
                    return null;
            }
        }

        private Card _checkedCard;
        public Card CheckedCard
        {
            get { return _checkedCard; }
            set
            {
                if (_checkedCard == value)
                    return;

                _checkedCard = value;
                RaisePropertyChanged(() => CheckedCard);
            }
        }

        private DateTime _checkInTime;
        public DateTime CheckInTime
        {
            get { return _checkInTime; }
            set
            {
                if (_checkInTime == value) return;

                _checkInTime = value;
                RaisePropertyChanged(() => CheckInTime);
            }
        }

        public LaneDirection Direction
        {
            get { return this.Section.Lane.Direction; }
            set
            {
                if (Section.Lane.Direction == value)
                    return;

                Section.Lane.Direction = value;

                RaisePropertyChanged(() => Direction);
            }
        }

        bool _showChooseVehicleType;
        public bool ShowChooseVehicleType
        {
            get { return _showChooseVehicleType; }
            set
            {
                _showChooseVehicleType = value;
                RaisePropertyChanged(() => ShowChooseVehicleType);
            }
        }


        bool _showEntries;
        public bool ShowEntries
        {
            get { return _showEntries; }
            set
            {
                _showEntries = value;
                RaisePropertyChanged(() => ShowEntries);
            }
        }
        CustomerInfo _customerInfo;
        public CustomerInfo CustomerInfo
        {
            get { return _customerInfo; }
            set
            {
                if (_customerInfo == value) return;
                _customerInfo = value;
                RaisePropertyChanged(() => CustomerInfo);
                RaisePropertyChanged(() => IsCustomer);
                RaisePropertyChanged(() => IsExpiredSubscription);
            }
        }

        public bool IsCustomer
        {
            get
            {
                return CustomerInfo != null && CustomerInfo.CustomerName != null;
            }
        }

        public bool IsExpiredSubscription { get { return IsCustomer && CustomerInfo.VehicleRegistrationInfo.RemainDays <= 0; } }

        protected bool CheckCustomerInfoValid()
        {
            if (CustomerInfo == null || CustomerInfo.VehicleRegistrationInfo == null)
                return false;
            return true;
        }
        #endregion

        public void SetupDevices()
        {
            this.Section.SetupDevice(_laneDirection);
        }

        public BaseLaneViewModel(IViewModelServiceLocator service, IStorageService storageService, IMvxMessenger messenger)
            : base(service)
        {
            FarCards = new List<FarCards>();
            BlackNumbers = new List<BlackNumber>();
            CurtentBlackNum = "";
            _modeManager = service.ModeManager;
            _storageService = storageService;
            _messenger = messenger;

            _testingService = Mvx.Resolve<ITestingService>();
            _resourceService = Mvx.Resolve<IResourceLocatorService>();
            _logService = Mvx.Resolve<ILogService>();
			_settings = Mvx.Resolve<IOptionsSettings>();
			_server = Mvx.Resolve<IServer>();
            _userPreferenceService = Mvx.Resolve<IUserPreferenceService>();
            //if (_userPreferenceService.OptionsSettings.PlateRecognitionBySfactors)
            //    _alprService = Mvx.Resolve<PlateRecognizeService>();
            //else
            //    _alprService = Mvx.Resolve<ANPRService>();
            _alprService = new ANPRV8();// Mvx.Resolve<ANPRV8>();
			_userServiceLocator = Mvx.Resolve<IUserServiceLocator>();
            //_keyService = Mvx.Resolve<IKeyService>();

            _keyPressedToken = service.Messenger.Subscribe<KeyPressedMessage>(OnKeyPressed);
            _closePopupToken = service.Messenger.Subscribe<ClosePopupMessage>(OnClosePopup);
            _logger = Log.Logger;
        }

        public virtual void Init(ParameterKey key)
        {
            this.Section = (ISection)Services.Parameter.Retrieve(key);
            _userService = _userServiceLocator.GetUserService(this.Section.Id);

            this.User = _userService.CurrentUser;
            _laneDirection = Section.TemporaryDirection;
            //if (this is CheckInLaneViewModel)
            //    _laneDirection = LaneDirection.In;
            //else
            //    _laneDirection = LaneDirection.Out;

            SetupDevices();
            CloseEvent();
        }
        private void ResetWaitingCardReader(string type)
        {
            switch (type)
            {
                case "Server":
                    lock (this.Section.TcpIpServerCards)
                    {
                        foreach (var item in this.Section.TcpIpServerCards)
                        {
                            item.TimeReset = _userPreferenceService.OptionsSettings.WaitingProlificCardReaderDuration;
                        }
                    }
                    break;
                case "Client":
                    lock (this.Section.TcpIpClientCards)
                    {
                        foreach (var item in this.Section.TcpIpClientCards)
                        {
                            item.TimeReset = _userPreferenceService.OptionsSettings.WaitingSoyalCardReaderDuration;
                        }
                    }
                    break;
                case "Scannel":
                    lock (this.Section.ScannelCards)
                    {
                        foreach (var item in this.Section.ScannelCards)
                        {
                            item.TimeReset = _userPreferenceService.OptionsSettings.WaitingProlificCardReaderDuration;
                        }
                    }
                    break;
                default:
                    break;
            }
        }
		#region Process for tracker camera
		private void startTracker()
		{
			if (_laneDirection == LaneDirection.In)
			{
				
				if (this.Section.BackInCamera != null && this.Section.BackInCamera.TrackerEvent != null)
				{
					this.Section.BackInCamera.TrackerEvent.BlueInUse = true;
					this.Section.BackInCamera.TrackerEvent.RedInUse = true;
					this.Section.BackInCamera.TrackerEvent.BackgroundName = $"{this.Section.Id}_BackInCamera";
					this.Section.BackInCamera.TrackerEvent.BlueOccurHandler += OnBackFireBlue;
					this.Section.BackInCamera.TrackerEvent.RedOccurHandler += OnBackFireRed;
					this.Section.BackInCamera.TrackerEvent.BlueLostHandler += OnBackLostBlue;
					this.Section.BackInCamera.TrackerEvent.RedLostHandler += OnBackLostRed;
				}
				if (this.Section.FrontInCamera != null && this.Section.FrontInCamera.TrackerEvent != null)
				{
					this.Section.FrontInCamera.TrackerEvent.BlueInUse = true;
					this.Section.FrontInCamera.TrackerEvent.RedInUse = true;
					this.Section.FrontInCamera.TrackerEvent.BackgroundName = $"{this.Section.Id}_FrontInCamera"; ;
					this.Section.FrontInCamera.TrackerEvent.BlueOccurHandler += OnFrontFireBlue;
					this.Section.FrontInCamera.TrackerEvent.RedOccurHandler += OnFrontFireRed;
					this.Section.FrontInCamera.TrackerEvent.BlueLostHandler += OnFrontLostBlue;
					this.Section.FrontInCamera.TrackerEvent.RedLostHandler += OnFrontLostRed;
				}
			}
			if (_laneDirection == LaneDirection.Out)
			{
				
				if (this.Section.BackOutCamera != null && this.Section.BackOutCamera.TrackerEvent != null)
				{
					this.Section.BackOutCamera.TrackerEvent.BlueInUse = true;
					this.Section.BackOutCamera.TrackerEvent.RedInUse = true;
					this.Section.BackOutCamera.TrackerEvent.BackgroundName = $"{this.Section.Id}_BackOutCamera";
					this.Section.BackOutCamera.TrackerEvent.BlueOccurHandler += OnBackFireBlue;
					this.Section.BackOutCamera.TrackerEvent.RedOccurHandler += OnBackFireRed;
					this.Section.BackOutCamera.TrackerEvent.BlueLostHandler += OnBackLostBlue;
					this.Section.BackOutCamera.TrackerEvent.RedLostHandler += OnBackLostRed;
				}
				if (this.Section.FrontOutCamera != null && this.Section.FrontOutCamera.TrackerEvent != null)
				{
					this.Section.FrontOutCamera.TrackerEvent.BlueInUse = true;
					this.Section.FrontOutCamera.TrackerEvent.RedInUse = true;
					this.Section.FrontOutCamera.TrackerEvent.BackgroundName = $"{this.Section.Id}_FrontOutCamera";
					this.Section.FrontOutCamera.TrackerEvent.BlueOccurHandler += OnFrontFireBlue;
					this.Section.FrontOutCamera.TrackerEvent.RedOccurHandler += OnFrontFireRed;
					this.Section.FrontOutCamera.TrackerEvent.BlueOccurHandler += OnFrontLostBlue;
					this.Section.FrontOutCamera.TrackerEvent.RedOccurHandler += OnFrontLostRed;
				}
			}
		}
		private void stopTracker()
		{
			if (_laneDirection == LaneDirection.In)
			{
				if (this.Section.BackInCamera != null && this.Section.BackInCamera.TrackerEvent != null)
				{
					this.Section.BackInCamera.TrackerEvent.BlueInUse = false;
					this.Section.BackInCamera.TrackerEvent.RedInUse = false;
					this.Section.BackInCamera.TrackerEvent.BlueOccurHandler -= OnBackFireBlue;
					this.Section.BackInCamera.TrackerEvent.RedOccurHandler -= OnBackFireRed;
					this.Section.BackInCamera.TrackerEvent.BlueLostHandler -= OnBackLostBlue;
					this.Section.BackInCamera.TrackerEvent.RedLostHandler -= OnBackLostRed;
				}
				if (this.Section.FrontInCamera != null && this.Section.FrontInCamera.TrackerEvent != null)
				{
					this.Section.FrontInCamera.TrackerEvent.BlueInUse = false;
					this.Section.FrontInCamera.TrackerEvent.RedInUse = false;
					this.Section.FrontInCamera.TrackerEvent.BlueOccurHandler -= OnFrontFireBlue;
					this.Section.FrontInCamera.TrackerEvent.RedOccurHandler -= OnFrontFireRed;
					this.Section.FrontInCamera.TrackerEvent.BlueLostHandler -= OnFrontLostBlue;
					this.Section.FrontInCamera.TrackerEvent.RedLostHandler -= OnFrontLostRed;
				}
			}
			if (_laneDirection == LaneDirection.Out)
			{
				if (this.Section.BackOutCamera.RawCamera != null && this.Section.BackOutCamera.RawCamera.TrackerEvent != null)
				{
					this.Section.BackOutCamera.TrackerEvent.BlueInUse = false;
					this.Section.BackOutCamera.TrackerEvent.RedInUse = false;
					this.Section.BackOutCamera.TrackerEvent.BlueOccurHandler -= OnBackFireBlue;
					this.Section.BackOutCamera.TrackerEvent.RedOccurHandler -= OnBackFireRed;
					this.Section.BackOutCamera.TrackerEvent.BlueLostHandler -= OnBackLostBlue;
					this.Section.BackOutCamera.TrackerEvent.RedLostHandler -= OnBackLostRed;
				}
				if (this.Section.FrontOutCamera.RawCamera != null && this.Section.FrontOutCamera.RawCamera.TrackerEvent != null)
				{
					this.Section.FrontOutCamera.RawCamera.TrackerEvent.BlueInUse = true;
					this.Section.FrontOutCamera.RawCamera.TrackerEvent.RedInUse = true;
					this.Section.FrontOutCamera.RawCamera.TrackerEvent.BlueOccurHandler -= OnFrontFireBlue;
					this.Section.FrontOutCamera.RawCamera.TrackerEvent.RedOccurHandler -= OnFrontFireRed;
					this.Section.FrontOutCamera.TrackerEvent.BlueOccurHandler -= OnFrontLostBlue;
					this.Section.FrontOutCamera.TrackerEvent.RedOccurHandler -= OnFrontLostRed;
				}
			}
		}

		protected virtual void OnBackFireBlue(object sender, Tuple<DateTime, double> e)
		{

		}
		protected virtual void OnBackLostBlue(object sender, Tuple<DateTime, double> e)
		{

		}
		protected virtual void OnBackFireRed(object sender, Tuple<DateTime, double> e)
		{

		}
		protected virtual void OnBackLostRed(object sender, Tuple<DateTime, double> e)
		{

		}
		protected virtual void OnFrontFireBlue(object sender, Tuple<DateTime, double> e)
		{

		}
		protected virtual void OnFrontLostBlue(object sender, Tuple<DateTime, double> e)
		{

		}
		protected virtual void OnFrontFireRed(object sender, Tuple<DateTime, double> e)
		{

		}
		protected virtual void OnFrontLostRed(object sender, Tuple<DateTime, double> e)
		{

		}
		#endregion
		public override void Start()
        {
            base.Start();
            this.StartAvailable();
            _server.GetBlackList((fc, ex) =>
            {
                if (ex == null && fc != null)
                {
                    lock (BlackNumbers)
                    {
                        BlackNumbers.Clear();
                        BlackNumbers.AddRange(fc);
                        CurtentBlackNum = "";
                    }
                }
            });
            //if (!string.IsNullOrEmpty(Section.BarrierIpController))
				//CurrentListBarrierIp.StartHandButtonClick(Section.BarrierIpController, Section.BarrierPortController, GreenHandButtonClicked);
				//this.Section.StartCardReader(ReadingCompleted, TakingOffCompleted);
			startTracker();
			this.Section.StartCameras(_laneDirection);

            if (this.Section.ModWinsCards != null)
            {
                var nomalCards = this.Section.ModWinsCards.Where(c => c.UsageAsTheSameFarCard == false).ToList();
                var farCards = this.Section.ModWinsCards.Where(c => c.UsageAsTheSameFarCard == true).ToList();
				if (nomalCards.Count > 0)
                    CurrentListCardReader.StartGreenCardReader(nomalCards, GreenReadingCompleted, GreenTakingOffCompleted);
                if (farCards.Count > 0)
					CurrentListCardReader.StartGreenCardReader(farCards, FarCardReadingCompleted, FarCardTakingOffCompleted);
			}
            //if (this.Section.TcpIpServerCards != null)
            //{
            //    _server.GetFarCards((fc, ex) =>
            //    {
            //        if (ex == null && fc != null)
            //        {
            //            FarCards.Clear();
            //            FarCards.AddRange(fc);
            //            CurrentListCardReader.StartGreenCardReader(this.Section.TcpIpServerCards, GreenReadingCompleted, GreenTakingOffCompleted);
            //            ResetWaitingCardReader("Server");
            //        }
            //    });

            //}
            //if (this.Section.ScannelCards != null)
            //{
            //    _server.GetFarCards((fc, ex) =>
            //    {
            //        if (ex == null && fc != null)
            //        {
            //            FarCards.Clear();
            //            FarCards.AddRange(fc);
            //            CurrentListCardReader.StartGreenCardReader(this.Section.ScannelCards, GreenReadingCompleted, GreenTakingOffCompleted);
            //            ResetWaitingCardReader("Scannel");
            //        }
            //    });
            //}
            if (this.Section.TcpIpClientCards != null)
            {
				var nomalCards = this.Section.TcpIpClientCards.Where(c => c.UsageAsTheSameFarCard == false).ToList();
				var farCards = this.Section.TcpIpClientCards.Where(c => c.UsageAsTheSameFarCard == true).ToList();
				if (nomalCards.Count > 0)
					CurrentListCardReader.StartGreenCardReader(nomalCards, GreenReadingCompleted, GreenTakingOffCompleted);
				if (farCards.Count > 0)
					CurrentListCardReader.StartGreenCardReader(farCards, FarCardReadingCompleted, FarCardTakingOffCompleted);
				
                //ResetWaitingCardReader("Client");
            }

            //if (this.Section.TcpIpRemodeCards != null)
            //{
            //    CurrentListCardReader.StartGreenCardReader(this.Section.TcpIpRemodeCards, GreenReadingCompleted, GreenTakingOffCompleted);
            //}
            
            //if (this.Section.TcpIpControllerCards != null)
            //{
            //    CurrentListCardReader.StartGreenCardReader(this.Section.TcpIpControllerCards, GreenReadingCompleted, GreenTakingOffCompleted);
            //}

            if (this.Section.NFCCards != null)
            {
                CurrentListCardReader.StartGreenCardReader(this.Section.NFCCards, GreenReadingCompleted, GreenTakingOffCompleted);
            }

            if (this.Section.ProxiesCards != null)
            {
				var nomalCards = this.Section.ProxiesCards.Where(c => c.UsageAsTheSameFarCard == false).ToList();
				var farCards = this.Section.ProxiesCards.Where(c => c.UsageAsTheSameFarCard == true).ToList();
				if (nomalCards.Count > 0)
					CurrentListCardReader.StartGreenCardReader(nomalCards, GreenReadingCompleted, GreenTakingOffCompleted);
				if (farCards.Count > 0)
					CurrentListCardReader.StartGreenCardReader(farCards, FarCardReadingCompleted, FarCardTakingOffCompleted);
            }

            if (this.Section.ZKFarCards != null)
            {
				var nomalCards = this.Section.ZKFarCards.Where(c => c.UsageAsTheSameFarCard == false).ToList();
				var farCards = this.Section.ZKFarCards.Where(c => c.UsageAsTheSameFarCard == true).ToList();
				if (nomalCards.Count > 0)
					CurrentListCardReader.StartGreenCardReader(nomalCards, GreenReadingCompleted, GreenTakingOffCompleted);
				if (farCards.Count > 0)
					CurrentListCardReader.StartGreenCardReader(farCards, FarCardReadingCompleted, FarCardTakingOffCompleted);
            }
            this.StartFarcardProcess();
        }
		public virtual void GreenReadingCompleted(object sender, GreenCardReaderEventArgs e)
		{
			if (e.ex != null)
			{
				//this.CheckedCard = new Card();
				return;
			}
			if (sender is TcpIpServerCardReader && !FarCards.Exists(fc => fc.CardId == e.CardID))
			{
				//this.CheckedCard = new Card();
				return;
			}
			if (e.CardReader != null && e.CardReader.Info.Type == "Scannel")
			{
				bool invalid = true;
				if (this.Section.ScannelCards != null)
				{
					var info = this.Section.ScannelCards.FirstOrDefault(inf => inf.TcpIp == e.CardReader.Info.TcpIp && inf.Port == e.CardReader.Info.Port && inf.Antenna == sender.ToString());
					if (info != null)
						invalid = false;
				}
				if (invalid)
					return;
			}
			if (sender.ToString() == "Tcp Ip Controller card" &&
			   (string.IsNullOrEmpty(this.Section.Door) || string.IsNullOrEmpty(this.Section.Reader) ||
				   e.Door != this.Section.Door || e.Reader != this.Section.Reader)
			   )
			{
				//this.CheckedCard = new Card();
				return;
			}

			if (_messenger.HasSubscriptionsFor<ClosePopupMessage>())
			{
				_messenger.Publish(new ClosePopupMessage(this));
			}
			this.CheckedCard = new Card(e.CardID);
			this.CheckedCard.TimeRide = e.TimeRide;
		}
		public virtual void GreenTakingOffCompleted(object sender, GreenCardReaderEventArgs e)
		{
			if (e.ex != null)
				return;
		}
		public virtual void GreenHandButtonClicked(object sender, GreenHandButtonEventArgs e)
        {
        }

		#region Process for Far Card
        protected readonly object _locker = new object();
        private List<AvailableCard> availableCards = new List<AvailableCard>();
		private List<DetectedTag> _tagBuffer = new List<DetectedTag>();
		protected Dictionary<string, DateTime> _processedTags = new Dictionary<string, DateTime>();
		private DispatcherTimer _processTimer;
		private DispatcherTimer _mapCardTimer;
        protected bool validating = false;
        protected (string, string , bool) CheckAvalable(string plate)
        {
            lock (_locker) { 
                var first = availableCards.FirstOrDefault(x=>x.VehicleNumber == plate);
                if (first != null)
                {
                    return (first.CardId, first.VehicleNumber, true);
                }
                else if(this.Section!=null && this.Section.OptionByLane!=null && this.Section.OptionByLane.CompareWithOnlySerialNumberOfPlate)
                {
                    var serial = ExtractVehicleNumber(plate);
                    var frst = availableCards.FirstOrDefault(x => ExtractVehicleNumber(x.VehicleNumber) == serial);
                    if (frst != null) {
						return (frst.CardId, frst.VehicleNumber, true);
					}
                    else
						return ("", "", false);
				}
				else    
                    return ("", "", false);
            }
        }
        protected bool AllowConfirmToCollectVisitor => this.Section != null && this.Section.OptionByLane != null && this.Section.OptionByLane.AllowConfirmToCollectVisitorPlate;

		protected virtual void ValidFarCardHandle(DetectedTag tag, string extention)
        {
			validating = true;
        }
        private void StartAvailable()
        {
			mapAvailable();
            if(_mapCardTimer == null)
            {
				_mapCardTimer = new DispatcherTimer
				{
					Interval = TimeSpan.FromSeconds(20)
				};
			}
			_mapCardTimer.Tick -= mapAvailableTick;
			_mapCardTimer.Tick += mapAvailableTick;
			_mapCardTimer.Start();
		}
        private void StopAvailable()
        {
            if(_mapCardTimer != null)
            {
				_mapCardTimer.Tick -= mapAvailableTick;
                _mapCardTimer.Stop();
			}
        }
		private void StartFarcardProcess()
        {
            //Trường hợp không cấu hình cách dùng thẻ tầm xa thì bỏ qua
            if (_settings == null || _settings.FarCardUsageRules == null)
                return;
   //         if (_settings.FarCardUsageRules.CheckWithAvalableCardFirst)
   //         {
   //             mapAvailable();
   //             _mapCardTimer = new DispatcherTimer
   //             {
   //                 Interval = TimeSpan.FromSeconds(20)
   //             };
   //             _mapCardTimer.Tick += mapAvailableTick;
   //             _mapCardTimer.Start();
			//}
			_processTimer = new DispatcherTimer
			{
				Interval = TimeSpan.FromMilliseconds(300)
			};
			_processTimer.Tick += ProcessBufferedTags;
			_processTimer.Start();
		}

		private void mapAvailableTick(object sender, EventArgs e)
		{
            mapAvailable();
		}

		private void mapAvailable()
		{
			_server.GetAvailableCards((res, ex) =>
			{
				if (ex == null)
				{
					lock (_locker)
					{
						availableCards = res;

					}
				}
			});
		}

		private void StopFarcardProcess()
        {
            _processTimer?.Stop();
            _mapCardTimer?.Stop();
        }

		private void CleanProcessedTags()
		{
			var now = DateTime.Now;
			var expiredKeys = _processedTags
				.Where(kvp => (now - kvp.Value).TotalSeconds > _settings.FarCardUsageRules.IgnoredDurationForProcessedTags) // giữ tối đa 30s
				.Select(kvp => kvp.Key)
				.ToList(); // tránh sửa khi đang lặp

			foreach (var key in expiredKeys)
			{
				_processedTags.Remove(key);
			}
		}

		private void ProcessBufferedTags(object sender, EventArgs e)
		{
            if (validating)
                return;
			DetectedTag selected = null;
            List<string> tags = new List<string>();
			lock (_locker)
			{
				CleanProcessedTags();
				var now = DateTime.Now;
                var groupedList = _tagBuffer
                    .GroupBy(t => t.EPC)
                    .Select(g => new
                    {
                        EPC = g.Key,
                        Count = g.Count(),
                        FirstSeen = g.Min(t => t.DetectedAt),
                        LastSeen = g.Max(t => t.DetectedAt),
                        AvgRssi = g.Average(t => t.Rssi ?? 0)
                    })
                    .Where(g =>
                        g.Count >= 3 &&
                        (now - g.LastSeen).TotalSeconds <= 1 &&
                        (g.LastSeen - g.FirstSeen).TotalSeconds <= 3)
                    .OrderByDescending(g => g.Count)
                    .ThenByDescending(g => g.AvgRssi);
                //.FirstOrDefault();
                var grouped = groupedList.FirstOrDefault();
				
				if (grouped != null && (!_processedTags.ContainsKey(grouped.EPC) ||
					(now - _processedTags[grouped.EPC]).TotalSeconds > _settings.FarCardUsageRules.IgnoredDurationForProcessedTags))
				{
					selected = new DetectedTag
					{
						EPC = grouped.EPC,
						DetectedAt = now,
                        Avlaible = availableCards.FirstOrDefault(x => x.CardId == grouped.EPC) != null
					};
					tags = groupedList.Select(g => g.EPC).ToList();
				}
			}

			if (selected != null)
			{
                if (_settings.FarCardUsageRules.IgnoreWhenMultiResult && tags.Count > 1)
                    return;
                ValidFarCardHandle(selected, string.Join(",", tags));
			}
		}
		#endregion
		public virtual void FarCardReadingCompleted(object sender, GreenCardReaderEventArgs e)
        {
            if (e.ex != null)
            {
                //this.CheckedCard = new Card();
                return;
            }
            if (!string.IsNullOrEmpty(e.CardID))
            {
				lock (_locker)
				{
					_tagBuffer.Add(new DetectedTag
					{
						EPC = e.CardID,
						//Rssi = e.rssi,
						DetectedAt = DateTime.Now
					});

					// Giữ buffer cần thiết theo cấu hình
					_tagBuffer = _tagBuffer
						.Where(t => (DateTime.Now - t.DetectedAt).TotalSeconds <= _settings.FarCardUsageRules.CycleTimeForCheckResult + 3)
						.ToList();
				}
			}
            //if (sender is TcpIpServerCardReader && !FarCards.Exists(fc => fc.CardId == e.CardID))
            //{
            //    //this.CheckedCard = new Card();
            //    return;
            //}
            //if (e.CardReader != null && e.CardReader.Info.Type == "Scannel")
            //{
            //    bool invalid = true;
            //    if (this.Section.ScannelCards != null)
            //    {
            //        var info = this.Section.ScannelCards.FirstOrDefault(inf => inf.TcpIp == e.CardReader.Info.TcpIp && inf.Port == e.CardReader.Info.Port && inf.Antenna == sender.ToString());
            //        if (info != null)
            //            invalid = false;
            //    }
            //    if (invalid)
            //        return;
            //}
            //if (sender.ToString() == "Tcp Ip Controller card" &&
            //   (string.IsNullOrEmpty(this.Section.Door) || string.IsNullOrEmpty(this.Section.Reader) ||
            //       e.Door != this.Section.Door || e.Reader != this.Section.Reader)
            //   )
            //{
            //    //this.CheckedCard = new Card();
            //    return;
            //}

            //if (_messenger.HasSubscriptionsFor<ClosePopupMessage>())
            //{
            //    _messenger.Publish(new ClosePopupMessage(this));
            //}
            //this.CheckedCard = new Card(e.CardID);
            //this.CheckedCard.TimeRide = e.TimeRide;
        }
        public virtual void FarCardTakingOffCompleted(object sender, GreenCardReaderEventArgs e)
        {
            if (e.ex != null)
                return;
        }
        public virtual void TakingOffCompleted(object sender, CardReaderEventArgs e)
        {
            if (e.ex != null)
                return;
        }

        private MvxCommand _VehicleNumberAdd;
        public ICommand VehicleNumberAdd
        {
            get
            {
                _VehicleNumberAdd = _VehicleNumberAdd ?? new MvxCommand(() =>
                {
                    Adnumber();
                });

                return _VehicleNumberAdd;
            }
        }

        private MvxCommand _VehicleNumberAddout;
        public ICommand VehicleNumberAddout
        {
            get
            {
                _VehicleNumberAddout = _VehicleNumberAddout ?? new MvxCommand(() =>
                {
                    Adnumberout();
                });

                return _VehicleNumberAddout;
            }
        }
        public virtual void Adnumber() { }

        public virtual void Adnumberout() { }

        public virtual void DisplayNumber() { }

        private MvxCommand _DisplayNumber;
        public ICommand cmdDisplayNumber
        {
            get
            {
                _DisplayNumber = _DisplayNumber ?? new MvxCommand(() =>
                {
                    DisplayNumber();
                });

                return _DisplayNumber;
            }
        }
        public virtual void PrintToPrinter() { }

        private MvxCommand _commandPrintToPrinter;
        public ICommand CommandPrintToPrinter
        {
            get
            {
                _commandPrintToPrinter = _commandPrintToPrinter ?? new MvxCommand(() =>
                {
                    Task.Factory.StartNew(() => PrintToPrinter());
                    //PrintToPrinter();
                });

                return _commandPrintToPrinter;
            }
        }
        public virtual void ReadingCompleted(object sender, CardReaderEventArgs e)
        {
            //MessageToUser = null;

            if (_messenger.HasSubscriptionsFor<ClosePopupMessage>())
            {
                _messenger.Publish(new ClosePopupMessage(this));
            }

            if (e.ex != null)
                return;

            this.CheckedCard = new Card(e.CardID);
        }

        public void PublishCloseChildEvent(SectionPosition position)
        {
            if (_messenger.HasSubscriptionsFor<CloseChildMessage>())
            {
                _messenger.Publish(new CloseChildMessage(this, position));
            }
        }

        public void PublishChangeLaneDirectionEvent(SectionPosition position)
        {
            if (_messenger.HasSubscriptionsFor<ShowChildMessage>())
            {
                LaneDirection dir = _laneDirection == LaneDirection.In ? LaneDirection.Out : LaneDirection.In;
                Section.TemporaryDirection = dir;
                Section.ShouldBeDisplayed = true;
                //_laneDirection = dir;
                //_userPreferenceService.SystemSettings.ChangeLaneDirection(Section.Id, dir);
                //_userPreferenceService.SystemSettings.Save();

                _messenger.Publish(new ShowChildMessage(this, position, typeof(BaseLaneViewModel)));
            }
        }
        public void PublishChangeLaneEvent()
        {
            //Chỉ đổi làn khi sử dụng cho trường hợp 2 làn thực
            if(_userPreferenceService.HostSettings.ActualSections!=1 && _userPreferenceService.HostSettings.ActualSections != 2)
            {
				if (_messenger.HasSubscriptionsFor<ShowChildMessage>())
				{
					if ((this.Section.DisplayedPosition == DisplayedPosition.Right && _userPreferenceService.OptionsSettings.IsVitualLaneRight)
						|| (this.Section.DisplayedPosition == DisplayedPosition.Left && _userPreferenceService.OptionsSettings.IsVitualLaneLeft))
					{
						List<Section> secs = _userPreferenceService.SystemSettings.GetAllSections(this.Section.DisplayedPosition);
						if (secs.Count < 2)
							return;

						PublishCloseChildEvent(this.Section.Id);

						int idx = secs.FindIndex(s => s.Id == this.Section.Id);
						int idxx = (idx + 1) % secs.Count;
						this.Section.ShouldBeDisplayed = false;
						secs[idxx].ShouldBeDisplayed = true;

						_messenger.Publish(new ShowChildMessage(this, secs[idxx].Id, typeof(BaseLaneViewModel)));
						Close();
					}
					else
						return;
				}
			}  
        }
		public void PublishShowEndingShiftView(SectionPosition position)
        {
            if (_messenger.HasSubscriptionsFor<ShowChildMessage>())
            {
                _messenger.Publish(new ShowChildMessage(this, position, typeof(EndingShiftInformationViewModel)));
            }
        }

        public void PublishShowSearchEvent(SectionPosition position)
        {
            if (_messenger.HasSubscriptionsFor<ShowChildMessage>())
            {
                _messenger.Publish(new ShowChildMessage(this, position, typeof(SearchViewModel)));
            }
        }

        public void PublishShowLoginView(SectionPosition position)
        {
            if (_messenger.HasSubscriptionsFor<ShowChildMessage>())
            {
                _messenger.Publish(new ShowChildMessage(this, position, typeof(LoginViewModel)));
            }
        }

        protected string ExtractVehicleNumber(string rawNumber)
        {
            return ANPRService.ExtractVehicleNumber(rawNumber);
        }

        protected string ExtractPrefixVehicleNumber(string rawNumber, string vehicleNumber)
        {
            return ANPRService.ExtractPrefixVehicleNumber(rawNumber, vehicleNumber);
        }

        protected virtual void OnKeyPressed(KeyPressedMessage msg)
        {


        }


        public virtual void ChooseVehicleType(VehicleType type)
        {

        }

        protected virtual void OnClosePopup(ClosePopupMessage msg)
        {
            this.MessageToUser = null;
        }

        public void HandleError(string icon, string err, bool showUp, bool isAppend)
        {
            InvokeOnMainThread(() =>
            {
                if (!isAppend)
                    this.Notices.Clear();
                
                this.Notices.Add(new NoticeToUser(icon, err));
                if (showUp)
                {
                    if (this.Notices.Where(n => n.Icon.Equals(IconEnums.Guide)).FirstOrDefault() != null)
                        this.Notices.TimeOut = INFINITIVE;
                    else if (this.Notices.TimeOut <= DEFAULT_NOTICE_TIMEOUT)
                        this.Notices.TimeOut = DEFAULT_NOTICE_TIMEOUT;
                    else
                        this.Notices.TimeOut = DEFAULT_NOTICE_TIMEOUT;
                }

                RaisePropertyChanged(() => Notices);
            });
        }

        public void HandleError(string icon, string err, bool isAppend)
        {
            InvokeOnMainThread(() =>
            {
                if (isAppend)
                {
                    this.Notices.Add(new NoticeToUser(icon, err));
                }
                else
                {
                    this.Notices.Add(new NoticeToUser(icon, err));

                    if (this.Notices.Where(n => n.Icon.Equals(IconEnums.Guide)).FirstOrDefault() != null)
                    {
                        this.Notices.TimeOut = INFINITIVE;
                    }
                    else if (this.Notices.TimeOut <= DEFAULT_NOTICE_TIMEOUT)
                        this.Notices.TimeOut = DEFAULT_NOTICE_TIMEOUT;
                    else
                        this.Notices.TimeOut = DEFAULT_NOTICE_TIMEOUT;

                    RaisePropertyChanged(() => Notices);
                }
            });
        }


        public void ReleaseResource()
        {
            this.StopAvailable();
            this.stopTracker();
            this.StopFarcardProcess();
            //this.Section.StopCardReader(ReadingCompleted, TakingOffCompleted);

            //if (this is CheckInLaneViewModel)
            if (!string.IsNullOrEmpty(Section.BarrierIpController))
                CurrentListBarrierIp.StoptHandButtonClick(Section.BarrierIpController, Section.BarrierPortController, GreenHandButtonClicked);
            if (_laneDirection == LaneDirection.In)
                this.Section.StopDevices(LaneDirection.In);
            //else if(this is CheckOutLaneViewModel)
            else if (_laneDirection == LaneDirection.Out)
                this.Section.StopDevices(LaneDirection.Out);
            if (this.Section.ModWinsCards != null)
            {
				var nomalCards = this.Section.ModWinsCards.Where(c => c.UsageAsTheSameFarCard == false).ToList();
				var farCards = this.Section.ModWinsCards.Where(c => c.UsageAsTheSameFarCard == true).ToList();
				if (nomalCards.Count > 0)
					CurrentListCardReader.StoptGreenCardReader(nomalCards, GreenReadingCompleted, GreenTakingOffCompleted);
                if(farCards.Count > 0)
                {
					CurrentListCardReader.StoptGreenCardReader(farCards, FarCardReadingCompleted, FarCardTakingOffCompleted);
				}
            }

            if (this.Section.TcpIpServerCards != null)
            {
				var nomalCards = this.Section.TcpIpServerCards.Where(c => c.UsageAsTheSameFarCard == false).ToList();
				var farCards = this.Section.TcpIpServerCards.Where(c => c.UsageAsTheSameFarCard == true).ToList();
				if (nomalCards.Count > 0)
					CurrentListCardReader.StoptGreenCardReader(nomalCards, GreenReadingCompleted, GreenTakingOffCompleted);
				if (farCards.Count > 0)
				{
					CurrentListCardReader.StoptGreenCardReader(farCards, FarCardReadingCompleted, FarCardTakingOffCompleted);
				}
				
            }

            if (this.Section.TcpIpClientCards != null)
            {
				var nomalCards = this.Section.TcpIpClientCards.Where(c => c.UsageAsTheSameFarCard == false).ToList();
				var farCards = this.Section.TcpIpClientCards.Where(c => c.UsageAsTheSameFarCard == true).ToList();
				if (nomalCards.Count > 0)
					CurrentListCardReader.StoptGreenCardReader(nomalCards, GreenReadingCompleted, GreenTakingOffCompleted);
				if (farCards.Count > 0)
				{
					CurrentListCardReader.StoptGreenCardReader(farCards, FarCardReadingCompleted, FarCardTakingOffCompleted);
				}	
            }

            if (this.Section.ScannelCards != null)
            {
				var nomalCards = this.Section.ScannelCards.Where(c => c.UsageAsTheSameFarCard == false).ToList();
				var farCards = this.Section.ScannelCards.Where(c => c.UsageAsTheSameFarCard == true).ToList();
				if (nomalCards.Count > 0)
					CurrentListCardReader.StoptGreenCardReader(nomalCards, GreenReadingCompleted, GreenTakingOffCompleted);
				if (farCards.Count > 0)
				{
					CurrentListCardReader.StoptGreenCardReader(farCards, FarCardReadingCompleted, FarCardTakingOffCompleted);
				}
				
            }

            if (this.Section.TcpIpRemodeCards != null)
            {
				var nomalCards = this.Section.TcpIpRemodeCards.Where(c => c.UsageAsTheSameFarCard == false).ToList();
				var farCards = this.Section.TcpIpRemodeCards.Where(c => c.UsageAsTheSameFarCard == true).ToList();
				if (nomalCards.Count > 0)
					CurrentListCardReader.StoptGreenCardReader(nomalCards, GreenReadingCompleted, GreenTakingOffCompleted);
				if (farCards.Count > 0)
				{
					CurrentListCardReader.StoptGreenCardReader(farCards, FarCardReadingCompleted, FarCardTakingOffCompleted);
				}
				
            }

            if (this.Section.TcpIpControllerCards != null)
            {
				var nomalCards = this.Section.TcpIpControllerCards.Where(c => c.UsageAsTheSameFarCard == false).ToList();
				var farCards = this.Section.TcpIpControllerCards.Where(c => c.UsageAsTheSameFarCard == true).ToList();
				if (nomalCards.Count > 0)
					CurrentListCardReader.StoptGreenCardReader(nomalCards, GreenReadingCompleted, GreenTakingOffCompleted);
				if (farCards.Count > 0)
				{
					CurrentListCardReader.StoptGreenCardReader(farCards, FarCardReadingCompleted, FarCardTakingOffCompleted);
				}
				
            }

            if (this.Section.NFCCards != null)
            {
                CurrentListCardReader.StoptGreenCardReader(this.Section.NFCCards, GreenReadingCompleted, GreenTakingOffCompleted);
            }

            if (this.Section.ProxiesCards != null)
            {
				var nomalCards = this.Section.ProxiesCards.Where(c => c.UsageAsTheSameFarCard == false).ToList();
				var farCards = this.Section.ProxiesCards.Where(c => c.UsageAsTheSameFarCard == true).ToList();
				if (nomalCards.Count > 0)
					CurrentListCardReader.StoptGreenCardReader(nomalCards, GreenReadingCompleted, GreenTakingOffCompleted);
				if (farCards.Count > 0)
				{
					CurrentListCardReader.StoptGreenCardReader(farCards, FarCardReadingCompleted, FarCardTakingOffCompleted);
				}
				
            }
			
            if (this.Section.ZKFarCards != null)
			{
				var nomalCards = this.Section.ZKFarCards.Where(c => c.UsageAsTheSameFarCard == false).ToList();
				var farCards = this.Section.ZKFarCards.Where(c => c.UsageAsTheSameFarCard == true).ToList();
				if (nomalCards.Count > 0)
					CurrentListCardReader.StoptGreenCardReader(nomalCards, GreenReadingCompleted, GreenTakingOffCompleted);
				if (farCards.Count > 0)
				{
					CurrentListCardReader.StoptGreenCardReader(farCards, FarCardReadingCompleted, FarCardTakingOffCompleted);
				}

			}
		}

        public override void Unloaded()
        {
            base.Unloaded();
            ReleaseResource();

            Mvx.Resolve<IMvxMessenger>().Unsubscribe<KeyPressedMessage>(_keyPressedToken);
            _keyPressedToken = null;
        }
        private void CloseEvent()
        {
			this.StopFarcardProcess();
			if (this.Section != null)
            {
				if (this.Section.ModWinsCards != null)
				{
					var nomalCards = this.Section.ModWinsCards.Where(c => c.UsageAsTheSameFarCard == false).ToList();
					var farCards = this.Section.ModWinsCards.Where(c => c.UsageAsTheSameFarCard == true).ToList();
					if (nomalCards.Count > 0)
						CurrentListCardReader.StoptGreenCardReader(nomalCards, GreenReadingCompleted, GreenTakingOffCompleted);
					if (farCards.Count > 0)
					{
						CurrentListCardReader.StoptGreenCardReader(farCards, FarCardReadingCompleted, FarCardTakingOffCompleted);
					}
				}

				if (this.Section.TcpIpServerCards != null)
				{
					var nomalCards = this.Section.TcpIpServerCards.Where(c => c.UsageAsTheSameFarCard == false).ToList();
					var farCards = this.Section.TcpIpServerCards.Where(c => c.UsageAsTheSameFarCard == true).ToList();
					if (nomalCards.Count > 0)
						CurrentListCardReader.StoptGreenCardReader(nomalCards, GreenReadingCompleted, GreenTakingOffCompleted);
					if (farCards.Count > 0)
					{
						CurrentListCardReader.StoptGreenCardReader(farCards, FarCardReadingCompleted, FarCardTakingOffCompleted);
					}

				}

				if (this.Section.TcpIpClientCards != null)
				{
					var nomalCards = this.Section.TcpIpClientCards.Where(c => c.UsageAsTheSameFarCard == false).ToList();
					var farCards = this.Section.TcpIpClientCards.Where(c => c.UsageAsTheSameFarCard == true).ToList();
					if (nomalCards.Count > 0)
						CurrentListCardReader.StoptGreenCardReader(nomalCards, GreenReadingCompleted, GreenTakingOffCompleted);
					if (farCards.Count > 0)
					{
						CurrentListCardReader.StoptGreenCardReader(farCards, FarCardReadingCompleted, FarCardTakingOffCompleted);
					}
				}

				if (this.Section.ScannelCards != null)
				{
					var nomalCards = this.Section.ScannelCards.Where(c => c.UsageAsTheSameFarCard == false).ToList();
					var farCards = this.Section.ScannelCards.Where(c => c.UsageAsTheSameFarCard == true).ToList();
					if (nomalCards.Count > 0)
						CurrentListCardReader.StoptGreenCardReader(nomalCards, GreenReadingCompleted, GreenTakingOffCompleted);
					if (farCards.Count > 0)
					{
						CurrentListCardReader.StoptGreenCardReader(farCards, FarCardReadingCompleted, FarCardTakingOffCompleted);
					}

				}

				if (this.Section.TcpIpRemodeCards != null)
				{
					var nomalCards = this.Section.TcpIpRemodeCards.Where(c => c.UsageAsTheSameFarCard == false).ToList();
					var farCards = this.Section.TcpIpRemodeCards.Where(c => c.UsageAsTheSameFarCard == true).ToList();
					if (nomalCards.Count > 0)
						CurrentListCardReader.StoptGreenCardReader(nomalCards, GreenReadingCompleted, GreenTakingOffCompleted);
					if (farCards.Count > 0)
					{
						CurrentListCardReader.StoptGreenCardReader(farCards, FarCardReadingCompleted, FarCardTakingOffCompleted);
					}

				}

				if (this.Section.TcpIpControllerCards != null)
				{
					var nomalCards = this.Section.TcpIpControllerCards.Where(c => c.UsageAsTheSameFarCard == false).ToList();
					var farCards = this.Section.TcpIpControllerCards.Where(c => c.UsageAsTheSameFarCard == true).ToList();
					if (nomalCards.Count > 0)
						CurrentListCardReader.StoptGreenCardReader(nomalCards, GreenReadingCompleted, GreenTakingOffCompleted);
					if (farCards.Count > 0)
					{
						CurrentListCardReader.StoptGreenCardReader(farCards, FarCardReadingCompleted, FarCardTakingOffCompleted);
					}

				}

				if (this.Section.NFCCards != null)
				{
					CurrentListCardReader.StoptGreenCardReader(this.Section.NFCCards, GreenReadingCompleted, GreenTakingOffCompleted);
				}

				if (this.Section.ProxiesCards != null)
				{
					var nomalCards = this.Section.ProxiesCards.Where(c => c.UsageAsTheSameFarCard == false).ToList();
					var farCards = this.Section.ProxiesCards.Where(c => c.UsageAsTheSameFarCard == true).ToList();
					if (nomalCards.Count > 0)
						CurrentListCardReader.StoptGreenCardReader(nomalCards, GreenReadingCompleted, GreenTakingOffCompleted);
					if (farCards.Count > 0)
					{
						CurrentListCardReader.StoptGreenCardReader(farCards, FarCardReadingCompleted, FarCardTakingOffCompleted);
					}

				}

				if (this.Section.ZKFarCards != null)
				{
					var nomalCards = this.Section.ZKFarCards.Where(c => c.UsageAsTheSameFarCard == false).ToList();
					var farCards = this.Section.ZKFarCards.Where(c => c.UsageAsTheSameFarCard == true).ToList();
					if (nomalCards.Count > 0)
						CurrentListCardReader.StoptGreenCardReader(nomalCards, GreenReadingCompleted, GreenTakingOffCompleted);
					if (farCards.Count > 0)
					{
						CurrentListCardReader.StoptGreenCardReader(farCards, FarCardReadingCompleted, FarCardTakingOffCompleted);
					}

				}
			}
        }
        public override void Close()
        {
            CloseEvent();
            base.Close();

            _showChooseVehicleType = false;
        }

        private MvxCommand _changeLaneDirectionCommand;
        public ICommand ChangeLaneDirectionCommand
        {
            get
            {
                if (!IsBusy)
                    _changeLaneDirectionCommand = _changeLaneDirectionCommand ?? new MvxCommand(() =>
                    {
                        //2024 disible changelane

                        IsBusy = true;
                        PublishCloseChildEvent(this.Section.Id);
                        PublishChangeLaneDirectionEvent(this.Section.Id);
                        //ResetCardReader("Server");
                        //ResetCardReader("Client");
                        Close();

                        IsBusy = false;
                    });
                return _changeLaneDirectionCommand;
            }
        }

        private MvxCommand _changeLaneCommand;
        public ICommand ChangeLaneCommand
        {
            get
            {
                if (!IsBusy)
                    _changeLaneCommand = _changeLaneCommand ?? new MvxCommand(() =>
                    {
                        IsBusy = true;
                        PublishChangeLaneEvent();
                        //ResetCardReader("Server");
                        //ResetCardReader("Client");
                        IsBusy = false;
                    });
                return _changeLaneCommand;
            }
        }

        private MvxCommand _showShiftReportCommand;
        public ICommand ShowShiftReportCommand
        {
            get
            {
                _showShiftReportCommand = _showShiftReportCommand ?? new MvxCommand(() =>
                {
                    //this.Section.ApmsUser = null;
                    PublishCloseChildEvent(this.Section.Id);
                    PublishShowEndingShiftView(this.Section.Id);
                });

                return _showShiftReportCommand;
            }
        }

        private MvxCommand _confirmLogoutCommand;
        public ICommand ConfirmLogoutCommand
        {
            get
            {
                _confirmLogoutCommand = _confirmLogoutCommand ?? new MvxCommand(() =>
                {
                    ShowConfirmLogout();
                });
                return _confirmLogoutCommand;
            }
        }
        private MvxCommand _changeSessionConfig;
        public ICommand ChangeSessionConfig
        {
            get
            {
                _changeSessionConfig = _changeSessionConfig ?? new MvxCommand(() =>
                {
                    this.Section.savecamconfig();
                    HandleError(IconEnums.Check, "SAVE CAMERA CONFIG SUCCESS", true, false);
                    RefreshDisplay();
                });
                return _changeSessionConfig;
            }
        }
        private void RefreshDisplay()
        {
            List<Section> secs = _userPreferenceService.SystemSettings.GetAllSections(this.Section.DisplayedPosition);
            foreach (var s in secs)
            {
                if (s.Id != this.Section.Id)
                    s.ShouldBeDisplayed = false;
                else
                    s.ShouldBeDisplayed = true;
            }
        }

        MvxCommand _showSearchCommand;
        public ICommand ShowSearchCommand
        {
            get
            {
                if (!IsBusy)
                    _showSearchCommand = _showSearchCommand ?? new MvxCommand(() =>
                    {
                        IsBusy = true;
                        PublishCloseChildEvent(this.Section.Id);
                        PublishShowSearchEvent(this.Section.Id);
                        //ResetCardReader("Server");
                        //ResetCardReader("Client");
                        IsBusy = false;
                    });
                return _showSearchCommand;
            }
        }

        private DispatcherTimer _prolificCardReaderTimer = null;
        private int _prolificCardReaderTimerCounter;
        private bool _prolificCardReaderOn;
        public bool ProlificCardReaderOn
        {
            get { return _prolificCardReaderOn; }
            set
            {
                _prolificCardReaderOn = value;
                RaisePropertyChanged(() => ProlificCardReaderOn);

                if (value)
                {
                    if (_prolificCardReaderTimer == null)
                    {
                        _prolificCardReaderTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                        _prolificCardReaderTimer.Tick += (sender, args) =>
                        {
                            if (--_prolificCardReaderTimerCounter <= 0)
                                ProlificCardReaderOn = false;
                        };
                    }
                    _prolificCardReaderTimerCounter =
                        _userPreferenceService.OptionsSettings.WaitingProlificCardReaderDuration;
                    _prolificCardReaderTimer.Start();
                }
                else
                {
                    if (_prolificCardReaderTimer != null)
                        _prolificCardReaderTimer.Stop();
                }
            }
        }
        private void ResetCardReader(string type)
        {
            switch (type)
            {
                case "Server":
                    lock (this.Section.TcpIpServerCards)
                    {
                        if (this.Section.TcpIpServerCards != null)
                        {
                            foreach (var item in this.Section.TcpIpServerCards)
                            {
                                item.IsReset = true;
                            }
                        }
                    }
                    break;
                case "Client":
                    lock (this.Section.TcpIpClientCards)
                    {
                        if (this.Section.TcpIpClientCards != null)
                        {
                            foreach (var item in this.Section.TcpIpClientCards)
                            {
                                item.IsReset = true;
                            }
                        }
                    }
                    break;
                case "Scannel":
                    lock (this.Section.ScannelCards)
                    {
                        if (this.Section.ScannelCards != null)
                        {
                            foreach (var item in this.Section.ScannelCards)
                            {
                                item.IsReset = true;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private MvxCommand _toggleProlificCardReader;
        public ICommand ToggleProlificCardReader
        {
            get
            {
                return _toggleProlificCardReader ??
                       (_toggleProlificCardReader = new MvxCommand(() =>
                       {
                           ProlificCardReaderOn = !ProlificCardReaderOn;
                           //ResetCardReader("Server");
                       }));
            }
        }
        private MvxCommand _resetSoyalCardReader;
        public ICommand ResetSoyalCardReader
        {
            get
            {
                return _resetSoyalCardReader ??
                       (_resetSoyalCardReader = new MvxCommand(() =>
                       {
                           //ResetCardReader("Client");
                           ;
                       }));
            }
        }

        #region Test Properties and Method
        protected void GetMockCardIds()
        {

        }

        protected void GetARandomCardIds()
        {

        }
        #endregion

        protected void PrintLog<T>(Exception exception, string logServer = null, bool captureScreen = false) where T : class
        {
            _logService.Log(exception, logServer, null, 0, null, captureScreen);
        }

        public void ShowConfirmLogout()
        {
            this.MessageToUser = new MessageToUser(GetText("logout.title"), GetText("logout.message"), GetText("logout.ok"), () => { /*ShowShiftReportCommand.Execute(null); */
                PublishCloseChildEvent(Section.Id);
                PublishShowLoginView(Section.Id);
                return true; }, GetText("logout.cancel"), () => false);
        }

        public void ChangeLane()
        {

        }
    }
}