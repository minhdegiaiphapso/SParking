using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.ViewModels;
using SP.Parking.Terminal.Core.Models;
using SP.Parking.Terminal.Core.Services;
using SP.Parking.Terminal.Core.Utilities;
using Green.Devices.Dal;
using Green.Devices.Dal.CardControler;
using Green.Devices.Dal.Siemens;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Linq.Expressions;

namespace SP.Parking.Terminal.Core.ViewModels
{

    public class CountdownArgs : EventArgs
    {
        public int Value { get; set; }
    }

    public class EditableTimer
    {
        private int BOOMB_DURATION = 15;
        private int _boomb;
        
        public DispatcherTimer CountDownTimer { get; set; }

        public string RecentCheckedInCardId = string.Empty;
        public string CurrentCardId = string.Empty;

        public event EventHandler TimeRunningOut;
        public event EventHandler Countdown;

        public EditableTimer()
        {
            CountDownTimer = new DispatcherTimer();
            CountDownTimer.Interval = TimeSpan.FromSeconds(1);
            CountDownTimer.Tick += timer_Tick;
        }

        public void SetInterval(double Interval)
        {
            CountDownTimer.Interval = TimeSpan.FromSeconds(Interval);
        }
        public EditableTimer(int duration)
            : this()
        {
            BOOMB_DURATION = duration;
            _boomb = BOOMB_DURATION;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            _boomb -= 1;
            var handle = Countdown;
            if (handle != null)
                handle(sender, new CountdownArgs { Value = _boomb });

            if (_boomb <= 0)
            {
                CountDownTimer.Stop();
                Stop();
                EditableTimer_Elapsed(null, null);
            }
        }

        void EditableTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var handle = TimeRunningOut;

            if (handle != null)
                handle(sender, e);
        }

        public bool Enabled
        {
            get { return CountDownTimer.IsEnabled; }
        }

        public void Start(string cardId)
        {
            CurrentCardId = cardId;

            if (!RecentCheckedInCardId.Equals(CurrentCardId))
            {
                Restart();
                RecentCheckedInCardId = CurrentCardId;
            }
        }

        public void Restart()
        {
            _boomb = BOOMB_DURATION;
            CountDownTimer.Stop();
            CountDownTimer.Start();
        }

