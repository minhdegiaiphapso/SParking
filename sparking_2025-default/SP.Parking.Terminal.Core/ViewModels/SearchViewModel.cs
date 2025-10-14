using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.ViewModels;
using Green.Devices.Dal;
using Newtonsoft.Json.Linq;
using OpenCvSharp.Aruco;
using SP.Parking.Terminal.Core.Models;
using SP.Parking.Terminal.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using static Green.APS.Devices.HikVision.CHCNetSDK.tagNET_DVR__WIFI_CFG_EX;

namespace SP.Parking.Terminal.Core.ViewModels
{
    public class SearchViewModel : BaseViewModel
    {
        IServer _server;
        IStorageService _storageService;
        //IKeyService _keyService;
        MvxSubscriptionToken _keyPressedToken;
        MvxSubscriptionToken _exceptionalCheckOutToken;
        protected IUserPreferenceService _userPreferenceService;
        protected IUserServiceLocator _userServiceLocator;
        protected IUserService _userService;

        IMvxMessenger _messenger;
        #region Properties
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

        private List<string> _numberOfResult;
        public List<string> NumberOfResult
        {
            get { return _numberOfResult; }
            set
            {
                _numberOfResult = value;
                RaisePropertyChanged(() => NumberOfResult);
            }
        }

        private string _selectedNumberOfResult;
        public string SelectedNumberOfResult
        {
            get { return _selectedNumberOfResult; }
            set
            {
                _selectedNumberOfResult = value;
                RaisePropertyChanged(() => SelectedNumberOfResult);
            }
        }

        private List<int> _pageNumbers;
        public List<int> PageNumbers
        {
            get { return _pageNumbers; }
            set
            {
                if (_pageNumbers == value) return;
                _pageNumbers = value;
                RaisePropertyChanged(() => PageNumbers);
            }
        }
        private int _totalPage = 1;
        public int TotalPage
        {
            get { return _totalPage; }
            set
            {
                if (_totalPage == value) return;
                _totalPage = value;
                RaisePropertyChanged(() => TotalPage);
            }
        }
        private int _selectedPage = 1;
        public int SelectedPage
        {
            get { return _selectedPage; }
            set
            {
                if (_selectedPage == value) return;
                _selectedPage = value;
                if (_selectedPage < 1)
                    _selectedPage = 1;
                if (_selectedPage > _totalPage)
                    _selectedPage = _totalPage;
                RaisePropertyChanged(() => SelectedPage);
            }
        }

        private VehicleType _selectedVehicleType;
        public VehicleType SelectedVehicleType
        {
            get { return _selectedVehicleType; }
            set
            {
                _selectedVehicleType = value;
                RaisePropertyChanged(() => SelectedVehicleType);
            }
        }

        private bool _selectedItemIsAvailableToCheckOut;
        public bool SelectedItemIsAvailableToCheckOut
        {
            get { return _selectedItemIsAvailableToCheckOut; }
            set
            {
                _selectedItemIsAvailableToCheckOut = value;
                RaisePropertyChanged(() => SelectedItemIsAvailableToCheckOut);
            }
        }


        List<VehicleType> _vehicleTypes;
        public List<VehicleType> VehicleTypes
        {
            get { return _vehicleTypes; }
            set
            {
                _vehicleTypes = value;
                RaisePropertyChanged(() => VehicleTypes);
            }
        }

        private Models.Terminal _selectedTerminal;
        public Models.Terminal SelectedTerminal
        {
            get { return _selectedTerminal; }
            set
            {
                _selectedTerminal = value;
                RaisePropertyChanged(() => SelectedTerminal);
            }
        }

        List<Models.Terminal> _terminals;
        public List<Models.Terminal> ListTerminals
        {
            get { return _terminals; }
            set
            {
                _terminals = value;
                RaisePropertyChanged(() => ListTerminals);
            }
        }

        List<TerminalGroup> _terminalGroups;
        public List<TerminalGroup> TerminalGroups
        {
            get { return _terminalGroups; }
            set
            {
                _terminalGroups = value;
                RaisePropertyChanged(() => TerminalGroups);
            }
        }

        private bool _isCheckedCurrentUser;
        public bool IsCheckedCurrentUser
        {
            get { return _isCheckedCurrentUser; }
            set
            {
                _isCheckedCurrentUser = value;
                if (ParkingSession != null)
                    ParkingSession.IsCurrentUser = _isCheckedCurrentUser;
                if (this._isCheckedCurrentUser)
                {
                    var v = Enum.GetValues(typeof(ParkingSessionEnum)).Cast<ParkingSessionEnum>().
                       Where(w => w == Models.ParkingSessionEnum.InParking ||
                       w == Models.ParkingSessionEnum.OnlyOut);

                    this.ParkingSessionEnum = v;
                    SelectedParkingSession = Models.ParkingSessionEnum.InParking;
                }
                else
                {
                    this.ParkingSessionEnum = Enum.GetValues(typeof(ParkingSessionEnum)).Cast<ParkingSessionEnum>();
                    SelectedParkingSession = Models.ParkingSessionEnum.All;
                }
                RaisePropertyChanged(() => IsCheckedCurrentUser);
            }
        }

        IEnumerable<ParkingSessionEnum> _parkingSessionEnum;
        public IEnumerable<ParkingSessionEnum> ParkingSessionEnum
        {
            get { return _parkingSessionEnum; }
            set
            {
                _parkingSessionEnum = value;
                RaisePropertyChanged(() => ParkingSessionEnum);
            }
        }

        public ParkingSessionEnum SelectedParkingSession { get; set; }

        ParkingSession _parkingSession;
        public ParkingSession ParkingSession
        {
            get { return _parkingSession; }
            set
            {
                if (_parkingSession == value)
                    return;
                _parkingSession = value;
                RaisePropertyChanged(() => ParkingSession);
            }
        }

