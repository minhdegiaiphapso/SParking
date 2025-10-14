using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Messenger;
using NLog;
using SP.Parking.Terminal.Core.Models;
using SP.Parking.Terminal.Core.Services;
using SP.Parking.Terminal.Core.Utilities;
using Green.Devices.Dal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Drawing.Printing;
using Green.Devices.Dal.CardControler;
using Green.Devices.Dal.Siemens;
using Cirrious.MvvmCross.ViewModels;
using System.Windows;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Text;
using System.Windows.Threading;
using System.IO;
using System.Windows.Documents;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Section = SP.Parking.Terminal.Core.Models.Section;

namespace SP.Parking.Terminal.Core.ViewModels
{
    public class CheckoutEventArgs : EventArgs
    {
        public KeyAction Key { get; set; }
    }

    public class CheckOutLaneViewModel : BaseLaneViewModel
    {
        private static System.IO.Ports.SerialPort _SerialPortLed = null;
        private static System.IO.Ports.SerialPort _SerialPortLedB = null;
        public static System.IO.Ports.SerialPort _SerialPortPrinter = null;
        public event EventHandler ShowPopupBarrier;
        public Action<CashierITC> ShowCashier;
        EditableTimer _editTimer = null;
		public System.Action<string> MainualPlateDoing { get; set; }
		public System.Action<long, long> RetailInvoice { get; set; }
        private bool isShowInvoice = false;
        private void ShowRetailInvoice()
        {
			if (isShowInvoice||this.CustomerInfo == null || this._parkingDuration == null 
                || this._checkinData == null || this.CustomerInfo.ParkingFee<=0)
			{
				return;
			}
            isShowInvoice = true;
			RetailInvoice?.Invoke(this._checkinData.ParkingSessionId, (long)(this.CustomerInfo.ParkingFee));
		}
		private MvxCommand _issueinvoiceCommand;
		public ICommand IssueinvoiceCommand
		{
			get
			{
				if (!IsBusy)
					_issueinvoiceCommand = _issueinvoiceCommand ?? new MvxCommand(() =>
					{
                        ShowRetailInvoice();
					});
				return _issueinvoiceCommand;
			}
		}
		public void CallRetailInvoice(long parking_id, long fee, bool completed = true,
			bool has_buyer = false, string buyer_code = null,
			string buyer_name = null, string legal_name = null,
			string taxcode = null, string phone = null, string email = null,
			string address = null, string receiver_name = null, string receiver_emails = null)
        {
            InvokeOnMainThread(() => {
				_server.RetailInvoice(ex => {
                    isShowInvoice = false;
					if (ex == null)
					{
						HandleError(IconEnums.Check, $"Đã xuất hóa đơn thành công", true, false);
					}
					else
					{
						HandleError(IconEnums.Warning, $"Không thể xuất hóa đơn", true, false);
					}
				},
			   parking_id, fee, completed, has_buyer, buyer_code,
			   buyer_name, legal_name, taxcode, phone, email, address,
			   receiver_name, receiver_emails);
			});
		}
		public void CancelCallInvoice()
        {
			isShowInvoice = false;
		}

		public event EventHandler CheckOutCompleted;

        private bool _allowCheckout = true;
        bool _hasConfirmChecking = false;

        string _previousRawVehicleNumber = string.Empty;

        string _referencePrefixNumber;
        public string ReferencePrefixNumber
        {
            get { return _referencePrefixNumber; }
            set
            {
                _referencePrefixNumber = value;
                RaisePropertyChanged(() => ReferencePrefixNumber);
            }
        }
        public bool IsVoucher { get; set; }
        public int CheckInVehicle { get; set; }
        public string CardIDVC { get; set; }
        public int SecondVC { get; set; }
        private System.Windows.Visibility _allow_ShowVoucher = System.Windows.Visibility.Hidden;
        public System.Windows.Visibility CanShowVoucher
        {
            get
            {
                return _allow_ShowVoucher;
            }
            private set
            {
                _allow_ShowVoucher = value;
                RaisePropertyChanged(() => CanShowVoucher);
            }
        }
        private System.Windows.Visibility _allowFree = System.Windows.Visibility.Hidden;
        public System.Windows.Visibility ShowFree
        {
            get
            {
                return _allowFree;
            }
            private set
            {
                _allowFree = value;
                RaisePropertyChanged(() => ShowFree);
            }
        }
        private System.Windows.Visibility _allow_ShowVoucherdetail = System.Windows.Visibility.Hidden;
        public System.Windows.Visibility CanShowVoucherDetail
        {
            get
            {
                return _allow_ShowVoucherdetail;
            }
            private set
            {
                _allow_ShowVoucherdetail = value;
                RaisePropertyChanged(() => CanShowVoucherDetail);
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
            }
        }

        CheckOut _checkOutData;
        public CheckOut CheckOutData
        {
            get { return _checkOutData; }
            set
            {
                if (_checkOutData == value) return;

                _checkOutData = value;

                if (_checkOutData == null)
                {
                    this.ParkingDuration = string.Empty;
                    CardType = null;
                    ReferencePrefixNumber = string.Empty;
                }
                RaisePropertyChanged(() => CheckOutData);
            }
        }

