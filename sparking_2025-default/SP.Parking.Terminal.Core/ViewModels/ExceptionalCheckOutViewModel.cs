using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.ViewModels;
using SP.Parking.Terminal.Core.Models;
using SP.Parking.Terminal.Core.Services;
using SP.Parking.Terminal.Core.Utilities;
using Green.Devices.Dal;
using Green.Devices.Dal.Siemens;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SP.Parking.Terminal.Core.ViewModels
{
    public class ExceptionalCheckOutMessage : MvxMessage
    {
        public ParkingSession CheckedOutItem { get; set; }
        public ExceptionalCheckOutMessage(object sender, ParkingSession item)
            : base(sender)
        {
            CheckedOutItem = item;
        }
    }

    public class ExceptionalCheckOutViewModel : BaseViewModel
    {
        IServer _server;
        IMvxMessenger _messenger;
        IUserService _userService;
        IUserServiceLocator _userServiceLocator;
        IUserPreferenceService _userPreferenceService;
        IMvxMessenger _exceptionCheckoutMessenger;
        IStorageService _storageService;
        ILogService _logService;

        string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                if (_errorMessage == value) return;
                _errorMessage = value;
                RaisePropertyChanged(() => ErrorMessage);
            }
        }

        private int _messageLevel;
        public int MessageLevel
        {
            get { return _messageLevel; }
            set
            {
                if (_messageLevel == value) return;
                _messageLevel = value;
                RaisePropertyChanged(() => MessageLevel);
            }
        }

        private float _parkingFee;

        public float ParkingFee
        {
            get { return _parkingFee; }
            set
            {
                _parkingFee = value;
                RaisePropertyChanged(() => ParkingFee);
            }
        }

        string _reason;
        public string Reason
        {
            get { return _reason; }
            set
            {
                if (_reason == value) return;
                _reason = value;
                RaisePropertyChanged(() => Reason);
            }
        }
       
        bool _isBlocked;
        public bool IsBlocked
        {
            get { return _isBlocked; }
            set
            {
                if (_isBlocked == value) return;
                _isBlocked = value;
                RaisePropertyChanged(() => IsBlocked);
            }
        }

        private ApmsUser _user;
        public ApmsUser User
        {
            get { return _user; }
            set { _user = value; }
        }

        ParkingSession _selectedItem;
        public ParkingSession SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (_selectedItem == value) return;
                _selectedItem = value;
                RaisePropertyChanged(() => SelectedItem);
            }
        }

        public ExceptionalCheckOutViewModel(IViewModelServiceLocator service,
            IStorageService storageService,
            IServer server)
            : base(service)
        {
            _server = server;
            _messenger = Mvx.Resolve<IMvxMessenger>();
            _exceptionCheckoutMessenger = Mvx.Resolve<IMvxMessenger>();
            _userServiceLocator = Mvx.Resolve<IUserServiceLocator>();
            _userPreferenceService = Mvx.Resolve<IUserPreferenceService>();
            _storageService = storageService;
            _logService = Mvx.Resolve<ILogService>();
        }

        public virtual void Init(ParameterKey key)
        {
            object[] objs = (object[])Services.Parameter.Retrieve(key);

            this.Section = (Section)objs[0];
            this.SelectedItem = (ParkingSession)objs[1];
            _userService = _userServiceLocator.GetUserService(this.Section.Id);
           
            this.User = _userService.CurrentUser;
            _server.GetCheckIn(SelectedItem.CardId, (@in, ex) =>
            {
                if (@in != null && @in.CustomerInfo != null)
                {
                    ParkingFee = @in.CustomerInfo.ParkingFee;
                }

                //2024 fee for ParcMall Temp Moment
                if (@in.CardTypeId == 0)
                    ParkingFee = recallfee(@in.CheckOutTimeServer, @in.CheckInTimeServer, @in.VehicleTypeId);
                else
                    ParkingFee = 0;
            });
        }

        private int countDay(DateTime startDate, DateTime endDate)
        {
            int count = 0;
            string fDate = startDate.ToShortDateString();
            string tDate = endDate.ToShortDateString();

            count = (DateTime.Parse(tDate + " 00:00:00") - DateTime.Parse(fDate + " 00:00:00")).Days;

            return count;
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

        public override void Start()
        {
            base.Start();
        }

        void SendReason(Action<Exception> complete)
        {
            if (SelectedItem == null)
            {
                ErrorMessage = GetText("exceptional_checkout.information_missing");
                MessageLevel = 3;
                return;
            }

            if (string.IsNullOrEmpty(Reason))
            {
                ErrorMessage = GetText("exceptional_checkout.reason_missing");
                MessageLevel = 3;
                return;
            }
            else
            {
                Reason = Reason.TrimEnd(' ').TrimStart();
                if (string.IsNullOrEmpty(Reason))
                {
                    ErrorMessage = GetText("exceptional_checkout.reason_missing");
                    MessageLevel = 3;
                    return;
                }
            }

            string cardId = SelectedItem.CardId;
            int terminalId = _userPreferenceService.HostSettings.Terminal.Id;
            int laneId = Section.Lane.Id;
            int operatorId = User.Id;

            _server.CreateExceptionalCheckOut(cardId, terminalId, laneId, operatorId, Reason, IsBlocked, ParkingFee, (exception) =>
            {
                if (complete != null)
                    complete(exception);
            });
        }

        public void PublishCloseChildEvent(SectionPosition position)
        {
            if (_messenger.HasSubscriptionsFor<CloseChildMessage>())
            {
                _messenger.Publish(new CloseChildMessage(this, position));
            }
        }

        MvxCommand _exceptionalCheckoutCommand;
        public ICommand ExceptionalCheckoutCommand
        {
            get
            {
                _exceptionalCheckoutCommand = _exceptionalCheckoutCommand ?? new MvxCommand(() =>
                {
                    SendReason(ex =>
                    {
                        if (ex == null)
                        {
                            if (_userPreferenceService.OptionsSettings.BarrierForcedWithCheckOutException)
                                HandForcedBarier();
                            SetMessage("exceptional_checkout.success");

                            if (_exceptionCheckoutMessenger.HasSubscriptionsFor<ExceptionalCheckOutMessage>())
                            {
                                _exceptionCheckoutMessenger.Publish(new ExceptionalCheckOutMessage(this, SelectedItem));
                                GoBackCommand.Execute(null);
                            }
                        }
                        else
                            SetMessage("exceptional_checkout.error");
                    });
                });
                return _exceptionalCheckoutCommand;
            }
        }
        void OpenBarrier()
        {
            if (this.Section != null)
            {
                if (this.Section.BarrierBySiemensControl != null && this.Section.BarrierBySiemensControl.Active && !string.IsNullOrEmpty(Section.BarrierBySiemensControl.IP))
                {
                    Port4 mydevice = Port4.GetInstance();
                    mydevice.AddCommandOut(new SiemenInfo {
                        TcpIp= this.Section.BarrierBySiemensControl.IP,
                        TypeOut= this.Section.BarrierBySiemensControl.TypeOut
                    });
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
                else if (this.Section.UseBarrierIpController)
                {
                  Green.Devices.Dal.CardControler.CurrentListBarrierIp.OpenBarrier(this.Section.BarrierIpController, this.Section.BarrierPortController, this.Section.BarrierDoorsController, this.Section.TimeTick);
                }
                else if (this.Section.Barrier != null)
                {
                    if (_userPreferenceService.OptionsSettings.IsSfactorsCom)
                    {
                        ComManagement sftCom = ComManagement.GetInstance();
                        if (this.Section.BarrierPort.ToUpper().Contains("B1") && this.Section.BarrierPort.ToUpper().ToUpper().Contains("B2"))
                        {
                            sftCom.AddCommand(new ComParameter()
                            {
                                ComName = this.Section.Barrier.PortName,
                                Description = string.Format("Mở Barrier Check-out Lane: {0}", this.Section.LaneName),
                                TimeApply = DateTime.Now,
                                Commands = new List<ComCommand>()
                                {
                                        new ComCommand()
                                        {
                                            Command ="Write",
                                            CommandMessage=string.Format("${0}#", "B1")
                                        }
                                }
                            });
                            sftCom.AddCommand(new ComParameter()
                            {
                                ComName = this.Section.Barrier.PortName,
                                Description = string.Format("Mở Barrier Check-out Lane: {0}", this.Section.LaneName),
                                TimeApply = DateTime.Now,
                                Commands = new List<ComCommand>()
                                {
                                        new ComCommand()
                                        {
                                            Command ="Write",
                                            CommandMessage=string.Format("${0}#", "B2")
                                        }
                                }
                            });
                        }
                        else
                        {
                            if (this.Section.BarrierPort.ToUpper().Contains("B1"))
                                sftCom.AddCommand(new ComParameter()
                                {
                                    ComName = this.Section.Barrier.PortName,
                                    Description = string.Format("Mở Barrier Check-out Lane: {0}", this.Section.LaneName),
                                    TimeApply = DateTime.Now,
                                    Commands = new List<ComCommand>()
                                    {
                                            new ComCommand()
                                            {
                                                Command ="Write",
                                                CommandMessage=string.Format("${0}#", "B1")
                                            }
                                    }
                                });
                            else if (this.Section.BarrierPort.ToUpper().Contains("B2"))
                                sftCom.AddCommand(new ComParameter()
                                {
                                    ComName = this.Section.Barrier.PortName,
                                    Description = string.Format("Mở Barrier Check-out Lane: {0}", this.Section.LaneName),
                                    TimeApply = DateTime.Now,
                                    Commands = new List<ComCommand>()
                                    {
                                            new ComCommand()
                                            {
                                                Command ="Write",
                                                CommandMessage=string.Format("${0}#", "B2")
                                            }
                                    }
                                });
                            else if (this.Section.LaneName.ToUpper().Equals("L1") || this.Section.LaneName.ToUpper().Equals("LAN1"))
                            {
                                sftCom.AddCommand(new ComParameter()
                                {
                                    ComName = this.Section.Barrier.PortName,
                                    Description = string.Format("Mở Barrier Check-out Lane: {0}", this.Section.LaneName),
                                    TimeApply = DateTime.Now,
                                    Commands = new List<ComCommand>()
                                    {
                                            new ComCommand()
                                            {
                                                Command ="Write",
                                                CommandMessage=string.Format("${0}#", "B1")
                                            }
                                    }
                                });
                            }
                            else
                            {
                                sftCom.AddCommand(new ComParameter()
                                {
                                    ComName = this.Section.Barrier.PortName,
                                    Description = string.Format("Mở Barrier Check-out Lane: {0}", this.Section.LaneName),
                                    TimeApply = DateTime.Now,
                                    Commands = new List<ComCommand>()
                                    {
                                            new ComCommand()
                                            {
                                                Command ="Write",
                                                CommandMessage=string.Format("${0}#", "B2")
                                            }
                                    }
                                });
                            }
                        }

                    }
                    else
                    {
                        if (this.Section.BarrierPort.ToUpper().Contains("B1") && this.Section.BarrierPort.ToUpper().Contains("B2"))
                        {
                            this.Section.Barrier.DevicePort = "B1";
                            this.Section.Barrier.Open();
                            Thread.Sleep(100);
                            this.Section.Barrier.DevicePort = "B2";
                            this.Section.Barrier.Open();
                            Thread.Sleep(100);
                            this.Section.Barrier.DevicePort = "B1B2";
                            this.Section.BarrierPort = "B1B2";
                        }
                        else if (this.Section.BarrierPort.ToUpper().Contains("B1"))
                        {
                            this.Section.Barrier.DevicePort = "B1";
                            this.Section.Barrier.Open();
                        }
                        else if (this.Section.BarrierPort.ToUpper().Contains("B2"))
                        {
                            this.Section.Barrier.DevicePort = "B2";
                            this.Section.Barrier.Open();
                        }
                        else if (this.Section.LaneName.ToUpper().Equals("L1") || this.Section.LaneName.ToUpper().Equals("LAN1"))
                        {
                            this.Section.Barrier.DevicePort = "B1";
                            this.Section.Barrier.Open();
                        }
                        else
                        {
                            this.Section.Barrier.DevicePort = "B2";

                            this.Section.Barrier.Open();
                        }
                    }
                }
            }
        }
        private void HandForcedBarier()
        {

            Task.Factory.StartNew(() => OpenBarrier());
            ForcedInfo data = new ForcedInfo();
            data.Lane = Section.Lane.Name + " - Check out ngoại lệ";
            data.PCAddress = string.Format("{0}-{1}", _userPreferenceService.HostSettings.Terminal.Ip, _userPreferenceService.HostSettings.Terminal.Name);
            data.User = Section.UserService.CurrentUser.DisplayName;
            data.Note = Reason;
            //var now = DateTime.Now;
            var now = TimeMapInfo.Current.LocalTime;
            data.ForcedTimeStamp = TimestampConverter.DateTime2Timestamp(now);
            var _frontImage = Section.FrontOutCamera.CaptureImage();
            var _backImage = Section.BackOutCamera.CaptureImage();

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
                if(ex == null)
                {
                    SaveImage(result);
                }

                //if (ex != null)
                //{
                //    if (ex is InternalServerErrorException)
                //    {
                //        HandleError(IconEnums.Error, "Mở Cưỡng bức Barrier không lưu được dữ liệu" + now.ToString("yyyy-MM-dd HH:mm:ss"), false, false);

                //        //HandleError(IconEnums.Error, "Mở Cưỡng bức Barrier không lưu được dữ liệu" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), false, false);
                //        PrintLog<CheckInLaneViewModel>(ex, _userPreferenceService.HostSettings.LogServerIP);
                //    }
                //    else
                //    {
                //        HandleError(IconEnums.Error, "Mở Cưỡng bức Barrier không lưu được dữ liệu" + now.ToString("yyyy-MM-dd HH:mm:ss"), false, false);
                //        //HandleError(IconEnums.Error, "Mở Cưỡng bức Barrier không lưu được dữ liệu" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), false, false);
                //        PrintLog<CheckInLaneViewModel>(ex);
                //    }
                //}
                //else
                //{
                //    HandleError(IconEnums.Guide, "Bạn vừa Mở cưỡng bức Barrier bằng nút", false, false);
                //}
            });
        }

        private void SaveImage(ForcedInfo forceInfo)
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
                            PrintLog<ExceptionalCheckOutViewModel>(exception);
                            return;
                        }
                    }
                });
        }

        protected void PrintLog<T>(Exception exception, string logServer = null, bool captureScreen = false) where T : class
        {
            _logService.Log(exception, logServer, null, 0, null, captureScreen);
        }

        private void SetMessage(string msg)
        {
            if (msg.Equals("exceptional_checkout.success"))
            {
                ErrorMessage = GetText("exceptional_checkout.success");
                MessageLevel = 1;
            }
            else if (msg.Equals("exceptional_checkout.error"))
            {
                ErrorMessage = GetText("exceptional_checkout.error");
                MessageLevel = 3;
            }
        }

        MvxCommand _goBackCommand;
        public ICommand GoBackCommand
        {
            get
            {
                _goBackCommand = _goBackCommand ?? new MvxCommand(() =>
                {
                    PublishCloseChildEvent(this.Section.Id);  
                });
                return _goBackCommand;
            }
        }
    }
}