        ParkingSession _selectedItem;
        public ParkingSession SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (_selectedItem == value) return;
                _selectedItem = value;
                SelectItem(_selectedItem);
                RaisePropertyChanged(() => SelectedItem);
            }
        }

        /*** 29/07/2016 ***/
        ObservableCollection<ParkingSession> _parkingSessions;
        public ObservableCollection<ParkingSession> ParkingSessions
        {
            get { return _parkingSessions; }
            set
            {
                if (_parkingSessions == value) return;
                _parkingSessions = value;

                if (_parkingSessions != null)
                {
                    float _toalParkingFee = _parkingSessions.Sum(s => s.ParkingFee);
                    int _Visitor = _parkingSessions.Count(c => c.CardTypeId == 0),
                        _vethang = _parkingSessions.Count(c => c.CardTypeId == 1),
                        _foc = _parkingSessions.Count(c => c.CardTypeId == 2);
                    if (_toalParkingFee > 0)
                        this.StrTotalParkingFee = string.Format("TỔNG: {0:0,0 VNĐ}", _toalParkingFee);
                    else
                        this.StrTotalParkingFee = string.Format("");

                    if (_Visitor > 0)
                        this.StrVisitors = string.Format("Thẻ vãng lai: {0:0,0}", _Visitor);
                    else
                        this.StrVisitors = "";
                    if (_vethang > 0)
                        this.StrVeThang = string.Format("Thẻ tháng: {0:0,0}", _vethang);
                    else
                        this.StrVeThang = "";
                    if (_foc == 0)
                        this.StrFOC = "";
                    else
                        this.StrFOC = string.Format("Thẻ Foc: {0:0,0}", _foc);

                }
                else
                {
                    this.StrTotalParkingFee = "TỔNG:";
                    this.StrVisitors = "The vang lai:";
                    this.StrFOC = "The Foc :";
                    this.StrVeThang = "The Thang:";
                }
                RaisePropertyChanged(() => ParkingSessions);
            }
        }

        private byte[] _frontImage;
        public byte[] FrontImage
        {
            get { return _frontImage; }
            set
            {
                _frontImage = value;
                RaisePropertyChanged(() => FrontImage);
            }
        }

        private byte[] _backImage;
        public byte[] BackImage
        {
            get { return _backImage; }
            set
            {
                _backImage = value;
                RaisePropertyChanged(() => BackImage);
            }
        }

        private byte[] _miniFrontImage;
        public byte[] MiniFrontImage
        {
            get { return _miniFrontImage; }
            set
            {
                _miniFrontImage = value;
                RaisePropertyChanged(() => MiniFrontImage);
            }
        }

        private byte[] _miniBackImage;
        public byte[] MiniBackImage
        {
            get { return _miniBackImage; }
            set
            {
                _miniBackImage = value;
                RaisePropertyChanged(() => MiniBackImage);
            }
        }
        //2018Jun08
        private byte[] _extra1;
        public byte[] Extra1
        {
            get
            {
                return _extra1;
            }
            set
            {
                _extra1 = value;
                RaisePropertyChanged(() => Extra1);
            }
        }
        private byte[] _extra2;
        public byte[] Extra2
        {
            get
            {
                return _extra2;
            }
            set
            {
                _extra2 = value;
                RaisePropertyChanged(() => Extra2);
            }
        }
        private byte[] _miniextra1;
        public byte[] MiniExtra1
        {
            get
            {
                return _miniextra1;
            }
            set
            {
                _miniextra1 = value;
                RaisePropertyChanged(() => MiniExtra1);
            }
        }
        private byte[] _miniextra2;
        public byte[] MiniExtra2
        {
            get
            {
                return _miniextra2;
            }
            set
            {
                _miniextra2 = value;
                RaisePropertyChanged(() => MiniExtra2);
            }
        }
        //2018Jun08
        string _resultMessage;
        public string ResultMessage
        {
            get { return _resultMessage; }
            set
            {
                _resultMessage = value;
                RaisePropertyChanged(() => ResultMessage);
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

        /*** 29/07/2016 ***/
        private string _strTotalParkingFee = "TỔNG: ";
        public string StrTotalParkingFee
        {
            get { return _strTotalParkingFee; }
            set
            {
                //if (_strTotalParkingFee == value) return;
                _strTotalParkingFee = value;
                RaisePropertyChanged(() => StrTotalParkingFee);
            }
        }

        private string _strVisitors = "Thẻ vãng lai:";
        public string StrVisitors
        {
            get { return _strVisitors; }
            set
            {
                //if (_strVisitors == value) return;
                _strVisitors = value;
                RaisePropertyChanged(() => StrVisitors);
            }
        }

        private string _strVeThang = "Thẻ tháng:";
        public string StrVeThang
        {
            get { return _strVeThang; }
            set
            {
                //if (_strVeThang == value) return;
                _strVeThang = value;
                RaisePropertyChanged(() => StrVeThang);
            }
        }

        private string _strFOC = "Thẻ Foc";
        public string StrFOC
        {
            get { return _strFOC; }
            set
            {
                //if (_strFOC == value) return;
                _strFOC = value;
                RaisePropertyChanged(() => StrFOC);
            }
        }
        private bool _IsDoSearch = false;

        #endregion
        //#region Properties
        //private ApmsUser _user;
        //public ApmsUser User
        //{
        //    get { return _user; }
        //    set { _user = value; RaisePropertyChanged(() => UserNameAndId); }
        //}

        //public string UserNameAndId
        //{
        //    get
        //    {
        //        if (_user != null)
        //            return _user.DisplayName + " - " + _user.StaffID;
        //        else
        //            return null;
        //    }
        //}

        //private List<string> _numberOfResult;
        //public List<string> NumberOfResult
        //{
        //    get { return _numberOfResult; }
        //    set
        //    {
        //        _numberOfResult = value;
        //        RaisePropertyChanged(() => NumberOfResult);
        //    }
        //}

        //private string _selectedNumberOfResult;
        //public string SelectedNumberOfResult
        //{
        //    get { return _selectedNumberOfResult; }
        //    set
        //    {
        //        _selectedNumberOfResult = value;
        //        RaisePropertyChanged(() => SelectedNumberOfResult);
        //    }
        //}

        //private List<int> _pageNumbers;
        //public List<int> PageNumbers
        //{
        //    get { return _pageNumbers; }
        //    set
        //    {
        //        if (_pageNumbers == value) return;
        //        _pageNumbers = value;
        //        RaisePropertyChanged(() => PageNumbers);
        //    }
        //}

        //private int _selectedPage = 1;
        //public int SelectedPage
        //{
        //    get { return _selectedPage; }
        //    set
        //    {
        //        if (_selectedPage == value) return;
        //        _selectedPage = value;
        //        RaisePropertyChanged(() => SelectedPage);
        //    }
        //}

        //private VehicleType _selectedVehicleType;
        //public VehicleType SelectedVehicleType
        //{
        //    get { return _selectedVehicleType; }
        //    set
        //    {
        //        _selectedVehicleType = value;
        //        RaisePropertyChanged(() => SelectedVehicleType);
        //    }
        //}

        //private bool _selectedItemIsAvailableToCheckOut;
        //public bool SelectedItemIsAvailableToCheckOut
        //{
        //    get { return _selectedItemIsAvailableToCheckOut; }
        //    set
        //    {
        //        _selectedItemIsAvailableToCheckOut = value;
        //        RaisePropertyChanged(() => SelectedItemIsAvailableToCheckOut);
        //    }
        //}


        //List<VehicleType> _vehicleTypes;
        //public List<VehicleType> VehicleTypes
        //{
        //    get { return _vehicleTypes; }
        //    set
        //    {
        //        _vehicleTypes = value;
        //        RaisePropertyChanged(() => VehicleTypes);
        //    }
        //}

        //private Models.Terminal _selectedTerminal;
        //public Models.Terminal SelectedTerminal
        //{
        //    get { return _selectedTerminal; }
        //    set
        //    {
        //        _selectedTerminal = value;
        //        RaisePropertyChanged(() => SelectedTerminal);
        //    }
        //}

        //List<Models.Terminal> _terminals;
        //public List<Models.Terminal> ListTerminals
        //{
        //    get { return _terminals; }
        //    set
        //    {
        //        _terminals = value;
        //        RaisePropertyChanged(() => ListTerminals);
        //    }
        //}

        //List<TerminalGroup> _terminalGroups;
        //public List<TerminalGroup> TerminalGroups
        //{
        //    get { return _terminalGroups; }
        //    set
        //    {
        //        _terminalGroups = value;
        //        RaisePropertyChanged(() => TerminalGroups);
        //    }
        //}

        //private bool _isCheckedCurrentUser;
        //public bool IsCheckedCurrentUser
        //{
        //    get { return _isCheckedCurrentUser; }
        //    set
        //    {
        //        _isCheckedCurrentUser = value;
        //        if (ParkingSession != null)
        //            ParkingSession.IsCurrentUser = _isCheckedCurrentUser;
        //        if (this._isCheckedCurrentUser)
        //        {
        //            var v = Enum.GetValues(typeof(ParkingSessionEnum)).Cast<ParkingSessionEnum>().
        //               Where(w => w == Models.ParkingSessionEnum.InParking ||
        //               w == Models.ParkingSessionEnum.OnlyOut);

        //            this.ParkingSessionEnum = v;
        //            SelectedParkingSession = Models.ParkingSessionEnum.InParking;
        //        }else
        //        {
        //            this.ParkingSessionEnum = Enum.GetValues(typeof(ParkingSessionEnum)).Cast<ParkingSessionEnum>();
        //            SelectedParkingSession = Models.ParkingSessionEnum.All;
        //        }
        //        RaisePropertyChanged(() => IsCheckedCurrentUser);
        //    }
        //}

        //IEnumerable<ParkingSessionEnum> _parkingSessionEnum;
        //public IEnumerable<ParkingSessionEnum> ParkingSessionEnum
        //{
        //    get { return _parkingSessionEnum; }
        //    set
        //    {
        //        _parkingSessionEnum = value;
        //        RaisePropertyChanged(() => ParkingSessionEnum);
        //    }
        //}

        //public ParkingSessionEnum SelectedParkingSession { get; set; }

        //ParkingSession _parkingSession;
        //public ParkingSession ParkingSession
        //{
        //    get { return _parkingSession; }
        //    set
        //    {
        //        if (_parkingSession == value)
        //            return;
        //        _parkingSession = value;
        //        RaisePropertyChanged(() => ParkingSession);
        //    }
        //}

        //ParkingSession _selectedItem;
        //public ParkingSession SelectedItem
        //{
        //    get { return _selectedItem; }
        //    set
        //    {
        //        if (_selectedItem == value) return;
        //        _selectedItem = value;
        //        SelectItem(_selectedItem);
        //        RaisePropertyChanged(() => SelectedItem);
        //    }
        //}

        ///*** 29/07/2016 ***/
        //ObservableCollection<ParkingSession> _parkingSessions;
        //public ObservableCollection<ParkingSession> ParkingSessions
        //{
        //    get { return _parkingSessions; }
        //    set
        //    {
        //        if (_parkingSessions == value) return;
        //        _parkingSessions = value;

        //        if (_parkingSessions != null)
        //        {
        //            float _toalParkingFee = _parkingSessions.Sum(s => s.ParkingFee);
        //            int _Visitor = _parkingSessions.Count(c =>c.CardTypeId==0),
        //                _vethang = _parkingSessions.Count(c => int.Parse(c.CardLabel) <= 4000),
        //                _foc = _parkingSessions.Count(c => int.Parse(c.CardLabel) > 9500);
        //            if (_toalParkingFee > 0)
        //                this.StrTotalParkingFee = string.Format("Tổng: {0:0,0 vnđ}", _toalParkingFee);
        //            else
        //                this.StrTotalParkingFee = string.Format("");

        //            if (_Visitor > 0)
        //                this.StrVisitors = string.Format("Thẻ vãng lai: {0:0,0}", _Visitor);
        //            else
        //                this.StrVisitors = "";
        //            if (_vethang > 0)
        //                this.StrVeThang = string.Format("Thẻ tháng: {0:0,0}", _vethang);
        //            else
        //                this.StrVeThang = "";
        //            if (_foc == 0)
        //                this.StrFOC = "";
        //            else
        //                this.StrFOC = string.Format("Thẻ Foc: {0:0,0}", _foc);

        //        }
        //        else
        //        {
        //            this.StrTotalParkingFee = "Tổng:";
        //            this.StrVisitors = "Thẻ vãng lai:";
        //            this.StrFOC = "Thẻ Foc";
        //            this.StrVeThang = "Thẻ tháng:";
        //        }
        //        RaisePropertyChanged(() => ParkingSessions);
        //    }
        //}

        //private byte[] _frontImage;
        //public byte[] FrontImage
        //{
        //    get { return _frontImage; }
        //    set
        //    {
        //        _frontImage = value;
        //        RaisePropertyChanged(() => FrontImage);
        //    }
        //}

        //private byte[] _backImage;
        //public byte[] BackImage
        //{
        //    get { return _backImage; }
        //    set
        //    {
        //        _backImage = value;
        //        RaisePropertyChanged(() => BackImage);
        //    }
        //}

        //private byte[] _miniFrontImage;
        //public byte[] MiniFrontImage
        //{
        //    get { return _miniFrontImage; }
        //    set
        //    {
        //        _miniFrontImage = value;
        //        RaisePropertyChanged(() => MiniFrontImage);
        //    }
        //}

        //private byte[] _miniBackImage;
        //public byte[] MiniBackImage
        //{
        //    get { return _miniBackImage; }
        //    set
        //    {
        //        _miniBackImage = value;
        //        RaisePropertyChanged(() => MiniBackImage);
        //    }
        //}

        //string _resultMessage;
        //public string ResultMessage
        //{
        //    get { return _resultMessage; }
        //    set
        //    {
        //        _resultMessage = value;
        //        RaisePropertyChanged(() => ResultMessage);
        //    }
        //}

        //private int _messageLevel;
        //public int MessageLevel
        //{
        //    get { return _messageLevel; }
        //    set
        //    {
        //        if (_messageLevel == value) return;
        //        _messageLevel = value;
        //        RaisePropertyChanged(() => MessageLevel);
        //    }
        //}

        ///*** 29/07/2016 ***/
        //private string _strTotalParkingFee = "Tổng: ";
        //public string StrTotalParkingFee
        //{
        //    get { return _strTotalParkingFee; }
        //    set
        //    {
        //        //if (_strTotalParkingFee == value) return;
        //        _strTotalParkingFee = value;
        //        RaisePropertyChanged(() => StrTotalParkingFee);
        //    }
        //}

        //private string _strVisitors = "Thẻ vãng lai:";
        //public string StrVisitors
        //{
        //    get { return _strVisitors; }
        //    set
        //    {
        //        //if (_strVisitors == value) return;
        //        _strVisitors = value;
        //        RaisePropertyChanged(() => StrVisitors);
        //    }
        //}

        //private string _strVeThang = "Thẻ tháng:";
        //public string StrVeThang
        //{
        //    get { return _strVeThang; }
        //    set
        //    {
        //        //if (_strVeThang == value) return;
        //        _strVeThang = value;
        //        RaisePropertyChanged(() => StrVeThang);
        //    }
        //}

        //private string _strFOC = "Thẻ Foc";
        //public string StrFOC
        //{
        //    get { return _strFOC; }
        //    set
        //    {
        //        //if (_strFOC == value) return;
        //        _strFOC = value;
        //        RaisePropertyChanged(() => StrFOC);
        //    }
        //}

        //#endregion

        public SearchViewModel(IViewModelServiceLocator service, IMvxMessenger messenger)
            : base(service)
        {
            _server = Mvx.Resolve<IServer>();
            _storageService = Mvx.Resolve<IStorageService>();
            //_keyService = Mvx.Resolve<IKeyService>();
            ParkingSession = new ParkingSession();
            ParkingSession.IsDoSearch = true;
            ParkingSession.Searching = System.Windows.Visibility.Hidden;
            _messenger = messenger;
            _keyPressedToken = service.Messenger.Subscribe<KeyPressedMessage>(OnKeyPressed);
            _exceptionalCheckOutToken = service.Messenger.Subscribe<ExceptionalCheckOutMessage>(OnExceptionalCheckOut);
            _userPreferenceService = Mvx.Resolve<IUserPreferenceService>();
            _userServiceLocator = Mvx.Resolve<IUserServiceLocator>();


            
        }
      
        public void Init(ParameterKey key)
        {
            this.Section = (Section)Services.Parameter.Retrieve(key);
            _userService = _userServiceLocator.GetUserService(this.Section.Id);
            this.User = _userService.CurrentUser;
        }
        public bool IsVoucher { get { return _userPreferenceService.OptionsSettings.IsVoucher; } }
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

        public override void Start()
        {
            base.Start();
            NumberOfResult = new List<string> { "50", "100", "500","1000"};
            SelectedNumberOfResult = NumberOfResult[1];

            ParkingSessionEnum = Enum.GetValues(typeof(ParkingSessionEnum)).Cast<ParkingSessionEnum>();
            SelectedParkingSession = Models.ParkingSessionEnum.All;
            bool bl = ParkingSession.IsCurrentUser;
            TypeHelper.GetTerminals(result =>
            {
                if (result != null)
                {
                    ListTerminals = result;
                    var _exists = this.ListTerminals.Exists(
                       delegate(Models.Terminal t)
                       {
                           return t.Id == 0;
                       });
                    if (!_exists)
                    {
                        Models.Terminal _item = new Models.Terminal();
                        _item.Id = 0;
                        _item.Ip = "127.0.0.1";
                        _item.TerminalId = Guid.Empty.ToString();
                        _item.Status = TerminalStatus.Enable;
                        _item.Name = "Tất cả";
                        ListTerminals.Insert(0, _item);
                    }
                    SelectedTerminal = ListTerminals[0];
                }

            });
            
            TypeHelper.GetVehicleTypes(result => {
                if(result != null)
                {
                    result.Reverse();
                    VehicleTypes = result;
                    TypeHelper.GetVehicleType((int)VehicleTypeEnum.All, type => ParkingSession.VehicleType = type);
                }
            });

            TypeHelper.GetTerminalGroups(result => {
                if (result != null)
                {
                    TerminalGroups = result;
                    TypeHelper.GetTerminalGroup(1, type => ParkingSession.TerminalGroup = type);
                }
            });
            
            //this.Section.StartCardReader(ReadingCompleted, null);
            if (this.Section.ModWinsCards != null)
            {
                CurrentListCardReader.StartGreenCardReader(this.Section.ModWinsCards, GreenReadingCompleted, null);
            }
            if (this.Section.TcpIpServerCards != null)
            {
                CurrentListCardReader.StartGreenCardReader(this.Section.TcpIpServerCards, GreenReadingCompleted, null);
            }
            if (this.Section.TcpIpClientCards != null)
            {
                CurrentListCardReader.StartGreenCardReader(this.Section.TcpIpServerCards, GreenReadingCompleted, null);
            }
            if (this.Section.TcpIpServerCards != null)
            {
                CurrentListCardReader.StartGreenCardReader(this.Section.TcpIpClientCards, GreenReadingCompleted, null);
            }
            if (this.Section.ScannelCards != null)
            {
                CurrentListCardReader.StartGreenCardReader(this.Section.ScannelCards, GreenReadingCompleted, null);
            }
            if (this.Section.TcpIpRemodeCards != null)
            {
                CurrentListCardReader.StartGreenCardReader(this.Section.TcpIpRemodeCards, GreenReadingCompleted, null);
            }
            BuildLaneTerminalDictionary();
        }

        public override void Loaded()
        {
            base.Loaded();
        }

        public void ReadingCompleted(object sender, CardReaderEventArgs e)
        {
            this.ParkingSession.CardId = e.CardID;
            _server.GetCardInfo(ParkingSession.CardId, (card, ex) => {
                if (ex == null)
                {
                    ParkingSession.CardLabel = card.Label;
                    ParkingSession.CardId = string.Empty;
                }
            });
        }
        public void GreenReadingCompleted(object sender, GreenCardReaderEventArgs e)
        {
            if (e.ex != null)
                return;
            this.ParkingSession.CardId = e.CardID;
            _server.GetCardInfo(ParkingSession.CardId, (card, ex) => {
                if (ex == null)
                {
                    ParkingSession.CardLabel = card.Label;
                    ParkingSession.CardId = string.Empty;
                }
            });
        }

        Dictionary<int, string> _laneTerminal = null;
        public void BuildLaneTerminalDictionary()
        {
            _laneTerminal = new Dictionary<int, string>();
            TypeHelper.GetLanes(lanes => {
                TypeHelper.GetTerminals(terminals => {
                    if (lanes == null || terminals == null) return;
                    foreach (var item in lanes)
                    {
                        int tId = item.TerminalId;
                        string tName = terminals.Where(t => t.Id == tId).Select(t => t.Name).FirstOrDefault();
                        if (!_laneTerminal.ContainsKey(item.Id))
                            _laneTerminal.Add(item.Id, tName);
                    }
                });
            });
        }

        SearchResult _currentResult = null;
        //int _currentPage = 1;
        int _maxPage = 1;
        private bool _canSearch = true;
        public bool CanSearch
        {
            get
            {
                return this._canSearch;
            }
            set
            {
                this._canSearch = value;
                this.RaisePropertyChanged(() => this.CanSearch);
            }
        }
        //private bool _IsDoSearch = false;
        public void DoSearch(Action complete)
        {
            if (_IsDoSearch)
            {
                //_IsDoSearch = true;
                return;
            }
            _IsDoSearch = true;
            CanSearch = false;
            ParkingSession.IsDoSearch = false;
            ParkingSession.Searching = System.Windows.Visibility.Visible;

            //System.Diagnostics.Debug.WriteLine("2: " + _IsDoSearch.ToString());

            int pageSize = 2;
            if (!int.TryParse(SelectedNumberOfResult, out pageSize)) pageSize = 2;
            if (ParkingSession.IsCurrentUser)
            {
                ParkingSession.CurrentUserId = Section.UserService.CurrentUser.Id;
            }
            else
            {
                ParkingSession.CurrentUserId = null;
                
            }
            Task.Factory.StartNew(() =>
            {
                _server.ParkingSessionSearch(ParkingSession, SelectedParkingSession, SelectedPage, pageSize, (result, ex) =>
                {
                    if (ex != null)
                    {
                        _IsDoSearch = false;
                        ParkingSession.IsDoSearch = true;
                        CanSearch = true;
                        ParkingSession.Searching = System.Windows.Visibility.Hidden;
                        ResultMessage = ex.Message;
                        MessageLevel = 3;
                        return;
                    }
                    else
                    {
                        _IsDoSearch = false;
                        ParkingSession.IsDoSearch = true;
                        CanSearch = true;
                        ParkingSession.Searching = System.Windows.Visibility.Hidden;
                        _currentResult = result;

                        _maxPage = _currentResult.Total / pageSize;
                        if (_currentResult.Total % pageSize > 0) _maxPage += 1;
                        TotalPage = _maxPage > 0 ? _maxPage : 1;


                        InvokeOnMainThread(() =>
                        {
                            if (ex != null)
                            {
                                ResultMessage = ex.Message;
                                MessageLevel = 3;
                                return;
                            }

                            List<Models.ParkingSession> _temp = _currentResult.ParkingSessions;
                            foreach (var item in _temp)
                            {
                                int id = 1;
                                int.TryParse(item.CheckInLane, out id);
                                /// Code mới từ 20250926
                                if (_laneTerminal.TryGetValue(id, out string value))
                                {
                                    // Key tồn tại, có thể dùng value
                                    item.TerminalName = value;
                                }
                                else
								{
                                    // Key không tồn tại
                                    item.TerminalName = "";
								}
                                /// Code cũ trước 20250926
								//item.TerminalName = _laneTerminal[id];
                            }


                            if (_currentResult.ParkingSessions.Count > 0 &&
                               this.SelectedTerminal != null &&
                               this.SelectedTerminal.Id > 0)
                            {
                                _temp = _currentResult.ParkingSessions.FindAll(
                                   delegate (Models.ParkingSession p)
                                   {
                                       return !string.IsNullOrEmpty(p.TerminalName) && p.TerminalName.Equals(this.SelectedTerminal.Name);
                                   });
                            }
                            ParkingSessions = new ObservableCollection<Models.ParkingSession>(_temp);


                            if (ParkingSessions != null && ParkingSessions.Count > 0)
                            {
                                SelectedItem = ParkingSessions[0];
                                ResultMessage = ParkingSessions[0].Total.ToString("#,###");// string.Format(GetText("search.number_of_results"), ParkingSessions[0].Total);
                                MessageLevel = 0;
                            }
                            else
                            {
                                ResultMessage = GetText("search.not_found");
                                MessageLevel = 3;
                            }

                            ParkingSession.CardId = string.Empty;

                            if (complete != null)
                                complete();
                        });
                    }

                });
            }
            );
        }

        public void GoNext()
        {
            int tmp = SelectedPage;
            tmp++;
            if (tmp > _maxPage)
                tmp = _maxPage;
            if (tmp != SelectedPage && tmp != 0)
            {
                SelectedPage = tmp;
                DoSearch(() => {

                });
            }
        }

        public void GoBack()
        {
            int tmp = SelectedPage;
            tmp--;
            if (tmp <= 1)
                tmp = 1;
            if (tmp != SelectedPage && tmp != 0)
            {
                SelectedPage = tmp;
                DoSearch(() => {

                });
            }
        }

        public void Search(Action complete)
        {
            int limit = 100;
            if (!int.TryParse(SelectedNumberOfResult, out limit)) limit = 0;
            _server.ParkingSessionSearchAdvance(ParkingSession, SelectedParkingSession, (result, ex) => {
                InvokeOnMainThread(() => {
                    if (ex != null)
                    {
                        ResultMessage = ex.Message;
                        MessageLevel = 3;
                        return;
                    }

                    ParkingSessions = new ObservableCollection<Models.ParkingSession>(result);
                    foreach(var item in ParkingSessions)
                    {
                        int id = 1;
                        int.TryParse(item.CheckInLane, out id);
                        if (item.TerminalName != null)
                            item.TerminalName = _laneTerminal[id];
                    }

                    //ParkingSessions = Filter(ParkingSessions, "Da");

                    if (ParkingSessions != null && ParkingSessions.Count > 0)
                    {
                        SelectedItem = ParkingSessions[0];
                        ResultMessage = ParkingSessions.Count.ToString("#,###");//string.Format(GetText("search.number_of_results"), ParkingSessions.Count);
                        MessageLevel = 0;
                    }
                    else
                    {
                        ResultMessage = GetText("search.not_found");
                        MessageLevel = 3;
                    }

                    ParkingSession.CardId = string.Empty;

                    if (complete != null)
                        complete();
                });
            }, limit);
        }

        private ObservableCollection<ParkingSession> Filter(ObservableCollection<ParkingSession> origin, string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) return origin;

            var rs = origin.Where(i => i.TerminalName.Contains(pattern));
            return new ObservableCollection<ParkingSession>(rs);
        }

        private void SelectItem(ParkingSession parkingSession)
        {
            if (parkingSession == null)
            {
                MiniFrontImage = null;
                MiniBackImage = null;
                FrontImage = null;
                BackImage = null;
                MiniExtra1 = null;
                MiniExtra2 = null;
                Extra1 = null;
                Extra2 = null;
                return;
            }

            if(parkingSession.CheckOutTime > 0)
            {
                _storageService.LoadImage(parkingSession.CheckOutFrontImage, string.Empty, (fiBytes, ex) => {
                    MiniFrontImage = fiBytes;
                });
                _storageService.LoadImage(parkingSession.CheckOutBackImage, string.Empty, (biBytes, ex) => {
                    MiniBackImage = biBytes;
                });

                _storageService.LoadImage(parkingSession.CheckInFrontImage, string.Empty, (biBytes, ex) => {
                    FrontImage = biBytes;
                });
                _storageService.LoadImage(parkingSession.CheckInBackImage, string.Empty, (biBytes, ex) => {
                    BackImage = biBytes;
                });
                _storageService.LoadImage(parkingSession.CheckInExtra1Image, string.Empty, (biBytes, ex) => {
                    Extra1 = biBytes;
                });
                _storageService.LoadImage(parkingSession.CheckInExtra2Image, string.Empty, (biBytes, ex) => {
                    Extra2 = biBytes;
                });
                _storageService.LoadImage(parkingSession.CheckOutExtra1Image, string.Empty, (biBytes, ex) => {
                    MiniExtra1 = biBytes;
                });
                _storageService.LoadImage(parkingSession.CheckOutExtra2Image, string.Empty, (biBytes, ex) => {
                    MiniExtra2 = biBytes;
                });
            }
            else
            {
                MiniFrontImage = null;
                MiniBackImage = null;
                _storageService.LoadImage(parkingSession.CheckInFrontImage, string.Empty, (fiBytes, ex) => {
                    FrontImage = fiBytes;
                });
                _storageService.LoadImage(parkingSession.CheckInBackImage, string.Empty, (biBytes, ex) => {
                    BackImage = biBytes;
                });
                MiniExtra1 = null;
                MiniExtra2 = null;
                _storageService.LoadImage(parkingSession.CheckInExtra1Image, string.Empty, (fiBytes, ex) => {
                    Extra1 = fiBytes;
                });
                _storageService.LoadImage(parkingSession.CheckInExtra2Image, string.Empty, (biBytes, ex) => {
                    Extra2 = biBytes;
                });
            }

            SelectedItemIsAvailableToCheckOut = CanCheckOut(parkingSession);
            /***Chinh sua 24-07-2016 ***/
            this.IsEnabledPrintBill = !CanCheckOut(parkingSession);
        }

        private bool CanCheckOut(ParkingSession parkingSession)
        {
            if (parkingSession == null) return false;

            if (parkingSession.CheckOutTime == -1)
                return true;

            return false;
        }

        MvxCommand _searchCommand;
        public ICommand SearchCommand
        {
            get
            {
                _searchCommand = _searchCommand ?? new MvxCommand(() => {
                    //System.Diagnostics.Debug.WriteLine("1: " + _IsDoSearch.ToString());
                    //DoSearch(null);
                    if (!_IsDoSearch)
                    {
                        //System.Diagnostics.Debug.WriteLine("1: " + _IsDoSearch.ToString());
                        DoSearch(null);
                    }

                    //System.Diagnostics.Debug.WriteLine("3: " + _IsDoSearch.ToString());
                });

                return _searchCommand;
            }
        }

        MvxCommand _backCommand;
        public ICommand BackCommand
        {
            get
            {
                _backCommand = _backCommand ?? new MvxCommand(() => {
                    PublishCloseChildEvent(this.Section.Id);
                    PublishShowCheckingLaneEvent();
                });

                return _backCommand;
            }
        }

        MvxCommand _selectPageCommand;
        public ICommand SelectPageCommand
        {
            get
            {
                _selectPageCommand = _selectPageCommand ?? new MvxCommand(() => {
                    DoSearch(null);
                });
                return _selectPageCommand;
            }
        }

        MvxCommand _goNextCommand;
        public ICommand GoNextCommand
        {
            get
            {
                _goNextCommand = _goNextCommand ?? new MvxCommand(() => {
                    GoNext();
                });
                return _goNextCommand;
            }
        }

        MvxCommand _goBackCommand;
        public ICommand GoBackCommand
        {
            get
            {
                _goBackCommand = _goBackCommand ?? new MvxCommand(() => {
                    GoBack();
                });
                return _goBackCommand;
            }
        }

        MvxCommand _goBackOUT;
        public ICommand goBackOUT
        {
            get
            {
                _goBackOUT = _goBackOUT ?? new MvxCommand(() =>
                {
                    PublishCloseChildEvent(this.Section.Id);
                    PublishShowCheckingLaneEvent();
                });

                return _goBackOUT;
            }
        }

        MvxCommand<ParkingSession> _exceptionalCheckoutCommand;
        public MvxCommand<ParkingSession> ExceptionalCheckoutCommand
        {
            get
            {
                _exceptionalCheckoutCommand = _exceptionalCheckoutCommand ?? new MvxCommand<ParkingSession>((parkingSession) => {
                    if (!CanCheckOut(parkingSession))
                    {
                        return;
                    }
                    PublishShowExceptionalCheckout(parkingSession);
                });

                return _exceptionalCheckoutCommand;
            }
        }

        /***Chinh sua 01-08-2016 ***/
        MvxCommand<ParkingSession> _printBillCommand;
        public MvxCommand<ParkingSession> PrintBillCommand
        {
            get
            {
                _printBillCommand = _printBillCommand ?? new MvxCommand<ParkingSession>((parkingSession) =>
                {
                    if (CanCheckOut(parkingSession))
                    {
                        return;
                    }
                    
                    var result = MsgBox.Show("BẠN THỰC SỰ MUỐN IN HÓA ĐƠN?", "XÁC NHẬN", MsgBox.Buttons.YesNo, MsgBox.Icon.Question);
                    //System.Windows.MessageBox.Show("Xác nhận In Phiếu","Xác nhận", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question)
                   //if (result == System.Windows.Forms.DialogResult.Yes)

                    if (result == System.Windows.Forms.DialogResult.Yes)
                    {
                        if (_userPreferenceService.OptionsSettings.IsPrintV2)
                            Task.Factory.StartNew(() => PublishPrintBillV2(parkingSession));
                            //PublishPrintBillV2(parkingSession);
                        else
                            Task.Factory.StartNew(() => PublishPrintBill(parkingSession));
                            //PublishPrintBill(parkingSession);
                    }
                });

                return _printBillCommand;
            }
        }

        //private CheckIn _checkinData;
        /***Chinh sua 01-08-2016 ***/
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
        ParkingSession tmp_print;
        private void PrintDoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            if (tmp_print == null)
                return;
            string _total = string.Empty;
            try
            {
                _total = ToReadableString(DateTime.ParseExact(tmp_print.StrCheckOutTime, "dd/MM/yyyy HH:mm:ss", new System.Globalization.CultureInfo("vi-VN")) -
                                               DateTime.ParseExact(tmp_print.StrCheckInTime, "dd/MM/yyyy HH:mm:ss", new System.Globalization.CultureInfo("vi-VN")));
            }
            catch
            {
                try
                {
                    _total = ToReadableString(DateTime.ParseExact(tmp_print.StrCheckOutTime, "dd-MM-yyyy HH:mm:ss", new System.Globalization.CultureInfo("vi-VN")) -
                                               DateTime.ParseExact(tmp_print.StrCheckInTime, "dd-MM-yyyy HH:mm:ss", new System.Globalization.CultureInfo("vi-VN")));
                }
                catch
                {
                    _total = "Vui lòng định dạng lại ngày giờ máy Server  'dd/MM/yyyy HH:mm:ss' hoặc 'dd-MM-yyyy HH:mm:ss'";
                }
            }

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
                //var url = System.Windows.Forms.Application.StartupPath + "\\config\\logo_print.png";
                //Image img = Image.FromFile(url);
                //if (img != null)
                //    e.Graphics.DrawImage(img, 5, 5);
                rect.Y = 35;
                e.Graphics.DrawString("Ngày in Bill:", font1, System.Drawing.Brushes.Black, rect, formatLeft);
                e.Graphics.DrawString(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 50;
                e.Graphics.DrawString("Nhân viên xử lý:", font1, System.Drawing.Brushes.Black, rect, formatLeft);
                e.Graphics.DrawString(this.User.DisplayName, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 80;
                e.Graphics.DrawString("Loại phương tiện:", font1, System.Drawing.Brushes.Black, rect, formatLeft);
                e.Graphics.DrawString(tmp_print.VehicleType.Name, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 95;
                e.Graphics.DrawString("Biển số xe:", font1, System.Drawing.Brushes.Black, rect, formatLeft);
                e.Graphics.DrawString(tmp_print.AlprVehicleNumber.IndexOf(" ") > -1 ? tmp_print.VehicleNumber : tmp_print.AlprVehicleNumber, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 110;
                e.Graphics.DrawString("Đối tượng:", font1, System.Drawing.Brushes.Black, rect, formatLeft);
                e.Graphics.DrawString(tmp_print.StrCardType, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 140;
                e.Graphics.DrawString("Ngày giờ vào:", font1, System.Drawing.Brushes.Black, rect, formatLeft);
                e.Graphics.DrawString(tmp_print.StrCheckInTime, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 155;
                e.Graphics.DrawString("NGày giờ ra:", font1, System.Drawing.Brushes.Black, rect, formatLeft);
                e.Graphics.DrawString(tmp_print.StrCheckOutTime, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 170;
                e.Graphics.DrawString("Thời gian lưu bãi:", font1, System.Drawing.Brushes.Black, rect, formatLeft);
                e.Graphics.DrawString(_total, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 200;
                e.Graphics.DrawString("Số tiền thanh toán:", font1, System.Drawing.Brushes.Black, rect, formatLeft);
                e.Graphics.DrawString(string.Format("{0:0,0 VND}", tmp_print.StrParkingFee), font1, System.Drawing.Brushes.Black, rect, format);
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
                string printAddress = "19/39A Trần Bình Trọng, Phường 5, Bình Thạnh, Tp. Hồ Chí Minh";
                string printCallFax = "Tel: +84 909 091 533 - Email: info@vietpark.tech";
                if (img != null && !string.IsNullOrEmpty(this.Section.PrintAddressTitle) && !string.IsNullOrEmpty(this.Section.PrintCallTile))
                {
                    e.Graphics.DrawImage(new Bitmap(img, 175, 75), 0, 0);
                    printAddress = this.Section.PrintAddressTitle;
                    printCallFax = this.Section.PrintCallTile;
                    e.Graphics.DrawString(printAddress, font3, System.Drawing.Brushes.Black, 95, 5);//"65 Le Loi Boulevard, District 1, HCMC, Vietnam"
                    e.Graphics.DrawString(printCallFax, font3, System.Drawing.Brushes.Black, 95, 20);//"Tel: (84.8) 3823 2500 - Fax: +84 (8) 38229 822"
                }
                else
                {  
                    url = System.Windows.Forms.Application.StartupPath + "\\config\\print_default_logo.png";
                    img = Image.FromFile(url);
                    if (img != null)
                    {
                        e.Graphics.DrawImage(new Bitmap(img, 75, 75), 0, 0);
                    }
                    e.Graphics.DrawString(printAddress, font3, System.Drawing.Brushes.Black, 5, 5);//"65 Le Loi Boulevard, District 1, HCMC, Vietnam"
                    e.Graphics.DrawString(printCallFax, font3, System.Drawing.Brushes.Black, 5, 20);//"Tel: (84.8) 3823 2500 - Fax: +84 (8) 38229 822"
                }    
                e.Graphics.DrawString("Ngày in Bill:", font1, System.Drawing.Brushes.Black, rect, formatLeft);
                e.Graphics.DrawString(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 85;  
                e.Graphics.DrawString("Nhân viên xử lý:", font1, System.Drawing.Brushes.Black, rect, formatLeft);

                e.Graphics.DrawString(this.User.DisplayName, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 100;
                e.Graphics.DrawString("Cổng kiểm soát:", font1, System.Drawing.Brushes.Black, rect, formatLeft);

                e.Graphics.DrawString(tmp_print.TerminalName, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 130;
                e.Graphics.DrawString("Loại phương tiện:", font1, System.Drawing.Brushes.Black, rect, formatLeft);

                e.Graphics.DrawString(tmp_print.VehicleType.Name, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 145;
                e.Graphics.DrawString("Biển số xe:", font1, System.Drawing.Brushes.Black, rect, formatLeft);

                e.Graphics.DrawString(tmp_print.AlprVehicleNumber.IndexOf(" ") > -1 ? tmp_print.VehicleNumber : tmp_print.AlprVehicleNumber, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 160;
                e.Graphics.DrawString("Đối tượng:", font1, System.Drawing.Brushes.Black, rect, formatLeft);

                e.Graphics.DrawString(tmp_print.StrCardType, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 190;
                e.Graphics.DrawString("Ngày giờ vào:", font1, System.Drawing.Brushes.Black, rect, formatLeft);

                e.Graphics.DrawString(tmp_print.StrCheckInTime, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 205;
                e.Graphics.DrawString("Ngày giờ ra:", font1, System.Drawing.Brushes.Black, rect, formatLeft);
                e.Graphics.DrawString(tmp_print.StrCheckOutTime, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 220;
                e.Graphics.DrawString("Thời gian lưu bãi:", font1, System.Drawing.Brushes.Black, rect, formatLeft);

                e.Graphics.DrawString(_total, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 250;
                e.Graphics.DrawString("Số tiền thanh toán", font1, System.Drawing.Brushes.Black, rect, formatLeft);
                e.Graphics.DrawString(tmp_print.StrParkingFee, font1, System.Drawing.Brushes.Black, rect, format);
                rect.Y = 265;
                e.Graphics.DrawString("-------------------------------------------------------------", font2, System.Drawing.Brushes.Black, rect, format1);
                rect.Y = 280;
                e.Graphics.DrawString("XIN CHÀO - HẸN GẶP LẠI QUÝ KHÁCH", font2, System.Drawing.Brushes.Black, rect, format1);
                rect.Y = 295;
                e.Graphics.DrawString("-------------------------------------------------------------", font2, System.Drawing.Brushes.Black, rect, format1);
            }
        }
        public void PublishPrintBillV2(ParkingSession data)
        {//Tong tien: 100,000vnd
            if (Section.PrintComActive)
            {
                try
                {
                    if (data == null) return;
                    string _total = string.Empty;
                    try
                    {
                        _total = ToReadableString(DateTime.ParseExact(data.StrCheckOutTime, "dd/MM/yyyy HH:mm:ss", new System.Globalization.CultureInfo("vi-VN")) -
                                                       DateTime.ParseExact(data.StrCheckInTime, "dd/MM/yyyy HH:mm:ss", new System.Globalization.CultureInfo("vi-VN")));
                    }
                    catch
                    {
                        try
                        {
                            _total = ToReadableString(DateTime.ParseExact(data.StrCheckOutTime, "dd-MM-yyyy HH:mm:ss", new System.Globalization.CultureInfo("vi-VN")) -
                                                       DateTime.ParseExact(data.StrCheckInTime, "dd-MM-yyyy HH:mm:ss", new System.Globalization.CultureInfo("vi-VN")));
                        }
                        catch
                        {
                            _total = "Vui lòng định dạng lại ngày giờ máy Server  'dd/MM/yyyy HH:mm:ss' hoặc 'dd-MM-yyyy HH:mm:ss'";
                        }
                    }


                    string _customer = data.StrCardType;

                    bool _IsIni = IniSerialPortPrinter();
                    if (!_IsIni)
                    {
                        System.Windows.MessageBox.Show("Vui lòng xem lại cổng máy in!", "Lỗi");
                        //HandleError(IconEnums.Error, "Vui lòng xem lại cổng máy in!", false, true);
                        return;
                    }

                    com.clsCom _clsCom = new com.clsCom();
                    string[] _Array = new string[14];

                    _Array[0] = _clsCom.CombineString("NGAY IN BILL :", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

                    _Array[1] = _clsCom.CombineString("NHAN VIEN XU LY :", this.User.DisplayName);

                    _Array[2] = _clsCom.CombineString(" ", " ");

                    _Array[3] = _clsCom.CombineString("LOAI PHUONG TIEN :", data.VehicleType.Name);
                    //_Array[5] = _clsCom.CombineString("BIEN SO XE :", data.AlprVehicleNumber);
                    _Array[4] = _clsCom.CombineString("BIEN SO: ", data.AlprVehicleNumber.IndexOf(" ") > -1 ? data.VehicleNumber : data.AlprVehicleNumber);
                    _Array[5] = _clsCom.CombineString("DOI TUONG :", _customer);

                    _Array[6] = _clsCom.CombineString(" ", " ");

                    _Array[7] = _clsCom.CombineString("NGAY GIO VAO :", data.StrCheckInTime);
                    _Array[8] = _clsCom.CombineString("NGAY GIO RA :", data.StrCheckOutTime);
                    _Array[9] = _clsCom.CombineString("THOI GIAN LUU BAI :", _total);

                    _Array[10] = _clsCom.CombineString(" ", " ");
                    _Array[11] = _clsCom.CombineString("SO TIEN THANH TOAN :", string.Format("{0:0,0 VND}", data.ParkingFee));

                    _Array[12] = _clsCom.CombineString("", "");
                    _Array[13] = _clsCom.CombineString("Thank you very much for using our services", "");
                    _clsCom.XinChao = "Sheraton Saigon Hotel & Towers";
                    byte[] _buffers = _clsCom.CommandESC(_Array);
                    CheckOutLaneViewModel._SerialPortPrinter.Write(_buffers, 0, _buffers.Length);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
            }
            else
            {
                tmp_print = data;
                DocPrintToBill();
            }
        }
        public void PublishPrintBill(ParkingSession data)
        {//Tong tien: 100,000vnd

            if (this.Section.PrintComActive)
            {
                try
                {
                    var now = TimeMapInfo.Current.LocalTime;
                    if (data == null) return;
                    string _total = ToReadableString(DateTime.ParseExact(data.StrCheckOutTime, "dd/MM/yyyy HH:mm:ss", new System.Globalization.CultureInfo("vi-VN")) -
                                                       DateTime.ParseExact(data.StrCheckInTime, "dd/MM/yyyy HH:mm:ss", new System.Globalization.CultureInfo("vi-VN")));

                    string _customer = data.StrCardType;

                    bool _IsIni = IniSerialPortPrinter();
                    if (!_IsIni) return;

                    com.clsCom _clsCom = new com.clsCom();
                    _clsCom.UrlLogo = System.Windows.Forms.Application.StartupPath + "\\config\\logovt.ini";

                    string[] _Array = new string[13];

                    _Array[0] = _clsCom.CombineString("NGAY IN BILL :", now.ToString("dd/MM/yyyy HH:mm:ss"));
                    //_Array[0] = _clsCom.CombineString("NGAY IN BILL :", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                    _Array[1] = _clsCom.CombineString("CONG KIEM SOAT :", data.TerminalName);
                    _Array[2] = _clsCom.CombineString("NHAN VIEN XU LY :", this.User.Username);

                    _Array[3] = _clsCom.CombineString(" ", " ");

                    _Array[4] = _clsCom.CombineString("LOAI PHUONG TIEN :", data.VehicleType.Name);
                    //_Array[5] = _clsCom.CombineString("BIEN SO XE :", data.AlprVehicleNumber);
                    _Array[5] = _clsCom.CombineString("BIEN SO: ", data.AlprVehicleNumber.IndexOf("???") > -1 ? data.VehicleNumber : data.AlprVehicleNumber);
                    _Array[6] = _clsCom.CombineString("DOI TUONG :", _customer);

                    _Array[7] = _clsCom.CombineString(" ", " ");

                    _Array[8] = _clsCom.CombineString("NGAY GIO VAO :", data.StrCheckInTime);
                    _Array[9] = _clsCom.CombineString("NGAY GIO RA :", data.StrCheckOutTime);
                    _Array[10] = _clsCom.CombineString("THOI GIAN LUU BAI :", _total);

                    _Array[11] = _clsCom.CombineString(" ", " ");
                    _Array[12] = _clsCom.CombineString("SO TIEN THANH TOAN :", string.Format("{0:0,0 VND}", data.ParkingFee));

                    _clsCom.XinChao = "XIN CHAO - HEN GAP LAI QUY KHACH";
                    byte[] _buffers = _clsCom.CommandESC(_Array);

                    CheckOutLaneViewModel._SerialPortPrinter.Write(_buffers, 0, _buffers.Length);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
            }
            else
            {
                tmp_print = data;
                DocPrintToBill();
            }
        }
        /***Chinh sua 24-07-2016 ***/
        private bool _IsEnabledPrintBill = false;
        public bool IsEnabledPrintBill
        {
            get { return this._IsEnabledPrintBill; }
            set
            {
                this._IsEnabledPrintBill = value;
                this.RaisePropertyChanged(() => this.IsEnabledPrintBill);
            }
        }

        MvxCommand<string> _editEndingCommand;
        public MvxCommand<string> EditEndingCommand
        {
            get
            {
                _editEndingCommand = _editEndingCommand ?? new MvxCommand<string>(value => {
                    //_server.UpdateCheckIn(new CheckIn { CardId = SelectedItem.CardId, VehicleNumber = value }, (result, ex) => {
                    //    SelectedItem.VehicleNumber = value;
                    //});
                    SelectedItem.VehicleNumber = value;
                    _server.UpdateParkingSession(SelectedItem, ex => { });
                });
                return _editEndingCommand;
            }
        }

        public void PublishCloseChildEvent(SectionPosition position)
        {
            if (_messenger.HasSubscriptionsFor<CloseChildMessage>())
            {
                _messenger.Publish(new CloseChildMessage(this, position));
            }
        }

        public void PublishShowCheckingLaneEvent()
        {
            if (_messenger.HasSubscriptionsFor<ShowChildMessage>())
            {
                _messenger.Publish(new ShowChildMessage(this, Section.Id, typeof(BaseLaneViewModel)));
            }
        }

        public void PublishShowExceptionalCheckout(ParkingSession data)
        {
            if (_messenger.HasSubscriptionsFor<ShowChildMessage>())
                _messenger.Publish(new ShowChildMessage(this, Section.Id, typeof(ExceptionalCheckOutViewModel), data));
        }

        public override void Close()
        {
            base.Close();
        }

        public override void Unloaded()
        {
            base.Unloaded();
            //this.Section.StopCardReader(ReadingCompleted, null);
            if (this.Section.ModWinsCards != null)
            {
                CurrentListCardReader.StoptGreenCardReader(this.Section.ModWinsCards, GreenReadingCompleted, null);
            }
            if (this.Section.TcpIpServerCards != null)
            {
                CurrentListCardReader.StoptGreenCardReader(this.Section.TcpIpServerCards, GreenReadingCompleted, null);
            }
            if (this.Section.TcpIpClientCards != null)
            {
                CurrentListCardReader.StoptGreenCardReader(this.Section.TcpIpClientCards, GreenReadingCompleted, null);
            }
            if (this.Section.TcpIpServerCards != null)
            {
                CurrentListCardReader.StoptGreenCardReader(this.Section.TcpIpClientCards, GreenReadingCompleted, null);
            }
            if (this.Section.ScannelCards != null)
            {
                CurrentListCardReader.StoptGreenCardReader(this.Section.ScannelCards, GreenReadingCompleted, null);
            }
            if (this.Section.TcpIpRemodeCards != null)
            {
                CurrentListCardReader.StoptGreenCardReader(this.Section.TcpIpRemodeCards, GreenReadingCompleted, null);
            }
            IMvxMessenger messenger = Mvx.Resolve<IMvxMessenger>();
            messenger.Unsubscribe<KeyPressedMessage>(_keyPressedToken);
            _keyPressedToken = null;
        }

        protected void OnExceptionalCheckOut(ExceptionalCheckOutMessage msg)
        {
            if (ParkingSessions.Contains(msg.CheckedOutItem))
            {
                InvokeOnMainThread(() => {
                    ParkingSessions.Remove(msg.CheckedOutItem);
                    if (ParkingSessions != null && ParkingSessions.Count > 0)
                    {
                        SelectedItem = ParkingSessions.Count > 0 ? ParkingSessions[0] : null;
                        SelectItem(SelectedItem);
                    }
                });
            }
        }

        public void KeyPressed(object sender, System.Windows.Input.KeyEventArgs e)
        {
            OnKeyPressed(new KeyPressedMessage(sender, e));
        }

        protected void OnKeyPressed(KeyPressedMessage msg)
        {
            string output;
            KeyAction action = this.Section.KeyMap.GetAction(msg.KeyEventArgs, out output, typeof(SearchViewModel));

            switch (action)
            {
                case KeyAction.DoSearch:
                    {
                        DoSearch(null);
                        break;
                    }
                case KeyAction.Back:
                    {
                        BackCommand.Execute(null);
                        break;
                    }
                case KeyAction.ExceptionalCheckout:
                    {
                        ExceptionalCheckoutCommand.Execute(SelectedItem);
                        break;
                    }
            }
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
    }

    public static class MyExtention
    {
        public static void Filter(this ObservableCollection<ParkingSession> sender, string pattern)
        {
            var rs = sender.Where(i => i.TerminalName.Contains(pattern));
            //return new ObservableCollection<ParkingSession>(rs);
        }

        public static void Test(this string myiii, int ohyeah)
        {

        }
    }
}