        private CheckIn _checkinData;
        public CheckIn CheckInData
        {
            get { return _checkinData; }
            set
            {
                if (_checkinData == value) return;

                _checkinData = value;
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

        //SP.Parking.Terminal.Core.Models.Terminal _Terminal;
        private Models.Terminal _terminal;
        public Models.Terminal lname
        {
            get { return _terminal; }
            set
            {
                if (_terminal == value) return;
                _terminal = value;
                RaisePropertyChanged(() => lname);
            }
        }
        bool _canCheckout = true;

        private string _parkingDuration;
        public string ParkingDuration
        {
            get { return _parkingDuration; }
            set
            {
                _parkingDuration = value;
                RaisePropertyChanged(() => ParkingDuration);
            }
        }
        private int _countDown;
        private int _countDownMax;
        public int CountDown
        {
            get
            {
                return _countDown;
            }
            set
            {
                _countDown = value;
                RaisePropertyChanged(() => CountDown);
                RaisePropertyChanged(() => StrCountDown);
                RaisePropertyChanged(() => StrCountDownLBL);
            }
        }
        public string StrCountDown
        {
            get
            {
                if (CountDown <= 0)
                    return "";
                else
                    return CountDown.ToString();
            }
        }
        public string StrCountDownLBL
        {
            get
            {
                if (CountDown <= 0)
                    return "";
                else
                    return "Đếm ngược";
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

        MvxCommand<string> _autoCheckOutCommand;
        public ICommand AutoCheckOutCommand
        {
            get
            {
                _autoCheckOutCommand = _autoCheckOutCommand ?? new MvxCommand<string>((manualVehicleNumber) =>
                {
                    AutoCheckOut(manualVehicleNumber);
                });
                return _autoCheckOutCommand;
            }
        }


        public CheckOutLaneViewModel(IViewModelServiceLocator service, IStorageService storageService, IMvxMessenger messenger)
            : base(service, storageService, messenger)
        {
            //_keyService = Mvx.Resolve<IKeyService>();
            this._terminal = _userPreferenceService.HostSettings.Terminal;
            _countDownMax = _userPreferenceService.OptionsSettings.DisplayCheckOutDuration;
            _editTimer = new EditableTimer(_countDownMax);
            
            _editTimer.TimeRunningOut += (sender, e) =>
            {
                //this.CheckOutData = null;
                ResetUIInformation();
                
            };
            _editTimer.Countdown += _editTimer_Countdown;
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
			if (!doAnpr && (n - LastRecognitPlate).TotalSeconds > 10)
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
		private void _editTimer_Countdown(object sender, EventArgs e)
        {
            if (CountDown > 0)
                CountDown--;
            else
                _hasConfirmChecking = false;
        }

        public void Voucher(Voucher voucher)
        {
            _server.CreateVoucher(voucher, ex =>
            {
                InvokeOnMainThread(() =>
                {
                    string msg = string.Empty;
                    if (ex != null)
                    {
                        if (ex is InternalServerErrorException)
                        {
                            HandleError(IconEnums.Error, "Không thể lưu Vouchers", false);
                            PrintLog<CheckOutLaneViewModel>(ex, _userPreferenceService.HostSettings.LogServerIP);
                        }
                        else
                            PrintLog<CheckOutLaneViewModel>(ex);
                    }
                    else
                    {
                        HandleError(IconEnums.Check, "Lưu Vouchers thành công.", false);
                        //this.CustomerInfo.ParkingFee = voucher.Actual_Fee;

                    }

                });


            });
        }
        public void DeleteVoucher(string CardId, DateTime checkinTime)
        {
            _server.DeleteVoucher(CardId, checkinTime, ex =>
            {
                InvokeOnMainThread(() =>
                {
                    ;

                });

            });
        }
        public int Recallfee(string cardId, int voucherhour)
        {
            int res = -1;
            ManualResetEvent rsev = new ManualResetEvent(false);
            _server.RecallFee(cardId, voucherhour,
                (r, ex) =>
                {
                    res = r;
                    rsev.Set();
                }
            );
            rsev.WaitOne();
            return res;
        }
        private Socket _clientSocket;
        public override void Init(ParameterKey key)
        {
            base.Init(key);
            //this.IniSerialPortLed();
            //this.IniSerialPortLedB();
            //SetupDevices();

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
        public override void Start()
        {
            base.Start();

            if (_modeManager.ArgumentParams.Mode == RunMode.Testing)
            {
                this._testingService.Start(() =>
                {
                    var now = TimeMapInfo.Current.LocalTime;
                    _testingService.CreateSchedule("checkout " + now.ToString("yyyyMMdd HHmm"), callback =>
                    {
                        SimulateTapCheckout(callback);
                    });
                    //_testingService.CreateSchedule("checkout " + DateTime.Now.ToString("yyyyMMdd HHmm"), callback =>
                    //{
                    //    SimulateTapCheckout(callback);
                    //});
                });
            }

            _allowCheckout = !_userPreferenceService.OptionsSettings.ForceUpdatePlateNumber;
            if (this.Section != null && this.Section.BarrierBySiemensControl != null && this.Section.BarrierBySiemensControl.Active && !string.IsNullOrEmpty(Section.BarrierBySiemensControl.IP))
            {
                Port4 mydevice = Port4.GetInstance();
                var Lane = GetLane();
                //mydevice.AddCommandIn(new SiemenInfo
                //{
                //    TcpIp = this.Section.BarrierBySiemensControl.IP,
                //    TypeIn = this.Section.BarrierBySiemensControl.TypeIn,
                //    Lane = Lane
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
            this.StartAutoPlate();
            this.StartMaintainace();
		}
        private void RaiseButtonClick(LogoTypeIn4 obj)
        {
            if (this.Section.TemporaryDirection == LaneDirection.Out)
                Task.Factory.StartNew(() => {
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
            if (Section.TemporaryDirection == LaneDirection.Out)
            {
                Task.Factory.StartNew(() => {
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
        }

        private Image _backImage = null;
        private Image _frontImage = null;
        private Image extra1 = null;
        private Image extra2 = null;
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

                CheckOut sendData = new CheckOut();
                //sendData.CheckOutTime = DateTime.Now;
                //sendData.CheckOutTime = _checkinData.CheckOutTimeServer;
                var now = TimeMapInfo.Current.LocalTime;
                sendData.TerminalId = _userPreferenceService.HostSettings.Terminal.Id;
                sendData.CardId = card.Id;
                sendData.LaneId = Section.Lane.Id;
                sendData.OperatorId = User.Id;
                _frontImage = Section.FrontOutCamera.CaptureImage();
                _backImage = Section.BackOutCamera.CaptureImage();

                //// TODO: Test only
                //_frontImage = Image.FromFile("Images/Car_MaskCar_Plate_637620756716689852.jpg");
                //_backImage = Image.FromFile("Images/Car_MaskCar_Plate_637620756716689852.jpg");

                if (Section.IsOutExtra && Section.ExtraOut1Camera != null && Section.ExtraOut2Camera != null)
                {
                    extra1 = Section.ExtraOut1Camera.CaptureImage();
                    extra2 = Section.ExtraOut1Camera.CaptureImage();
                    using (Image tmpExtra1 = (Image)extra1.Clone())
                    {
                        ImageUtility.Watermark(extra1 as Bitmap, card.Id + "  " + now.ToString("dd/MM/yyyy HH:mm:ss"));
                        //ImageUtility.Watermark(extra1 as Bitmap, card.Id + "  " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                        sendData.Extra1Image = tmpExtra1.ToByteArray(ImageFormat.Jpeg);
                    }
                    using (Image tmpExtra2 = (Image)extra2.Clone())
                    {
                        ImageUtility.Watermark(tmpExtra2 as Bitmap, card.Id + "  " + now.ToString("dd/MM/yyyy HH:mm:ss"));
                        //ImageUtility.Watermark(tmpExtra2 as Bitmap, card.Id + "  " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                        sendData.Extra2Image = tmpExtra2.ToByteArray(ImageFormat.Jpeg);
                    }
                }
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

        /// <summary>
        /// Gets the check in data before checking out
        /// </summary>
        public void GetCheckInData(Action<Exception> complete, string plate = null)
        {
            
            try
            {
                ShowFree = System.Windows.Visibility.Hidden;
                var card = this.CheckedCard;
                
                if (card == null || string.IsNullOrEmpty(card.Id))
                {
                    HandleError(IconEnums.Warning, GetText("cannot_read_card"), false, true);
                    if (complete != null)
                        complete(new Exception(GetText("cannot_read_card")));

                    return;
                }
                var timeRide = card.TimeRide;
                var data = PackCheckOutData();

                GetCheckIn(data, (result, ex) =>
                {
                    if (ex != null)
                    {
                        //if (_userPreferenceService.OptionsSettings.IsSfactorsCom)
                        //    Task.Factory.StartNew(() => PrintToLedSfactorsStanard("0 VND", "", 0));
                        if (complete != null)
                            complete(ex);
                    }
                    else
                    {

                        if (result != null)
                        {
                            _checkinData = result;
                            TimeSpan dtClaim = (_checkinData.CheckOutTimeServer - _checkinData.CheckInTimeServer);
                            TimeSpan newTime;
                            if (dtClaim.Seconds > 30)
                            {
                                newTime = dtClaim.Add(TimeSpan.FromSeconds(60 - dtClaim.Seconds));
                            }
                            else
                                newTime = dtClaim;
                            
                            ParkingDuration = ToReadableString(newTime);
                            CustomerInfo = result.CustomerInfo;
                            if (!string.IsNullOrEmpty(result.StrClaimTime))
                                CustomerInfo.StrClaimTime = "Claimed " + result.StrClaimTime;

                            //2024 recallfee if can't get tool fee
                            //if (String.IsNullOrEmpty(CustomerInfo.ParkingFeeDetail) || CustomerInfo.ParkingFeeDetail != "Dynamic tool fee" || CustomerInfo.ParkingFee<=5000)
                            if (_checkinData.CardTypeId == 0)
                            {
                                //callGardenMallfee - 2024
                                switch (DateTime.Now.ToShortDateString())
                                {
                                    case "1/22/2025":
                                    case "1/23/2025":
                                    case "1/24/2025":
                                    case "1/25/2025":
                                    case "1/26/2025":
                                    case "1/27/2025":
                                    case "1/28/2025":
                                    case "1/29/2025":
                                    case "1/30/2025":
                                    case "1/31/2025":
                                    case "2/1/2025":
                                    case "2/2/2025":
                                        CustomerInfo.ParkingFee = recallfeeEvent(_checkinData.CheckOutTimeServer, _checkinData.CheckInTimeServer, _checkinData.VehicleTypeId);
                                        break;
                                    default:
                                        CustomerInfo.ParkingFee = recallfee(_checkinData.CheckOutTimeServer, _checkinData.CheckInTimeServer, _checkinData.VehicleTypeId);
                                        break;
                                }
                            }
                                
                            //Show info to LED matrix
                            if (this.CustomerInfo != null && !string.IsNullOrEmpty(this.CustomerInfo.StrParkingFee) && this.CustomerInfo.StrParkingFee.Trim().Length > 0)
                                Task.Factory.StartNew(() => ShowLed(_checkinData.AlprVehicleNumber, this.CustomerInfo.StrParkingFee));

                            //if (_userPreferenceService.OptionsSettings.IsSfactorsCom)
                            //{
                            //    if (this.CustomerInfo != null)
                            //    {
                            //        int fee = (int)this.CustomerInfo.ParkingFee;
                            //        string strFee = this.CustomerInfo.StrParkingFee;
                            //        string PkTime = this.ParkingDuration;
                            //        Task.Factory.StartNew(() => PrintToLedSfactorsStanard(strFee, PkTime, fee));
                            //    }
                            //    ;
                            //}
                            //else
                            //{
                            //    this.IniSerialPortLed();
                            //    switch (this.Section.LedOfKind)
                            //    {
                            //        case LedStyle.SGCT:
                            //            Task.Factory.StartNew(() => this.PrintToLedSGCT(_SerialPortLed));
                            //            break;
                            //        case LedStyle.VietTell:
                            //            Task.Factory.StartNew(() => this.PrintToLedViettel(_SerialPortLed));
                            //            break;
                            //        case LedStyle.Matrixs:
                            //            if (this.CustomerInfo != null)
                            //            {
                            //                int fee = (int)this.CustomerInfo.ParkingFee;
                            //                Task.Factory.StartNew(() => this.PrintToLedMatrix(_SerialPortLed, fee));
                            //            }
                            //            break;
                            //        default:
                            //            Task.Factory.StartNew(() => this.PrintToLedGateWay(_SerialPortLed));
                            //            break;
                            //    }
                            //}

                            //if (CheckCustomerInfoValid() && CustomerInfo.VehicleRegistrationInfo.Status == VehicleRegistrationStatus.InUse)
                            //    CustomerInfo.ParkingFee = 0;


                            if (_userPreferenceService.OptionsSettings.ConfirmCheckout)// && _checkinData.VehicleTypeId != 2000101)
                            {
                                //2024 - Ngưng táp thẻ nếu bật chức năng ConfirmCheckout
                                //_hasConfirmChecking=true;

                                var checkoutKey = Section.KeyMap.KeysMap[KeyAction.CheckOut];
                                var cancelCheckoutKey = Section.KeyMap.KeysMap[KeyAction.CancelCheckOut];//checkoutwithcashier
                                //var cashierKey = Section.KeyMap.KeysMap[KeyAction.Cashier];
                                var cashierKey = "";
                                string msg = string.Empty;
                                if (this.Section.ComIctCashierEnanble && this.CustomerInfo.ParkingFee > 0 && !string.IsNullOrEmpty(this.Section.ComIctCashier) && !string.IsNullOrEmpty(cashierKey))
                                    msg = string.Format(GetText("checkoutwithcashier.guide"), checkoutKey, Environment.NewLine, cancelCheckoutKey, cashierKey);
                                else
                                    msg = string.Format(GetText("checkout.guide"), checkoutKey, Environment.NewLine, cancelCheckoutKey);
                                HandleError(IconEnums.Guide, msg, true, true);
                            }
                            else
                                _hasConfirmChecking = false;

                            _editTimer.Stop();
                        }

                        //_server.GetServerTime((serverTime, ex1) =>
                        //{serverTime,
                        ConvertToCheckOutData(data, result);
                        //2019Jan19
                        //if (data != null && CheckShowFreeBuyt(timeRide, data.StrReferenceCheckInTime, data.StrCheckOutTime))
                        //    ShowFree = System.Windows.Visibility.Visible;
                        //2019Jan19
                        
                        RecognizePlate(result);


                        LoadImages(result, (e) =>
                        {
                            if (complete != null)
                            {
                                //_canCheckout = false;
                                complete(ex);
                            }
                        });
                        //});
                    }
                    //this.IniSerialPortLed();
                    //switch(this.Section.LedOfKind)
                    //{
                    //    case LedStyle.SGCT:
                    //        Task.Factory.StartNew(() => this.PrintToLedSGCT(_SerialPortLed));    
                    //        break;
                    //    case LedStyle.VietTell:
                    //        Task.Factory.StartNew(() => this.PrintToLedViettel(_SerialPortLed));  
                    //        break;
                    //    case LedStyle.Matrixs:
                    //        if (this.CustomerInfo != null)
                    //        {
                    //            int fee = (int)this.CustomerInfo.ParkingFee;
                    //            Task.Factory.StartNew(() => this.PrintToLedMatrix(_SerialPortLed, fee));
                    //        }
                    //        break;
                    //    default:
                    //        Task.Factory.StartNew(() => this.PrintToLedGateWay(_SerialPortLed));     
                    //        break;
                    //}

                });
            }
            catch (Exception ex)
            {
                PrintLog<CheckOutLaneViewModel>(ex, _userPreferenceService.HostSettings.LogServerIP);
                if (complete != null)
                    complete(ex);
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
                    //_clientSocket.Connected.ToString();
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

        private int countDay(DateTime startDate, DateTime endDate)
        {
            int count = 0;
            string fDate = startDate.ToShortDateString();
            string tDate = endDate.ToShortDateString(); 

            count = (DateTime.Parse(tDate + " 00:00:00") - DateTime.Parse(fDate + " 00:00:00")).Days;

            return count;
        }

        //2024 - recallfee Citigate
        //if((CheckOutTime - CheckInTime).TotalSeconds > 43200)
        //{
        //    int tfee = (int)((CheckOutTime - CheckInTime).TotalSeconds/43200);

        //    if ((CheckOutTime - CheckInTime).TotalSeconds % 43200 > 0)
        //        tfee += 1;

        //    _fee = tfee*5000;
        //}

        private float recallfeeGardenMall(DateTime CheckOutTime, DateTime CheckInTime, int _VehicleTypeId)
        {
            //2024 - recallfee GardenMall

            float _fee = 30000, _penaltyfee = 150000, _fee1h = 15000, totalfee = 0;
            float _ffirst = 0, _fmiddle = 0, _fend = 0;
            int _countDay = 0;
            float _cHour = 0;

            _countDay = countDay(CheckInTime, CheckOutTime);

            if (_VehicleTypeId == 2000101)
            {
                _fee = 30000;
                _penaltyfee = 150000;
                _fee1h = 15000;
            }

            //Fee of first day
            //Tính phí dem neu vao truoc 6h
            if ((CheckInTime - DateTime.Parse(CheckInTime.ToShortDateString() + " 06:00:00")).TotalHours < 0)
                _ffirst = _penaltyfee;
            else
                _ffirst = _fee;

            if (_countDay > 0)
            {
                if ((CheckInTime - DateTime.Parse(CheckInTime.ToShortDateString() + " 06:00:00")).TotalHours < 0)
                    _cHour = (float)(Math.Ceiling((DateTime.Parse(CheckInTime.ToShortDateString() + " 00:00:00").AddDays(1) - DateTime.Parse(CheckInTime.ToShortDateString() + " 06:00:00")).TotalHours));
                else
                    _cHour = (float)(Math.Ceiling((DateTime.Parse(CheckInTime.ToShortDateString() + " 00:00:00").AddDays(1) - CheckInTime).TotalHours));
            }
            else
            {
                double _cMinute = 0;
                if ((CheckInTime - DateTime.Parse(CheckInTime.ToShortDateString() + " 06:00:00")).TotalHours < 0)
                {
                    _cMinute = ((CheckOutTime - DateTime.Parse(CheckInTime.ToShortDateString() + " 06:00:00")).TotalHours - (int)(CheckOutTime - DateTime.Parse(CheckInTime.ToShortDateString() + " 06:00:00")).TotalHours) * 60;

                    if (_cMinute > 10)
                        _cHour = (float)(Math.Ceiling((CheckOutTime - DateTime.Parse(CheckInTime.ToShortDateString() + " 06:00:00")).TotalHours));
                    else
                        _cHour = (float)(Math.Floor((CheckOutTime - DateTime.Parse(CheckInTime.ToShortDateString() + " 06:00:00")).TotalHours));
                }
                else
                {
                    _cMinute = ((CheckOutTime - CheckInTime).TotalHours - (int)(CheckOutTime - CheckOutTime).TotalHours) * 60;

                    if (_cMinute > 10)
                        _cHour = (float)(Math.Ceiling((CheckOutTime - CheckInTime).TotalHours));
                    else
                        _cHour = (float)(Math.Floor((CheckOutTime - CheckInTime).TotalHours));
                }
            }

            if (_ffirst > _fee)
                _ffirst += _cHour * _fee1h;
            else
            { if (_cHour > 2) _ffirst += (_cHour - 2) * _fee1h; }


            //Fee of Middle day
            _fmiddle = (_penaltyfee + ((24 - 6) * _fee1h)) * (_countDay - 1); //(Phí phạt qua đêm + (số giờ tiếp theo tinh tu luc 6h)*phí mỗi giờ) * số ngày
            if (_fmiddle < 0) _fmiddle = 0;


            //Fee of End day
            if (_countDay > 0)
            {
                if ((CheckOutTime - DateTime.Parse(CheckOutTime.ToShortDateString() + " 00:00:00")).TotalMinutes > 15)
                    _fend = _penaltyfee;

                if ((CheckOutTime - DateTime.Parse(CheckOutTime.ToShortDateString() + " 06:00:00")).TotalHours > 0) //Tính phí ngày mới sau 6h
                {
                    double _cMinute = 0;
                    //_fend += _fee;
                    if ((CheckOutTime - DateTime.Parse(CheckOutTime.ToShortDateString() + " 06:00:00")).TotalHours > 0)//Tính phí mỗi giờ tiếp theo
                    {
                        _cHour = 0;

                        _cMinute = ((CheckOutTime - DateTime.Parse(CheckOutTime.ToShortDateString() + " 06:00:00")).TotalHours - (int)(CheckOutTime - DateTime.Parse(CheckOutTime.ToShortDateString() + " 06:00:00")).TotalHours) * 60;

                        if (_cMinute > 10)
                            _cHour = (float)(Math.Ceiling((CheckOutTime - DateTime.Parse(CheckOutTime.ToShortDateString() + " 06:00:00")).TotalHours));
                        else
                            _cHour = (float)(Math.Floor((CheckOutTime - DateTime.Parse(CheckOutTime.ToShortDateString() + " 06:00:00")).TotalHours));

                        _fend += _cHour * _fee1h;
                    }
                }
            }

            totalfee = _ffirst + _fmiddle + _fend;

            return totalfee;
        }

        private float recallfee(DateTime CheckOutTime, DateTime CheckInTime, int _VehicleTypeId)
        {
            float totalfee = 0;

            if ((CheckOutTime - CheckInTime).TotalMinutes > 2)
            {
                float _fee = 5000, _penaltyfee = 200000, _fee1h = 1000;
                float _ffirst = 0, _fmiddle = 0, _fend = 0;
                int _countDay = 0;
                float _cHour = 0;

                _countDay = countDay(CheckInTime, CheckOutTime);

                if (_VehicleTypeId == 2000101)
                {
                    _fee = 25000;
                    _penaltyfee = 2000000;
                    _fee1h = 10000;
                }

                //2024 - recallfee ParcMall
                //Fee of first day
                if (_countDay > 0)
                    _cHour = (float)(Math.Ceiling((DateTime.Parse(CheckInTime.ToShortDateString() + " 00:00:00").AddDays(1) - CheckInTime).TotalHours));
                else
                    _cHour = (float)(Math.Ceiling((CheckOutTime - CheckInTime).TotalHours));

                _ffirst = _fee;
                if (_cHour > 4)
                    _ffirst += (_cHour - 4) * _fee1h;


                //Fee of Middle day
                _fmiddle = (_penaltyfee + _fee + ((24 - 4 - 5) * _fee1h)) * (_countDay - 1); //(Phí phạt qua đêm + phí 4h đầu + (số giờ tiếp theo)*phí mỗi giờ) * số ngày
                if (_fmiddle < 0) _fmiddle = 0;

                //Fee of End day
                if (_countDay > 0)
                {
                    _fend = _penaltyfee;
                    if ((CheckOutTime - DateTime.Parse(CheckOutTime.ToShortDateString() + " 05:00:00")).TotalHours > 0) //Tính phí ngày mới sau 5h
                    {
                        _fend += _fee;
                        if ((CheckOutTime - DateTime.Parse(CheckOutTime.ToShortDateString() + " 09:00:00")).TotalHours > 0)//Tính phí mỗi giờ tiếp theo
                        {
                            _cHour = 0;
                            _cHour = (float)(Math.Ceiling((CheckOutTime - DateTime.Parse(CheckOutTime.ToShortDateString() + " 09:00:00")).TotalHours));

                            _fend += _cHour * _fee1h;
                        }
                    }
                }

                totalfee = _ffirst + _fmiddle + _fend;
            }
            
            return totalfee;
        }

        private float recallfeeEvent(DateTime CheckOutTime, DateTime CheckInTime, int _VehicleTypeId)
        {
            float totalfee = 0;

            if ((CheckOutTime - CheckInTime).TotalMinutes > 2)
            {
                float _fee = 5000, _penaltyfee = 200000, _fee1h = 1000;
                float _ffirst = 0, _fmiddle = 0, _fend = 0;
                int _countDay = 0;

                _countDay = countDay(CheckInTime, CheckOutTime);

                if (_VehicleTypeId == 2000101)
                {
                    _fee = 25000;
                    _penaltyfee = 2000000;
                    _fee1h = 10000;
                }

                //2025 - recallfee ParcMall event
                //Fee of first day                
                _ffirst = _fee;
                
                //Fee of Middle day
                _fmiddle = (_penaltyfee + _fee) * (_countDay - 1); //(Phí phạt qua đêm + phí 4h đầu + (số giờ tiếp theo)*phí mỗi giờ) * số ngày
                if (_fmiddle < 0) _fmiddle = 0;

                //Fee of End day
                if (_countDay > 0)
                {
                    _fend = _penaltyfee;
                    if ((CheckOutTime - DateTime.Parse(CheckOutTime.ToShortDateString() + " 05:00:00")).TotalHours > 0) //Tính phí ngày mới sau 5h
                    {
                        _fend += _fee;
                    }
                }

                totalfee = _ffirst + _fmiddle + _fend;
            }

            return totalfee;
        }

        private bool CheckShowFreeBuyt(string timeride, string checkintime, string checkouttime)
        {
            try
            {
                if (timeride.Substring(0, 10) == checkintime.Substring(0, 10) && checkintime.Substring(0, 10) == checkouttime.Substring(0, 10))
                    return true;
                return false;
            }
            catch
            {
                return false;
            }
        }
        private void LogTotalAmount()
        {
            /*** Log lai so tien ***/
            if (CheckOutData != null && this.CustomerInfo != null)
            {
                Section _sec = this.Section as Section;
                _sec.TotalAmount += this.CustomerInfo.ParkingFee;
                //_sec.TotalAmount += 100;
                //_userPreferenceService.SystemSettings.UpdateSection(_sec);
                //_userPreferenceService.SystemSettings.Save();
            }

        }
        //, ,this._checkinData.VehicleType.Id

        private void WarningBeforeCheckOut(CheckOut checkout, int _CardTypeId, int _VehicleTypeId, CheckIn recivedData)
        {
            if (_userPreferenceService.OptionsSettings.NoMatchingPlateNoticeEnalbe)
            {
                if (checkout != null && _userPreferenceService.OptionsSettings.ConfirmCheckout 
                    && _userPreferenceService.OptionsSettings.PlateRecognitionBySfactors)
                {
                    //2024 - Tu dong xac nhan khi bat kiem tra dung bien so Vao & Ra doi voi Xe thang
                    if (checkout.VehicleNumber == checkout.ReferenceVehicleNumber)
                    {
                        if (_CardTypeId == 1 && recivedData.CustomerInfo != null && !string.IsNullOrEmpty(recivedData.CustomerInfo.VehicleRegistrationInfo.VehicleNumber))
                        {
                            if (_allowCheckout)
                            {
                                _allowCheckout = false;
                                CheckOut(checkout, exception =>
                                {
                                    if (exception == null)
                                    {
                                        Task.Factory.StartNew(() => {
                                            OpenBarrier();
                                            Alarm("success");
                                        });
                                        var tempCheckoutData = checkout;
                                        //_hasConfirmChecking = false;
                                        _editTimer.Start(tempCheckoutData.CardId);
                                        CountDown = _countDownMax;
                                        _allowCheckout = true;
                                        _canCheckout = true;
                                        
                                        if (CheckOutCompleted != null)
                                            CheckOutCompleted(null, new CheckoutEventArgs { Key = KeyAction.CheckOut });
                                    }
                                    else
                                    {
                                        _allowCheckout = true;
                                        IsVoucher = false;
                                        CanShowVoucher = System.Windows.Visibility.Hidden;
                                        CardIDVC = string.Empty;
                                        Task.Factory.StartNew(() => Alarm("fail"));
                                        HandleError(IconEnums.Error, GetText("checkout.something_wrong"), false);

                                    }
                                    
                                });
                            }

                            _canCheckout = true;

                            //if (_VehicleTypeId == 2000101)
                            //    Thread.Sleep(3000);

                            _hasConfirmChecking = false;

                            //ResetUIInformation();
                            Notices.Clear();
                            //Notices = null;
                        }
                    }
                    else
                    {
                        HandleError(IconEnums.Warning, "Biển số không trùng khớp", false);
                    }    
                }
            }
        }

        private void RecognizePlate(CheckIn receivedData)
        {
            var tempCheckOutData = CheckOutData;
            if (tempCheckOutData != null)
            {
                _alprService.RecognizeLicensePlate(_frontImage.ToByteArray(ImageFormat.Jpeg), receivedData.VehicleType.Id, (recognizedPlateNumber, ex1) =>
                {
                    if (ex1 != null)
                    {
                        HandleError(IconEnums.Warning, GetText("anpr.cannot_connect"), false);
                    }
                    else
                    {
                        ProcessAfterRecognizeLicensePlate(receivedData, recognizedPlateNumber, tempCheckOutData);
                    }
                });
            }
        }

        private void ProcessAfterRecognizeLicensePlate(CheckIn receivedData, string recognizedPlateNumber, CheckOut tempCheckOutData)
        {
            //if (!string.IsNullOrEmpty(recognizedPlateNumber) && recognizedPlateNumber.Equals(_previousRawVehicleNumber))
            //{
            //    PrintLog<CheckInLaneViewModel>(new Exception("There is something fishy about ANPR"), null, true);
            //}

            if(recognizedPlateNumber != null && _previousRawVehicleNumber != null)
            {
                tempCheckOutData.AlprVehicleNumber = recognizedPlateNumber;
                tempCheckOutData.VehicleNumber = ExtractVehicleNumber(recognizedPlateNumber);
                tempCheckOutData.PrefixNumberVehicle = ExtractPrefixVehicleNumber(tempCheckOutData.AlprVehicleNumber, tempCheckOutData.VehicleNumber);
                WarningBeforeCheckOut(tempCheckOutData, receivedData.CardTypeId, receivedData.VehicleTypeId, receivedData);
                _previousRawVehicleNumber = recognizedPlateNumber;

                //if (!string.IsNullOrEmpty(tempCheckOutData.VehicleNumber) && !string.IsNullOrEmpty(receivedData.VehicleNumber)
                //    && !receivedData.VehicleNumber.Equals(tempCheckOutData.VehicleNumber)
                //    && _userPreferenceService.OptionsSettings.NoMatchingPlateNoticeEnalbe)
                ////&& CustomerInfo != null
                ////&& CustomerInfo.VehicleRegistrationInfo != null
                ////&& string.Compare(CustomerInfo.VehicleRegistrationInfo.VehicleNumber, recognizedPlateNumber) == 0
                //{
                //    HandleError(IconEnums.Close, GetText("checkin.not_match_vehicle_number"), true);
                //}
            }
            
        }

        /// <summary>
        /// Gets the check in data of tapped card.
        /// </summary>
        /// <param name="checkoutData">The checkout data.</param>
        /// <param name="complete">The complete.</param>
        public void GetCheckIn(CheckOut checkoutData, Action<CheckIn, Exception> complete)
        {
            if (checkoutData == null)
            {
                complete(null, new Exception("Cannot get checkout"));
                return;
            }

            _server.GetCheckIn(checkoutData.CardId, (checkinData, ex) =>
            {
                if (complete != null)
                    complete(checkinData, ex);
            });
        }
        //ServerTimeInfo serverTime,
        private void ConvertToCheckOutData( CheckOut checkoutData, CheckIn checkinData, string palte = null)
        {
            if (checkinData != null)
            {
                //ParkingDuration = ToReadableString(serverTime.LocalTime - checkinData.CheckInTime);

                //checkoutData.CheckOutTime = serverTime.LocalTime;

                //TimeSpan dtClaim = (DateTime.Now - checkinData.CheckInTime);
				TimeSpan dtClaim = (checkinData.CheckOutTimeServer - checkinData.CheckInTimeServer);
                TimeSpan newTime;
                if (dtClaim.Seconds > 30)
                {
                    newTime = dtClaim.Add(TimeSpan.FromSeconds(60 - dtClaim.Seconds));
                }
                else
                    newTime = dtClaim;

                ParkingDuration = ToReadableString(newTime);

                //ParkingDuration = ToReadableString(DateTime.Now - checkinData.CheckInTime);
       
                checkoutData.CheckOutTime = checkinData.CheckOutTimeServer;
                checkoutData.AlprVehicleNumber = checkinData.AlprVehicleNumber;
                string s = ExtractVehicleNumber(checkoutData.AlprVehicleNumber);
                ReferencePrefixNumber = ExtractPrefixVehicleNumber(checkoutData.AlprVehicleNumber, s);
                checkoutData.ReferenceCheckInTime = checkinData.CheckInTime;
                checkoutData.ReferenceVehicleNumber = checkinData.VehicleNumber.Equals("????") ? "" : checkinData.VehicleNumber;
                checkoutData.CardLabel = checkinData.CardLabel;
                checkoutData.VehicleType = checkinData.VehicleType;
                TypeHelper.GetCardType(checkinData.CardTypeId, result => CardType = result);
            }
            this.CheckOutData = checkoutData;
        }

        Logger _classLogger = LogManager.GetCurrentClassLogger();
        private void LoadImages(CheckIn checkinData, Action<Exception> complete)
        {
            Exception exception = null;

            // only call complete callback when
            if (checkinData != null)
            {
                ManualResetEvent resetEvent = new ManualResetEvent(false);
                int numberOfThreads = 2;
                var tempCheckOutData = CheckOutData;

                Stopwatch watch = new Stopwatch();
                watch.Start();
                _storageService.DoubleLoadImage(checkinData.FrontImagePath, checkinData.ImageHosts, (fiBytes, e) =>
                {
                    if (e != null)
                    {
                        exception = e;
                        PrintLog<CheckOutLaneViewModel>(exception, _userPreferenceService.HostSettings.LogServerIP);
                    }
                    tempCheckOutData.ReferenceFrontImage = fiBytes;
                    Interlocked.Decrement(ref numberOfThreads);
                    if (numberOfThreads == 0)
                        resetEvent.Set();
                });
                _storageService.DoubleLoadImage(checkinData.BackImagePath, checkinData.ImageHosts, (biBytes, e) =>
                {
                    if (e != null)
                    {
                        exception = e;
                        PrintLog<CheckOutLaneViewModel>(exception, _userPreferenceService.HostSettings.LogServerIP);
                    }
                    tempCheckOutData.ReferenceBackImage = biBytes;
                    Interlocked.Decrement(ref numberOfThreads);
                    if (numberOfThreads == 0)
                        resetEvent.Set();
                });

                resetEvent.WaitOne(3000);
                watch.Stop();
                _classLogger.Info(string.Format("{0} - {1}-{2} ({3})", OtherUtilities.GetVersion(), checkinData.FrontImagePath, checkinData.BackImagePath, watch.ElapsedMilliseconds));
            }

            if (complete != null) complete(exception);
        }

        public void CheckOut(CheckOut checkout, Action<Exception> complete)
        {
            _server.CreateCheckOut(checkout, CustomerInfo, ex =>
            {
                if (ex == null)
                {
                    LastProcessed = DateTime.Now;
                    SaveImage(checkout);
                }
                checkout.is_cancel = "NULL";
                InvokeOnMainThread(() =>
                {
                    string msg = string.Empty;
                    if (ex != null)
                    {
                        //if (ex is InternalServerErrorException)
                        //{
                        //    HandleError(IconEnums.Error, GetText("checkout.something_wrong"), false);
                        //    PrintLog<CheckOutLaneViewModel>(ex, _userPreferenceService.HostSettings.LogServerIP);
                        //}
                        //else
                        //    PrintLog<CheckOutLaneViewModel>(ex);
                        HandleError(IconEnums.Error, GetText("checkout.something_wrong"), false);
                    }
                    else
                    {
                        HandleError(IconEnums.Check, GetText("checkout.success"), false);
                        _canCheckout = true;
                        _notEditPlateNumberYet = true;
                        /*** Log ***/
                        LogTotalAmount();
                    }
                    _canCheckout = true;
                    if (complete != null)
                        complete(ex);
                });

                //ParkingDuration = string.Empty;
            });
        }

        public string ToReadableString(TimeSpan span)
        {
            string formatted = string.Format("{0}{1}{2}",
                span.Duration().Days > 0 ? string.Format("{0:0} N ", span.Days) : string.Empty,
                span.Duration().Hours > 0 ? string.Format("{0:0} H", span.Hours) : string.Empty,
                span.Duration().Minutes > 0 ? string.Format("{0:0} P", span.Minutes) : string.Empty);

            if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

            if (string.IsNullOrEmpty(formatted)) formatted = "< 1 P";

            return formatted;
        }

        int _flag = 0;
        int _lastReadTime = 0;

        private void SaveImage(CheckOut checkoutInfo)
        {
            _storageService.SaveImage(
                new List<string>() { checkoutInfo.FrontImagePath, checkoutInfo.BackImagePath },
                new List<byte[]>() { checkoutInfo.FrontImage, checkoutInfo.BackImage },
                (List<Exception> lstExceptions) =>
                {
                    foreach (Exception exception in lstExceptions)
                    {
                        if (exception != null)
                        {
                            PrintLog<CheckOutLaneViewModel>(exception);
                            return;
                        }
                    }
                    //_server.ReplicateImages(checkoutInfo, null);
                });
        }

        private void IniSerialPortLed()
        {
            //if (_SerialPortLed == null)
            //{
            //    var _portExists = System.IO.Ports.SerialPort.GetPortNames().Any(p => p == "COM20");
            //    if (!_portExists) return;
            //    _SerialPortLed = new System.IO.Ports.SerialPort();
            //    _SerialPortLed.PortName = "COM20";
            //    _SerialPortLed.BaudRate = 9600;
            //}
            if (_SerialPortLed == null)
            {
                if ((this.Section == null || string.IsNullOrWhiteSpace(this.Section.ComLed)))
                    return;
                string comname = this.Section.ComLed.ToUpper(); 
                var _portExists = System.IO.Ports.SerialPort.GetPortNames().Any(p => p == comname);
                if (!_portExists) return;
                _SerialPortLed = new System.IO.Ports.SerialPort();
                _SerialPortLed.PortName = comname;
                _SerialPortLed.BaudRate = 9600;
            }
            else
            {
                if ((this.Section == null || string.IsNullOrWhiteSpace(this.Section.ComLed)))
                    return;
                string comname = this.Section.ComLed.ToUpper();
                var _portExists = System.IO.Ports.SerialPort.GetPortNames().Any(p => p == comname);
                if (!_portExists)
                {
                    if (_SerialPortLed.IsOpen)
                    {
                        _SerialPortLed.Close(); _SerialPortLed.Dispose(); _SerialPortLed = null;
                    }
                    return;
                }
                if (_SerialPortLed.IsOpen)
                {
                    _SerialPortLed.Close(); _SerialPortLed.Dispose(); _SerialPortLed = null;
                }
                _SerialPortLed = new System.IO.Ports.SerialPort();
                _SerialPortLed.PortName = comname;
                _SerialPortLed.BaudRate = 9600;
            }
            try
            {
                if (!_SerialPortLed.IsOpen)
                    _SerialPortLed.Open();
            }
            catch (System.IO.IOException ex)
            {
                _SerialPortLed.Close(); _SerialPortLed.Dispose(); _SerialPortLed = null;
            }
        }

        //private void IniSerialPortLedB()
        //{
        //    //if (_SerialPortLedB == null)
        //    //{
        //    //    var _portExists = System.IO.Ports.SerialPort.GetPortNames().Any(p => p == "COM18");
        //    //    if (!_portExists) return;

        //    //    _SerialPortLedB = new System.IO.Ports.SerialPort();
        //    //    _SerialPortLedB.PortName = "COM18";
        //    //    _SerialPortLedB.BaudRate = 9600;
        //    //}
        //    if (_SerialPortLedB == null)
        //    {
        //        string comname = (this.Section == null || string.IsNullOrWhiteSpace(this.Section.ComLedB)) ? "COM18" : this.Section.ComLedB.ToUpper();
        //        var _portExists = System.IO.Ports.SerialPort.GetPortNames().Any(p => p == comname);
        //        if (!_portExists) return;
        //        _SerialPortLedB = new System.IO.Ports.SerialPort();
        //        _SerialPortLedB.PortName = comname;
        //        _SerialPortLedB.BaudRate = 9600;
        //    }
        //    try
        //    {
        //        if (!_SerialPortLedB.IsOpen)
        //            _SerialPortLedB.Open();
        //    }
        //    catch (Exception ex)
        //    {
        //        _SerialPortLedB.Close(); _SerialPortLedB.Dispose(); _SerialPortLedB = null;
        //    }
        //}

        private bool IniSerialPortPrinter()
        {
            bool IsIni = false;
            //if (_SerialPortPrinter == null)
            //{
            //    var _portExists = System.IO.Ports.SerialPort.GetPortNames().Any(p => p == "COM22");
            //    if (!_portExists) return false;

            //    _SerialPortPrinter = new System.IO.Ports.SerialPort();
            //    _SerialPortPrinter.PortName = "COM22";
            //    _SerialPortPrinter.BaudRate = 9600;

            //}
            if (_SerialPortPrinter == null)
            {
                if (this.Section == null || string.IsNullOrWhiteSpace(this.Section.ComPrint))
                    return false;
                string comname =  this.Section.ComPrint.ToUpper();    
                var _portExists = System.IO.Ports.SerialPort.GetPortNames().Any(p => p == comname);
                if (!_portExists) return false;
                _SerialPortPrinter = new System.IO.Ports.SerialPort();
                _SerialPortPrinter.PortName = comname;
                _SerialPortPrinter.BaudRate = 9600;
            }
            else
            {
                if (this.Section == null || string.IsNullOrWhiteSpace(this.Section.ComPrint))
                    return false;
                string comname = this.Section.ComPrint.ToUpper();
                var _portExists = System.IO.Ports.SerialPort.GetPortNames().Any(p => p == comname);
                if (!_portExists)
                {
                    if(_SerialPortPrinter.IsOpen)
                    {
                        _SerialPortPrinter.Close(); _SerialPortPrinter.Dispose(); _SerialPortPrinter = null;
                    }
                    return false;
                }
                if (_SerialPortPrinter.IsOpen)
                {
                    _SerialPortPrinter.Close(); _SerialPortPrinter.Dispose(); _SerialPortPrinter = null;
                }
                _SerialPortPrinter = new System.IO.Ports.SerialPort();
                _SerialPortPrinter.PortName = comname;
                _SerialPortPrinter.BaudRate = 9600;
            }
            try
            {
                if (!_SerialPortPrinter.IsOpen)
                    _SerialPortPrinter.Open();
                IsIni = true;
            }
            catch (Exception ex)
            {
                _SerialPortPrinter.Close(); _SerialPortPrinter.Dispose(); _SerialPortPrinter = null;
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
            FontFamily fontFamily = new FontFamily("Arial");
            
            Font font = new Font(fontFamily,12, System.Drawing.FontStyle.Bold,GraphicsUnit.Pixel);
            Font font1 = new Font(fontFamily, 9);
            Font font2 = new Font(fontFamily, 8, System.Drawing.FontStyle.Italic | System.Drawing.FontStyle.Bold);
            Font font3 = new Font(fontFamily, 6, System.Drawing.FontStyle.Bold);

            var format = new StringFormat() { Alignment=StringAlignment.Far };
            var formatLeft = new StringFormat() { Alignment = StringAlignment.Near };
            var format1 = new StringFormat() { Alignment = StringAlignment.Center };
            var format11= new StringFormat() { Alignment = StringAlignment.Center, FormatFlags=StringFormatFlags.FitBlackBox };
            RectangleF rect = new RectangleF(0, 70, e.PageBounds.Width-30, 15);
            if (_userPreferenceService.OptionsSettings.IsPrintV2)
            {
                rect.Y = 0;
                e.Graphics.DrawString("Sheraton Saigon Hotel & Towers", font, System.Drawing.Brushes.Black, rect, format1);
                rect.Y = 15;
                e.Graphics.DrawString("-------------------------------------------------------------", font2, System.Drawing.Brushes.Black, rect, format1);
                //var url = System.Windows.Forms.Application.StartupPath + "\\config\\logo_print.png";
                //Image img = Image.FromFile(url);
                //if (img != null)
                //    e.Graphics.DrawImage(img, 5, 5);
                rect.Y = 35;
                e.Graphics.DrawString("Ngày in Bill:", font1, System.Drawing.Brushes.Black, rect, formatLeft);
                e.Graphics.DrawString(this._checkinData.StrCheckOutTimeServer, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 50;
                e.Graphics.DrawString("Nhân viên xử lý:" , font1, System.Drawing.Brushes.Black, rect, formatLeft);
                e.Graphics.DrawString(this.User.DisplayName, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 80;
                e.Graphics.DrawString("Loại phương tiện:", font1, System.Drawing.Brushes.Black, rect, formatLeft);
                e.Graphics.DrawString(this._checkinData.VehicleType.Name, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 95;
                e.Graphics.DrawString("Biển số xe:", font1, System.Drawing.Brushes.Black, rect, formatLeft);
                e.Graphics.DrawString(string.IsNullOrWhiteSpace(this.ReferencePrefixNumber) ? "" : this.ReferencePrefixNumber + "-" + this._checkOutData.ReferenceVehicleNumber, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 110;
                e.Graphics.DrawString("Đối tượng:", font1, System.Drawing.Brushes.Black, rect, formatLeft);
                e.Graphics.DrawString(this._cardType.Name, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 140;
                e.Graphics.DrawString("Ngày giờ vào:", font1, System.Drawing.Brushes.Black, rect, formatLeft);
                e.Graphics.DrawString(this._checkinData.StrCheckInTimeServer, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 155;
                e.Graphics.DrawString("NGày giờ ra:", font1, System.Drawing.Brushes.Black, rect, formatLeft);
                e.Graphics.DrawString(this._checkinData.StrCheckOutTimeServer, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 170;
                e.Graphics.DrawString("Thời gian lưu bãi:", font1, System.Drawing.Brushes.Black, rect, formatLeft);
                e.Graphics.DrawString(this.ParkingDuration, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 200;
                e.Graphics.DrawString("Số tiền thanh toán:", font1, System.Drawing.Brushes.Black, rect, formatLeft);
                e.Graphics.DrawString(this.CustomerInfo.StrParkingFee, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 215;
                e.Graphics.DrawString("-------------------------------------------------------------", font2, System.Drawing.Brushes.Black, rect, format1);
                rect.Y = 230;
                e.Graphics.DrawString("XIN CHÀO - HẸN GẶP LẠI QUÝ KHÁCH", font2, System.Drawing.Brushes.Black, rect, format1);
                rect.Y = 245;
                e.Graphics.DrawString("-------------------------------------------------------------", font2, System.Drawing.Brushes.Black, rect, format1);
            }
            else
            {
                //e.Graphics.DrawString("SAI GOAN CENTRE TOWERS", font, System.Drawing.Brushes.Black, 0, 5);
                //e.Graphics.DrawString("-------------------------------------------------------", font2, System.Drawing.Brushes.Black, 10, 25);
                var url = System.Windows.Forms.Application.StartupPath + "\\config\\print_logo.png";
                Image img = Image.FromFile(url);
                string printAddress = "375A/21 Nguyễn Trọng Tuyển, Phường 1, Tân Bình, Tp. Hồ Chí Minh";
                string printCallFax = "Tel: +84 862 767 888 - Email: info@greenparking.vn";
                if(img!=null && !string.IsNullOrEmpty(this.Section.PrintAddressTitle) && !string.IsNullOrEmpty(this.Section.PrintCallTile))
                {
                    printAddress = this.Section.PrintAddressTitle;
                    printCallFax = this.Section.PrintCallTile;
                }
                else
                {
                    url = System.Windows.Forms.Application.StartupPath + "\\config\\print_default_logo.png";
                    img = Image.FromFile(url);
                }
                if (img != null)
                {
                    e.Graphics.DrawImage(new Bitmap(img,175,75),0,0);     
                }
                e.Graphics.DrawString(printAddress, font3, System.Drawing.Brushes.Black, 95, 5);//"65 Le Loi Boulevard, District 1, HCMC, Vietnam"
                e.Graphics.DrawString(printCallFax, font3, System.Drawing.Brushes.Black, 95, 20);//"Tel: (84.8) 3823 2500 - Fax: +84 (8) 38229 822"
               
                e.Graphics.DrawString("Ngày in Bill:", font1, System.Drawing.Brushes.Black, rect, formatLeft);
                e.Graphics.DrawString(this._checkinData.StrCheckOutTimeServer, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 85;
                e.Graphics.DrawString("Cổng kiểm soát:", font1, System.Drawing.Brushes.Black, rect, formatLeft);
              
                e.Graphics.DrawString(this._terminal.Name, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 100;
                e.Graphics.DrawString("Nhân viên xử lý:", font1, System.Drawing.Brushes.Black, rect, formatLeft);
               
                e.Graphics.DrawString(this.User.DisplayName, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 130;
                e.Graphics.DrawString("Loại phương tiện:", font1, System.Drawing.Brushes.Black, rect, formatLeft);
                
                e.Graphics.DrawString(this._checkinData.VehicleType.Name, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 145;
                e.Graphics.DrawString("Biển số xe:" , font1, System.Drawing.Brushes.Black, rect, formatLeft);
               
                e.Graphics.DrawString(string.IsNullOrWhiteSpace(this.ReferencePrefixNumber) ? "" : this.ReferencePrefixNumber + "-" + this._checkOutData.ReferenceVehicleNumber, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 160;
                e.Graphics.DrawString("Đối tượng:", font1, System.Drawing.Brushes.Black, rect, formatLeft);
                
                e.Graphics.DrawString(this._cardType.Name, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 190;
                e.Graphics.DrawString("Ngày giờ vào:", font1, System.Drawing.Brushes.Black, rect, formatLeft);
               
                e.Graphics.DrawString(this._checkinData.StrCheckInTimeServer, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 205;
                e.Graphics.DrawString("Ngày giờ ra:", font1, System.Drawing.Brushes.Black, rect, formatLeft);
                e.Graphics.DrawString(this._checkinData.StrCheckOutTimeServer, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 220;
                e.Graphics.DrawString("Thời gian lưu bãi:", font1, System.Drawing.Brushes.Black, rect, formatLeft);

                e.Graphics.DrawString(this.ParkingDuration, font1, System.Drawing.Brushes.Black, rect, format);
                if (string.IsNullOrEmpty(this.CustomerInfo.StrClaimTime))
                {           
                    rect.Y = 250;
                    e.Graphics.DrawString("Số tiền thanh toán", font1, System.Drawing.Brushes.Black, rect, formatLeft);
                    e.Graphics.DrawString(this.CustomerInfo.StrParkingFee, font1, System.Drawing.Brushes.Black, rect, format);
                    rect.Y = 265;
                    e.Graphics.DrawString("-------------------------------------------------------------", font2, System.Drawing.Brushes.Black, rect, format1);
                    rect.Y = 280;
                    e.Graphics.DrawString("XIN CHÀO - HẸN GẶP LẠI QUÝ KHÁCH", font2, System.Drawing.Brushes.Black, rect, format1);
                    rect.Y = 295;
                    e.Graphics.DrawString("-------------------------------------------------------------", font2, System.Drawing.Brushes.Black, rect, format1);
                }
                else
                {
                    rect.Y = 235;
                    e.Graphics.DrawString("Ngày giờ Claimed:", font1, System.Drawing.Brushes.Black, rect, formatLeft);

                    e.Graphics.DrawString(this.CustomerInfo.StrClaimTime.Replace("Claimed ",""), font1, System.Drawing.Brushes.Black, rect, format);
                    rect.Y = 265;
                    e.Graphics.DrawString("Số tiền thanh toán", font1, System.Drawing.Brushes.Black, rect, formatLeft);
                    e.Graphics.DrawString(this.CustomerInfo.StrParkingFee, font1, System.Drawing.Brushes.Black, rect, format);
                    rect.Y = 280;
                    e.Graphics.DrawString("-------------------------------------------------------------", font2, System.Drawing.Brushes.Black, rect, format1);
                    rect.Y = 295;
                    e.Graphics.DrawString("SAU 30 PHÚT KỂ TỪ LÚC MIỄN GIẢM", font2, System.Drawing.Brushes.Black, rect, format1);
                    rect.Y = 310;
                    e.Graphics.DrawString("PHÍ GIỮ XE SẼ ĐƯỢC TÍNH THEO GIÁ QUY ĐỊNH", font2, System.Drawing.Brushes.Black, rect, format1);
                }
                
            }
        }

        public override void PrintToPrinter()
        {
            base.PrintToPrinter();
           

            if (this.CustomerInfo == null || this._parkingDuration == null)
            {
                return;
            }
            if (Section.PrintComActive)
            {
                try
                {
                    if (!IniSerialPortPrinter())
                        return;
                    com.clsCom _clsCom = new com.clsCom();
                    if (_userPreferenceService.OptionsSettings.IsPrintV2)
                    {
                        string[] _Array = new string[14];

                        _Array[0] = _clsCom.CombineString("NGAY IN BILL :", this._checkinData.StrCheckOutTimeServer);

                        _Array[1] = _clsCom.CombineString("NHAN VIEN XU LY :", this.User.DisplayName);

                        _Array[2] = _clsCom.CombineString(" ", " ");

                        _Array[3] = _clsCom.CombineString("LOAI PHUONG TIEN :", this._checkinData.VehicleType.Name);
                        //_Array[5] = _clsCom.CombineString("BIEN SO XE :", data.AlprVehicleNumber);
                        _Array[4] = _clsCom.CombineString("BIEN SO: ", _checkinData.AlprVehicleNumber.IndexOf("???") > -1 ? _checkinData.VehicleNumber : _checkinData.AlprVehicleNumber);
                        _Array[5] = _clsCom.CombineString("DOI TUONG :", this._cardType.Name);

                        _Array[6] = _clsCom.CombineString(" ", " ");

                        _Array[7] = _clsCom.CombineString("NGAY GIO VAO :", this._checkinData.StrCheckInTimeServer);
                        _Array[8] = _clsCom.CombineString("NGAY GIO RA :", this._checkinData.StrCheckOutTimeServer);
                        _Array[9] = _clsCom.CombineString("THOI GIAN LUU BAI :", this.ParkingDuration);

                        _Array[10] = _clsCom.CombineString(" ", " ");
                        _Array[11] = _clsCom.CombineString("SO TIEN THANH TOAN :", string.Format("{0:0,0 VND}", this.CustomerInfo.StrParkingFee));

                        _Array[12] = _clsCom.CombineString("", "");
                        _Array[13] = _clsCom.CombineString("Thank you very much for using our services", "");
                        _clsCom.XinChao = "Sheraton Saigon Hotel & Towers";
                        byte[] _buffers = _clsCom.CommandESC(_Array);
                        _SerialPortPrinter.Write(_buffers, 0, _buffers.Length);
                    }
                    else
                    {
                        _clsCom.UrlLogo = System.Windows.Forms.Application.StartupPath + "\\config\\logovt.ini";

                        string[] _Array = new string[13];
                        // _Array[0] = _clsCom.CombineString("NGAY IN BILL :", this._checkOutData.StrCheckOutTime);
                        _Array[0] = _clsCom.CombineString("NGAY IN BILL :", this._checkinData.StrCheckOutTimeServer);
                        _Array[1] = _clsCom.CombineString("CONG KIEM SOAT :", this._terminal.Name);
                        _Array[2] = _clsCom.CombineString("NHAN VIEN XU LY :", this.User.Username);

                        _Array[3] = _clsCom.CombineString(" ", " ");

                        _Array[4] = _clsCom.CombineString("LOAI PHUONG TIEN :", this._checkinData.VehicleType.Name);
                        //_Array[5] = _clsCom.CombineString("BIEN SO XE :", string.IsNullOrWhiteSpace(this.ReferencePrefixNumber) ? "" : this.ReferencePrefixNumber + "-" + this._checkOutData.ReferenceVehicleNumber);
                        _Array[5] = _clsCom.CombineString("BIEN SO: ", _checkinData.AlprVehicleNumber.IndexOf("???") > -1 ? _checkinData.VehicleNumber : _checkinData.AlprVehicleNumber);
                        _Array[6] = _clsCom.CombineString("DOI TUONG :", this._cardType.Name);

                        _Array[7] = _clsCom.CombineString(" ", " ");

                        //_Array[8] = _clsCom.CombineString("NGAY GIO VAO :", this._checkOutData.StrReferenceCheckInTime);
                        //_Array[9] = _clsCom.CombineString("NGAY GIO RA :", this._checkOutData.StrCheckOutTime);
                        _Array[8] = _clsCom.CombineString("NGAY GIO VAO :", this._checkinData.StrCheckInTimeServer);
                        _Array[9] = _clsCom.CombineString("NGAY GIO RA :", this._checkinData.StrCheckOutTimeServer);
                        _Array[10] = _clsCom.CombineString("THOI GIAN LUU BAI :", this.ParkingDuration);

                        _Array[11] = _clsCom.CombineString(" ", " ");
                        _Array[12] = _clsCom.CombineString("SO TIEN THANH TOAN :", this.CustomerInfo.StrParkingFee);

                        _clsCom.XinChao = "XIN CHAO - HEN GAP LAI QUY KHACH";
                        byte[] _buffers = _clsCom.CommandESC(_Array);
                        _SerialPortPrinter.Write(_buffers, 0, _buffers.Length);
                    }
                }
                catch (Exception ex)
                {

                    System.Windows.MessageBox.Show(ex.Message, "Error");
                }
            }
            else
                DocPrintToBill();

        }

        private void PrintToLedMatrix(System.IO.Ports.SerialPort Port, string Cmd)
        {
            if (Port == null) return;
            Port.Write(Cmd);
        }
        private void PrintToLedMatrix(System.IO.Ports.SerialPort Port, int ParkingFee)
        {
            if (Port == null || ParkingFee > 10000000 || ParkingFee < 0)
                return;   
            string trieu = ((ParkingFee%10000000)/1000000).ToString();
            string tramnghin = ((ParkingFee % 1000000) / 100000).ToString();
            string chucnghin = ((ParkingFee % 100000) / 10000).ToString();
            string ngin = ((ParkingFee % 10000) / 1000).ToString();
            string StrFee = "@" + trieu + tramnghin + chucnghin + ngin;
            PrintToLedMatrix(Port, StrFee);
            wait = false;      
        }
        private void PrintToLedSGCT(System.IO.Ports.SerialPort Port)
        {
            if (Port == null) return;
            if (this.CustomerInfo != null && this._parkingDuration != null)
            {
                string _line2 = "                    ";
                int _maxLength = 20,
                    _diff = _maxLength - this.CustomerInfo.StrParkingFee.Length;

                if (_diff < 0)
                    _diff = _maxLength;
                var _txt = _line2.Substring(0, _diff) + this.CustomerInfo.StrParkingFee;

                Port.Write(Convert.ToString((char)12));
                Port.WriteLine(string.Format("T.TIME: {0}", this.ParkingDuration));
                Port.WriteLine(string.Format("\r{0}", _txt));
            }
            else
            {
                Port.Write(Convert.ToString((char)12));
                Port.WriteLine(string.Format("..CARD NOT CHECKIN.."));

            }


        }
        private void PrintToLedViettel(System.IO.Ports.SerialPort Port)
        {
            if (Port == null)
            {
                return;
            }
            if (base.CustomerInfo != null && this._parkingDuration != null)
            {
                string _line2 = "        ";
                int _maxLength = 8;
                int _diff = _maxLength - base.CustomerInfo.StrParkingFee.Length;
                if (_diff < 0)
                {
                    _diff = _maxLength;
                }
                string _txt = _line2.Substring(0, _diff) + base.CustomerInfo.StrParkingFee;
                Port.Write(System.Convert.ToString('\b'));
                Port.WriteLine(string.Format("{0}", _txt));
                return;
            }
            else
            {
                Port.Write(System.Convert.ToString('\b'));
                Port.WriteLine(string.Format("{0}", new object[0]));
            }
        }
        ///case Gatway 2018Jul27
        private void PrintToLedGateWay(System.IO.Ports.SerialPort Port)
        {
            if (Port == null) return;
            if (this.CustomerInfo != null && this._parkingDuration != null)
            {
                string _line2 = "                     ";
                int _maxLength = 21,
                    _diff = _maxLength - this.CustomerInfo.StrParkingFee.Length;

                if (_diff < 0)
                    _diff = _maxLength;
                var _txt = _line2.Substring(0, _diff) + this.CustomerInfo.StrParkingFee;

                Port.Write(Convert.ToString((char)12));
                Port.WriteLine(string.Format("T.TIME: {0}", this.ParkingDuration));
                Port.Write(string.Format("\r{0}", _txt));
            }
            else
            {
                Port.Write(Convert.ToString((char)12));
               // Port.WriteLine(string.Format("..CARD NOT CHECKIN.."));

            }


        }
        private void PrintToLedSfactorsStanard(string strFee, string parkingDuration, int ParkingFee)
        {
            if ((this.Section == null || string.IsNullOrWhiteSpace(this.Section.ComLed)))
                return;
            ComManagement sftCom = ComManagement.GetInstance();
            ComParameter comparam =new ComParameter();
            string comname = this.Section.ComLed.ToUpper();
            comparam = new ComParameter()
            {
                ComName = comname, 
                TimeApply = DateTime.Now  
            };
            switch (this.Section.LedOfKind)
            {
                case LedStyle.Matrixs:
                    if (!string.IsNullOrEmpty(strFee) &&  ParkingFee >= 0 && ParkingFee < 10000000)
                    {     
                        comparam.Description = string.Format("Đèn Led báo phí chuẩn `Maxtrixs`, Lane: {0}", this.Section.LaneName);
                        string trieu = ((ParkingFee % 10000000) / 1000000).ToString();
                        string tramnghin = ((ParkingFee % 1000000) / 100000).ToString();
                        string chucnghin = ((ParkingFee % 100000) / 10000).ToString();
                        string ngin = ((ParkingFee % 10000) / 1000).ToString();
                        string StrFee = "@" + trieu + tramnghin + chucnghin + ngin;
                        comparam.Commands = new List<ComCommand>()
                                    {
                                            new ComCommand()
                                            {
                                                Command ="Write",
                                                CommandMessage=StrFee
                                            }
                                    };
                    }
                    break;
                case LedStyle.SGCT:
                    if (!string.IsNullOrEmpty(strFee))
                    {
                        comparam.Description = string.Format("Đèn Led báo phí chuẩn `SGCT`, Lane: {0}", this.Section.LaneName);
                        string _line2 = "                    ";
                        int _maxLength = 20,
                            _diff = _maxLength - strFee.Length;

                        if (_diff < 0)
                            _diff = _maxLength;
                        var _txt = _line2.Substring(0, _diff) + strFee;
                        comparam.Commands = new List<ComCommand>()
                                    {
                                            new ComCommand()
                                            {
                                                Command ="Write",
                                                CommandMessage=Convert.ToString((char)12)
                                            },
                                            new ComCommand()
                                            {
                                                Command ="WriteLine",
                                                CommandMessage=string.Format("T.TIME: {0}", parkingDuration)
                                            },
                                            new ComCommand()
                                            {
                                                Command ="Write",
                                                CommandMessage=string.Format("\r{0}", _txt)
                                            },
                                    };
                    }
                    break;
                case LedStyle.Standard:
                    if (!string.IsNullOrEmpty(strFee))
                    {
                        comparam.Description = string.Format("Đèn Led báo phí chuẩn `Standart`, Lane: {0}", this.Section.LaneName);
                        string _line2 = "                     ";
                        int _maxLength = 21,
                            _diff = _maxLength - strFee.Length;

                        if (_diff < 0)
                            _diff = _maxLength;
                        var _txt = _line2.Substring(0, _diff) + strFee;
                        comparam.Commands = new List<ComCommand>()
                                    {
                                            new ComCommand()
                                            {
                                                Command ="Write",
                                                CommandMessage=Convert.ToString((char)12)
                                            },
                                            new ComCommand()
                                            {
                                                Command ="WriteLine",
                                                CommandMessage=string.Format("T.TIME: {0}", parkingDuration)
                                            },
                                            new ComCommand()
                                            {
                                                Command ="Write",
                                                CommandMessage=string.Format("\r{0}", _txt)
                                            },
                                    };     
                    }
                    break;
                case LedStyle.VietTell:
                    if (!string.IsNullOrEmpty(strFee))
                    {
                        comparam.Description = string.Format("Đèn Led báo phí chuẩn `Viettel`, Lane: {0}", this.Section.LaneName);
                        string _line2 = "        ";
                        int _maxLength = 8;
                        int _diff = _maxLength - strFee.Length;
                        if (_diff < 0)
                        {
                            _diff = _maxLength;
                        }
                        string _txt = _line2.Substring(0, _diff) +strFee;
                        comparam.Commands = new List<ComCommand>()
                                    {
                                            new ComCommand()
                                            {
                                                Command ="Write",
                                                CommandMessage=System.Convert.ToString('\b')
                                            },
                                            new ComCommand()
                                            {
                                                Command ="WriteLine",
                                                CommandMessage=string.Format("{0}", _txt)
                                            }
                                    };
                    }
                    break;
            }
            if (comparam.Commands != null && comparam.Commands.Count > 0)
            {        
                sftCom.AddCommand(comparam);
            }
        }
        private bool ReadControllerCardFlag = false;
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
                        break;
                    }
                    if (ifo.InactiveCode == e.EventType)
                    {
                        ReadControllerCardFlag = false;
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
		private bool CheckTracker()
		{
			if (this.Section == null || this.Section.OptionByLane == null || this.Section.OptionByLane.MethodTracker == UsageCameraTrackerMethod.Unusage)
				return true;
			else if (this.Section.OptionByLane.MethodTracker == UsageCameraTrackerMethod.AllowInOutWhenTrackerCamBackOnBlue)
			{
                if (this.Section.BackOutCamera == null || this.Section.BackOutCamera.RawCamera == null)
                    return false;
				return this.Section.BackOutCamera.RawCamera.BlueTriggerStatus;
			}
			else if (this.Section.OptionByLane.MethodTracker == UsageCameraTrackerMethod.AllowInOutWhenTrackerCamBackOnRedAndBlue)
			{
				if (this.Section.BackOutCamera == null || this.Section.BackOutCamera.RawCamera == null)
					return false;
				return this.Section.BackOutCamera.RawCamera.RedTriggerStatus && this.Section.BackOutCamera.RawCamera.BlueTriggerStatus;
			}
			else if (this.Section.OptionByLane.MethodTracker == UsageCameraTrackerMethod.AllowInOutWhenTrackerCamFrontOnBlue)
			{
				if (this.Section.FrontOutCamera == null || this.Section.FrontOutCamera.RawCamera == null)
					return false;
				return this.Section.FrontOutCamera.RawCamera.BlueTriggerStatus;
			}
			else if (this.Section.OptionByLane.MethodTracker == UsageCameraTrackerMethod.AllowInOutWhenTrackerCamFrontOnRedAndBlue)
			{
				if (this.Section.FrontOutCamera == null || this.Section.FrontOutCamera.RawCamera == null)
					return false;
				return this.Section.FrontOutCamera.RawCamera.RedTriggerStatus && this.Section.FrontOutCamera.RawCamera.BlueTriggerStatus;
			}
			else { return false; }
		}
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
            //if (e.CardReader is ProlificCardReader)
            //{
            //    if (!ProlificCardReaderOn) return;
            //    ProlificCardReaderOn = false;
            //}

            // thread-safe when doing checkout
            lock (this)
            {
                Interlocked.Increment(ref _flag);
                var curTime = Environment.TickCount;
                if (_flag > 1 && (curTime - _lastReadTime) < 3000)
                    return;
                Interlocked.Exchange(ref _flag, 1);
                _lastReadTime = curTime;
            }

            base.GreenReadingCompleted(sender, e);
            HandleCheckout();

            //Thread.Sleep(3000);
        }
		//public override void ReadingCompleted(object sender, CardReaderEventArgs e)
		//{
		//    if (e.CardReader is ProlificCardReader)
		//    {
		//        if (!ProlificCardReaderOn) return;
		//        ProlificCardReaderOn = false;
		//    }

		//    // thread-safe when doing checkout
		//    lock (this)
		//    {
		//        Interlocked.Increment(ref _flag);
		//        var curTime = Environment.TickCount;
		//        if (_flag > 1 && (curTime - _lastReadTime) < 3000)
		//            return;
		//        Interlocked.Exchange(ref _flag, 1);
		//        _lastReadTime = curTime;
		//    }

		//    base.ReadingCompleted(sender, e);
		//    HandleCheckout();
		//}
		#region Auto Plate Reconition
		private Dictionary<string, DateTime> _processedPlate = new Dictionary<string, DateTime>();
		private string CurrentPlate = string.Empty;
	
		private string lastVehicleNumber = string.Empty;
		private DateTime n = DateTime.Now;
		private DateTime LastAnpr = DateTime.Now;
		private DateTime LastProcessed = DateTime.Now;
        private bool autoStart = false;
		private bool _doAuto = false;
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
			if (this.Section != null && this.Section.OptionByLane != null && this.Section.OptionByLane.AutoOutByVehicleRecognition)
			{
                autoStart = true;
				_autoTask = Task.Factory.StartNew(() => doTaskAutoAnpr(), TaskCreationOptions.LongRunning);
				
			}
		}
		private void StopAutoPlate()
		{
            autoStart = false;
			
		}
		
        private void doTaskAutoAnpr()
        {
            while (autoStart)
            {
				if (TimeAgo.IsDailog || _doAuto || !CheckTracker())
                {
					Thread.Sleep(1000);
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
					var tmpV = string.Empty;
					for (int i = 0; i < AmountRetry; i++)
					{
						var img = Section.BackOutCamera.CaptureImage();
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
                                    //rs = "51H-56789";// rs;
                                    tmpV = rs;
                                }
								if (rs != lastVehicleNumber)
								{
									lastVehicleNumber = rs;
									LastAnpr = DateTime.Now;
								}
								else if ((n - LastAnpr).TotalMilliseconds < 5000 && (n - LastAnpr).TotalMilliseconds > 0)
								{
									tmpV = string.Empty;
								}
							});
							while (chk && sw.ElapsedMilliseconds < 3000) ;
							sw.Stop();
							if (!string.IsNullOrEmpty(tmpV) && tmpV.Length >= 7 && tmpV.Length<=11)
							{
								var chkA = CheckAvalable(tmpV);
								if (chkA.Item3 && !CheckLastProcessed(chkA.Item1))
								{
									CurrentPlate = chkA.Item2;
									lock (this)
									{
										Interlocked.Increment(ref _flag);
										var curTime = Environment.TickCount;
										if (_flag > 1 && (curTime - _lastReadTime) < 3000)
										{
											return;
										}
										Interlocked.Exchange(ref _flag, 1);
										_lastReadTime = curTime;
									}
									lock (_locker)
									{
										_processedPlate[chkA.Item1] = DateTime.Now; ;
									}
									this.CheckedCard = new Card(chkA.Item1);
                                    InvokeOnMainThread(() => HandleCheckout(CurrentPlate));
									;
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
										Task.Delay(1000);
									}	
								}

							}
							else
							{
								Task.Delay(300);
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

		public void PlateCheckOut(string plate)
		{
            _doAuto = true;
			string card_id = "";
			string vehicleNumber = "";
			var cardId = plate.Replace("-", "_");
			var sw = new Stopwatch();
			sw.Start();
			var chk = true;
			_server.CollectPlate(cardId, plate, (res, ex) =>
			{
				chk = false;
				if (ex == null)
				{
					card_id = res.CardId;
					vehicleNumber = res.VehicleNumber;
				}
			});
			while (chk && sw.ElapsedMilliseconds < 3000) ;
			sw.Stop();
			if (!string.IsNullOrEmpty(card_id) && !string.IsNullOrEmpty(vehicleNumber))
			{
				CurrentPlate = vehicleNumber;
				lock (this)
				{
					Interlocked.Increment(ref _flag);
					var curTime = Environment.TickCount;
					if (_flag > 1 && (curTime - _lastReadTime) < 3000)
					{
						return;
					}
					Interlocked.Exchange(ref _flag, 1);
					_lastReadTime = curTime;
				}
				lock (_locker)
				{
					_processedPlate[card_id] = DateTime.Now; ;
				}
				this.CheckedCard = new Card(card_id);
				InvokeOnMainThread(() => HandleCheckout(CurrentPlate));
               
			}
			_doAuto = false;
		}
		#endregion
		protected override void ValidFarCardHandle(DetectedTag tag, string extention)
		{
			base.ValidFarCardHandle(tag, extention);
			if (!CheckTracker())
            {
				validating = false;
				return;
			}
			lock (this)
			{
				Interlocked.Increment(ref _flag);
				var curTime = Environment.TickCount;
				if (_flag > 1 && (curTime - _lastReadTime) < 3000)
                {
                    validating = false;
                    return;
                }					
				Interlocked.Exchange(ref _flag, 1);
				_lastReadTime = curTime;
			}
			lock (_locker)
			{
				_processedTags[tag.EPC] = tag.DetectedAt;
			}
			this.CheckedCard = new Card(tag.EPC);
			HandleCheckout();
			validating = false;
		}
		void OpenBarrier()
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
                //else if (this.Section.UseBarrierIpController)
                //{
                //    Devices.Dal.CardControler.CurrentListBarrierIp.OpenBarrier(this.Section.BarrierIpController, this.Section.BarrierPortController, this.Section.BarrierDoorsController, this.Section.TimeTick);
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
                //else if (this.Section.Barrier != null)
                //{
                //    if (_userPreferenceService.OptionsSettings.IsSfactorsCom)
                //    {
                //        ComManagement sftCom = ComManagement.GetInstance();  
                //        if (this.Section.BarrierPort.ToUpper().Contains("B1") && this.Section.BarrierPort.ToUpper().ToUpper().Contains("B2"))
                //        {   
                //            sftCom.AddCommand(new ComParameter()
                //            {
                //                ComName = this.Section.Barrier.PortName,
                //                Description = string.Format("Mở Barrier Check-out Lane: {0}", this.Section.LaneName),
                //                TimeApply = DateTime.Now,
                //                Commands = new List<ComCommand>()
                //                {
                //                        new ComCommand()
                //                        {
                //                            Command ="Write",
                //                            CommandMessage=string.Format("${0}#", "B1")
                //                        }       
                //                }
                //            });
                //            sftCom.AddCommand(new ComParameter()
                //            {
                //                ComName = this.Section.Barrier.PortName,
                //                Description = string.Format("Mở Barrier Check-out Lane: {0}", this.Section.LaneName),
                //                TimeApply = DateTime.Now,
                //                Commands = new List<ComCommand>()
                //                {
                //                        new ComCommand()
                //                        {
                //                            Command ="Write",
                //                            CommandMessage=string.Format("${0}#", "B2")
                //                        }
                //                }
                //            });
                //        }
                //        else
                //        {
                //            if (this.Section.BarrierPort.ToUpper().Contains("B1"))
                //                sftCom.AddCommand(new ComParameter()
                //                {
                //                    ComName = this.Section.Barrier.PortName,
                //                    Description = string.Format("Mở Barrier Check-out Lane: {0}", this.Section.LaneName),
                //                    TimeApply = DateTime.Now,
                //                    Commands = new List<ComCommand>()
                //                    {
                //                            new ComCommand()
                //                            {
                //                                Command ="Write",
                //                                CommandMessage=string.Format("${0}#", "B1")
                //                            }
                //                    }
                //                });
                //            else if (this.Section.BarrierPort.ToUpper().Contains("B2"))
                //                sftCom.AddCommand(new ComParameter()
                //                {
                //                    ComName = this.Section.Barrier.PortName,
                //                    Description = string.Format("Mở Barrier Check-out Lane: {0}", this.Section.LaneName),
                //                    TimeApply = DateTime.Now,
                //                    Commands = new List<ComCommand>()
                //                    {
                //                            new ComCommand()
                //                            {
                //                                Command ="Write",
                //                                CommandMessage=string.Format("${0}#", "B2")
                //                            }
                //                    }
                //                });
                //            else if (this.Section.LaneName.ToUpper().Equals("L1") || this.Section.LaneName.ToUpper().Equals("LAN1"))
                //            {   
                //                sftCom.AddCommand(new ComParameter()
                //                {
                //                    ComName = this.Section.Barrier.PortName,
                //                    Description = string.Format("Mở Barrier Check-out Lane: {0}", this.Section.LaneName),
                //                    TimeApply = DateTime.Now,
                //                    Commands = new List<ComCommand>()
                //                    {
                //                            new ComCommand()
                //                            {
                //                                Command ="Write",
                //                                CommandMessage=string.Format("${0}#", "B1")
                //                            }
                //                    }
                //                });
                //            }
                //            else
                //            {
                //                sftCom.AddCommand(new ComParameter()
                //                {
                //                    ComName = this.Section.Barrier.PortName,
                //                    Description = string.Format("Mở Barrier Check-out Lane: {0}", this.Section.LaneName),
                //                    TimeApply = DateTime.Now,
                //                    Commands = new List<ComCommand>()
                //                    {
                //                            new ComCommand()
                //                            {
                //                                Command ="Write",
                //                                CommandMessage=string.Format("${0}#", "B2")
                //                            }
                //                    }
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
        public void FindAndNotifyBlacklist()
        {
            //if (CheckOutData != null && !string.IsNullOrEmpty(CheckOutData.AlprVehicleNumber))
            //{
            //    int gate = _userPreferenceService.HostSettings.Terminal.Id;
            //    int user = this.Section.UserService.CurrentUser.Id;
            //    string vehiclenumber = CheckOutData.AlprVehicleNumber;
            //    string imgp = CheckOutData.BackImagePath;
            //    int pid = _checkinData.ParkingSessionId;
            //    _server.FindAndNotifyBlacklist(pid, imgp, vehiclenumber, gate, user, 1,
            //        (res, ex) =>
            //        {
            //            //do something
            //        }
            //    );
            //}
            
        }
        private void HandleWelcomLed()
        {
            if (this.Section.LedOfKind == LedStyle.Matrixs)
            {
                if(_userPreferenceService.OptionsSettings.IsSfactorsCom)
                {
                    string comname = this.Section.ComLed.ToUpper();
                    ComManagement sftCom = ComManagement.GetInstance();
                    ComParameter comparam = new ComParameter();
                    comparam = new ComParameter()
                    {
                        ComName = comname,
                        TimeApply = DateTime.Now,
                        Description = string.Format("Đèn báo cảm ơn, Lane:{0}", this.Section.LaneName),
                        Commands=new List<ComCommand>()
                        {
                            new ComCommand()
                            {
                                Command = "Write",
                                CommandMessage ="CMD2"
                            }
                        }
                    };
                    sftCom.AddCommand(comparam);
                }
                else
                {
                    this.IniSerialPortLed();
                    this.PrintToLedMatrix(_SerialPortLed, "CMD2");
                }      
            }
        }
        private void HandleCheckout(string plate = null)
        {
            if (_canCheckout &&  !_hasConfirmChecking)
            {
                IsVoucher = false;
                CanShowVoucher = System.Windows.Visibility.Hidden;
                CanShowVoucherDetail = System.Windows.Visibility.Hidden;
                InvokeOnMainThread(() => this.Notices = null);
                var card = this.CheckedCard;

                // log card id information
                _logger.Information($"Check out: {card.Id} {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}");

                if (_editTimer.Enabled && card.Id.Equals(_editTimer.CurrentCardId))
                {
                        GetCheckInData(ex =>
                    {
                        if (ex == null)
                        {
                           
                            Interlocked.Exchange(ref _flag, 0);
                            if (_userPreferenceService.OptionsSettings.IsVoucher)
                            {
                                CanShowVoucher = System.Windows.Visibility.Visible;
                                CheckInVehicle = _checkinData.VehicleTypeId;
                                CardIDVC = CheckOutData.CardId;
                                SecondVC = (int)(CheckOutData.CheckOutTime - CheckOutData.ReferenceCheckInTime).TotalSeconds;
                            }
                            else
                            {
                                CanShowVoucher = System.Windows.Visibility.Hidden;
                                CardIDVC = string.Empty;
                            }
                           
                            if (!_userPreferenceService.OptionsSettings.ConfirmCheckout && ex == null)
                            {
                                 _allowCheckout = false;
                                CheckOut(this.CheckOutData, exception =>
                                {
                                    if (exception == null)
                                    {
                                        Task.Factory.StartNew(() => {
                                            OpenBarrier();
                                            //Task.Factory.StartNew(() => FindAndNotifyBlacklist());
                                            //Thread.Sleep(300);
                                            //HandleWelcomLed();
                                            //Thread.Sleep(300) ;
                                            Alarm("success"); });     
                                        var tempCheckoutData = CheckOutData;
                                        _hasConfirmChecking = false;
                                        _editTimer.Start(tempCheckoutData.CardId);
                                        
                                        CountDown = _countDownMax;
                                        _allowCheckout = true;
                                        _canCheckout = true;
                                        //HandleError(IconEnums.Check, GetText("checkout.success"), false);
                                        if (CheckOutCompleted != null)
                                            CheckOutCompleted(null, new CheckoutEventArgs { Key = KeyAction.CheckOut });
                                    }
                                    else
                                    {
                                        _allowCheckout = true;
                                        IsVoucher = false;
                                        CanShowVoucher = System.Windows.Visibility.Hidden;
                                        CardIDVC = string.Empty;
                                        Task.Factory.StartNew(()=> Alarm("fail"));
                                        HandleError(IconEnums.Error, GetText("checkout.something_wrong"), false);
                                       
                                    }
                                    _canCheckout = true;     
                                });
                                
                            }
                        }
                        else
                        {
                            IsVoucher = false;
                            CanShowVoucher = System.Windows.Visibility.Hidden;
                            CardIDVC = string.Empty;
                            Task.Factory.StartNew(() => Alarm("fail"));
                        }
                    });
                }
                else
                {
                    //CheckOutData = null;
                    ResetUIInformation();
                    GetCheckInData(ex =>
                    {
                        if (ex != null)
                        {
                            Task.Factory.StartNew(() => Alarm("fail"));
                            IsVoucher = false;
                            CanShowVoucher = System.Windows.Visibility.Hidden;
                            CardIDVC = string.Empty;
                            if (ex is NotFoundException)
                                HandleError(IconEnums.Card, GetText("checkin.card_not_found"), false);
                            else
                            {
                                if (ex is InternalServerErrorException || ex is ServerDisconnectException)
                                {
                                    HandleError(IconEnums.Error, GetText("checkout.something_wrong"), false);
                                }
                                if (!(ex is ServerErrorException))
                                {
                                    PrintLog<CheckOutLaneViewModel>(ex, _userPreferenceService.HostSettings.LogServerIP);
                                }
                                if (RequestExceptionManager.GetExceptionMessage<CheckIn>(ex.Message).Key == RequestExceptionEnum.CardIsNotInUse)
                                {
                                    HandleError(IconEnums.Card, GetText("checkin.not_check_in_yet"), false);
                                }
                            }
                        }
                        else
                        {
                            //if (_userPreferenceService.OptionsSettings.IsVoucher)
                            //{
                            //    CanShowVoucher = System.Windows.Visibility.Visible;
                            //    CheckInVehicle = _checkinData.VehicleTypeId;
                            //    CardIDVC = CheckOutData.CardId;
                            //    SecondVC = (int)(CheckOutData.CheckOutTime - CheckOutData.ReferenceCheckInTime).TotalSeconds;
                            //}
                            //else
                            //{
                            //    CanShowVoucher = System.Windows.Visibility.Hidden;
                            //    CardIDVC = string.Empty;
                            //}
                            Interlocked.Exchange(ref _flag, 0);
                            if (!_userPreferenceService.OptionsSettings.ConfirmCheckout && ex == null)
                            {
                                _allowCheckout = false;
                                CheckOut(this.CheckOutData, exception =>
                                {
                                    if (exception == null)
                                    {
                                        Task.Factory.StartNew(() =>
                                        {
                                            OpenBarrier();
                                            //Task.Factory.StartNew(() => FindAndNotifyBlacklist());
                                            //Thread.Sleep(300);
                                            //HandleWelcomLed();
                                            //Thread.Sleep(300);
                                            Alarm("success");
                                        });
                                        var tempCheckoutData = CheckOutData;
                                        _hasConfirmChecking = false;
                                        _editTimer.Start(tempCheckoutData.CardId);
                                        CountDown = _countDownMax;
                                        _canCheckout = true;
                                        _allowCheckout = true;
                                        if (CheckOutCompleted != null)
                                            CheckOutCompleted(null, new CheckoutEventArgs { Key = KeyAction.CheckOut });
                                        //HandleError(IconEnums.Check, GetText("checkout.success"), false);
                                    }
                                    else
                                    {
                                        _allowCheckout = true;
                                        IsVoucher = false;
                                        CanShowVoucher = System.Windows.Visibility.Hidden;
                                        CardIDVC = string.Empty;
                                        Task.Factory.StartNew(() => Alarm("fail"));
                                        HandleError(IconEnums.Error, GetText("checkout.something_wrong"), false);    
                                    }
                                    _canCheckout = true;
                                });

                            }
                        }
                    });
                }
            }
        }

        private void SimulateTapCheckout(Action<ISection, TestCard, Exception> complete)
        {
            if (_hasConfirmChecking)
                return;
            Mvx.Resolve<IWebApiTestingServer>().GetCardCheckout(this.Section, (card, exception) =>
            {
                if (card == null || card.Delay == -1)
                {
                    if (complete != null)
                        complete(Section, card, exception);
                    return;
                }

                string cardId = card.CardId;
                if (cardId != null)
                {
                    this.CheckedCard = new Card(cardId.ToString());
                    if (this.CheckOutData == null)
                        GetCheckInData(async exc =>
                        {
                            if (exc != null || this.CheckOutData == null)
                            {
                                if (complete != null)
                                    complete(Section, card, exc);
                            }
                            else
                            {
                                CheckOut(this.CheckOutData, (ex) =>
                                {
                                    if (complete != null)
                                        complete(Section, card, ex);
                                });
                            }
                            await Task.Delay(500);
                            InvokeOnMainThread(() =>
                            {
                                //this.CheckOutData = null;
                                ResetUIInformation();
                                this.Notices = null;
                            });
                        });
                }
                else
                {
                    if (complete != null)
                        complete(Section, card, exception);
                }
            });
        }

        public override void TakingOffCompleted(object sender, CardReaderEventArgs e)
        {
            base.TakingOffCompleted(sender, e);
        }
        private bool wait = false;
        public void ShowCashAmount(int amount)
        {
            HandleError(IconEnums.Guide, string.Format("Khách đã trả {0} đồng", amount), true);
        }

        //public event EventHandler ShowPopupBarrier;

        protected override void OnKeyPressed(KeyPressedMessage msg)
        {
            //msg.KeyEventArgs.Handled = true;

            //if (this.CheckOutData == null)
            //    return;
            
            string output;
            KeyAction action = this.Section.KeyMap.GetAction(msg.KeyEventArgs, out output);

            switch (action)
            {
                case KeyAction.CheckOut:
                    {   
                        if (this.CheckOutData == null || !_userPreferenceService.OptionsSettings.ConfirmCheckout
                            || (_editTimer.Enabled && _editTimer.CurrentCardId== this.CheckOutData.CardId))
                            return;
                        if (_allowCheckout)
                        {
                            _allowCheckout = false;
                            this.CheckOut(this.CheckOutData, (ex) =>
                            {
                                if (ex == null)
                                {
                                    Task.Factory.StartNew(() =>
                                    {
                                        OpenBarrier();
                                        //Task.Factory.StartNew(() => FindAndNotifyBlacklist());
                                        //Thread.Sleep(300);
                                        //HandleWelcomLed();
                                        //Thread.Sleep(300);
                                        Alarm("success");
                                    });
                                    
                                    var tempCheckoutData = CheckOutData;
                                    _editTimer.Start(tempCheckoutData.CardId);
                                    CountDown = _countDownMax;
                                    
                                    this.Notices = null;
                                    if (CheckOutCompleted != null)
                                        CheckOutCompleted(null, new CheckoutEventArgs { Key = action });
                                    HandleError(IconEnums.Check, GetText("checkout.success"), false);
                                    _allowCheckout = true;
                                    _canCheckout = true;
                                }
                                else
                                {
                                    _allowCheckout = true;
                                    IsVoucher = false;
                                    CanShowVoucher = System.Windows.Visibility.Hidden;
                                    Task.Factory.StartNew(() => Alarm("fail"));
                                    HandleError(IconEnums.Error, GetText("checkout.something_wrong"), false);
                                }
                                _hasConfirmChecking = false;
                            });   
                        }
                        //else
                        //{
                        //    //_allowCheckout = true;
                        //    HandleError(IconEnums.Error, GetText("checkout.force_update_plate_number"), true);
                        //}
                        break;
                    }
                case KeyAction.Cashier:
                    {
                        
                        //if(ShowCashier!=null)
                        //{
                        //    ShowCashier(new CashierITC(new Devices.Dal.CashierCounter.ItcCashierInfo()
                        //    {
                        //        Bill = (int)(CustomerInfo.ParkingFee),
                        //        Total = 0,
                        //        Amount = 0,     
                        //        From = DateTime.Now,
                        //        TimeOutSeconds = 600
                        //    }));
                        //}

                            //if (this.CheckOutData == null || !_userPreferenceService.OptionsSettings.ConfirmCheckout
                            //    || (_editTimer.Enabled && _editTimer.CurrentCardId == this.CheckOutData.CardId))
                            //    return;
                            //if (_allowCheckout)
                            //{
                            //    _allowCheckout = false;
                            //    this.CheckOut(this.CheckOutData, (ex) =>
                            //    {
                            //        if (ex == null)
                            //        {
                            //            Task.Factory.StartNew(() =>
                            //            {
                            //                OpenBarrier();
                            //                Task.Factory.StartNew(() => FindAndNotifyBlacklist());
                            //                Thread.Sleep(300);
                            //                HandleWelcomLed();
                            //                Thread.Sleep(300);
                            //                Alarm("success");
                            //            });
                            //            var tempCheckoutData = CheckOutData;
                            //            _editTimer.Start(tempCheckoutData.CardId);
                            //            CountDown = _countDownMax;
                            //            _allowCheckout = true;
                            //            this.Notices = null;
                            //            if (CheckOutCompleted != null)
                            //                CheckOutCompleted(null, new CheckoutEventArgs { Key = action });
                            //            HandleError(IconEnums.Check, GetText("checkout.success"), false);
                            //            _allowCheckout = true;
                            //            _canCheckout = true;
                            //        }
                            //        else
                            //        {
                            //            _allowCheckout = true;
                            //            IsVoucher = false;
                            //            CanShowVoucher = System.Windows.Visibility.Hidden;
                            //            Task.Factory.StartNew(() => Alarm("fail"));
                            //            HandleError(IconEnums.Error, GetText("checkout.something_wrong"), false);
                            //        }

                            //    });
                            //}
                            ////else
                            ////{
                            ////    //_allowCheckout = true;
                            ////    HandleError(IconEnums.Error, GetText("checkout.force_update_plate_number"), true);
                            ////}
                        break;
                    }
                case KeyAction.Number:
                    {
                        var tempCheckoutData = CheckOutData;
                        if (tempCheckoutData == null)
                            return;
                        if (_canCheckout)
                            return;

                        if (_notEditPlateNumberYet)
                        {
                            tempCheckoutData.VehicleNumber = string.Empty;
                            _notEditPlateNumberYet = false;
                        }
                        tempCheckoutData.VehicleNumber = tempCheckoutData.VehicleNumber.TrimEnd() + output + " ";
                        //tempCheckoutData.VehicleNumber += output; // response.Output
                        tempCheckoutData.AlprVehicleNumber = tempCheckoutData.VehicleNumber;
                        if (_userPreferenceService.OptionsSettings.ForceUpdatePlateNumber)
                            _allowCheckout = true;
                        break;
                    }
                case KeyAction.Delete:
                    {
                        var tempCheckoutData = CheckOutData;
                        if (tempCheckoutData == null)
                            return;
                        //if (CheckOutData == null)
                        //    break;
                        int leng = tempCheckoutData.VehicleNumber.Length;
                        if (leng > 0)
                            tempCheckoutData.VehicleNumber = tempCheckoutData.VehicleNumber.Remove(leng - 1);
                        break;
                    }
                case KeyAction.CancelCheckOut:
                    {
                        
                        Task.Factory.StartNew(() => 
                        {
                            //if (Section.LedOfKind == LedStyle.Matrixs)
                            //    HandleWelcomLed();
                            //else
                            //{
                            //    if (_userPreferenceService.OptionsSettings.IsSfactorsCom)
                            //        PrintToLedSfactorsStanard("0 VND", "", 0);
                            //}
                        });
                        _hasConfirmChecking = false;
                        CanShowVoucher = System.Windows.Visibility.Hidden;
                        IsVoucher = false;
                        _canCheckout = true;
                        CardIDVC = string.Empty;
                        if (CheckOutData !=null && _userPreferenceService.OptionsSettings.IsVoucher)
                        {
                            DeleteVoucher(CheckOutData.CardId, CheckOutData.ReferenceCheckInTime);      
                        }
                        _notEditPlateNumberYet = true;
                        if (this.CheckOutData == null)
                            return;
                        this.Notices = null;
                        //this.CheckOutData = null;
                        ResetUIInformation();
                        if (CheckOutCompleted != null)
                            CheckOutCompleted(null, new CheckoutEventArgs { Key = action });
                        //this.ParkingDuration = string.Empty;
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
                        ConfirmLogoutCommand.Execute(null);
                        break;
                    }
                case KeyAction.ChangeLaneDirection:
                    {
                        ChangeLaneDirectionCommand.Execute(null);
                        break;
                    }
                case KeyAction.ShowVehicleType:
                    if (CustomerInfo != null && CustomerInfo.VehicleRegistrationInfo != null)
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
                case KeyAction.PrintBill:
                    if (this.CheckOutData == null)
                        return;
                    Task.Factory.StartNew(() => this.PrintToPrinter());        
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
                        _hasConfirmChecking = false;
                    }
                    break;
                case KeyAction.CashDrawer:
                    if (this.CheckOutData == null || !_userPreferenceService.OptionsSettings.ConfirmCheckout
                            || (_editTimer.Enabled && _editTimer.CurrentCardId == this.CheckOutData.CardId))
                        return;

                    if (countDay(_checkinData.CheckInTimeServer,_checkinData.CheckOutTimeServer) > 0)
                    {
                        if (_checkinData.VehicleTypeId == 2000101)
                        {
                            if (CustomerInfo.ParkingFee > 2000000)
                                CustomerInfo.ParkingFee -= 2000000;
                        }
                        else
                        {
                            if (CustomerInfo.ParkingFee > 200000)
                                CustomerInfo.ParkingFee -= 200000;
                        }
                    }

                    //Task.Factory.StartNew(() => Opencash());
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
                ComParameter comparam = new ComParameter();
                comparam = new ComParameter()
                {
                    ComName = comname,
                    TimeApply = DateTime.Now,
                    Description = string.Format("Mở Cash tính tiền Check-out, Lane:{0}", this.Section.LaneName),
                    Commands = new List<ComCommand>()
                    {
                        new ComCommand()
                        {
                            Command="Write",
                            CommandMessage="1"
                        }
                    }
                };
                sftCom.AddCommand(comparam);
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
                    Description = string.Format("Đèn cảnh báo Check-out, Lane:{0}", this.Section.LaneName),
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
                    Description = string.Format("Đèn báo Check-out thành công, Lane:{0}", this.Section.LaneName),
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
               AlarmSuccess();
            else if (key == "fail")
                AlarmWarning();
        }
        private DateTime chekHandleBarrier = DateTime.Now;
        private void HandForcedBarier()
        {
           
            Task.Factory.StartNew(() => OpenBarrier());
            ForcedInfo data = new ForcedInfo();
            data.Lane = Section.Lane.Name + " - OUT";
            data.PCAddress = string.Format("{0}-{1}", _userPreferenceService.HostSettings.Terminal.Ip, _userPreferenceService.HostSettings.Terminal.Name);
            data.User = Section.UserService.CurrentUser.DisplayName;
            //var now = DateTime.Now;
            var now = TimeMapInfo.Current.LocalTime;
            data.ForcedTimeStamp = TimestampConverter.DateTime2Timestamp(now);
            _frontImage = Section.FrontOutCamera.CaptureImage();
            _backImage = Section.BackOutCamera.CaptureImage();
           
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
                if (ex == null)
                {
                    SaveForcedBrImage(data);
                }

                if (ex != null)
                {
                    if (ex is InternalServerErrorException)
                    {
                        HandleError(IconEnums.Error, "Mở Cưỡng bức Barrier không lưu được dữ liệu" + now.ToString("yyyy-MM-dd HH:mm:ss"), false, false);

                        //HandleError(IconEnums.Error, "Mở Cưỡng bức Barrier không lưu được dữ liệu" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), false, false);
                        PrintLog<CheckOutLaneViewModel>(ex, _userPreferenceService.HostSettings.LogServerIP);
                    }
                    else
                    {
                        HandleError(IconEnums.Error, "Mở Cưỡng bức Barrier không lưu được dữ liệu" + now.ToString("yyyy-MM-dd HH:mm:ss"), false, false);
                        //HandleError(IconEnums.Error, "Mở Cưỡng bức Barrier không lưu được dữ liệu" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), false, false);
                        PrintLog<CheckOutLaneViewModel>(ex);
                    }
                }
                else
                {
                    HandleError(IconEnums.Guide, "Bạn vừa Mở cưỡng bức Barrier bằng nút", false, false);
                }
            });
        }

        public void ForcedBarier(string note)
        {
            isdoinng = false;
            Task.Factory.StartNew(() => OpenBarrier());
            ForcedInfo data = new ForcedInfo();
            data.Lane = this.Section.Lane.Name + " - OUT";
            data.PCAddress = string.Format("{0}-{1}", _userPreferenceService.HostSettings.Terminal.Ip, _userPreferenceService.HostSettings.Terminal.Name);
            data.User = this.Section.UserService.CurrentUser.DisplayName;
            var now = TimeMapInfo.Current.LocalTime;
            //var now = DateTime.Now;
            data.ForcedTimeStamp = TimestampConverter.DateTime2Timestamp(now);
            _frontImage = Section.FrontOutCamera.CaptureImage();
            _backImage = Section.BackOutCamera.CaptureImage();

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
                    SaveForcedBrImage(data);
                }

                isdoinng = true;
                if (ex != null)
                {
                    if (ex is InternalServerErrorException)
                    {
                        HandleError(IconEnums.Error, "Mở Cưỡng bức Barrier không lưu được dữ liệu" + now.ToString("yyyy-MM-dd HH:mm:ss"), false, false);

                        //HandleError(IconEnums.Error, "Mở Cưỡng bức Barrier không lưu được dữ liệu" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), false, false);
                        PrintLog<CheckOutLaneViewModel>(ex, _userPreferenceService.HostSettings.LogServerIP);
                    }
                    else
                    {
                        HandleError(IconEnums.Error, "Mở Cưỡng bức Barrier không lưu được dữ liệu" + now.ToString("yyyy-MM-dd HH:mm:ss"), false, false);
                        //HandleError(IconEnums.Error, "Mở Cưỡng bức Barrier không lưu được dữ liệu" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), false, false);
                        PrintLog<CheckOutLaneViewModel>(ex);
                    }
                }
                else
                {
                    HandleError(IconEnums.Guide, "Bạn vừa Mở cưỡng bức Barrier", false, false);
                }
            });
        }

        private void HandleForcedBarier(ISection section)
        {
            Task.Factory.StartNew(() => OpenBarrier());
            ForcedInfo data = new ForcedInfo();
            data.Lane = section.Lane.Name + " - OUT";
            data.PCAddress = string.Format("{0}-{1}", _userPreferenceService.HostSettings.Terminal.Ip, _userPreferenceService.HostSettings.Terminal.Name);
            data.User = section.UserService.CurrentUser.DisplayName;
            //var now = DateTime.Now;
            var now = TimeMapInfo.Current.LocalTime;
            data.ForcedTimeStamp = TimestampConverter.DateTime2Timestamp(now);
            _frontImage = Section.FrontOutCamera.CaptureImage();
            _backImage = Section.BackOutCamera.CaptureImage();

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
                if (ex == null)
                {
                    SaveForcedBrImage(data);
                }

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
                            PrintLog<CheckOutLaneViewModel>(exception);
                            return;
                        }
                    }
                });
        }

        public override void ChooseVehicleType(VehicleType type)
        {
            var card = this.CheckedCard;
            if (_checkinData != null)
            {
                _checkinData.CardId = card.Id;
                _checkinData.VehicleType = type;
                _server.UpdateCheckIn(_checkinData.GetClone, (result, ex) =>
                {
                    if (ex == null)
                    {
                        _server.GetCheckIn(result.CardId, (result1, ex1) =>
                        {
                            if (ex1 == null && result1 != null)
                                CustomerInfo = result1.CustomerInfo;
                        });
                    }
                });
            }
        }

        private void ResetUIInformation()
        {
            CheckOutData = null;
            CustomerInfo = null;
            IsVoucher = false;
            CanShowVoucher = System.Windows.Visibility.Hidden;
            CanShowVoucherDetail = System.Windows.Visibility.Hidden;
            CountDown = 0;

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
            _editTimer.Stop();
            this.StopMaintainace();
            this.StopAutoPlate();
            base.Close();
        }

        #region Private Methods

        private void AutoCheckOut(string manualVehicleNumber)
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
                    try
                    {
                        this.CheckedCard = new Card { Id = result.CardId };
                        HandleCheckout();
                        AllowManualInputVehicleNumber = false;
                    }
                    catch (Exception ex3)
                    {
                        Console.WriteLine(ex3);
                        PrintLog<CheckInLaneViewModel>(ex3, _userPreferenceService.HostSettings.LogServerIP);
                    }
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