        public void Stop()
        {
            CountDownTimer.Stop();

            RecentCheckedInCardId = string.Empty;
            CurrentCardId = string.Empty;
        }
    }

    public class CheckInLaneViewModel : BaseLaneViewModel
    {
        private const int AUTO_CHECKIN_DURATION = 5000;
        private bool _allowupdate = false;

        EditableTimer _editTimer = null;
        System.Timers.Timer _autoCheckInTimer = null;
        string _previousRawVehicleNumber = string.Empty;
        private const string DEFAULT_VEHICLE_NUMBER = "????";
        private string _previousCheckInVehicleNumber = DEFAULT_VEHICLE_NUMBER;
        private IOptionsSettings _settings;
		public System.Action<string> MainualPlateDoing { get; set; }
		private bool _allowCheckIn = true;
        private bool _flagCheckIn = true;
        private Dictionary<string, int> _notExistedCards = new Dictionary<string, int>();

        private CheckIn _checkInResult;
        bool _hasConfirmChecking = false;

        MediaPlayer _player = new MediaPlayer();
        Uri _uri = new Uri(@"Sounds/checkin.success.mp3", UriKind.Relative);
        public event EventHandler ShowAddRegisteredCardWindow;

        CheckIn _checkInData;
        public CheckIn CheckInData
        {
            get { return _checkInData; }
            set
            {
                if (_checkInData == value) return;

                _checkInData = value;
                if (_checkInData == null)
                {
                    CardType = null;
                }
                RaisePropertyChanged(() => CheckInData);
            }
        }
        CardType _cardType;
        public CardType CardType
        {
            get { return _cardType; }
            set
            {
                if (_cardType == value) return;
                _cardType = value;
                RaisePropertyChanged(() => CardType);
            }
        }

        private string _countdown;
        public string Countdown
        {
            get { return _countdown; }
            set
            {
                _countdown = value;
                RaisePropertyChanged(() => Countdown);
            }
        }
        public System.Windows.Visibility CanShowPrinter
        {
            get
            {
                if (_settings.IsPrintV2)
                    return System.Windows.Visibility.Visible;
                else
                    return System.Windows.Visibility.Hidden;
            }
        }
        private bool _showCountdown;
        public bool ShowCountdown
        {
            get { return _showCountdown; }
            set
            {
                _showCountdown = value;
                RaisePropertyChanged(() => ShowCountdown);
                RaisePropertyChanged(() => ShowCountdown);
            }
        }

        private bool _checkCameraDoNotWork = false;
        public bool CheckCameraDoNotWork
        {
            get { return _checkCameraDoNotWork; }
            set
            {
                _checkCameraDoNotWork = value;
                RaisePropertyChanged(() => CheckCameraDoNotWork);
            }
        }

        private string _manualAutoVehicleNumberCheck;
        public string ManualAutoVehicleNumberCheck
        {
            get { return _manualAutoVehicleNumberCheck; }
            set
            {
                _manualAutoVehicleNumberCheck = value;
                RaisePropertyChanged(() => ManualAutoVehicleNumberCheck);
            }
        }

        private bool _allowManualInputVehicleNumber = false;
        public bool AllowManualInputVehicleNumber
        {
            get { return _allowManualInputVehicleNumber; }
            set
            {
                _allowManualInputVehicleNumber = value;
                RaisePropertyChanged(() => AllowManualInputVehicleNumber);
            }
        }

        MvxCommand<string> _autoCheckInCommand;
        public ICommand AutoCheckInCommand
        {
            get
            {
                _autoCheckInCommand = _autoCheckInCommand ?? new MvxCommand<string>((manualVehicleNumber) =>
                {
                    AutoCheckIn(manualVehicleNumber);
                });
                return _autoCheckInCommand;
            }
        }

        public CheckInLaneViewModel(IViewModelServiceLocator service, IStorageService storageService, IMvxMessenger messenger)
            : base(service, storageService, messenger)
        {
           
            _settings = Mvx.Resolve<IOptionsSettings>();
            _editTimer = new EditableTimer(_userPreferenceService.OptionsSettings.WaitingCheckDuration);
            _editTimer.TimeRunningOut += (sender, e) =>
            {
                if (!this._allowCheckIn && _hasConfirmChecking)
                {
                    CancelCheckIn();
                    _hasConfirmChecking = false;
                }
                //this.CheckInData = null;
                ResetUIInformation();
                this.Notices.Clear();
                Notices = null;
                _autoCheckInTimer.Stop();

            };
            _editTimer.Countdown += (sender, e) =>
            {
                int cd = (e as CountdownArgs).Value;
                if (cd <= 0)
                {
                    Countdown = string.Empty;
                    ShowCountdown = false;
                }
                else
                {
                    Countdown = cd.ToString();
                    ShowCountdown = true;
                }
            };

            _autoCheckInTimer = new System.Timers.Timer();
            _autoCheckInTimer.Interval = AUTO_CHECKIN_DURATION;
            _autoCheckInTimer.Elapsed += (sender, e) =>
            {
                //_allowCheckIn = false;
                if (!_allowCheckIn)
                {
                    return;
                }
                if (this.CheckInData != null && _editTimer.Enabled && this.CheckInData.CardId.Equals(_editTimer.CurrentCardId))
                {
                    UpdateCheckIn(this.CheckInData.GetClone, (result, ex) =>
                    {
                        InvokeOnMainThread(() =>
                        {
                            if (ex != null)
                            {
                                //this.Notices.Clear();in

                                if (ex is InternalServerErrorException)
                                {
                                    HandleError(IconEnums.Error, GetText("checkin.something_wrong_update"), true, false);
                                    PrintLog<CheckInLaneViewModel>(ex, _userPreferenceService.HostSettings.LogServerIP);
                                }

                                else PrintLog<CheckInLaneViewModel>(ex);
                            }
                            else
                            {
                                if (result != null && result.VehicleNumberExists && !string.IsNullOrEmpty(result.AlprVehicleNumber))
                                {
                                    HandleError(IconEnums.Warning, GetText("checkin.same_vehicle_number"), true, false);
                                }
                                else if (result != null && (!result.VehicleNumberExists || string.IsNullOrEmpty(result.VehicleNumber)))
                                    HandleError(IconEnums.update_ok, GetText("checkin.update_success"), false, false);
                            }

                            _autoCheckInTimer.Stop();
                        });
                    });
                }
            };
			_mTimer = new DispatcherTimer();
			_mTimer.Interval = TimeSpan.FromSeconds(15);
			_mTimer.Tick += _mTimer_Tick;
		}

		#region Maintainace Recognition
		DispatcherTimer _mTimer;
		bool isCar = false;
		byte[] carImg;
		byte[] bikeImg;
        DateTime LastRecognitPlate = DateTime.Now;
        bool doAnpr = false;
		private void StartMaintainace()
		{
			var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestImgs");
			var carPath = Path.Combine(path, "car.jpg");
			var bikePath = Path.Combine(path, "bike.jpg");
			if (File.Exists(carPath) && File.Exists(bikePath))
			{
				carImg = File.ReadAllBytes(carPath);
				bikeImg = File.ReadAllBytes(bikePath);
				_mTimer.Start();
			}
		}
		private void StopMaintainace()
		{
			_mTimer?.Stop();
		}
		private void _mTimer_Tick(object sender, EventArgs e)
		{
			var n = DateTime.Now;
			if (!doAnpr&& (n - LastRecognitPlate).TotalSeconds > 10)
			{
				LastRecognitPlate = DateTime.Now;
				if (isCar)
				{
					isCar = false;
					doAnpr = true;
					_alprService.RecognizeLicensePlate(carImg, (res, ex) =>
                    {
						if (ex == null)
                        {
							/// Bug xem nhận diện có OK không
							//HandleError(IconEnums.Check, res, true, false);
						}
						doAnpr = false;
					});
					
				}
				else
				{
					isCar = true;
					doAnpr = true;
					_alprService.RecognizeLicensePlate(bikeImg, (res, ex) =>
					{
						if (ex == null)
						{
							/// Bug xem nhận diện có OK không
							//HandleError(IconEnums.Check, res, true, false);
						}
						doAnpr = false;
					});
					;
				}
			}
		}
		#endregion

		private Socket _clientSocket;

        public override void Init(ParameterKey key)
        {
            base.Init(key);
            //SetupDevices();

            //try
            //{
            //    _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //    _clientSocket.Connect(Section.LedIP, Convert.ToInt32(Section.ComLed));
            //}
            //catch (Exception ex)
            //{
            //    _logger.Error("Cant init led device", ex);
            //}

            try
            {
                if (!string.IsNullOrEmpty(Section.LedIP) && !string.IsNullOrEmpty(Section.ComLed))
                {
                    _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    _clientSocket.Connect(Section.LedIP, Convert.ToInt32(Section.ComLed));
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Cant init led device", ex);
            }
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
        //[SerilogTrace]
        public override void Start()
        {
            base.Start();

            if (_modeManager.ArgumentParams.Mode == RunMode.Testing)
            {
                this._testingService.Start(() =>
                {
                    var now = TimeMapInfo.Current.LocalTime;
                    _testingService.CreateSchedule("checkin " + now.ToString("yyyMMdd HHmm"), (callback) =>
                    {
                        SimulateTapCheckIn(callback);
                    });
                    //_testingService.CreateSchedule("checkin " + DateTime.Now.ToString("yyyMMdd HHmm"), (callback) =>
                    //{
                    //    SimulateTapCheckIn(callback);
                    //});
                });
            }
            if (this.Section != null && this.Section.BarrierBySiemensControl != null && this.Section.BarrierBySiemensControl.Active && !string.IsNullOrEmpty(Section.BarrierBySiemensControl.IP))
            {
                var Lane = GetLane();
                Port4 mydevice = Port4.GetInstance();
                //mydevice.AddCommandIn(new SiemenInfo {
                //    TcpIp= this.Section.BarrierBySiemensControl.IP,
                //    TypeIn= this.Section.BarrierBySiemensControl.TypeIn,
                //    Lane= Lane
                //});
                switch (Lane)
                {
                    case 1:
                        mydevice.HandleButtonIn1 = RaiseButtonClick;
                        break;
                    case 2:
                        mydevice.HandleButtonIn2 = RaiseButtonClick;
                        break;
                    case 3:
                        mydevice.HandleButtonIn3 = RaiseButtonClick;
                        break;
                    case 4:
                        mydevice.HandleButtonIn4 = RaiseButtonClick;
                        break;
                }
            }
            else if (this.Section != null && this.Section.BarrierByInternetControl != null && this.Section.BarrierByInternetControl.Active
                 && !string.IsNullOrEmpty(Section.BarrierByInternetControl.IP) && !string.IsNullOrEmpty(Section.BarrierByInternetControl.ButtonNumber))
            {
                var butnum = 0;
                if (int.TryParse(Section.BarrierByInternetControl.ButtonNumber, out butnum) && butnum >= 5 && butnum <= 8)
                {
                    var dv = InternetControllerDevice.GetInstance();
                    if (dv.StatusInfo != null)
                    {
                        dv.StatusInfo.IP = Section.BarrierByInternetControl.IP;
                        dv.StatusInfo.Port = Section.BarrierByInternetControl.Port;
                        dv.StatusInfo.UserName = Section.BarrierByInternetControl.UserName;
                        dv.StatusInfo.Password = Section.BarrierByInternetControl.Password;
                    }
                    else
                    {
                        dv.StatusInfo = new ControllerDeviceInfo()
                        {
                            IP = Section.BarrierByInternetControl.IP,
                            Port = Section.BarrierByInternetControl.Port,
                            UserName = Section.BarrierByInternetControl.UserName,
                            Password = Section.BarrierByInternetControl.Password
                        };
                    }
                    switch (butnum)
                    {
                        case 5:
                            dv.RaiseStatusChangedG5 = Dv_RaiseStatusChangedG;

                            break;
                        case 6:
                            dv.RaiseStatusChangedG6 = Dv_RaiseStatusChangedG;
                            break;
                        case 7:
                            dv.RaiseStatusChangedG7 = Dv_RaiseStatusChangedG;
                            break;
                        case 8:
                            dv.RaiseStatusChangedG8 = Dv_RaiseStatusChangedG;
                            break;
                    }
                }
            }
			this.LastRecognitPlate = DateTime.Now;
			this.StartMaintainace();
            this.StartAutoPlate();
		}

        private void RaiseButtonClick(LogoTypeIn4 obj)
        {
            if (this.Section.TemporaryDirection == LaneDirection.In)
                Task.Factory.StartNew(() =>
                {
                    if (isdoinng)
                    {
                        if (_userPreferenceService.OptionsSettings.BarrierForcedWithPopup)
                        {
                            if (ShowPopupBarrier != null)
                            {
                                InvokeOnMainThread(() => ShowPopupBarrier(this, null));
                            }
                        }
                        else
                        {
                            ForcedBarier("...");
                        }
                    }
                });
        }

        private void Dv_RaiseStatusChangedG(ControllerDeviceInfo sender)
        {
            if (this.Section.TemporaryDirection == LaneDirection.In)
                Task.Factory.StartNew(() =>
                {
                    if (isdoinng)
                    {
                        if (_userPreferenceService.OptionsSettings.BarrierForcedWithPopup)
                        {
                            if (ShowPopupBarrier != null)
                            {
                                InvokeOnMainThread(() => ShowPopupBarrier(this, null));
                            }
                        }
                        else
                        {
                            ForcedBarier("...");
                        }
                    }
                });
        }

        private bool isfirst = false;
        //[SerilogTrace]
        private void HandleCheckIn(CheckIn sendData, Action<CheckIn, Exception> complete, string plate = null)
        {
            _autoCheckInTimer.Stop();
            if (!sendData.CardId.Equals(_preCardId))
            {
                if (!HandleCaptureImage(sendData, sendData.CardId, CheckCameraDoNotWork))
                {
                    if (complete != null)
                    {
                        if (CheckCameraDoNotWork)
                        {
                            Task.Factory.StartNew(() => Alarm("fail"));
                            HandleError(IconEnums.Error, GetText("camera.capture_same_image"), false, false);
                            complete(null, new Exception("camera.capture_same_image"));
                        }
                        else
                        {
                            Task.Factory.StartNew(() => Alarm("fail"));
                            HandleError(IconEnums.Error, GetText("camera.capture_null_image"), false, false);
                            complete(null, new Exception("camera.capture_null_image"));
                        }
                    }
                    //CheckInData = null;
                    ResetUIInformation();
                    return;
                }
            }
            else
            {
                if (!HandleCaptureImage(sendData, sendData.CardId, false))
                {
                    if (complete != null)
                    {
                        Task.Factory.StartNew(() => Alarm("fail"));
                        HandleError(IconEnums.Error, GetText("camera.capture_null_image"), false, false);
                        complete(null, new Exception("camera.capture_null_image"));
                    }
                    //CheckInData = null;
                    ResetUIInformation();
                    return;
                }
            }

            // update if tapping same card within WaitingCheckDuration
            if (_editTimer.Enabled && sendData.CardId.Equals(_editTimer.CurrentCardId))
            {
                var tempCheckinData = CheckInData;


                if (tempCheckinData != null && !string.IsNullOrEmpty(tempCheckinData.VehicleNumber))
                    sendData.VehicleNumber = tempCheckinData.VehicleNumber;

                if (!_hasConfirmChecking)
                {
                    UpdateCheckIn(sendData, (data, exception) =>
                    {
                        if (exception == null)
                        {
                            if (_editTimer.Enabled && data != null && data.CardId.Equals(_editTimer.CurrentCardId))
                            {
                                SaveImage(data);

                                this.CheckInData = data;
                                //isfirst = false;
                                if (plate == null)
                                    RecognizePlate(sendData.GetClone);
                            }
                            //Alarm("success");
                            HandleError(IconEnums.update_ok, GetText("checkin.update_success"), true, false);
                            if (complete != null)
                                complete(CheckInData, exception);
                        }

                        // hanle : BlacklistPlateDetected

                        else if (RequestExceptionManager.GetExceptionMessage<CheckIn>(exception.Message).Key ==
                              RequestExceptionEnum.BlacklistPlateDetected)
                        {
                            Task.Factory.StartNew(() => Alarm("fail"));
                            HandleError(IconEnums.Error, GetText("checkin.blacklist_plate_detected"), false, true);
                            PrintLog<CheckInLaneViewModel>(exception, _userPreferenceService.HostSettings.LogServerIP);
                        }

                        // handle: checkout and checkin again within WaitingCheckDuration 
                        else if (RequestExceptionManager.GetExceptionMessage<CheckIn>(exception.Message).Key == RequestExceptionEnum.CardIsNotInUse)
                        {
                            //CheckIn(sendData, (checkInResult, ex) =>
                            //{
                            //    CheckInData = checkInResult;
                            //    if (checkInResult != null && tempCheckinData != null)
                            //    {
                            //        TypeHelper.GetCardType(tempCheckinData.CardTypeId, result => CardType = result);
                            //    }

                            //    _notEditPlateNumberYet = true;
                            //    RecognizePlate(sendData);

                            //    if (complete != null)
                            //        complete(CheckInData, ex);
                            //});
                            Task.Factory.StartNew(() => Alarm("fail"));
                            HandleError(IconEnums.Error, GetText("checkin.something_wrong_update"), false, true);
                            PrintLog<CheckInLaneViewModel>(exception, _userPreferenceService.HostSettings.LogServerIP);
                        }
                    });
                }
            }
            else
            {
                InvokeOnMainThread(() => this.Notices.Clear());

                // update checkin data of previous checkin session if it is still available for editing
                if (_editTimer.Enabled && this.CheckInData != null)
                {
                    UpdateCheckIn(this.CheckInData.GetClone, null);
                }

                CheckIn(sendData, (checkInResult, ex) =>
                {
                    CheckInData = checkInResult;
                    if (checkInResult != null)
                    {
                        isfirst = true;
                        if (checkInResult.CustomerInfo != null && this.CustomerInfo.VehicleRegistrationInfo != null && this.CustomerInfo.VehicleRegistrationInfo.Status != VehicleRegistrationStatus.Suspend
                                && this.CustomerInfo.VehicleRegistrationInfo.Status != VehicleRegistrationStatus.OutOfDate)
                        {
                            TypeHelper.GetCardType(checkInResult.CardTypeId, result => CardType = result);
                            //MessageBox.Show(checkInResult.CardTypeId.ToString() + ", " + checkInResult.CustomerInfo); checkInResult.CardTypeId != 0 &&
                            if (checkInResult.CustomerInfo != null && _userPreferenceService.OptionsSettings.ConfirmCheckin)
                            {
                                _allowCheckIn = false;
                            }
                            else
                            {
                                _allowCheckIn = true;
                            }
                        }
                        //-----------------------------
                        //Log.Logger.Information(JsonConvert.SerializeObject(CheckInData));
                        var tempCheckinData = CheckInData;
                        if (tempCheckinData != null)
                            _editTimer.Start(tempCheckinData.CardId);
                        _notEditPlateNumberYet = true;
                        if (plate == null)
                            RecognizePlate(sendData);
                        else if (tempCheckinData != null)
						{
							lock (_locker)
							{
								_processedPlate[tempCheckinData.CardId] = DateTime.Now;
							}
						}

                    }
                    if (complete != null)
                        complete(CheckInData, ex);
                });
            }
        }
        private bool HandleCaptureImage(CheckIn data, string cardId, bool shouldCheck)
        {
            try
            {
                var now = TimeMapInfo.Current.LocalTime;
                _frontImage = Section.FrontInCamera.CaptureImage();
                _backImage = Section.BackInCamera.CaptureImage();

                //// TODO: Test only
                //_frontImage = Image.FromFile("Images/Car_MaskCar_Plate_637620756716689852.jpg");
                //_backImage = Image.FromFile("Images/Car_MaskCar_Plate_637620756716689852.jpg"); //Image.FromFile("Images/Car_MaskCar_Plate_637620756716689852.jpg");

                //if (Section.IsInExtra && Section.ExtraIn1Camera != null && Section.ExtraIn2Camera != null)
                //{
                //    extra1 = Section.ExtraIn1Camera.CaptureImage();
                //    extra2 = Section.ExtraIn2Camera.CaptureImage();
                //}
                //if (shouldCheck)
                //{
                //    if (CompareImage(_frontImage, _preFrontImage))
                //    {
                //        using (Image tmpFront = (Image)_frontImage.Clone())
                //        {
                //            //ImageUtility.Watermark(tmpFront as Bitmap, cardId + "  " + now.ToString("dd/MM/yyyy HH:mm:ss"));
                //            //ImageUtility.Watermark(tmpFront as Bitmap, cardId + "  " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                //            data.FrontImage = tmpFront.ToByteArray(ImageFormat.Jpeg);
                //        }
                //    }
                //    if (CompareImage(_backImage, _preBackImage))
                //    {
                //        using (Image tmpBack = (Image)_backImage.Clone())
                //        {
                //            //ImageUtility.Watermark(tmpBack as Bitmap, cardId + "  " + now.ToString("dd/MM/yyyy HH:mm:ss"));
                //            //ImageUtility.Watermark(tmpBack as Bitmap, cardId + "  " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                //            data.BackImage = tmpBack.ToByteArray(ImageFormat.Jpeg);
                //        }
                //    }
                //    //if (Section.IsInExtra && Section.ExtraIn1Camera != null && Section.ExtraIn2Camera != null)
                //    //{
                //    //    if (CompareImage(extra1, _preExtra1))
                //    //    {
                //    //        using (Image tmpExtra1 = (Image)extra1.Clone())
                //    //        {
                //    //            //ImageUtility.Watermark(tmpExtra1 as Bitmap, cardId + "  " + now.ToString("dd/MM/yyyy HH:mm:ss"));
                //    //            //ImageUtility.Watermark(tmpExtra1 as Bitmap, cardId + "  " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                //    //            data.Extra1Image = tmpExtra1.ToByteArray(ImageFormat.Jpeg);
                //    //        }
                //    //    }
                //    //    if (CompareImage(extra2, _preExtra2))
                //    //    {
                //    //        using (Image tmpExtra2 = (Image)extra2.Clone())
                //    //        {
                //    //            //ImageUtility.Watermark(tmpExtra2 as Bitmap, cardId + "  " + now.ToString("dd/MM/yyyy HH:mm:ss"));
                //    //            //ImageUtility.Watermark(tmpExtra2 as Bitmap, cardId + "  " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                //    //            data.Extra2Image = tmpExtra2.ToByteArray(ImageFormat.Jpeg);
                //    //        }
                //    //    }
                //    //}
                //}
                //else
                {
                    using (Image tmpFront = (Image)_frontImage.Clone())
                    {
                        ImageUtility.Watermark(tmpFront as Bitmap, cardId + "  " + now.ToString("dd/MM/yyyy HH:mm:ss"));
                        //ImageUtility.Watermark(tmpFront as Bitmap, cardId + "  " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                        data.FrontImage = tmpFront.ToByteArray(ImageFormat.Jpeg);
                    }
                    using (Image tmpBack = (Image)_backImage.Clone())
                    {
                        ImageUtility.Watermark(tmpBack as Bitmap, cardId + "  " + now.ToString("dd/MM/yyyy HH:mm:ss"));
                        //ImageUtility.Watermark(tmpBack as Bitmap, cardId + "  " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                        data.BackImage = tmpBack.ToByteArray(ImageFormat.Jpeg);
                    }
                    //if (Section.IsInExtra && Section.ExtraIn1Camera != null && Section.ExtraIn2Camera != null)
                    //{

                    //    using (Image tmpExtra1 = (Image)extra1.Clone())
                    //    {
                    //        //ImageUtility.Watermark(tmpExtra1 as Bitmap, cardId + "  " + now.ToString("dd/MM/yyyy HH:mm:ss"));
                    //        //ImageUtility.Watermark(tmpExtra1 as Bitmap, cardId + "  " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                    //        data.Extra1Image = tmpExtra1.ToByteArray(ImageFormat.Jpeg);
                    //    }
                    //    using (Image tmpExtra2 = (Image)extra2.Clone())
                    //    {
                    //        //ImageUtility.Watermark(tmpExtra2 as Bitmap, cardId + "  " + now.ToString("dd/MM/yyyy HH:mm:ss"));
                    //        //ImageUtility.Watermark(tmpExtra2 as Bitmap, cardId + "  " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                    //        data.Extra2Image = tmpExtra2.ToByteArray(ImageFormat.Jpeg);
                    //    }

                    //}
                    //data.FrontImage = Section.FrontInCamera.CaptureImage(cardId + "  " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                    //data.BackImage = Section.BackInCamera.CaptureImage(cardId + "  " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                }

                if (data.FrontImage == null || data.BackImage == null)
                {
                    //HandleError(IconEnums.Error, GetText("camera.capture_null_image"), false, false);
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }

        }
        private Image _preBackImage = null;
        private Image _preFrontImage = null;
        private Image _frontImage = null;
        private Image _backImage = null;
        private Image _preExtra1 = null;
        private Image _preExtra2 = null;
        private Image extra1 = null;
        private Image extra2 = null;
        private string _preCardId = string.Empty;
        private bool CompareImage(Image capturedImage, Image desImage)
        {
            if (desImage == null) return true;

            float difference = capturedImage.PercentageDifference(desImage);
            if (difference > 0.05)
                return true;
            return false;
        }
        private string mysub(string str)
        {
            var res = str.Substring(str.LastIndexOf('-') + 1, str.Length - 1 - str.LastIndexOf('-'));
            return res.Trim();
        }
        public void FindAndNotifyBlacklist()
        {
            if (CheckInData != null && !string.IsNullOrEmpty(CheckInData.VehicleNumber))
            {
                int gate = _userPreferenceService.HostSettings.Terminal.Id;
                int user = this.Section.UserService.CurrentUser.Id;
                int pid = CheckInData.ParkingSessionId;
                string imgP = CheckInData.BackImagePath;
                string vehiclenumber = CheckInData.VehicleNumber;//string.Format("{0}-{1}", string.IsNullOrEmpty(CheckInData.PrefixNumberVehicle) ? "" : CheckInData.PrefixNumberVehicle, CheckInData.VehicleNumber);
                //var subvehiclenumber
                if (!string.IsNullOrEmpty(vehiclenumber) && vehiclenumber != CurtentBlackNum && BlackNumbers.Count > 0)
                {
                    List<string> lst = new List<string>();
                    lock (BlackNumbers)
                    {
                        lst = BlackNumbers.Select(b => b.Number).ToList();
                    }
                    var compare = mysub(vehiclenumber);
                    if (lst.Exists(s => s.Trim() == compare.Trim()))
                    {
                        CurtentBlackNum = vehiclenumber;
                        Task.Factory.StartNew(() =>
                            _server.FindAndNotifyBlacklist(pid, imgP, vehiclenumber, gate, user, 0,
                               (res, ex) =>
                               {
                                   //do something
                               }
                           )
                        );
                    }
                }
            }
        }
        public void BeginCheckin(Action<CheckIn, Exception> complete, string plate = null)
        {
            if (_hasConfirmChecking)
                return;
            try
            {
                var card = this.CheckedCard;

                // log card id information
                _logger.Information($"Check in: {card.Id} {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}");

                if (card == null || string.IsNullOrEmpty(card.Id))
                {
                    Task.Factory.StartNew(() => Alarm("fail"));
                    HandleError(IconEnums.Warning, GetText("cannot_read_card"), false, true);
                    if (complete != null)
                        complete(null, new Exception(GetText("cannot_read_card")));
                    return;
                }

                if (this.Section == null || this.Section.FrontInCamera == null || this.Section.BackInCamera == null) return;

                CheckIn sendData = new CheckIn();
                sendData.TerminalId = _userPreferenceService.HostSettings.Terminal.Id;

                sendData.CardId = this.CheckedCard.Id;
                sendData.LaneId = Section.Lane.Id;
                sendData.VehicleTypeId = Section.VehicleTypeId;
                //sendData.VehicleSubType = VehicleSubType.Bike_Auto;
                sendData.OperatorId = User.Id;
                //sendData.FrontImage = Section.FrontInCamera.CaptureImage(card.Id + "  " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                //sendData.BackImage = Section.BackInCamera.CaptureImage(card.Id + "  " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                if(!string.IsNullOrEmpty(plate))
                {
					sendData.AlprVehicleNumber = plate;
					sendData.VehicleNumber = ExtractVehicleNumber(plate);
					sendData.PrefixNumberVehicle = ExtractPrefixVehicleNumber(sendData.AlprVehicleNumber, sendData.VehicleNumber);

				}
                else
                {
					sendData.VehicleNumber = "";
					sendData.AlprVehicleNumber = "";
				}   
			
				HandleCheckIn(sendData, (data, ex) =>
                {
                    if (ex == null && _allowCheckIn)
                    {
                        //var tempCheckInData = CheckInData;
                        //RecognizePlate(tempCheckInData);
                        LastProcessed = DateTime.Now;
                        _preBackImage = _backImage;
                        _preFrontImage = _frontImage;
                        _preCardId = sendData.CardId;
                    }
                    if (ex != null)
                        Task.Factory.StartNew(() => Alarm("fail"));
                    if (complete != null)
                        complete(CheckInData, ex);
                }, plate);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                PrintLog<CheckInLaneViewModel>(ex, _userPreferenceService.HostSettings.LogServerIP);
                complete(null, ex);
            }
        }

        //[SerilogTrace]
        private void AffterProcess(CheckIn tempCheckInData)
        {
            Task.Factory.StartNew(() =>
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                while (isfirst && sw.ElapsedMilliseconds <= 300)
                {
                    Thread.Sleep(50);
                }
                sw.Stop();

                //if (tempCheckInData != null && tempCheckInData.CustomerInfo != null && tempCheckInData.CustomerInfo.VehicleRegistrationInfo != null &&
                //    !string.IsNullOrEmpty(tempCheckInData.CustomerInfo.VehicleRegistrationInfo.VehicleNumber) &&
                //    ExtractVehicleNumber(tempCheckInData.CustomerInfo.VehicleRegistrationInfo.VehicleNumber) != tempCheckInData.VehicleNumber)
                //{
                //    if (_userPreferenceService.OptionsSettings.ConfirmCheckin)
                //    {
                //        _hasConfirmChecking = true;
                //        var confirmCheckInKeyvehicel = Section.KeyMap.KeysMap[KeyAction.ConfirmCheckInKey];
                //        var confirmCheckInkey = Section.KeyMap.KeysMap[KeyAction.AddNewNumber];
                //        var cancelCheckInKey = Section.KeyMap.KeysMap[KeyAction.CancelCheckInKey];
                //        string msg = string.Format(GetText("checkin.guide"), confirmCheckInKeyvehicel, Environment.NewLine, confirmCheckInkey, Environment.NewLine, cancelCheckInKey);
                //        HandleError(IconEnums.Guide, msg, true, false);
                //    }
                //    else
                //    {
                //        _hasConfirmChecking = false;
                //        if (isfirst)
                //        {
                //            ProcessAfterCheckInSuccess(CheckInData, _checkInResult);
                //        }
                //    }
                //}
                //else
                //{
                //    _hasConfirmChecking = false;
                //    if (isfirst)
                //    {
                //        ProcessAfterCheckInSuccess(CheckInData, _checkInResult);
                //    }
                //}

            });
        }

        //[SerilogTrace]
        private void RecognizePlate(CheckIn tempCheckInData)
        {
            //if (tempCheckInData != null && tempCheckInData.CustomerInfo != null && tempCheckInData.CustomerInfo.VehicleRegistrationInfo != null &&
            //       tempCheckInData.CustomerInfo.VehicleRegistrationInfo.Status != VehicleRegistrationStatus.InUse && this.CheckedCard.CardTypeId != 0 && tempCheckInData.CustomerInfo != null)
            //{
            //    this.HandleError(IconEnums.Error, string.Format("--- Thẻ đã bị hủy ---", this.CustomerInfo.VehicleRegistrationInfo.RemainDays + 1), false);
            //    var checkOut = PackCheckOutData();
            //    checkOut.is_cancel = "Cancel-CheckIn";
            //    CheckOut(checkOut, (e) =>
            //    {

            //    });
            //    ResetUIInformation();
            //    _autoCheckInTimer.Stop();
            //    _editTimer.Stop();
            //    Countdown = string.Empty;
            //    ShowCountdown = false;
            //    _allowCheckIn = false;
            //    return;
            //}
            //MessageBox.Show(CheckedCard.CardType.Name);
            _logger.Information($"[{nameof(RecognizePlate)}] Begin RecognizePlate");

            //2024 Trich xuat bien so tu hinh chup truoc neu chup sau khong duoc
            _alprService.RecognizeLicensePlate(_backImage.ToByteArray(ImageFormat.Jpeg), tempCheckInData.VehicleType.Id, (result, ex1) =>
            {
                if (ex1 != null)
                {
                    HandleError(IconEnums.Warning, GetText("anpr.cannot_connect"), false, true);
                }
                else if (string.IsNullOrEmpty(result))
                {
                    _alprService.RecognizeLicensePlate(_frontImage.ToByteArray(ImageFormat.Jpeg), tempCheckInData.VehicleType.Id, (resultFromFrontImg, ex2) =>
                    {
                        ProcessDataAfterRecognizeLicensePlate(tempCheckInData, resultFromFrontImg);
                    });
                }
                else
                {
                    ProcessDataAfterRecognizeLicensePlate(tempCheckInData, result);
                }

                return;
            });

            AffterProcess(tempCheckInData.GetClone);

            Log.Logger.Information("End Check In -------------------------------------------------------------");
        }

        private void ProcessDataAfterRecognizeLicensePlate(CheckIn tempCheckInData, string result)
        {
            if (_notEditPlateNumberYet && tempCheckInData != null && CheckInData != null && tempCheckInData.CardId == CheckInData.CardId)
            {
                if (result.Equals(_previousRawVehicleNumber) && !string.IsNullOrEmpty(result))
                {
                    _previousRawVehicleNumber = result;
                    //PrintLog<CheckInLaneViewModel>(new Exception("There is something fishy about ANPR"), null, true);
                }
                else
                {
                    _hasConfirmChecking = false;
                    if (tempCheckInData.AlprVehicleNumber != result || tempCheckInData.VehicleNumber != ExtractVehicleNumber(result))
                    {
                        CheckInData.AlprVehicleNumber = tempCheckInData.AlprVehicleNumber = result;
                        CheckInData.VehicleNumber = tempCheckInData.VehicleNumber = ExtractVehicleNumber(result);
                        CheckInData.PrefixNumberVehicle = tempCheckInData.PrefixNumberVehicle = ExtractPrefixVehicleNumber(tempCheckInData.AlprVehicleNumber, tempCheckInData.VehicleNumber);

                        UpdateCheckIn(tempCheckInData.GetClone, null);
                    }
                }

                ////2024 Confirm Checkin for All
                if (_userPreferenceService.OptionsSettings.ConfirmCheckin)
                {
                    if ((string.IsNullOrEmpty(tempCheckInData.CustomerInfo.VehicleRegistrationInfo.VehicleNumber) ||
                        (ExtractVehicleNumber(tempCheckInData.CustomerInfo.VehicleRegistrationInfo.VehicleNumber.Replace(".", "")) != ExtractVehicleNumber(result))) 
                        || (string.IsNullOrEmpty(result) || (tempCheckInData.CardTypeId == 0 && _userPreferenceService.OptionsSettings.NoMatchingPlateNoticeEnalbe)) 
                       )
                    {//!_allowupdate && 
                        _hasConfirmChecking = true;
                       // _allowupdate = false;
                        var confirmCheckInKeyvehicel = Section.KeyMap.KeysMap[KeyAction.ConfirmCheckInKey];
                        var confirmCheckInkey = Section.KeyMap.KeysMap[KeyAction.AddNewNumber];
                        var cancelCheckInKey = Section.KeyMap.KeysMap[KeyAction.CancelCheckInKey];
                        string msg = string.Empty;
                        msg = string.Format(GetText("checkin.guide"), confirmCheckInKeyvehicel, Environment.NewLine, confirmCheckInkey, Environment.NewLine, cancelCheckInKey);
                        HandleError(IconEnums.Guide, msg, true, true);
                    }
                    else
                    {
                        _hasConfirmChecking = false;
                        if (isfirst)
                        {
                            ProcessAfterCheckInSuccess(CheckInData, _checkInResult);
                        }
                    }
                }
                else
                {
                    _hasConfirmChecking = false;
                    if (isfirst)
                    {
                        ProcessAfterCheckInSuccess(CheckInData, _checkInResult);
                    }
                }

                ////2024: push K to Vehicle when result null
                //if (string.IsNullOrEmpty(result))
                //{
                //    CheckInData.VehicleNumber = tempCheckInData.VehicleNumber = "K";
                //    UpdateCheckIn(tempCheckInData.GetClone, null);
                //}

                //GardenMall-2023: push VehicleRegistrationNumber to RecognizeLicensePlate if null
                if (!string.IsNullOrEmpty(tempCheckInData.CustomerInfo.VehicleRegistrationInfo.VehicleNumber))//string.IsNullOrEmpty(tempCheckInData.VehicleNumber) && 
                {
                    string vtemp = tempCheckInData.CustomerInfo.VehicleRegistrationInfo.VehicleNumber;
                    CheckInData.AlprVehicleNumber = tempCheckInData.AlprVehicleNumber = vtemp;
                    CheckInData.VehicleNumber = tempCheckInData.VehicleNumber = ExtractVehicleNumber(vtemp);
                    CheckInData.PrefixNumberVehicle = tempCheckInData.PrefixNumberVehicle = ExtractPrefixVehicleNumber(tempCheckInData.AlprVehicleNumber, tempCheckInData.VehicleNumber);
                    UpdateCheckIn(tempCheckInData.GetClone, null);
                }

                Task.Factory.StartNew(() => ShowLed(CheckInData.AlprVehicleNumber, ""));
            }
        }

        private void ShowLed(string vehicleNumber, string parkingfee)
        {
            try
            {
                if (!string.IsNullOrEmpty(Section.LedIP))
                {
                    vehicleNumber = vehicleNumber.Replace("-", "").Replace(" ", "");
                    parkingfee = parkingfee.Replace(" VND", "");

                    // Print total fee //*[H1][C1]VHB[H2][C2]Company[H3][C3]123456789[!]
                    var stringLed = $"*[H1][C5]{vehicleNumber}[H2][C1]{parkingfee}[H3][C1][!]";
                    byte[] buffer = Encoding.ASCII.GetBytes(stringLed);
                    _clientSocket.Send(buffer);
                }
            }
            catch (Exception ex1)
            {
                _logger.Error($"[{nameof(ResetUIInformation)}] Print total fee: {ex1.Message}", ex1);
            }
        }

        private void SaveImage(CheckIn checkInInfo)
        {
            _storageService.SaveImage(
                new List<string>() { checkInInfo.FrontImagePath, checkInInfo.BackImagePath },
                new List<byte[]>() { checkInInfo.FrontImage, checkInInfo.BackImage },
                (List<Exception> lstExceptions) =>
                {
                    foreach (Exception exception in lstExceptions)
                    {
                        if (exception != null)
                        {
                            PrintLog<CheckInLaneViewModel>(exception);
                            return;
                        }
                    }
                    _server.ReplicateImages(checkInInfo, null);
                });
        }
        //[SerilogTrace]
        public void CheckIn(CheckIn data, Action<CheckIn, Exception> complete)
        {
            if (_editTimer.Enabled)
            {
                _editTimer.Stop();
                ShowCountdown = false;
            }

            // Set entry check from setting
            data.EntryCheck = _settings.EntryCheck;

            // Reset previousCheckInVehicleNumber
            _previousCheckInVehicleNumber = DEFAULT_VEHICLE_NUMBER;

            _server.CreateCheckIn(data, (result, ex) =>
            {
                _checkInResult = result;
                if (ex == null)
                {
                    SaveImage(result);
                }
                InvokeOnMainThread(() =>
                {
                    string msg = string.Empty;
                    //this.Notices.Clear();

                    if (ex != null)
                    {
                        if (ex is NotFoundException)
                        {
                            //if (!_notExistedCards.ContainsKey(data.CardId))
                            //{
                            //    _notExistedCards.Add(data.CardId, 0);
                            //    if (ShowAddRegisteredCardWindow != null)
                            //    {
                            //        ShowAddRegisteredCardWindow(this, new EventArgs());
                            //    }
                            //}
                            //else
                            //{
                            //    HandleError(IconEnums.Card, GetText("checkin.card_not_found"), false, false);
                            //}

                            //_notExistedCards[data.CardId]++;
                            HandleError(IconEnums.Card, GetText("checkin.card_not_found"), false, false);
                        }
                        else
                        {
                            string noticeMsg = RequestExceptionManager.GetExceptionMessage<CheckIn>(ex.Message).Value;
                            HandleError(IconEnums.Error, noticeMsg, true, false);
                            if (ex is NotAcceptableException)
                            {
                                CustomerInfo = result.CustomerInfo;
                            }

                        }

                        PrintLog<CheckInLaneViewModel>(ex);
                    }
                    else
                    {
                        CustomerInfo = result.CustomerInfo;
                        if (!_userPreferenceService.OptionsSettings.ConfirmCheckin)//(CustomerInfo == null || !_userPreferenceService.OptionsSettings.ConfirmCheckin || !_userPreferenceService.OptionsSettings.PlateRecognitionEnable)
                        {
                            ProcessAfterCheckInSuccess(data, result);
                        }
                    }
                    if (complete != null)
                    {
                        complete(result, ex);

                        //if (CheckInData != null)
                        //{
                        //    if (_player != null)
                        //    {
                        //        _player.Open(_uri);
                        //        _player.Play();
                        //    }
                        //}

                    }
                });
            });
        }
        //[SerilogTrace]
        void OpenBarrier()
        {
            try
            {
                if (this.Section != null)
                {
                    if (this.Section.UseZKController)
                    {
                        var zkController = ZKControllerProcessor.GetInstance(this.Section.BarrierIpController, this.Section.BarrierPortController);
                        zkController.SendOutputCommand(this.Section.BarrierDoorsController);
                    }
                    else if (this.Section.BarrierByInternetControl != null && this.Section.BarrierByInternetControl.Active
                        && !string.IsNullOrEmpty(Section.BarrierByInternetControl.IP) && !string.IsNullOrEmpty(Section.BarrierByInternetControl.PortNumber))
                    {
                        InternetControllerDevice mydevice = InternetControllerDevice.GetInstance();
                        mydevice.AddCommandInfo(new ControllerDeviceInfo
                        {
                            IP = Section.BarrierByInternetControl.IP,
                            Port = Section.BarrierByInternetControl.Port,
                            UserName = Section.BarrierByInternetControl.UserName,
                            Password = Section.BarrierByInternetControl.Password,
                            PortNumber = Section.BarrierByInternetControl.PortNumber
                        });
                    }

                    _allowupdate = false;

                    //if (CheckInData != null)
                    //{
                    //    if (_player != null)
                    //    {
                    //        _player.Open(_uri);
                    //        _player.Play();
                    //    }
                    //}

                    //else if (this.Section.BarrierBySiemensControl != null && this.Section.BarrierBySiemensControl.Active && !string.IsNullOrEmpty(Section.BarrierBySiemensControl.IP))
                    //{
                    //    Port4 mydevice = Port4.GetInstance();
                    //    mydevice.AddCommandOut(new SiemenInfo
                    //    {
                    //        TcpIp = this.Section.BarrierBySiemensControl.IP,
                    //        TypeOut = this.Section.BarrierBySiemensControl.TypeOut
                    //    });
                    //}
                    //else if (this.Section.UseBarrierIpController)
                    //{
                    //    CurrentListBarrierIp.OpenBarrier(this.Section.BarrierIpController, this.Section.BarrierPortController, this.Section.BarrierDoorsController, this.Section.TimeTick);
                    //}
                    //else if (this.Section != null && this.Section.Barrier != null)
                    //{
                    //    if (_userPreferenceService.OptionsSettings.IsSfactorsCom)
                    //    {
                    //        ComManagement sftCom = ComManagement.GetInstance();
                    //        if (this.Section.BarrierPort.ToUpper().Contains("B1") && this.Section.BarrierPort.ToUpper().ToUpper().Contains("B2"))
                    //        {

                    //            sftCom.AddCommand(new ComParameter()
                    //            {
                    //                ComName = this.Section.Barrier.PortName,
                    //                Description = string.Format("Mở Barrier Check-in Lane: {0}", this.Section.LaneName),
                    //                TimeApply = DateTime.Now,
                    //                Commands = new List<ComCommand>()
                    //            {
                    //                    new ComCommand()
                    //                    {
                    //                        Command ="Write",
                    //                        CommandMessage=string.Format("${0}#", "B1")
                    //                    }
                    //            }
                    //            });
                    //            sftCom.AddCommand(new ComParameter()
                    //            {
                    //                ComName = this.Section.Barrier.PortName,
                    //                Description = string.Format("Mở Barrier Check-in Lane: {0}", this.Section.LaneName),
                    //                TimeApply = DateTime.Now,
                    //                Commands = new List<ComCommand>()
                    //            {
                    //                    new ComCommand()
                    //                    {
                    //                        Command ="Write",
                    //                        CommandMessage=string.Format("${0}#", "B2")
                    //                    }
                    //            }
                    //            });
                    //        }
                    //        else
                    //        {
                    //            if (this.Section.BarrierPort.ToUpper().Contains("B1"))
                    //            {
                    //                sftCom.AddCommand(new ComParameter()
                    //                {
                    //                    ComName = this.Section.Barrier.PortName,
                    //                    Description = string.Format("Mở Barrier Check-in Lane: {0}", this.Section.LaneName),
                    //                    TimeApply = DateTime.Now,
                    //                    Commands = new List<ComCommand>()
                    //                {
                    //                        new ComCommand()
                    //                        {
                    //                            Command ="Write",
                    //                            CommandMessage=string.Format("${0}#", "B1")
                    //                        }
                    //                }
                    //                });
                    //            }
                    //            else if (this.Section.BarrierPort.ToUpper().Contains("B2"))
                    //            {
                    //                sftCom.AddCommand(new ComParameter()
                    //                {
                    //                    ComName = this.Section.Barrier.PortName,
                    //                    Description = string.Format("Mở Barrier Check-in Lane: {0}", this.Section.LaneName),
                    //                    TimeApply = DateTime.Now,
                    //                    Commands = new List<ComCommand>()
                    //                {
                    //                        new ComCommand()
                    //                        {
                    //                            Command ="Write",
                    //                            CommandMessage=string.Format("${0}#", "B2")
                    //                        }
                    //                }
                    //                });
                    //            }
                    //            else if (this.Section.LaneName.ToUpper().Equals("L1") || this.Section.LaneName.ToUpper().Equals("LAN1"))
                    //            {
                    //                sftCom.AddCommand(new ComParameter()
                    //                {
                    //                    ComName = this.Section.Barrier.PortName,
                    //                    Description = string.Format("Mở Barrier Check-in Lane: {0}", this.Section.LaneName),
                    //                    TimeApply = DateTime.Now,
                    //                    Commands = new List<ComCommand>()
                    //                {
                    //                        new ComCommand()
                    //                        {
                    //                            Command ="Write",
                    //                            CommandMessage=string.Format("${0}#", "B1")
                    //                        }
                    //                }
                    //                });
                    //            }
                    //            else
                    //            {
                    //                sftCom.AddCommand(new ComParameter()
                    //                {
                    //                    ComName = this.Section.Barrier.PortName,
                    //                    Description = string.Format("Mở Barrier Check-in Lane: {0}", this.Section.LaneName),
                    //                    TimeApply = DateTime.Now,
                    //                    Commands = new List<ComCommand>()
                    //                {
                    //                        new ComCommand()
                    //                        {
                    //                            Command ="Write",
                    //                            CommandMessage=string.Format("${0}#", "B2")
                    //                        }
                    //                }
                    //                });
                    //            }
                    //        }

                    //    }
                    //    else
                    //    {
                    //        if (this.Section.BarrierPort.ToUpper().Contains("B1") && this.Section.BarrierPort.ToUpper().Contains("B2"))
                    //        {
                    //            this.Section.Barrier.DevicePort = "B1";
                    //            this.Section.Barrier.Open();
                    //            Thread.Sleep(100);
                    //            this.Section.Barrier.DevicePort = "B2";
                    //            this.Section.Barrier.Open();
                    //            Thread.Sleep(100);
                    //            this.Section.Barrier.DevicePort = "B1B2";
                    //            this.Section.BarrierPort = "B1B2";
                    //        }
                    //        else if (this.Section.BarrierPort.ToUpper().Contains("B1"))
                    //        {
                    //            this.Section.Barrier.DevicePort = "B1";
                    //            this.Section.Barrier.Open();
                    //        }
                    //        else if (this.Section.BarrierPort.ToUpper().Contains("B2"))
                    //        {
                    //            this.Section.Barrier.DevicePort = "B2";
                    //            this.Section.Barrier.Open();
                    //        }
                    //        else if (this.Section.LaneName.ToUpper().Equals("L1") || this.Section.LaneName.ToUpper().Equals("LAN1"))
                    //        {
                    //            this.Section.Barrier.DevicePort = "B1";
                    //            this.Section.Barrier.Open();
                    //        }
                    //        else
                    //        {
                    //            this.Section.Barrier.DevicePort = "B2";

                    //            this.Section.Barrier.Open();
                    //        }
                    //    }
                    //}
                }
            }
            catch//(Exception ex)
            {
                //HandleError(IconEnums.Error, "Không mở được thiết bị Barrier", true);
            }
        }

        //[SerilogTrace]
        private void ProcessAfterCheckInSuccess(CheckIn data, CheckIn result)
        {
            _allowCheckIn = false;
            if (_checkInResult != null)
            {

                HandleError(IconEnums.Check, GetText("checkin.success"), true, false);
                OpenBarrier();
                isfirst = false;
                //FindAndNotifyBlacklist();

                //if (_player != null)
                //{
                //    _player.Open(_uri);
                //    _player.Play();
                //}

                Task.Factory.StartNew(() =>
                {
                    Alarm("success");
                });
                //int roomLeft = _checkInResult.LimitNumSlots - _checkInResult.CurrentNumSlots;
                //if (roomLeft < 0)
                //{
                //    roomLeft *= -1;
                //    string noticeMsg = string.Format(GetText("checkin.out_of_room"), roomLeft);
                //    HandleError(IconEnums.Parking, noticeMsg, false, true);
                //}
                //else
                //{
                //    string noticeMsg = string.Format(GetText("checkin.left_room"), roomLeft);
                //    HandleError(IconEnums.Parking, noticeMsg, false, true);
                //}

            }
        }

        public void CheckOut(CheckOut checkout, Action<Exception> complete)
        {

            _server.CreateCheckOut(checkout, CustomerInfo, ex =>
            {

                InvokeOnMainThread(() =>
                {
                    string msg = string.Empty;
                    if (ex != null)
                    {

                        if (ex is InternalServerErrorException)
                        {

                            HandleError(IconEnums.Error, GetText("checkout.something_wrong"), false);
                            PrintLog<CheckOutLaneViewModel>(ex, _userPreferenceService.HostSettings.LogServerIP);
                        }
                        else
                            PrintLog<CheckOutLaneViewModel>(ex);
                    }
                    else
                    {
                        //HandleError(IconEnums.Check, GetText("checkout.success"), false);
                        ////_canCheckout = true;
                        //_notEditPlateNumberYet = true;
                        ///*** Log ***/

                    }

                    if (complete != null)

                        complete(ex);
                });

            });
        }

        /// <summary>
        /// Packing the check out data.
        /// </summary>
        /// <param name="complete">The complete.</param>
        public CheckOut PackCheckOutData()
        {
            try
            {
                var card = this.CheckedCard;
                if (card == null)
                    return null;
                var now = TimeMapInfo.Current.LocalTime;
                CheckOut sendData = new CheckOut();
                sendData.CheckOutTime = now;
                //sendData.CheckOutTime = DateTime.Now;
                sendData.TerminalId = _userPreferenceService.HostSettings.Terminal.Id;
                sendData.CardId = card.Id;
                sendData.LaneId = Section.Lane.Id;
                sendData.OperatorId = User.Id;
                _frontImage = Section.FrontInCamera.CaptureImage();
                _backImage = Section.BackInCamera.CaptureImage();

                using (Image tmpFront = (Image)_frontImage.Clone())
                {
                    ImageUtility.Watermark(tmpFront as Bitmap, card.Id + "  " + now.ToString("dd/MM/yyyy HH:mm:ss"));
                    //ImageUtility.Watermark(tmpFront as Bitmap, card.Id + "  " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                    sendData.FrontImage = tmpFront.ToByteArray(ImageFormat.Jpeg);
                }
                using (Image tmpBack = (Image)_backImage.Clone())
                {
                    ImageUtility.Watermark(tmpBack as Bitmap, card.Id + "  " + now.ToString("dd/MM/yyyy HH:mm:ss"));
                    //ImageUtility.Watermark(tmpBack as Bitmap, card.Id + "  " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                    sendData.BackImage = tmpBack.ToByteArray(ImageFormat.Jpeg);
                }

                //sendData.FrontImage = Section.FrontOutCamera.CaptureImage(card.Id + "  " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                //sendData.BackImage = Section.BackOutCamera.CaptureImage(card.Id + "  " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                sendData.VehicleNumber = "...";
                sendData.AlprVehicleNumber = "...";
                if (sendData.FrontImage == null || sendData.BackImage == null)
                {
                    HandleError(IconEnums.Error, GetText("camera.capture_null_image"), false);
                    //complete(null);
                    return null;
                }
                return sendData;
            }
            catch (Exception ex)
            {
                PrintLog<CheckOutLaneViewModel>(ex, _userPreferenceService.HostSettings.LogServerIP);
                return null;
            }
        }

        int flag = 0;
        int lastReadTime = 0;
        object objlock = new object();
        public override void DisplayNumber()
        {
            base.DisplayNumber();

            /*** Test ***/

            //string _strNumber = "54-V1-67894";
            //this._checkInData.PrefixNumberVehicle = _strNumber.Substring(0, 5);
            //this._checkInData.VehicleNumber = _strNumber.Substring(6);
            //
            if (this.CustomerInfo != null && this.CustomerInfo.VehicleRegistrationInfo.VehicleNumber != null)
                this.CustomerInfo.VehicleRegistrationInfo.VehicleNumber = this.CustomerInfo.VehicleRegistrationInfo.VehicleNumber;
            if (this._checkInData == null || string.IsNullOrWhiteSpace(this.CustomerInfo.VehicleRegistrationInfo.VehicleNumber)) return;
            this._checkInData.VehicleNumber = ExtractVehicleNumber(this.CustomerInfo.VehicleRegistrationInfo.VehicleNumber.Replace(".", ""));
            this._checkInData.PrefixNumberVehicle = ExtractPrefixVehicleNumber(this.CustomerInfo.VehicleRegistrationInfo.VehicleNumber.Replace(".", ""), this._checkInData.VehicleNumber);
            //this._checkInData.PrefixNumberVehicle = this.CustomerInfo.VehicleRegistrationInfo.VehicleNumber.Substring(0, 5);
            //if (this.CustomerInfo.VehicleRegistrationInfo.VehicleNumber.Length >= 6)
            //    this._checkInData.VehicleNumber = this.CustomerInfo.VehicleRegistrationInfo.VehicleNumber.Substring(6);

        }
        private bool ReadControllerCardFlag = false;
        //[SerilogTrace]
        public override void GreenHandButtonClicked(object sender, GreenHandButtonEventArgs e)
        {
            base.GreenHandButtonClicked(sender, e);
            if (e.ex != null)
                return;
            if (e.EventType == Section.BarrierHardButtonCode)
                HandForcedBarier();
            if (Section.TcpIpControllerCards != null && Section.TcpIpControllerCards.Count > 0)
            {
                foreach (var ifo in Section.TcpIpControllerCards)
                {
                    if (ifo.ActiveCode == e.EventType)
                    {
                        ReadControllerCardFlag = true;
                        HandleError(IconEnums.Parking, "UHF READER IS ACTIVE", true, false);
                        break;
                    }
                    if (ifo.InactiveCode == e.EventType)
                    {
                        ReadControllerCardFlag = false;
                        HandleError(IconEnums.Close, "UHF READER IS DEACTIVE", true, false);
                        break;
                    }
                }
            }
        }
        private bool CallFlag()
        {
            foreach (var ifo in Section.TcpIpControllerCards)
            {
                if (ifo.ActiveCode == 0)
                {
                    return true;
                }
            }
            return ReadControllerCardFlag;
        }
        private DateTime chekHandleBarrier = DateTime.Now;
        //[SerilogTrace]
        private void HandForcedBarier()
        {

            Task.Factory.StartNew(() => OpenBarrier());
            ForcedInfo data = new ForcedInfo();
            data.Lane = Section.Lane.Name + " - IN";
            data.PCAddress = string.Format("{0}-{1}", _userPreferenceService.HostSettings.Terminal.Ip, _userPreferenceService.HostSettings.Terminal.Name);
            data.User = Section.UserService.CurrentUser.DisplayName;
            var now = TimeMapInfo.Current.LocalTime;
            //var now = DateTime.Now;
            data.ForcedTimeStamp = TimestampConverter.DateTime2Timestamp(now);
            _frontImage = Section.FrontInCamera.CaptureImage();
            _backImage = Section.BackInCamera.CaptureImage();

            using (Image tmpFront = (Image)_frontImage.Clone())
            {
                ImageUtility.Watermark(tmpFront as Bitmap, "Forced_Front" + now.ToString("dd/MM/yyyy HH:mm:ss"));
                data.ReferenceFrontImage = tmpFront.ToByteArray(ImageFormat.Jpeg);
            }
            using (Image tmpBack = (Image)_backImage.Clone())
            {
                ImageUtility.Watermark(tmpBack as Bitmap, "Forced_Back" + now.ToString("dd/MM/yyyy HH:mm:ss"));
                data.ReferenceBackImage = tmpBack.ToByteArray(ImageFormat.Jpeg);
            }
            _server.ForcedBarier(data, (result, ex) =>
            {
                if (ex != null)
                {
                    if (ex is InternalServerErrorException)
                    {
                        HandleError(IconEnums.Error, "FORCED OPEN BARRIER. CANN'T SAVE DATA!!!" + now.ToString("yyyy-MM-dd HH:mm:ss"), false, false);

                        //HandleError(IconEnums.Error, "Mở Cưỡng bức Barrier không lưu được dữ liệu" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), false, false);
                        PrintLog<CheckInLaneViewModel>(ex, _userPreferenceService.HostSettings.LogServerIP);
                    }
                    else
                    {
                        HandleError(IconEnums.Error, "FORCED OPEN BARRIER. CANN'T SAVE DATA!!!" + now.ToString("yyyy-MM-dd HH:mm:ss"), false, false);
                        //HandleError(IconEnums.Error, "Mở Cưỡng bức Barrier không lưu được dữ liệu" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), false, false);
                        PrintLog<CheckInLaneViewModel>(ex);
                    }
                }
                else
                {
                    HandleError(IconEnums.Guide, "YOU JUST OPEN BARIER BY BUTTON", false, false);
                }
            });
        }
        private bool CheckTracker()
        {
            if (this.Section == null || this.Section.OptionByLane == null || this.Section.OptionByLane.MethodTracker == UsageCameraTrackerMethod.Unusage)
                return true;
            else if (this.Section.OptionByLane.MethodTracker == UsageCameraTrackerMethod.AllowInOutWhenTrackerCamBackOnBlue)
            {
                if (this.Section.BackInCamera == null || this.Section.BackInCamera.RawCamera == null)
                    return false;
                return this.Section.BackInCamera.RawCamera.BlueTriggerStatus;
            }
            else if (this.Section.OptionByLane.MethodTracker == UsageCameraTrackerMethod.AllowInOutWhenTrackerCamBackOnRedAndBlue)
            {
				if (this.Section.BackInCamera == null || this.Section.BackInCamera.RawCamera == null)
					return false;
				return this.Section.BackInCamera.RawCamera.RedTriggerStatus && this.Section.BackInCamera.RawCamera.BlueTriggerStatus;
            }
            else if (this.Section.OptionByLane.MethodTracker == UsageCameraTrackerMethod.AllowInOutWhenTrackerCamFrontOnBlue)
            {
				if (this.Section.FrontInCamera == null || this.Section.FrontInCamera.RawCamera == null)
					return false;
				return this.Section.FrontInCamera.RawCamera.BlueTriggerStatus;
            }
            else if (this.Section.OptionByLane.MethodTracker == UsageCameraTrackerMethod.AllowInOutWhenTrackerCamFrontOnRedAndBlue)
            {
				if (this.Section.FrontInCamera == null || this.Section.FrontInCamera.RawCamera == null)
					return false;
				return this.Section.FrontInCamera.RawCamera.RedTriggerStatus && this.Section.FrontInCamera.RawCamera.BlueTriggerStatus;
            }
            else { return false; }
        }
        //[SerilogTrace]
        public override void GreenReadingCompleted(object sender, GreenCardReaderEventArgs e)
        {
			if (!CheckTracker())
			{
				base.GreenReadingCompleted(sender, e);
				return;
			}
			if (e.ex != null)
            {
                base.GreenReadingCompleted(sender, e);
                return;
            }
            if (sender is TcpIpServerCardReader && !FarCards.Exists(fc => fc.CardId == e.CardID))
            {
                base.GreenReadingCompleted(sender, e);
                return;
            }
            if (sender.ToString() == "Tcp Ip Controller card" &&
               (string.IsNullOrEmpty(this.Section.Door) || string.IsNullOrEmpty(this.Section.Reader) ||
                   e.Door != this.Section.Door || e.Reader != this.Section.Reader)
               )
            {
                base.GreenReadingCompleted(sender, e);
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
                {
                    base.GreenReadingCompleted(sender, e);
                    return;
                }
            }
            if (e.CardReader != null && e.CardReader.Info.Type == "Remode Card")
            {
                if (!string.IsNullOrEmpty(this.Section.KeyMap.GetKey(KeyAction.ActivateProlificCardReader)) && !ProlificCardReaderOn)
                {
                    base.GreenReadingCompleted(sender, e);
                    return;
                }
                ProlificCardReaderOn = false;
            }
            lock (objlock)
            {
                Interlocked.Increment(ref flag);
                var curTime = Environment.TickCount;
                if (flag > 1 && (curTime - lastReadTime) < 3000)
                    return;
                Interlocked.Exchange(ref flag, 1);
                lastReadTime = curTime;
            }

            base.GreenReadingCompleted(sender, e);

            if (_notExistedCards.ContainsKey(e.CardID) && _notExistedCards[e.CardID] > 3)
            {
                return;
            }

            BeginCheckin((data, ex) =>
            {
                Interlocked.Exchange(ref flag, 0);
            });

            //Thread.Sleep(3000);
        }
		//[SerilogTrace]
		protected override void ValidFarCardHandle(DetectedTag tag, string extention)
		{
            
            base.ValidFarCardHandle(tag, extention);
            if (!CheckTracker())
            {
				validating = false;
				return;
            }
            if (tag.Avlaible)
            {
				CheckInExec(tag);
			}
            else if (_settings.FarCardUsageRules.AllowCollectFarCard)
            {
                _server.CollectCard(tag.EPC, extention, ex =>
                {
                    if (ex == null)
                    {
                        CheckInExec(tag);
					}
                    else
                    {
                        validating = false;

					}
                });
            }
        }
		#region Auto Plate Reconition
		private Dictionary<string, DateTime> _processedPlate = new Dictionary<string, DateTime>();
        private string CurrentPlate = string.Empty;
        private string lastVehicleNumber = string.Empty;
		private DateTime LastAnpr = DateTime.Now;
		private DateTime n = DateTime.Now;
		private DateTime LastProcessed = DateTime.Now;
        private bool _doAuto = false;
		private bool autoStart = false;
        private Task _autoTask;
		private bool CheckLastProcessed(string cardId)
        {
            lock (_locker)
            {
				var now = DateTime.Now;
				var expiredKeys = _processedPlate
					.Where(kvp => (now - kvp.Value).TotalSeconds > this.Section.OptionByLane.IgnoredDurationForProcessedPlate) // giữ tối đa 30s
					.Select(kvp => kvp.Key)
					.ToList(); // tránh sửa khi đang lặp

				foreach (var key in expiredKeys)
				{
					_processedPlate.Remove(key);
				}
				return _processedPlate.ContainsKey(cardId);
            }
        }
        private void StartAutoPlate()
        {
            if(this.Section!=null && this.Section.OptionByLane!=null && this.Section.OptionByLane.AutoInByVehicleRecognition)
            {
				autoStart = true;
				_autoTask = Task.Factory.StartNew(() => doTaskAutoAnpr(), TaskCreationOptions.LongRunning);
	
			}
        }
		private void doTaskAutoAnpr()
        {
            while (autoStart)
            {
				if (_doAuto || !CheckTracker() || TimeAgo.IsDailog)
				{
					Thread.Sleep(3000);
					continue;
				}
				n = DateTime.Now;
				if ((n - LastProcessed).TotalMilliseconds < this.Section.OptionByLane.AmountSecondsDelayForNext * 1000)
                {
                    Thread.Sleep(1000);
					continue;
				}					
                    
				try
				{
					_doAuto = true;
					var AmountRetry = 3;
					string tmpV = string.Empty;
					for (int i = 0; i < AmountRetry; i++)
					{
						var img = Section.BackInCamera.CaptureImage();
						if (img != null)
						{
							this.LastRecognitPlate = DateTime.Now;
							var sw = new Stopwatch();
							sw.Start();
							var chk = true;
							_alprService.RecognizeLicensePlate(img.ToByteArray(ImageFormat.Jpeg), (rs, ex) => {
								chk = false;
                                if (ex == null)
                                {
                                    //rs = "51H-22553";// rs;
                                    tmpV = rs;
                                }
                                if (rs != lastVehicleNumber)
                                {
                                    lastVehicleNumber = rs;
                                    LastAnpr = DateTime.Now;
								}
                                else if((n-LastAnpr).TotalMilliseconds < 5000 && (n - LastAnpr).TotalMilliseconds>0)
								{
                                    tmpV = string.Empty;
								}
                            });
							while (chk && sw.ElapsedMilliseconds < 3000) ;
							sw.Stop();
							
							if (!string.IsNullOrEmpty(tmpV) && tmpV.Length >= 7)
							{
								var chkA = CheckAvalable(tmpV);
								if (chkA.Item3 && !CheckLastProcessed(chkA.Item1))
								{
									CurrentPlate = chkA.Item2;
									lock (objlock)
									{
										Interlocked.Increment(ref flag);
										var curTime = Environment.TickCount;
										if (flag > 1 && (curTime - lastReadTime) < 3000)
										{
											validating = false;
											return;
										}
										Interlocked.Exchange(ref flag, 1);
										lastReadTime = curTime;
									}

									this.CheckedCard = new Card(chkA.Item1);
                                    this.InvokeOnMainThread(() => {
										BeginCheckin((data, ex) =>
										{
											validating = false;
											Interlocked.Exchange(ref flag, 0);
										}, CurrentPlate);
									});	
									Thread.Sleep(1000);
									break;
								}
								else
								{
                                    if (AllowConfirmToCollectVisitor)
                                    {
                                        if (CurrentPlate != tmpV || (n - LastAnpr).TotalMilliseconds > 5000)
                                        {
                                            TimeAgo.IsDailog = true;
											MainualPlateDoing?.Invoke(tmpV); 
                                        }
									}
                                    else
                                    {
										HandleError(IconEnums.Guide, $"BSX: {tmpV} Chưa đăng ký", true, false);
										Thread.Sleep(300);
									}
								}

							}
							else
							{
								Thread.Sleep(1000);
							}
						}
						else
						{
							Thread.Sleep(300);
						}
					}
				}
				catch
				{
					Thread.Sleep(1000);
				}
				finally
				{
					_doAuto = false;
				}
			}
        }
		private void StopAutoPlate()
        {
			autoStart =false;
        }
		public void PlateCheckIn(string plate)
		{
            doAnpr = true;
            string card_id = "";
            string vehicleNumber = "";
            var cardId = plate.Replace("-", "_");
			var sw = new Stopwatch();
			sw.Start();
			var chk = true;
			_server.CollectPlate(cardId, plate, (res, ex) =>
            {
                chk = false;
				if (ex == null) {
                    card_id = res.CardId;
                    vehicleNumber = res.VehicleNumber;
                }
            });
			while (chk && sw.ElapsedMilliseconds < 3000) ;
			sw.Stop();
			if (!string.IsNullOrEmpty(card_id) && !string.IsNullOrEmpty(vehicleNumber))
			{
				CurrentPlate = vehicleNumber;
				lock (objlock)
				{
					Interlocked.Increment(ref flag);
					var curTime = Environment.TickCount;
					if (flag > 1 && (curTime - lastReadTime) < 3000)
					{
						validating = false;
						return;
					}
					Interlocked.Exchange(ref flag, 1);
					lastReadTime = curTime;
				}

				this.CheckedCard = new Card(card_id);
				this.InvokeOnMainThread(() => {
					BeginCheckin((data, ex) =>
					{
						validating = false;
						Interlocked.Exchange(ref flag, 0);
						_doAuto = false;
					}, CurrentPlate);
				});
			}
            else
            {
				_doAuto = false;
			}
		}
		#endregion
		private void CheckInExec(DetectedTag tag)
        {
			lock (objlock)
			{
				Interlocked.Increment(ref flag);
				var curTime = Environment.TickCount;
				if (flag > 1 && (curTime - lastReadTime) < 3000)
				{
					validating = false;
					return;
				}
				Interlocked.Exchange(ref flag, 1);
				lastReadTime = curTime;
			}
			lock (_locker)
			{
				_processedTags[tag.EPC] = tag.DetectedAt;
			}
			this.CheckedCard = new Card(tag.EPC);
			BeginCheckin((data, ex) =>
			{
				validating = false;
				Interlocked.Exchange(ref flag, 0);
			});
		}
		
        //public override void ReadingCompleted(object sender, CardReaderEventArgs e)
		//{
		//    
		//    //this.Section.CardReaders[0].Run

		//    // thread-safe when doing checkin
		//    lock (this)
		//    {
		//        Interlocked.Increment(ref flag);u
		//        var curTime = Environment.TickCount;
		//        if (flag > 1 && (curTime - lastReadTime) < 3000)
		//            return;
		//        Interlocked.Exchange(ref flag, 1);
		//        lastReadTime = curTime;
		//    }

		//    base.ReadingCompleted(sender, e);

		//    BeginCheckin((data, ex) =>
		//    {
		//        Interlocked.Exchange(ref flag, 0);
		//    });
		//}

		public void SimulateTapCheckIn(Action<ISection, TestCard, Exception> complete)
        {
            Mvx.Resolve<IWebApiTestingServer>().GetCardCheckin(this.Section, (card, exception) =>
            {
                if (card == null || card.Delay == -1)
                {
                    if (complete != null)
                        complete(Section, card, exception);
                    return;
                }

                if (card.CardId != null)
                {
                    string cardId = card.CardId;
                    this.CheckedCard = new Card(card.CardId.ToString());
                    
                    if (this.CheckInData != null)
                        UpdateCheckIn(this.CheckInData.GetClone, null);
                    if (cardId != null)
                    {
                        BeginCheckin((result, ex) =>
                        {
                            complete(Section, card, ex);
                        });
                    }
                }
                else
                {
                    if (complete != null)
                        complete(Section, card, exception);
                }
            });
        }

        //public override void TakingOffCompleted(object sender, CardReaderEventArgs e)
        //{
        //    base.TakingOffCompleted(sender, e);
        //    var tempCheckinData = CheckInData;
        //    if (tempCheckinData != null)
        //        _editTimer.Start(tempCheckinData.CardId);
        //}
        //[SerilogTrace]
        public override void GreenTakingOffCompleted(object sender, GreenCardReaderEventArgs e)
        {
            if (sender.ToString() == "Tcp Ip Controller card" && !CallFlag() &&
                (string.IsNullOrEmpty(this.Section.Door) || string.IsNullOrEmpty(this.Section.Reader) ||
                    e.Door != this.Section.Door || e.Reader != this.Section.Reader)
                )
                return;
            base.GreenTakingOffCompleted(sender, e);
            //var tempCheckinData = CheckInData;
            //if (tempCheckinData != null && e.ex == null)
            //    _editTimer.Start(tempCheckinData.CardId);
        }
        //[SerilogTrace]
        /// <summary>
        /// Updates the check in.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="complete">The complete.</param>
        public void UpdateCheckIn(CheckIn data, Action<CheckIn, Exception> complete)
        {
            if (data == null)
            {
                if (complete != null)
                    complete(null, null);
                return;
            }
            // Set entry check from setting
            data.EntryCheck = _settings.EntryCheck;

            _server.UpdateCheckIn(data, (result, ex) =>
            {
                if (ex != null)
                {
                    if (ex is InternalServerErrorException)
                    {
                        HandleError(IconEnums.Error, GetText("checkin.something_wrong_update"), false, true);

                        //hanle :BlacklistPlateDetected
                        PrintLog<CheckInLaneViewModel>(ex, _userPreferenceService.HostSettings.LogServerIP);
                    }
                    else if (RequestExceptionManager.GetExceptionMessage<CheckIn>(ex.Message).Key ==
                        RequestExceptionEnum.BlacklistPlateDetected)
                    {
                        HandleError(IconEnums.Error, GetText("checkin.blacklist_plate_detected"), false, true);

                        PrintLog<CheckInLaneViewModel>(ex, _userPreferenceService.HostSettings.LogServerIP);
                    }
                    else
                    {
                        PrintLog<CheckInLaneViewModel>(ex);
                    }
                }

                if (data.VehicleNumber != _previousCheckInVehicleNumber && result != null && result.EntryCount > 0 && _settings.EntryCheck)
                {
                    _previousCheckInVehicleNumber = data.VehicleNumber;
                    ShowEntries = true;
                }
                if (ex == null && _editTimer.Enabled && data != null && data.CardId == _editTimer.CurrentCardId)
                {
                    var customerInfo = this.CheckInData.CustomerInfo;
                    this.CheckInData = data;
                    this.CheckInData.CustomerInfo = customerInfo;
                    //FindAndNotifyBlacklist();
                }
                if (complete != null)
                    complete(result, ex);
            });
        }

        private void ResetAutoCheckInTimer()
        {
            _autoCheckInTimer.Stop();
            _autoCheckInTimer.Start();
        }
        //private void Swap<T>(ref T a, ref T b)
        //{
        //    T tmp = a;
        //    a = b;
        //    b = tmp;
        //}

        public event EventHandler ShowPopupBarrier;
        protected override void OnKeyPressed(KeyPressedMessage msg)
        {
            //msg.KeyEventArgs.Handled = false;
            //KeyResponse response = _keyService.HandleLaneKey<CheckIn>(this.Section, msg.KeyEventArgs);     
            VehicleType _type = null;
            string output;
            KeyAction action = this.Section.KeyMap.GetAction(msg.KeyEventArgs, out output);
            switch (action)
            {
                case KeyAction.Number:
                    {
                        var tempCheckInData = CheckInData;
                        if (tempCheckInData == null)
                            return;
                        if (_notEditPlateNumberYet)
                        {
                            tempCheckInData.VehicleNumber = string.Empty;
                            _notEditPlateNumberYet = false;
                            _allowCheckIn = true;
                            ResetAutoCheckInTimer();
                        }
                        tempCheckInData.VehicleNumber = tempCheckInData.VehicleNumber.TrimEnd() + output + " "; //response.Output                       
                        break;
                    }
                case KeyAction.Delete:
                    {
                        var tempCheckInData = CheckInData;
                        if (tempCheckInData == null)
                            return;
                        int leng = tempCheckInData.VehicleNumber.Length;
                        if (leng > 0)
                            tempCheckInData.VehicleNumber = tempCheckInData.VehicleNumber.Remove(leng - 1);
                        ResetAutoCheckInTimer();
                        break;
                    }
                case KeyAction.Search:
                    {
                        ShowSearchCommand.Execute(null);
                        break;
                    }
                case KeyAction.Logout:
                    {
                        //ShowShiftReportCommand.Execute(null);
                        //ShowConfirmLogout();
                        ConfirmLogoutCommand.Execute(null);
                        break;
                    }
                case KeyAction.ChangeLaneDirection:
                    {
                        ChangeLaneDirectionCommand.Execute(null);
                        break;
                    }
                case KeyAction.ShowVehicleType:
                    if (CustomerInfo != null && CustomerInfo.VehicleRegistrationInfo == null)
                        return;
                    ShowChooseVehicleType = true;
                    break;
                case KeyAction.ChangeLane:
                    ChangeLaneCommand.Execute(null);
                    break;
                case KeyAction.ActivateProlificCardReader:
                    ToggleProlificCardReader.Execute(null);
                    break;
                case KeyAction.ActivateSoyalCardReader:
                    ResetSoyalCardReader.Execute(null);
                    break;
                case KeyAction.AddNewNumber:
                    if (CheckInData == null) return;
                    //this.DisplayNumber();
                    _allowCheckIn = true;
                    if (!this._allowCheckIn && _hasConfirmChecking)
                    { return; }
                    if (isfirst)
                        ProcessAfterCheckInSuccess(CheckInData, _checkInResult);
                    UpdateCheckIn(this.CheckInData.GetClone, null);
                    _hasConfirmChecking = false;
                    _allowupdate = true;
                    //_editTimer.SetInterval(1);

                    break;
                case KeyAction.TypeBike:
                    TypeHelper.GetVehicleType((int)KeyAction.TypeBike, result =>
                    {
                        if (result != null)
                            _type = result;
                    });
                    if (_type != null)
                    {
                        ChooseVehicleType(_type);
                        HandleError(IconEnums.update_ok, GetText("VehicleType.Bike"), true, false);
                    }
                    break;
                case KeyAction.TypeCar:
                    TypeHelper.GetVehicleType((int)KeyAction.TypeCar, result =>
                    {
                        if (result != null)
                            _type = result;
                    });
                    if (_type != null)
                    {
                        ChooseVehicleType(_type);
                        HandleError(IconEnums.update_ok, GetText("VehicleType.Car"), true, false);
                    }
                    break;
                case KeyAction.ConfirmCheckInKey:
                    if (CheckInData == null) return;
                    this.DisplayNumber();
                    _allowCheckIn = true;
                    if (!this._allowCheckIn && _hasConfirmChecking)
                    { return; }
                    if (isfirst)
                        ProcessAfterCheckInSuccess(CheckInData, _checkInResult);
                    UpdateCheckIn(this.CheckInData.GetClone, null);
                    _hasConfirmChecking = false;
                    _allowupdate = true;
                    //_editTimer.SetInterval(1);

                    break;
                case KeyAction.CancelCheckInKey:
                    if (CheckInData == null || !isfirst) return;
                    _allowCheckIn = false;
                    CancelCheckIn();
                    _hasConfirmChecking = false;
                    _allowupdate = false;
                    break;
                case KeyAction.ForcedBarier:
                    if (isdoinng)
                    {
                        if (_userPreferenceService.OptionsSettings.BarrierForcedWithPopup)
                        {
                            if (ShowPopupBarrier != null)
                            {
                                ShowPopupBarrier(this, null);
                            }
                        }
                        else
                        {
                            ForcedBarier("...");
                        }
                    }
                    break;
                case KeyAction.CashDrawer:
                    Task.Factory.StartNew(() => Opencash());
                    break;

            }
        }
        private bool isdoinng = true;
        private void Opencash()
        {
            if (string.IsNullOrEmpty(Section.ComCash))
                return;
            if (_userPreferenceService.OptionsSettings.IsSfactorsCom)
            {
                string comname = this.Section.ComCash.ToUpper();
                ComManagement sftCom = ComManagement.GetInstance();
                sftCom.AddCommand(new ComParameter()
                {
                    ComName = comname,
                    TimeApply = DateTime.Now,
                    Description = string.Format("Mở Cash tính tiền Check-in, Lane:{0}", this.Section.LaneName),
                    Commands = new List<ComCommand>()
                    {
                        new ComCommand()
                        {
                            Command="Write",
                            CommandMessage="1"
                        }
                    }
                });
            }
            else
            {
                CashDrawer cash = new CashDrawer(Section.ComCash.ToUpper());
                cash.Open();
            }
        }
        private void AlarmWarning()
        {
            if (string.IsNullOrEmpty(Section.ComAlarm) || string.IsNullOrEmpty(Section.AlarmWarningKeys))
                return;
            if (_userPreferenceService.OptionsSettings.IsSfactorsCom)
            {
                string comname = this.Section.ComAlarm.ToUpper();
                ComManagement sftCom = ComManagement.GetInstance();
                ComParameter comparam = new ComParameter();
                comparam = new ComParameter()
                {
                    ComName = comname,
                    TimeApply = DateTime.Now,
                    Description = string.Format("Đèn cảnh báo Check-in, Lane:{0}", this.Section.LaneName),
                    Commands = new List<ComCommand>()
                    {
                        new ComCommand()
                        {
                            Command="Write",
                            CommandMessage=Section.AlarmWarningKeys
                        }
                    }
                };
                sftCom.AddCommand(comparam);
            }
            else
            {
                ComAlarm com = new ComAlarm(Section.ComAlarm.ToUpper());
                com.Open(Section.AlarmWarningKeys);
            }
        }
        private void AlarmSuccess()
        {
            if (string.IsNullOrEmpty(Section.ComAlarm) || string.IsNullOrEmpty(Section.AlarmSuccessKeys))
                return;
            if (_userPreferenceService.OptionsSettings.IsSfactorsCom)
            {
                string comname = this.Section.ComAlarm.ToUpper();
                ComManagement sftCom = ComManagement.GetInstance();
                ComParameter comparam = new ComParameter();
                comparam = new ComParameter()
                {
                    ComName = comname,
                    TimeApply = DateTime.Now,
                    Description = string.Format("Đèn báo Check-in thành công, Lane:{0}", this.Section.LaneName),
                    Commands = new List<ComCommand>()
                    {
                        new ComCommand()
                        {
                            Command="Write",
                            CommandMessage=Section.AlarmSuccessKeys
                        }
                    }
                };
                sftCom.AddCommand(comparam);
            }
            else
            {
                ComAlarm com = new ComAlarm(Section.ComAlarm.ToUpper());
                com.Open(Section.AlarmSuccessKeys);
            }
        }
        private void Alarm(string key)
        {
            if (key == "success")
            {

                AlarmSuccess();
            }

            else if (key == "fail")
                AlarmWarning();
        }
        //[SerilogTrace]
        public void ForcedBarier(string note)
        {
            isdoinng = false;
            Task.Factory.StartNew(() => OpenBarrier());
            ForcedInfo data = new ForcedInfo();
            data.Lane = this.Section.Lane.Name + " - IN";
            data.PCAddress = string.Format("{0}-{1}", _userPreferenceService.HostSettings.Terminal.Ip, _userPreferenceService.HostSettings.Terminal.Name);
            data.User = this.Section.UserService.CurrentUser.DisplayName;
            var now = TimeMapInfo.Current.LocalTime;
            //var now = DateTime.Now;
            data.ForcedTimeStamp = TimestampConverter.DateTime2Timestamp(now);
            _frontImage = Section.FrontInCamera.CaptureImage();
            _backImage = Section.BackInCamera.CaptureImage();

            using (Image tmpFront = (Image)_frontImage.Clone())
            {
                ImageUtility.Watermark(tmpFront as Bitmap, "Forced_Front" + now.ToString("dd/MM/yyyy HH:mm:ss"));
                data.ReferenceFrontImage = tmpFront.ToByteArray(ImageFormat.Jpeg);
            }
            using (Image tmpBack = (Image)_backImage.Clone())
            {
                ImageUtility.Watermark(tmpBack as Bitmap, "Forced_Back" + now.ToString("dd/MM/yyyy HH:mm:ss"));
                data.ReferenceBackImage = tmpBack.ToByteArray(ImageFormat.Jpeg);
            }
            data.Note = note;
            _server.ForcedBarier(data, (result, ex) =>
            {
                if (ex == null)
                {
                    SaveForcedBrImage(result);
                }

                isdoinng = true;
                if (ex != null)
                {
                    if (ex is InternalServerErrorException)
                    {
                        HandleError(IconEnums.Error, "Mở Cưỡng bức Barrier không lưu được dữ liệu" + now.ToString("yyyy-MM-dd HH:mm:ss"), false, false);

                        //HandleError(IconEnums.Error, "Mở Cưỡng bức Barrier không lưu được dữ liệu" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), false, false);
                        PrintLog<CheckInLaneViewModel>(ex, _userPreferenceService.HostSettings.LogServerIP);
                    }
                    else
                    {
                        HandleError(IconEnums.Error, "Mở Cưỡng bức Barrier không lưu được dữ liệu" + now.ToString("yyyy-MM-dd HH:mm:ss"), false, false);
                        //HandleError(IconEnums.Error, "Mở Cưỡng bức Barrier không lưu được dữ liệu" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), false, false);
                        PrintLog<CheckInLaneViewModel>(ex);
                    }
                }
                else
                {
                    HandleError(IconEnums.Guide, "Bạn vừa Mở cưỡng bức Barrier", false, false);
                }
            });

        }

        private void SaveForcedBrImage(ForcedInfo forceInfo)
        {
            _storageService.SaveImage(
                new List<string>() { forceInfo.FrontImagePath, forceInfo.BackImagePath },
                new List<byte[]>() { forceInfo.ReferenceFrontImage, forceInfo.ReferenceBackImage },
                (List<Exception> lstExceptions) =>
                {
                    foreach (Exception exception in lstExceptions)
                    {
                        if (exception != null)
                        {
                            PrintLog<CheckInLaneViewModel>(exception);
                            return;
                        }
                    }
                });
        }

        private void HandleForcedBarier(ISection section)
        {
            Task.Factory.StartNew(() => OpenBarrier());
            ForcedInfo data = new ForcedInfo();
            data.Lane = section.Lane.Name + " - IN";
            data.PCAddress = string.Format("{0}-{1}", _userPreferenceService.HostSettings.Terminal.Ip, _userPreferenceService.HostSettings.Terminal.Name);
            data.User = section.UserService.CurrentUser.DisplayName;
            var now = TimeMapInfo.Current.LocalTime;
            //var now = DateTime.Now;
            data.ForcedTimeStamp = TimestampConverter.DateTime2Timestamp(now);
            _frontImage = Section.FrontInCamera.CaptureImage();
            _backImage = Section.BackInCamera.CaptureImage();

            using (Image tmpFront = (Image)_frontImage.Clone())
            {
                ImageUtility.Watermark(tmpFront as Bitmap, "Forced_Front" + now.ToString("dd/MM/yyyy HH:mm:ss"));
                data.ReferenceFrontImage = tmpFront.ToByteArray(ImageFormat.Jpeg);
            }
            using (Image tmpBack = (Image)_backImage.Clone())
            {
                ImageUtility.Watermark(tmpBack as Bitmap, "Forced_Back" + now.ToString("dd/MM/yyyy HH:mm:ss"));
                data.ReferenceBackImage = tmpBack.ToByteArray(ImageFormat.Jpeg);
            }
            _server.ForcedBarier(data, (result, ex) =>
            {
                if (ex != null)
                {
                    if (ex is InternalServerErrorException)
                    {
                        HandleError(IconEnums.Error, "Mở Cưỡng bức Barrier không lưu được dữ liệu" + now.ToString("yyyy-MM-dd HH:mm:ss"), false, false);

                        //HandleError(IconEnums.Error, "Mở Cưỡng bức Barrier không lưu được dữ liệu" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), false, false);
                        PrintLog<CheckInLaneViewModel>(ex, _userPreferenceService.HostSettings.LogServerIP);
                    }
                    else
                    {
                        HandleError(IconEnums.Error, "Mở Cưỡng bức Barrier không lưu được dữ liệu" + now.ToString("yyyy-MM-dd HH:mm:ss"), false, false);
                        //HandleError(IconEnums.Error, "Mở Cưỡng bức Barrier không lưu được dữ liệu" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), false, false);
                        PrintLog<CheckInLaneViewModel>(ex);
                    }
                }
                else
                {
                    HandleError(IconEnums.Guide, "Bạn vừa Mở cưỡng bức Barrier", false, false);
                }
            });
        }
        //[SerilogTrace]
        private void CancelCheckIn()
        {
            if (CheckInData != null)
            {
                var checkOut = PackCheckOutData();
                checkOut.is_cancel = "Cancel-CheckIn";
                CheckOut(checkOut, (e) =>
                {
                    if (e != null)
                    {
                        var m = e.Message;
                    }
                });
                ResetUIInformation();
                this.Notices.Clear();
                Notices = null;
                _autoCheckInTimer.Stop();
                _editTimer.Stop();
                Countdown = string.Empty;
                ShowCountdown = false;
            }
        }

        private void ResetUIInformation()
        {
            CustomerInfo = null;
            CheckInData = null;
            _allowupdate = false;

            // Reset led
            try
            {
                ShowLed("", "");
            }
            catch (Exception ex1)
            {
                _logger.Error($"[{nameof(ResetUIInformation)}]Reset led {ex1.Message}", ex1);
            }
        }

        public override void Close()
        {

            if (_hasConfirmChecking && this.CheckInData != null)
            {
                CancelCheckIn();
                return;
            }
            if (this.CheckInData != null && _editTimer.Enabled && this.CheckInData.CardId.Equals(_editTimer.CurrentCardId))
            {
                UpdateCheckIn(this.CheckInData.GetClone, (result, ex) =>
                {
                    InvokeOnMainThread(() =>
                    {
                        if (ex != null)
                        {
                            //this.Notices.Clear();in

                            if (ex is InternalServerErrorException)
                            {
                                HandleError(IconEnums.Error, GetText("checkin.something_wrong_update"), true, false);
                                PrintLog<CheckInLaneViewModel>(ex, _userPreferenceService.HostSettings.LogServerIP);
                            }

                            else PrintLog<CheckInLaneViewModel>(ex);
                        }
                        else
                        {
                            if (result != null && result.VehicleNumberExists && !string.IsNullOrEmpty(result.AlprVehicleNumber))
                            {
                                HandleError(IconEnums.Warning, GetText("checkin.same_vehicle_number"), true, false);
                            }
                            else if (result != null && (!result.VehicleNumberExists || string.IsNullOrEmpty(result.VehicleNumber)))
                                HandleError(IconEnums.update_ok, GetText("checkin.update_success"), false, false);
                        }

                        _autoCheckInTimer.Stop();
                    });
                });
            }

            _editTimer.Stop();
            _autoCheckInTimer.Stop();
            this.StopMaintainace();
            this.StopAutoPlate();
            base.Close();
        }

        public override void ChooseVehicleType(VehicleType type)
        {
            if (CheckInData != null)
            {
                CheckInData.VehicleType = type;
                if (this.CheckInData != null)
                    UpdateCheckIn(this.CheckInData.GetClone, null);
                //ResetAutoCheckInTimer();
            }
        }
        private bool IniSerialPortPrinter()
        {
            bool IsIni = false;

            if (CheckOutLaneViewModel._SerialPortPrinter == null)
            {
                //2018-04-07
                string comname = string.IsNullOrWhiteSpace(this.Section.ComPrint) ? "COM22" : this.Section.ComPrint.ToUpper();
                //2018-04-07
                string[] pn = System.IO.Ports.SerialPort.GetPortNames();
                //System.IO.Ports.
                var _portExists = System.IO.Ports.SerialPort.GetPortNames().Any(p => p == comname);
                if (!_portExists) return false;

                CheckOutLaneViewModel._SerialPortPrinter = new System.IO.Ports.SerialPort();
                CheckOutLaneViewModel._SerialPortPrinter.PortName = comname;
                CheckOutLaneViewModel._SerialPortPrinter.BaudRate = 9600;

            }
            else
            {
                if (CheckOutLaneViewModel._SerialPortPrinter.IsOpen)
                {
                    CheckOutLaneViewModel._SerialPortPrinter.Close();
                    CheckOutLaneViewModel._SerialPortPrinter.PortName = string.IsNullOrWhiteSpace(this.Section.ComPrint) ? "COM22" : this.Section.ComPrint.ToUpper();
                }
            }

            try
            {
                if (!CheckOutLaneViewModel._SerialPortPrinter.IsOpen)
                    CheckOutLaneViewModel._SerialPortPrinter.Open();
                IsIni = true;
            }
            catch (Exception ex)
            {
                CheckOutLaneViewModel._SerialPortPrinter.Close();
                CheckOutLaneViewModel._SerialPortPrinter.Dispose();
                CheckOutLaneViewModel._SerialPortPrinter = null;
            }
            return IsIni;
        }
        private void DocPrintToBill()
        {
            PrintDocument printDoc = new PrintDocument();
            if (printDoc.PrinterSettings.IsValid)
            {
                printDoc.PrintPage += PrintDoc_PrintPage;
                Margins margins = new Margins(3, 3, 3, 3);
                printDoc.DefaultPageSettings.Landscape = false;
                printDoc.DefaultPageSettings.Margins = margins;
                printDoc.Print();
            }
        }

        private void PrintDoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            Font font = new Font("Arial", 12, FontStyle.Bold);
            Font font1 = new Font("Arial", 9);
            Font font2 = new Font("Arial", 8, FontStyle.Italic | FontStyle.Bold);
            Font font3 = new Font("Arial", 6, FontStyle.Bold);
            var format = new StringFormat() { Alignment = StringAlignment.Far };
            var formatLeft = new StringFormat() { Alignment = StringAlignment.Near };
            var format1 = new StringFormat() { Alignment = StringAlignment.Center };
            var format11 = new StringFormat() { Alignment = StringAlignment.Center, FormatFlags = StringFormatFlags.FitBlackBox };
            RectangleF rect = new RectangleF(0, 70, e.PageBounds.Width - 30, 15);
            if (_userPreferenceService.OptionsSettings.IsPrintV2)
            {
                rect.Y = 0;
                e.Graphics.DrawString("Sheraton Saigon Hotel & Towers", font, System.Drawing.Brushes.Black, rect, format1);
                rect.Y = 15;
                e.Graphics.DrawString("-------------------------------------------------------------", font2, System.Drawing.Brushes.Black, rect, format1);
                rect.Y = 35;
                e.Graphics.DrawString("Time in:", font1, System.Drawing.Brushes.Black, rect, formatLeft);
                e.Graphics.DrawString(CheckInData.StrCheckInTime, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 50;
                e.Graphics.DrawString("Vehicle type:", font1, System.Drawing.Brushes.Black, rect, formatLeft);
                e.Graphics.DrawString(CheckInData.VehicleType.Name, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 65;
                e.Graphics.DrawString("Card ID:", font1, System.Drawing.Brushes.Black, rect, formatLeft);
                e.Graphics.DrawString(CheckInData.CardLabel, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 80;
                e.Graphics.DrawString("Parking card number:", font1, System.Drawing.Brushes.Black, rect, formatLeft);
                e.Graphics.DrawString(CheckInData.VehicleNumber, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 95;
                e.Graphics.DrawString("-------------------------------------------------------------", font2, System.Drawing.Brushes.Black, rect, format1);
                rect.Y = 110;
                e.Graphics.DrawString("Thank you very much for using our services", font2, System.Drawing.Brushes.Black, rect, format1);
                rect.Y = 125;
                e.Graphics.DrawString("-------------------------------------------------------------", font2, System.Drawing.Brushes.Black, rect, format1);
            }
            else
            {
                rect.Y = 0;
                e.Graphics.DrawString("Hệ thống giữ xe thông minh", font, System.Drawing.Brushes.Black, rect, format1);
                rect.Y = 15;
                e.Graphics.DrawString("-------------------------------------------------------------", font2, System.Drawing.Brushes.Black, rect, format1);
                rect.Y = 35;
                e.Graphics.DrawString("Thời gian vào:", font1, System.Drawing.Brushes.Black, rect, formatLeft);
                e.Graphics.DrawString(CheckInData.StrCheckInTime, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 50;
                e.Graphics.DrawString("Loại phương tiện:", font1, System.Drawing.Brushes.Black, rect, formatLeft);
                e.Graphics.DrawString(CheckInData.VehicleType.Name, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 65;
                e.Graphics.DrawString("Số thẻ:", font1, System.Drawing.Brushes.Black, rect, formatLeft);
                e.Graphics.DrawString(CheckInData.CardLabel, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 80;
                e.Graphics.DrawString("Biển số xe:", font1, System.Drawing.Brushes.Black, rect, formatLeft);
                e.Graphics.DrawString(CheckInData.VehicleNumber, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 95;
                e.Graphics.DrawString("-------------------------------------------------------------", font2, System.Drawing.Brushes.Black, rect, format1);
                rect.Y = 110;
                e.Graphics.DrawString("XIN CHÀO - HẸN GẶP LẠI QUÝ KHÁCH", font2, System.Drawing.Brushes.Black, rect, format1);
                rect.Y = 125;
                e.Graphics.DrawString("-------------------------------------------------------------", font2, System.Drawing.Brushes.Black, rect, format1);
            }
        }
        public override void PrintToPrinter()
        {
            base.PrintToPrinter();

            if (CheckInData == null || CheckInData.CardTypeId != 0)
            {
                return;
            }
            if (this.Section.PrintComActive)
            {
                try
                {
                    if (!IniSerialPortPrinter())
                    {
                        //System.Windows.MessageBox.Show("Vui lòng xem lại cổng máy in!", "Lỗi");
                        HandleError(IconEnums.Error, "Vui lòng xem lại cổng máy in!", false, true);
                        return;
                    }

                    com.clsCom _clsCom = new com.clsCom();
                    // _clsCom.UrlLogo = System.Windows.Forms.Application.StartupPath + "\\config\\logo.ini"; 
                    string[] _Array = new string[10];

                    // _Array[0] = _clsCom.CombineString("NGAY IN BILL :", this._checkOutData.StrCheckOutTime);
                    //_clsCom.XinChao = "Sheraton Saigon Hotel & Towers";
                    if (_userPreferenceService.OptionsSettings.IsPrintV2)
                    {
                        _Array[0] = _clsCom.CombineString(" ", " ");
                        _clsCom.XinChao = "Sheraton Saigon Hotel & Towers";
                        _Array[1] = _clsCom.CombineString(" ", " ");

                        _Array[2] = _clsCom.CombineString("Time in :", CheckInData.StrCheckInTime);
                        _Array[3] = _clsCom.CombineString("Vehicle type :", CheckInData.VehicleType.Name);
                        _Array[4] = _clsCom.CombineString("Card ID :", CheckInData.CardLabel);
                        _Array[5] = _clsCom.CombineString("Parking card number :", CheckInData.VehicleNumber);
                        _Array[6] = _clsCom.CombineString(" ", " ");
                        _Array[7] = _clsCom.CombineString(" ", " ");

                        _Array[8] = _clsCom.CombineString("Thank you very much for using our services", "");
                        _Array[9] = _clsCom.CombineString(" ", " ");
                    }
                    else
                    {
                        _Array[0] = _clsCom.CombineString(" ", " ");
                        _clsCom.XinChao = "Hệ thống giữ xe thông minh";
                        _Array[1] = _clsCom.CombineString(" ", " ");

                        _Array[2] = _clsCom.CombineString("Thời gian vào:", CheckInData.StrCheckInTime);
                        _Array[3] = _clsCom.CombineString("Loại phương tiện:", CheckInData.VehicleType.Name);
                        _Array[4] = _clsCom.CombineString("Mã thẻ:", CheckInData.CardLabel);
                        _Array[5] = _clsCom.CombineString("Biển số xe:", CheckInData.VehicleNumber);
                        _Array[6] = _clsCom.CombineString(" ", " ");
                        _Array[7] = _clsCom.CombineString(" ", " ");

                        _Array[8] = _clsCom.CombineString("XIN CHÀO - HẸN GẶP LẠI QUÝ KHÁCH", "");
                        _Array[9] = _clsCom.CombineString(" ", " ");
                    }
                    byte[] _buffers = _clsCom.CommandESC(_Array);
                    CheckOutLaneViewModel._SerialPortPrinter.Write(_buffers, 0, _buffers.Length);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message, "Error");
                }
            }
            else
                DocPrintToBill();
        }

        #region Private Method

        private void AutoCheckIn(string manualVehicleNumber)
        {
            if (!string.IsNullOrWhiteSpace(manualVehicleNumber))
            {
                ProcessCheckVehicleNumber(manualVehicleNumber);
            }
            else
            {
                CaptureImage();
                _alprService.RecognizeLicensePlate(_backImage.ToByteArray(ImageFormat.Jpeg), (vehicleNumber, ex) =>
                {
                    ProcessCheckVehicleNumber(vehicleNumber);
                });
            }
        }

        private void ProcessCheckVehicleNumber(string vehicleNumber)
        {
            _server.CheckVehicleNumber(vehicleNumber, "", (result, ex1) =>
            {
                if (result.IsValid)
                {
                    CheckedCard = new Card { Id = result.CardId };
                    BeginCheckin((data, ex3) =>
                    {
                        Interlocked.Exchange(ref flag, 0);
                        if (ex3 == null)
                        {
                            AllowManualInputVehicleNumber = false;
                        }
                    });
                }
                else
                {
                    HandleError(IconEnums.Card, string.Format(GetText("checkin.vehicle_number_not_found"), vehicleNumber), false, false);
                    AllowManualInputVehicleNumber = true;
                }
            });
        }

        private void CaptureImage()
        {
            _frontImage = Section.FrontInCamera.CaptureImage();
            _backImage = Section.BackInCamera.CaptureImage();

            //// TODO: Test only
            //_frontImage = Image.FromFile("Images/Car_MaskCar_Plate_637620756716689852.jpg");
            //_backImage = Image.FromFile("Images/Car_MaskCar_Plate_637620756716689852.jpg");
        }

        #endregion
    }
}