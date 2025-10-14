using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using SP.Parking.Terminal.Core.Models;
using SP.Parking.Terminal.Core.Services;
using Green.Devices.CardReader;
using Green.Devices.Dal;
using Green.Devices.Dal.CardControler;
using Green.Devices.Dal.Siemens;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SP.Parking.Terminal.Core.ViewModels
{
    public class SubLaneConfigurationViewModel : BaseViewModel
    {
        IResourceLocatorService _resourceLocatorService;

        IUserPreferenceService _userPreferenceService;

        ICardReaderService _cardReaderService;

        IBarrierDeviceManager _barrierDeviceManager;

        IServer _server;
        private string _printAddressTitle;
        private string _printCallTile;
        //private string _printLogoPath;
        private bool _printComActive;
        private string _comPrint;
        public string _ledIP;
        private string _comLed;
        private string _comCash;
        private string _comLedB;
        private string _comAlarm;
        string _comIctCashier;
        bool _comIctCashierEnanble;
        private string _alarmWarningKeys;
        private string _alarmSuccessKeys;
        private bool _usageAsTheSameFarCard = false;
        private OptionByLane _ptionByLane;
		public OptionByLane OptionByLane
		{
			get
			{
				return _ptionByLane;
			}
			set
			{
				if (_ptionByLane == value)
					return;
				_ptionByLane = value;
				RaisePropertyChanged(() => OptionByLane);
			}
		}
		
		public bool UsageAsTheSameFarCard
		{
			get
			{
				return _usageAsTheSameFarCard;
			}
			set
			{
				if (_usageAsTheSameFarCard == value)
					return;
				_usageAsTheSameFarCard = value;
				RaisePropertyChanged(() => UsageAsTheSameFarCard);
			}
		}
		
        
        private LedStyle _LedStyle;
        public List<LedStyle> LedStyles
        {
            get { return Enum.GetValues(typeof(LedStyle)).Cast<LedStyle>().ToList<LedStyle>(); }
        }
        private bool _useBarrierIp = false;
        public bool UseBarrierIpController
        {
            get
            {
                return _useBarrierIp;
            }
            set
            {
                if (_useBarrierIp == value)
                    return;
                _useBarrierIp = value;
                RaisePropertyChanged(() => UseBarrierIpController);
            }
        }
        private InternetControl _barrierByInternetControl;
        public InternetControl BarrierByInternetControl
        {
            get
            {
                return _barrierByInternetControl;
            }
            set
            {
                if (_barrierByInternetControl == value)
                    return;
                _barrierByInternetControl = value;
                RaisePropertyChanged(() => BarrierByInternetControl);
            }
        }
        private SiemensControl _barrierBySiemensControl;
        public SiemensControl BarrierBySiemensControl
        {
            get
            {
                return _barrierBySiemensControl;
            }
            set
            {
                if (_barrierBySiemensControl == value)
                    return;
                _barrierBySiemensControl = value;
                RaisePropertyChanged(() => BarrierBySiemensControl);
            }
        }
        private string _barrierIpController;
        public string BarrierIpController
        {
            get
            {
                return _barrierIpController;
            }
            set
            {
                if (_barrierIpController == value)
                    return;
                _barrierIpController = value;
                RaisePropertyChanged(() => BarrierIpController);
            }
        }
        private ushort _barrierPortController;
        public ushort BarrierPortController
        {
            get
            {
                return _barrierPortController;
            }
            set
            {
                if (_barrierPortController == value)
                    return;
                _barrierPortController = value;
                RaisePropertyChanged(() => BarrierPortController);
            }
        }
        private string _barrierDoorsController;
        public string BarrierDoorsController
        {
            get
            {
                return _barrierDoorsController;
            }
            set
            {
                if (_barrierDoorsController == value)
                    return;
                _barrierDoorsController = value;
                RaisePropertyChanged(() => BarrierDoorsController);
            }
        }
        private byte _bBarrierHardButtonCode;
        public byte BarrierHardButtonCode
        {
            get
            {
                return _bBarrierHardButtonCode;
            }
            set
            {
                if (_bBarrierHardButtonCode == value)
                    return;
                _bBarrierHardButtonCode = value;
                RaisePropertyChanged(() => BarrierHardButtonCode);
            }
        }

        private bool _useZKController = false;
        public bool UseZKController
        {
            get
            {
                return _useZKController;
            }
            set
            {
                if (_useZKController == value)
                    return;
                _useZKController = value;
                RaisePropertyChanged(() => UseBarrierIpController);
            }
        }

        private int _timeTick;
        public int TimeTick
        {
            get { return _timeTick; }
            set
            {
                if (_timeTick == value)
                    return;
                _timeTick = value;
                RaisePropertyChanged(() => TimeTick);
            }
        }
        public LedStyle LedStyle
        {
            get { return _LedStyle; }
            set
            {
                if (_LedStyle == value) return;
                _LedStyle = value;
                this.Section.LedOfKind = _LedStyle;
                RaisePropertyChanged(() => LedStyle);
            }
        }
        public string PrintAddressTitle
        {
            get { return _printAddressTitle; }
            set
            {
                if (_printAddressTitle == value) return;
                _printAddressTitle = value;
                this.Section.PrintAddressTitle = _printAddressTitle;
                RaisePropertyChanged(() => PrintAddressTitle);
            }
        }
        public string PrintCallTile
        {
            get { return _printCallTile; }
            set
            {
                if (_printCallTile == value) return;
                _printCallTile = value;
                this.Section.PrintCallTile = _printCallTile;
                RaisePropertyChanged(() => PrintCallTile);
            }
        }
        //public string PrintLogoPath
        //{
        //    get { return _printLogoPath; }
        //    set
        //    {
        //        if (_printLogoPath == value) return;
        //        _printLogoPath = value;
        //        this.Section.PrintLogoPath = _printLogoPath;
        //        RaisePropertyChanged(() => PrintLogoPath);
        //    }
        //}
        public bool PrintComActive
        {
            get { return _printComActive; }
            set
            {
                if (_printComActive == value) return;
                _printComActive = value;
                this.Section.PrintComActive = _printComActive;
                RaisePropertyChanged(() => PrintComActive);
            }
        }
        public string ComAlarm
        {
            get { return _comAlarm; }
            set
            {
                if (_comAlarm == value) return;
                _comAlarm = value;
                this.Section.ComAlarm = _comAlarm;
                RaisePropertyChanged(() => ComAlarm);
            }
        }
        public string ComIctCashier
        {
            get { return _comIctCashier; }
            set
            {
                if (_comIctCashier == value) return;
                _comIctCashier = value;
                this.Section.ComIctCashier = _comIctCashier;
                RaisePropertyChanged(() => ComIctCashier);
            }
        }
        public bool ComIctCashierEnanble
        {
            get { return _comIctCashierEnanble; }
            set
            {
                if (_comIctCashierEnanble == value) return;
                _comIctCashierEnanble = value;
                this.Section.ComIctCashierEnanble = _comIctCashierEnanble;
                RaisePropertyChanged(() => ComIctCashierEnanble);
            }
        }
        public string AlarmWarningKeys
        {
            get { return _alarmWarningKeys; }
            set
            {
                if (_alarmWarningKeys == value) return;
                _alarmWarningKeys = value;
                this.Section.AlarmWarningKeys = _alarmWarningKeys;
                RaisePropertyChanged(() => AlarmWarningKeys);
            }
        }
        public string AlarmSuccessKeys
        {
            get { return _alarmSuccessKeys; }
            set
            {
                if (_alarmSuccessKeys == value) return;
                _alarmSuccessKeys = value;
                this.Section.AlarmSuccessKeys = _alarmSuccessKeys;
                RaisePropertyChanged(() => AlarmSuccessKeys);
            }
        }
        public string ComPrint
        {
            get { return _comPrint; }
            set
            {
                if (_comPrint == value) return;
                _comPrint = value;
                this.Section.ComPrint = _comPrint;
                RaisePropertyChanged(() => ComPrint);
            }
        }
        public string LedIP
        {
            get { return _ledIP; }
            set
            {
                if (_ledIP == value) return;
                _ledIP = value;
                this.Section.LedIP = _ledIP;
                RaisePropertyChanged(() => LedIP);
            }
        }
        public string ComLed
        {
            get { return _comLed; }
            set
            {
                if (_comLed == value) return;
                _comLed = value;
                this.Section.ComLed = _comLed;
                RaisePropertyChanged(() => ComLed);
            }
        }
        public string ComCash
        {
            get { return _comCash; }
            set
            {
                if (_comCash == value) return;
                _comCash = value;
                this.Section.ComCash = _comCash;
                RaisePropertyChanged(() => ComCash);
            }
        }
        public string ComLedB
        {
            get { return _comLedB; }
            set
            {
                if (_comLedB == value) return;
                _comLedB = value;
                this.Section.ComLedB = _comLedB;
                RaisePropertyChanged(() => ComLedB);
            }
        }
        CustomCombo _door;
        CustomCombo _reader;
        public CustomCombo Door
        {
            get { return _door; }
            set
            {
                if (_door == value) return;
                _door = value;
                if (value != null && value.Value != "-1")
                    this.Section.Door = value.Value;
                else
                    this.Section.Door = null;
                RaisePropertyChanged(() => Door);
            }
        }
        public CustomCombo Reader
        {
            get { return _reader; }
            set
            {
                if (_reader == value) return;
                _reader = value;
                if (value != null && value.Value != "-1")
                    this.Section.Reader = value.Value;
                else
                    this.Section.Reader = null;
                RaisePropertyChanged(() => Reader);
            }
        }
        //private const string DEFAULT_BARRIER_PORT = "B1";
        private const string REMOTE_CARD_READER = "Remote Card Reader";

        #region Properties
        int _selectedZoom;
        public int SelectedZoom
        {
            get { return _selectedZoom; }
            set
            {
                if (_selectedZoom == value) return;
                _selectedZoom = value;
                RaisePropertyChanged(() => SelectedZoom);
            }
        }


        bool _isDetecting;
        public bool IsDetecting
        {
            get { return _isDetecting; }
            set
            {
                _isDetecting = value;
                RaisePropertyChanged(() => IsDetecting);
            }
        }
        ObservableCollection<CustomCombo> _zoomlist;
        public ObservableCollection<CustomCombo> ZoomList
        {
            get { return _zoomlist; }
            set
            {
                if (_zoomlist == value) return;
                _zoomlist = value;
                RaisePropertyChanged(() => ZoomList);
            }
        }
        ObservableCollection<CardReaderWrapper> _allCardReaders;
        public ObservableCollection<CardReaderWrapper> AllCardReaders
        {
            get { return _allCardReaders; }
            set
            {
                if (_allCardReaders == value) return;
                _allCardReaders = value;
                RaisePropertyChanged(() => AllCardReaders);
            }
        }

        ObservableCollection<CardReaderWrapper> _availableCardReaders;
        public ObservableCollection<CardReaderWrapper> AvailableCardReaders
        {
            get { return _availableCardReaders; }
            set
            {
                if (_availableCardReaders == value) return;
                _availableCardReaders = value;
                RaisePropertyChanged(() => AvailableCardReaders);
            }
        }

        ObservableCollection<CardReaderWrapper> _cardReaders;
        public ObservableCollection<CardReaderWrapper> CardReaders
        {
            get { return _cardReaders; }
            set
            {
                if (_cardReaders == value) return;
                _cardReaders = value;
                RaisePropertyChanged(() => CardReaders);
            }
        }

        List<Video_Device> _webcams;
        public List<Video_Device> Webcams
        {
            get { return _webcams; }
            set
            {
                if (_webcams == value) return;
                _webcams = value;
                RaisePropertyChanged(() => Webcams);
            }
        }

        private string _laneId;
        public string LaneId
        {
            get { return _laneId; }
            set
            {
                if (_laneId == value) return;
                _laneId = value;
            }
        }

        private Section _section;
        public new Section Section
        {
            get { return _section; }
            set
            {
                if (_section == value) return;
                _section = value;
                LaneId = _section.Id.ToString();
                SetupView();
            }
        }

        private LaneDirection _direction;
        public LaneDirection Direction
        {
            get { return _direction; }
            set
            {
                if (_direction == value) return;
                _direction = value;
            }
        }

        string _laneName;
        public string LaneName
        {
            get { return _laneName; }
            set
            {
                if (_laneName == value) return;

                _laneName = value;
                this.Section.LaneName = _laneName;
                RaisePropertyChanged(() => LaneName);
            }
        }

        string _cardReaderIP;
        public string CardReaderIP
        {
            get { return _cardReaderIP; }
            set
            {
                if (_cardReaderIP == value) return;
                _cardReaderIP = value;
                RaisePropertyChanged(() => CardReaderIP);
            }
        }

        string _frontInCamera;
        public string FrontInCamera
        {
            get { return _frontInCamera; }
            set
            {
                if (_frontInCamera == value)
                    return;

                _frontInCamera = value;
                LoadZoomList();
                RaisePropertyChanged(() => FrontInCamera);
            }
        }
        ZoomFactor _frontInZoom;
        public ZoomFactor FrontInZoom
        {
            get { return _frontInZoom; }
            set
            {
                if (_frontInZoom == value)
                    return;

                _frontInZoom = value;
                RaisePropertyChanged(() => FrontInZoom);
            }
        }
        private VehicleTypeEnum _selectedVehicleType;
        public VehicleTypeEnum SelectedVehicleType
        {
            get { return _selectedVehicleType; }
            set
            {
                _selectedVehicleType = value;
                RaisePropertyChanged(() => SelectedVehicleType);
            }
        }

        IEnumerable<VehicleTypeEnum> _vehicleTypes;
        public IEnumerable<VehicleTypeEnum> VehicleTypes
        {
            get { return _vehicleTypes; }
            set
            {
                _vehicleTypes = value;
                RaisePropertyChanged(() => VehicleTypes);
            }
        }

        private IEnumerable<string> _barriers;
        public IEnumerable<string> Barriers
        {
            get
            {
                if (_barriers != null)
                    return _barriers;

                List<string> barrierNames = new List<string>();
                barrierNames.Add("");
                barrierNames.AddRange(_barrierDeviceManager.GetAllDeviceNames());
                _barriers = barrierNames;
                return _barriers;
            }
        }

        private string _selectedBarrier;
        public string SelectedBarrier
        {
            get { return _selectedBarrier; }
            set
            {
                _selectedBarrier = value;
                RaisePropertyChanged(() => SelectedBarrier);
            }
        }

        private string _barrierPort;
        public string BarrierPort
        {
            get { return _barrierPort; }
            set
            {
                _barrierPort = value;
                RaisePropertyChanged(() => BarrierPort);
            }
        }

        string _backInCamera;
        public string BackInCamera
        {
            get { return _backInCamera; }
            set
            {
                if (_backInCamera == value)
                    return;

                _backInCamera = value;
                LoadZoomList();
                RaisePropertyChanged(() => BackInCamera);
            }
        }
        ZoomFactor _backInZoom;
        public ZoomFactor BackInZoom
        {
            get { return _backInZoom; }
            set
            {
                if (_backInZoom == value)
                    return;
                _backInZoom = value;
                RaisePropertyChanged(() => BackInZoom);
            }
        }
        string _frontOutCamera;
        public string FrontOutCamera
        {
            get { return _frontOutCamera; }
            set
            {
                if (_frontOutCamera == value)
                    return;

                _frontOutCamera = value;
                LoadZoomList();
                RaisePropertyChanged(() => FrontOutCamera);
            }
        }
        ZoomFactor _frontOutZoom;
        public ZoomFactor FrontOutZoom
        {
            get { return _frontOutZoom; }
            set
            {
                if (_frontOutZoom == value)
                    return;
                _frontOutZoom = value;
                RaisePropertyChanged(() => FrontOutZoom);
            }
        }
        string _backOutCamera;
        public string BackOutCamera
        {
            get { return _backOutCamera; }
            set
            {
                if (_backOutCamera == value)
                    return;

                _backOutCamera = value;
                LoadZoomList();
                RaisePropertyChanged(() => BackOutCamera);
            }
        }
        ZoomFactor _backOutZoom;
        public ZoomFactor BackOutZoom
        {
            get { return _backOutZoom; }
            set
            {
                if (_backOutZoom == value)
                    return;
                _backOutZoom = value;
                RaisePropertyChanged(() => BackOutZoom);
            }
        }
        string _currentCardReader;
        public string CurrentCardReader
        {
            get { return _currentCardReader; }
            set
            {
                if (_currentCardReader == value)
                    return;

                _currentCardReader = value;

                if (AvailableCardReaders != null && value != null)
                {
                    var cardReader = AvailableCardReaders.Where(c => c.SerialNumber.Equals(value));
                    var info = cardReader.First().CardReaderInfo as ProlificCardReaderInfo;
                    CardReaderIP = info != null ? info.IP : string.Empty;
                }
                RaisePropertyChanged(() => CurrentCardReader);
            }
        }
        /// <summary>
        /// Mở rộng camera và đầu đọc tầm xa
        /// </summary>

        ObservableCollection<IGreenCardReaderInfo> _modwinscards;
        public ObservableCollection<IGreenCardReaderInfo> ModWinsCards
        {
            get { return _modwinscards; }
            set
            {
                //if (_modwinscards == value) return;
                _modwinscards = value;
                RaisePropertyChanged(() => ModWinsCards);
            }
        }
        ObservableCollection<IGreenCardReaderInfo> _avilablemodwinscards;
        public ObservableCollection<IGreenCardReaderInfo> AvilableModWinsCards
        {
            get { return _avilablemodwinscards; }
            set
            {
                //if (_avilablemodwinscards == value) return;
                _avilablemodwinscards = value;
                RaisePropertyChanged(() => AvilableModWinsCards);
                if (_avilablemodwinscards != null && _avilablemodwinscards.Count > 0)
                    CurrentCardSelected = _avilablemodwinscards[0];
            }
        }
        ObservableCollection<IGreenCardReaderInfo> _availableNFCCards;
        public ObservableCollection<IGreenCardReaderInfo> AvailableNFCCards
        {
            get { return _availableNFCCards; }
            set
            {
                _availableNFCCards = value;
                RaisePropertyChanged(() => AvailableNFCCards);
            }
        }

        ObservableCollection<IGreenCardReaderInfo> _availableProxiesCards;
        public ObservableCollection<IGreenCardReaderInfo> AvailableProxiesCards
        {
            get { return _availableProxiesCards; }
            set
            {
                _availableProxiesCards = value;
                RaisePropertyChanged(() => AvailableProxiesCards);
            }
        }

        ObservableCollection<IGreenCardReaderInfo> _availableZKFarCards;
        public ObservableCollection<IGreenCardReaderInfo> AvailableZKFarCards
        {
            get { return _availableZKFarCards; }
            set
            {
                _availableZKFarCards = value;
                RaisePropertyChanged(() => AvailableZKFarCards);
            }
        }

        IGreenCardReaderInfo _currentcardselected;
        public IGreenCardReaderInfo CurrentCardSelected
        {
            get
            {
                return _currentcardselected;
            }
            set
            {
                if (_currentcardselected == value) return;
                _currentcardselected = value;
                RaisePropertyChanged(() => CurrentCardSelected);
            }
        }
        ObservableCollection<IGreenCardReaderInfo> _tcpipservercards;
        public ObservableCollection<IGreenCardReaderInfo> TcpIpServerCards
        {
            get { return _tcpipservercards; }
            set
            {
                //if (_tcpipservercards == value) return;
                _tcpipservercards = value;
                RaisePropertyChanged(() => TcpIpServerCards);
            }
        }
        ObservableCollection<IGreenCardReaderInfo> _tcpipclientcards;
        public ObservableCollection<IGreenCardReaderInfo> TcpIpClientCards
        {
            get { return _tcpipclientcards; }
            set
            {
                //if (_tcpipclientcards == value) return;
                _tcpipclientcards = value;
                RaisePropertyChanged(() => TcpIpClientCards);
            }
        }
        ObservableCollection<IGreenCardReaderInfo> _tcpipremodecards;
        public ObservableCollection<IGreenCardReaderInfo> TcpIpRemoderCards
        {
            get { return _tcpipremodecards; }
            set
            {
                //if (_tcpipservercards == value) return;
                _tcpipremodecards = value;
                RaisePropertyChanged(() => TcpIpRemoderCards);
            }
        }
        ObservableCollection<IGreenCardReaderInfo> _scannelcards;
        public ObservableCollection<IGreenCardReaderInfo> ScannelCards
        {
            get { return _scannelcards; }
            set
            {
                //if (_tcpipclientcards == value) return;
                _scannelcards = value;
                RaisePropertyChanged(() => ScannelCards);
            }
        }
        ObservableCollection<IGreenCardReaderInfo> _tcpipcontrollercards;
        public ObservableCollection<IGreenCardReaderInfo> TcpIpControllerCards
        {
            get { return _tcpipcontrollercards; }
            set
            {
                //if (_tcpipservercards == value) return;
                _tcpipcontrollercards = value;
                RaisePropertyChanged(() => TcpIpControllerCards);
            }
        }
        ObservableCollection<IGreenCardReaderInfo> _nfcCards;
        public ObservableCollection<IGreenCardReaderInfo> NFCCards
        {
            get { return _nfcCards; }
            set
            {
                //if (_tcpipservercards == value) return;
                _nfcCards = value;
                RaisePropertyChanged(() => NFCCards);
            }
        }

        ObservableCollection<IGreenCardReaderInfo> _proxiesCards;
        public ObservableCollection<IGreenCardReaderInfo> ProxiesCards
        {
            get { return _proxiesCards; }
            set
            {
                //if (_tcpipservercards == value) return;
                _proxiesCards = value;
                RaisePropertyChanged(() => ProxiesCards);
            }
        }

        ObservableCollection<IGreenCardReaderInfo> _zkFarCards;
        public ObservableCollection<IGreenCardReaderInfo> ZKFarCards
        {
            get { return _zkFarCards; }
            set
            {
                //if (_tcpipservercards == value) return;
                _zkFarCards = value;
                RaisePropertyChanged(() => ZKFarCards);
            }
        }

        private ObservableCollection<string> _cardtypes;
        private ObservableCollection<CustomCombo> _doortypes;
        public ObservableCollection<CustomCombo> DoorTypes
        {
            get { return _doortypes; }
        }
        private ObservableCollection<CustomCombo> _readertypes;
        public ObservableCollection<CustomCombo> ReaderTypes
        {
            get { return _readertypes; }
        }
        public ObservableCollection<string> CardTypes { get { return _cardtypes; } }
        public ObservableCollection<string> AntennaTypes { get { return _antennatypes; } }
        private ObservableCollection<string> _antennatypes;
        public string CurrentIp { get; set; }
        public ushort CurrentPort { get; set; }
        public string CurrentCardType
        {
            get => currentCardType; set
            {
                currentCardType = value;
                LoadCardData(currentCardType);
            }
        }

        public string CurrentAntennaType { get; set; }
        bool _isInExtra;
        public bool IsInExtra
        {
            get { return _isInExtra; }
            set
            {
                if (_isInExtra == value)
                    return;
                _isInExtra = value;
                RaisePropertyChanged(() => IsInExtra);
            }
        }
        bool _isOutExtra;
        public bool IsOutExtra
        {
            get { return _isOutExtra; }
            set
            {
                if (_isOutExtra == value)
                    return;
                _isOutExtra = value;
                RaisePropertyChanged(() => IsOutExtra);
            }
        }
        string _extraIn1Camera;
        public string ExtraIn1Camera
        {
            get { return _extraIn1Camera; }
            set
            {
                if (_extraIn1Camera == value)
                    return;

                _extraIn1Camera = value;
                LoadZoomList();
                RaisePropertyChanged(() => ExtraIn1Camera);
            }
        }
        ZoomFactor _extraIn1Zoom;
        public ZoomFactor ExtraIn1Zoom
        {
            get { return _extraIn1Zoom; }
            set
            {
                if (_extraIn1Zoom == value)
                    return;
                _extraIn1Zoom = value;
                RaisePropertyChanged(() => ExtraIn1Zoom);
            }
        }
        ZoomFactor _extraIn2Zoom;
        public ZoomFactor ExtraIn2Zoom
        {
            get { return _extraIn2Zoom; }
            set
            {
                if (_extraIn2Zoom == value)
                    return;
                _extraIn2Zoom = value;
                RaisePropertyChanged(() => ExtraIn2Zoom);
            }
        }
        ZoomFactor _extraOut1Zoom;
        public ZoomFactor ExtraOut1Zoom
        {
            get { return _extraOut1Zoom; }
            set
            {
                if (_extraOut1Zoom == value)
                    return;
                _extraOut1Zoom = value;
                RaisePropertyChanged(() => ExtraOut1Zoom);
            }
        }
        ZoomFactor _extraOut2Zoom;
        public ZoomFactor ExtraOut2Zoom
        {
            get { return _extraOut2Zoom; }
            set
            {
                if (_extraOut2Zoom == value)
                    return;
                _extraOut2Zoom = value;
                RaisePropertyChanged(() => ExtraOut2Zoom);
            }
        }
        string _extraIn2Camera;
        public string ExtraIn2Camera
        {
            get { return _extraIn2Camera; }
            set
            {
                if (_extraIn2Camera == value)
                    return;

                _extraIn2Camera = value;
                LoadZoomList();
                RaisePropertyChanged(() => ExtraIn2Camera);
            }
        }
        string _extraOut1Camera;
        public string ExtraOut1Camera
        {
            get { return _extraOut1Camera; }
            set
            {
                if (_extraOut1Camera == value)
                    return;

                _extraOut1Camera = value;
                LoadZoomList();
                RaisePropertyChanged(() => ExtraOut1Camera);
            }
        }
        string _extraOut2Camera;
        public string ExtraOut2Camera
        {
            get { return _extraOut2Camera; }
            set
            {
                if (_extraOut2Camera == value)
                    return;

                _extraOut2Camera = value;
                LoadZoomList();
                RaisePropertyChanged(() => ExtraOut2Camera);
            }
        }

        public IEnumerable<CameraType> CameraTypes
        {
            get { return Enum.GetValues(typeof(CameraType)).Cast<CameraType>(); }
        }
        CameraType _frontInType = CameraType.Vivotek;
        public CameraType FrontInType
        {
            get { return _frontInType; }
            set
            {
                if (_frontInType == value) return;
                _frontInType = value;
                RaisePropertyChanged(() => FrontInType);
            }
        }
        CameraType _frontOutType = CameraType.Vivotek;
        public CameraType FrontOutType
        {
            get { return _frontOutType; }
            set
            {
                if (_frontOutType == value) return;
                _frontOutType = value;
                RaisePropertyChanged(() => FrontOutType);
            }
        }
        CameraType _backInType = CameraType.Vivotek;
        public CameraType BackInType
        {
            get { return _backInType; }
            set
            {
                if (_backInType == value) return;
                _backInType = value;
                RaisePropertyChanged(() => BackInType);
            }
        }
        CameraType _backOutType = CameraType.Vivotek;
        public CameraType BackOutType
        {
            get { return _backOutType; }
            set
            {
                if (_backOutType == value) return;
                _backOutType = value;
                RaisePropertyChanged(() => BackOutType);
            }
        }
        CameraType _extraIn1Type = CameraType.Vivotek;
        public CameraType ExtraIn1Type
        {
            get { return _extraIn1Type; }
            set
            {
                if (_extraIn1Type == value) return;
                _extraIn1Type = value;
                RaisePropertyChanged(() => ExtraIn1Type);
            }
        }
        CameraType _extraIn2Type = CameraType.Vivotek;
        public CameraType ExtraIn2Type
        {
            get { return _extraIn2Type; }
            set
            {
                if (_extraIn2Type == value) return;
                _extraIn2Type = value;
                RaisePropertyChanged(() => ExtraIn2Type);
            }
        }
        CameraType _extraOut1Type = CameraType.Vivotek;
        public CameraType ExtraOut1Type
        {
            get { return _extraOut1Type; }
            set
            {
                if (_extraOut1Type == value) return;
                _extraOut1Type = value;
                RaisePropertyChanged(() => ExtraOut1Type);
            }
        }
        CameraType _extraOut2Type = CameraType.Vivotek;
        public CameraType ExtraOut2Type
        {
            get { return _extraOut2Type; }
            set
            {
                if (_extraOut2Type == value) return;
                _extraOut2Type = value;
                RaisePropertyChanged(() => ExtraOut2Type);
            }
        }
        string _frontInPort;
        public string FrontInPort
        {
            get { return _frontInPort; }
            set
            {
                if (_frontInPort == value)
                    return;
                _frontInPort = value;
                RaisePropertyChanged(() => FrontInPort);
            }
        }
        string _frontInUserName;
        public string FrontInUserName
        {
            get { return _frontInUserName; }
            set
            {
                if (_frontInUserName == value)
                    return;
                _frontInUserName = value;
                RaisePropertyChanged(() => FrontInUserName);
            }
        }
        string _frontInPassword;
        public string FrontInPassword
        {
            get { return _frontInPassword; }
            set
            {
                if (_frontInPassword == value)
                    return;
                _frontInPassword = value;
                RaisePropertyChanged(() => FrontInPassword);
            }
        }
        string _frontInWayType;
        public string FrontInWayType
        {
            get { return _frontInWayType; }
            set
            {
                if (_frontInWayType == value)
                    return;
                _frontInWayType = value;
                RaisePropertyChanged(() => _frontInWayType);
            }
        }
        string _frontOutWayType;
        public string FrontOutWayType
        {
            get { return _frontOutWayType; }
            set
            {
                if (_frontOutWayType == value)
                    return;
                _frontOutWayType = value;
                RaisePropertyChanged(() => _frontOutWayType);
            }
        }
        string _backInWayType;
        public string BackInWayType
        {
            get { return _backInWayType; }
            set
            {
                if (_backInWayType == value)
                    return;
                _backInWayType = value;
                RaisePropertyChanged(() => _backInWayType);
            }
        }
        string _backOutWayType;
        public string BackOutWayType
        {
            get { return _backOutWayType; }
            set
            {
                if (_backOutWayType == value)
                    return;
                _backOutWayType = value;
                RaisePropertyChanged(() => _backOutWayType);
            }
        }
        string _backInPort;
        public string BackInPort
        {
            get { return _backInPort; }
            set
            {
                if (_backInPort == value)
                    return;
                _backInPort = value;
                RaisePropertyChanged(() => BackInPort);
            }
        }
        string _backInUserName;
        public string BackInUserName
        {
            get { return _backInUserName; }
            set
            {
                if (_backInUserName == value)
                    return;
                _backInUserName = value;
                RaisePropertyChanged(() => BackInUserName);
            }
        }
        string _backInPassword;
        public string BackInPassword
        {
            get { return _backInPassword; }
            set
            {
                if (_backInPassword == value)
                    return;
                _backInPassword = value;
                RaisePropertyChanged(() => BackInPassword);
            }
        }
        string _frontOutPort;
        public string FrontOutPort
        {
            get { return _frontOutPort; }
            set
            {
                if (_frontOutPort == value)
                    return;
                _frontOutPort = value;
                RaisePropertyChanged(() => FrontOutPort);
            }
        }
        string _frontOutUserName;
        public string FrontOutUserName
        {
            get { return _frontOutUserName; }
            set
            {
                if (_frontOutUserName == value)
                    return;
                _frontOutUserName = value;
                RaisePropertyChanged(() => FrontOutUserName);
            }
        }
        string _frontOutPassword;
        public string FrontOutPassword
        {
            get { return _frontOutPassword; }
            set
            {
                if (_frontOutPassword == value)
                    return;
                _frontOutPassword = value;
                RaisePropertyChanged(() => FrontOutPassword);
            }
        }
        string _backOutPort;
        public string BackOutPort
        {
            get { return _backOutPort; }
            set
            {
                if (_backOutPort == value)
                    return;
                _backOutPort = value;
                RaisePropertyChanged(() => BackOutPort);
            }
        }
        string _backOutUserName;
        public string BackOutUserName
        {
            get { return _backOutUserName; }
            set
            {
                if (_backOutUserName == value)
                    return;
                _backOutUserName = value;
                RaisePropertyChanged(() => BackOutUserName);
            }
        }
        string _backOutPassword;
        public string BackOutPassword
        {
            get { return _backOutPassword; }
            set
            {
                if (_backOutPassword == value)
                    return;
                _backOutPassword = value;
                RaisePropertyChanged(() => BackOutPassword);
            }
        }
        string _extraIn1Port;
        public string ExtraIn1Port
        {
            get { return _extraIn1Port; }
            set
            {
                if (_extraIn1Port == value)
                    return;
                _extraIn1Port = value;
                RaisePropertyChanged(() => ExtraIn1Port);
            }
        }
        string _extraIn1UserName;
        public string ExtraIn1UserName
        {
            get { return _extraIn1UserName; }
            set
            {
                if (_extraIn1UserName == value)
                    return;
                _extraIn1UserName = value;
                RaisePropertyChanged(() => ExtraIn1UserName);
            }
        }
        string _extraIn1Password;
        public string ExtraIn1Password
        {
            get { return _extraIn1Password; }
            set
            {
                if (_extraIn1Password == value)
                    return;
                _extraIn1Password = value;
                RaisePropertyChanged(() => ExtraIn1Password);
            }
        }
        string _extraIn2Port;
        public string ExtraIn2Port
        {
            get { return _extraIn2Port; }
            set
            {
                if (_extraIn2Port == value)
                    return;
                _extraIn2Port = value;
                RaisePropertyChanged(() => ExtraIn2Port);
            }
        }
        string _extraIn2UserName;
        public string ExtraIn2UserName
        {
            get { return _extraIn2UserName; }
            set
            {
                if (_extraIn2UserName == value)
                    return;
                _extraIn2UserName = value;
                RaisePropertyChanged(() => ExtraIn2UserName);
            }
        }
        string _extraIn2Password;
        public string ExtraIn2Password
        {
            get { return _extraIn2Password; }
            set
            {
                if (_extraIn2Password == value)
                    return;
                _extraIn2Password = value;
                RaisePropertyChanged(() => ExtraIn2Password);
            }
        }
        string _extraOut1Port;
        public string ExtraOut1Port
        {
            get { return _extraOut1Port; }
            set
            {
                if (_extraOut1Port == value)
                    return;
                _extraOut1Port = value;
                RaisePropertyChanged(() => ExtraOut1Port);
            }
        }
        string _extraOut1UserName;
        public string ExtraOut1UserName
        {
            get { return _extraOut1UserName; }
            set
            {
                if (_extraOut1UserName == value)
                    return;
                _extraOut1UserName = value;
                RaisePropertyChanged(() => ExtraOut1UserName);
            }
        }
        string _extraOut1Password;
        public string ExtraOut1Password
        {
            get { return _extraOut1Password; }
            set
            {
                if (_extraOut1Password == value)
                    return;
                _extraOut1Password = value;
                RaisePropertyChanged(() => ExtraOut1Password);
            }
        }
        string _extraOut2Port;
        public string ExtraOut2Port
        {
            get { return _extraOut2Port; }
            set
            {
                if (_extraOut2Port == value)
                    return;
                _extraOut2Port = value;
                RaisePropertyChanged(() => ExtraOut2Port);
            }
        }
        string _extraOut2UserName;
        public string ExtraOut2UserName
        {
            get { return _extraOut2UserName; }
            set
            {
                if (_extraOut2UserName == value)
                    return;
                _extraOut2UserName = value;
                RaisePropertyChanged(() => ExtraOut2UserName);
            }
        }
        string _extrra2OutPassword;
        public string ExtraOut2Password
        {
            get { return _extrra2OutPassword; }
            set
            {
                if (_extrra2OutPassword == value)
                    return;
                _extrra2OutPassword = value;
                RaisePropertyChanged(() => ExtraOut2Password);
            }
        }
        int _frontInChanel;
        public int FrontInChannel
        {
            get { return _frontInChanel; }
            set
            {
                if (_frontInChanel == value)
                    return;
                _frontInChanel = value;
                RaisePropertyChanged(() => FrontInChannel);
            }
        }
        int _backInChanel;
        public int BackInChannel
        {
            get { return _backInChanel; }
            set
            {
                if (_backInChanel == value)
                    return;
                _backInChanel = value;
                RaisePropertyChanged(() => BackInChannel);
            }
        }
        int _frontOutChanel;
        public int FrontOutChannel
        {
            get { return _frontOutChanel; }
            set
            {
                if (_frontOutChanel == value)
                    return;
                _frontOutChanel = value;
                RaisePropertyChanged(() => FrontOutChannel);
            }
        }
        int _backOutChanel;
        public int BackOutChannel
        {
            get { return _backOutChanel; }
            set
            {
                if (_backOutChanel == value)
                    return;
                _backOutChanel = value;
                RaisePropertyChanged(() => BackOutChannel);
            }
        }
        int _extraIn1Channel;
        public int ExtraIn1Channel
        {
            get { return _extraIn1Channel; }
            set
            {
                if (_extraIn1Channel == value)
                    return;
                _extraIn1Channel = value;
                RaisePropertyChanged(() => ExtraIn1Channel);
            }
        }
        int _extraIn2Channel;
        public int ExtraIn2Channel
        {
            get { return _extraIn2Channel; }
            set
            {
                if (_extraIn2Channel == value)
                    return;
                _extraIn2Channel = value;
                RaisePropertyChanged(() => ExtraIn2Channel);
            }
        }
        int _extraOut1Channel;
        public int ExtraOut1Channel
        {
            get { return _extraOut1Channel; }
            set
            {
                if (_extraOut1Channel == value)
                    return;
                _extraOut1Channel = value;
                RaisePropertyChanged(() => ExtraOut1Channel);
            }
        }
        int _extraOut2Channel;
        public int ExtraOut2Channel
        {
            get { return _extraOut2Channel; }
            set
            {
                if (_extraOut2Channel == value)
                    return;
                _extraOut2Channel = value;
                RaisePropertyChanged(() => ExtraOut2Channel);
            }
        }
        #endregion

        public SubLaneConfigurationViewModel(IViewModelServiceLocator service
            , IResourceLocatorService resourceLocatorService
            , ICardReaderService cardReaderService
            , IUserPreferenceService userPreferenceService
            , IServer server
            , IBarrierDeviceManager barrierDeviceManager)
            : base(service)
        {
            _resourceLocatorService = resourceLocatorService;
            _cardReaderService = cardReaderService;
            _userPreferenceService = userPreferenceService;
            _server = server;
            _barrierDeviceManager = barrierDeviceManager;
        }

        private void LoadZoomList()
        {
            if (_zoomlist == null)
            {
                ZoomList = new ObservableCollection<CustomCombo>();
                ZoomList.Add(new CustomCombo() { Name = string.Format("Front_In: {0}", FrontInCamera), Value = "frontin" });
                SelectedZoom = 0;
            }
            var cc = ZoomList.FirstOrDefault(c => c.Value == "frontin");
            if (cc != null)
            {
                cc.Name = string.Format("Front_In: {0}", FrontInCamera);
            }
            else
            {
                ZoomList.Add(new CustomCombo() { Name = string.Format("Front_In: {0}", FrontInCamera), Value = "frontin" });
            }
            cc = ZoomList.FirstOrDefault(c => c.Value == "frontout");
            if (cc != null)
            {
                cc.Name = string.Format("Front_Out: {0}", FrontOutCamera);
            }
            else
            {
                ZoomList.Add(new CustomCombo() { Name = string.Format("Front_Out: {0}", FrontOutCamera), Value = "frontout" });
            }
            cc = ZoomList.FirstOrDefault(c => c.Value == "backin");
            if (cc != null)
            {
                cc.Name = string.Format("Back_In: {0}", BackInCamera);
            }
            else
            {
                ZoomList.Add(new CustomCombo() { Name = string.Format("Back_In: {0}", BackInCamera), Value = "backin" });
            }
            cc = ZoomList.FirstOrDefault(c => c.Value == "backout");
            if (cc != null)
            {
                cc.Name = string.Format("Back_Out: {0}", BackOutCamera);
            }
            else
            {
                ZoomList.Add(new CustomCombo() { Name = string.Format("Back_Out: {0}", BackOutCamera), Value = "backout" });
            }
            cc = ZoomList.FirstOrDefault(c => c.Value == "extra1in");
            if (cc != null)
            {
                if (IsInExtra)
                    cc.Name = string.Format("Extra1_In: {0}", ExtraIn1Camera);
                else
                    ZoomList.Remove(cc);
            }
            else
            {
                if (IsInExtra)
                    ZoomList.Add(new CustomCombo() { Name = string.Format("Extra1_In: {0}", ExtraIn1Camera), Value = "extra1in" });
            }
            cc = ZoomList.FirstOrDefault(c => c.Value == "extra1out");
            if (cc != null)
            {
                if (IsOutExtra)
                    cc.Name = string.Format("Extra1_Out: {0}", ExtraOut1Camera);
                else
                    ZoomList.Remove(cc);
            }
            else
            {
                if (IsOutExtra)
                    ZoomList.Add(new CustomCombo() { Name = string.Format("Extra1_Out: {0}", ExtraOut1Camera), Value = "extra1out" });
            }
            cc = ZoomList.FirstOrDefault(c => c.Value == "extra2in");
            if (cc != null)
            {
                if (IsInExtra)
                    cc.Name = string.Format("Extra2_In: {0}", ExtraIn2Camera);
                else
                    ZoomList.Remove(cc);
            }
            else
            {
                if (IsInExtra)
                    ZoomList.Add(new CustomCombo() { Name = string.Format("Extra2_In: {0}", ExtraIn2Camera), Value = "extra2in" });
            }
            cc = ZoomList.FirstOrDefault(c => c.Value == "extra2out");
            if (cc != null)
            {
                if (IsOutExtra)
                    cc.Name = string.Format("Extra2_Out: {0}", ExtraOut2Camera);
                else
                    ZoomList.Remove(cc);
            }
            else
            {
                if (IsOutExtra)
                    ZoomList.Add(new CustomCombo() { Name = string.Format("Extra2_Out: {0}", ExtraOut2Camera), Value = "extra2out" });
            }
        }
        private void SetupView()
        {
            this.LaneName = this.Section.LaneName;
            VehicleTypes = Enum.GetValues(typeof(VehicleTypeEnum)).Cast<VehicleTypeEnum>().Where(e => e != VehicleTypeEnum.All);

            TypeHelper.GetVehicleType(Section.VehicleTypeId, result =>
            {
                if (result != null)
                    SelectedVehicleType = (VehicleTypeEnum)result.Id;
            });
            if (Section.OptionByLane == null)
            {
                OptionByLane = new OptionByLane();
            }
            else
            {
                OptionByLane = Section.OptionByLane;
            }
            if (Section.FrontInCamera != null && Section.FrontInCamera != null &&
                Section.BackInCamera != null && Section.BackOutCamera != null)
            {
                this.FrontInCamera = this.Section.FrontInCamera.IP;
                this.FrontInType = this.Section.FrontInCamera.CameraType;
                this.FrontInPort = this.Section.FrontInCamera.Port;
                this.FrontInUserName = this.Section.FrontInCamera.UserName;
                this.FrontInPassword = this.Section.FrontInCamera.Password;
                this.FrontInChannel = this.Section.FrontInCamera.Channel;
                //this.FrontInWayType = this.Section.FrontInCamera.WayType="IN";
                if (this.Section.FrontInCamera.ZoomFactor != null)
                    this.FrontInZoom = new ZoomFactor() { Factor = this.Section.FrontInCamera.ZoomFactor.Factor, ZoomEnabled = false, ZoomX = this.Section.FrontInCamera.ZoomFactor.ZoomX, ZoomY = this.Section.FrontInCamera.ZoomFactor.ZoomY };
                else
                    this.FrontInZoom = new ZoomFactor() { Factor = 100, ZoomEnabled = false, ZoomX = 0, ZoomY = 0 };
                this.FrontOutType = this.Section.FrontOutCamera.CameraType;
                this.FrontOutCamera = this.Section.FrontOutCamera.IP;
                this.FrontOutPort = this.Section.FrontOutCamera.Port;
                this.FrontOutUserName = this.Section.FrontOutCamera.UserName;
                this.FrontOutPassword = this.Section.FrontOutCamera.Password;
                this.FrontOutChannel = this.Section.FrontOutCamera.Channel;
                //this.FrontOutWayType = this.Section.FrontOutCamera.WayType="OUT";
                if (this.Section.FrontOutCamera.ZoomFactor != null)
                    this.FrontOutZoom = new ZoomFactor() { Factor = this.Section.FrontOutCamera.ZoomFactor.Factor, ZoomEnabled = false, ZoomX = this.Section.FrontOutCamera.ZoomFactor.ZoomX, ZoomY = this.Section.FrontOutCamera.ZoomFactor.ZoomY };
                else
                    this.FrontOutZoom = new ZoomFactor() { Factor = 100, ZoomEnabled = false, ZoomX = 0, ZoomY = 0 };
                this.BackInType = this.Section.BackInCamera.CameraType;
                this.BackInCamera = this.Section.BackInCamera.IP;
                this.BackInPort = this.Section.BackInCamera.Port;
                this.BackInUserName = this.Section.BackInCamera.UserName;
                this.BackInPassword = this.Section.BackInCamera.Password;
                this.BackInChannel = this.Section.BackInCamera.Channel;
                //this.BackInWayType = this.Section.BackInCamera.WayType="IN";
                if (this.Section.BackInCamera.ZoomFactor != null)
                    this.BackInZoom = new ZoomFactor() { Factor = this.Section.BackInCamera.ZoomFactor.Factor, ZoomEnabled = false, ZoomX = this.Section.BackInCamera.ZoomFactor.ZoomX, ZoomY = this.Section.BackInCamera.ZoomFactor.ZoomY };
                else
                    this.BackInZoom = new ZoomFactor() { Factor = 100, ZoomEnabled = false, ZoomX = 0, ZoomY = 0 };
                this.BackOutType = this.Section.BackOutCamera.CameraType;
                this.BackOutCamera = this.Section.BackOutCamera.IP;
                this.BackOutPort = this.Section.BackOutCamera.Port;
                this.BackOutUserName = this.Section.BackOutCamera.UserName;
                this.BackOutPassword = this.Section.BackOutCamera.Password;
                this.BackOutChannel = this.Section.BackOutCamera.Channel;
                //this.BackOutWayType = this.Section.BackOutCamera.WayType = "OUT";
                this.PrintComActive = this.Section.PrintComActive;
                this.PrintAddressTitle = this.Section.PrintAddressTitle;
                this.PrintCallTile = this.Section.PrintCallTile;
                if (this.Section.BackOutCamera.ZoomFactor != null)
                    this.BackOutZoom = new ZoomFactor() { Factor = this.Section.BackOutCamera.ZoomFactor.Factor, ZoomEnabled = false, ZoomX = this.Section.BackOutCamera.ZoomFactor.ZoomX, ZoomY = this.Section.BackOutCamera.ZoomFactor.ZoomY };
                else
                    this.BackOutZoom = new ZoomFactor() { Factor = 100, ZoomEnabled = false, ZoomX = 0, ZoomY = 0 };

            }
            this.Direction = this.Section.Direction;
            this.LedIP = this.Section.LedIP;
            if (this.Section.BarrierName != null)
                SelectedBarrier = this.Section.BarrierName;
            this.BarrierPort = this.Section.BarrierPort;
            this.LedStyle = this.Section.LedOfKind;
            this.UseBarrierIpController = Section.UseBarrierIpController;
            this.UseZKController = Section.UseZKController;
            if (Section.BarrierByInternetControl != null)
                this.BarrierByInternetControl = Section.BarrierByInternetControl;
            else
                this.BarrierByInternetControl = new InternetControl();
            if (Section.BarrierBySiemensControl != null)
                this.BarrierBySiemensControl = Section.BarrierBySiemensControl;
            else
                this.BarrierBySiemensControl = new SiemensControl();

            if (Section.BarrierByZKTekco != null)
            {
                this.BarrierByZKTekco = Section.BarrierByZKTekco;
            }
            else
            {
                this.BarrierByZKTekco = new InternetControl();

            }
            this.BarrierIpController = Section.BarrierIpController;
            this.BarrierPortController = Section.BarrierPortController;
            this.BarrierDoorsController = Section.BarrierDoorsController;
            this.BarrierHardButtonCode = Section.BarrierHardButtonCode;
            this.ComIctCashier = Section.ComIctCashier;
            this.ComIctCashierEnanble = Section.ComIctCashierEnanble;
            this.TimeTick = Section.TimeTick;
            if (!string.IsNullOrEmpty(Section.ComCash))
            {
                this.ComCash = this.Section.ComCash;
            }
            if (!string.IsNullOrEmpty(Section.ComAlarm))
            {
                this.ComAlarm = this.Section.ComAlarm;
            }
            if (!string.IsNullOrEmpty(Section.AlarmWarningKeys))
            {
                this.AlarmWarningKeys = this.Section.AlarmWarningKeys;
            }
            if (!string.IsNullOrEmpty(Section.AlarmSuccessKeys))
            {
                this.AlarmSuccessKeys = this.Section.AlarmSuccessKeys;
            }
            if (!string.IsNullOrEmpty(Section.ComPrint))
            {
                this.ComPrint = this.Section.ComPrint;
            }
            if (!string.IsNullOrEmpty(Section.ComLed))
            {
                this.ComLed = this.Section.ComLed;

            }
            if (!string.IsNullOrEmpty(Section.ComLedB))
            {
                this.ComLedB = this.Section.ComLedB;
            }
            if (!string.IsNullOrEmpty(Section.Door))
            {
                switch (Section.Door)
                {
                    case "1":
                        this.Door = new CustomCombo() { Name = "Door 1", Value = "1" };
                        break;
                    case "2":
                        this.Door = new CustomCombo() { Name = "Door 2", Value = "2" };
                        break;
					case "3":
						this.Door = new CustomCombo() { Name = "Door 3", Value = "3" };
						break;
					case "4":
						this.Door = new CustomCombo() { Name = "Door 4", Value = "4" };
						break;
					default:
                        this.Door = new CustomCombo() { Name = "-Chọn-", Value = "-1" };
                        break;
                }
            }
            else
            {
                this.Door = new CustomCombo() { Name = "-Chọn-", Value = "-1" };
            }
            if (!string.IsNullOrEmpty(Section.Reader))
            {
                switch (Section.Reader)
                {
                    case "1":
                        this.Reader = new CustomCombo() { Name = "Reader 1", Value = "1" };
                        break;
                    case "2":
                        this.Reader = new CustomCombo() { Name = "Reader 2", Value = "2" };
                        break;
					case "3":
						this.Reader = new CustomCombo() { Name = "Reader 3", Value = "3" };
						break;
					case "4":
						this.Reader = new CustomCombo() { Name = "Reader 4", Value = "4" };
						break;
					default:
                        this.Reader = new CustomCombo() { Name = "-Chọn-", Value = "-1" };
                        break;
                }
            }
            else
            {
                this.Reader = new CustomCombo() { Name = "-Chọn-", Value = "-1" };
            }
            CurrentPort = 8000;
            ///Mở rộng camera và đầu đọc tầm xa - 2018Jul25
            if (Section.ExtraIn1Camera != null && Section.ExtraIn2Camera != null)
            {
                this.ExtraIn1Type = this.Section.ExtraIn1Camera.CameraType;
                this.ExtraIn1Camera = this.Section.ExtraIn1Camera.IP;
                this.ExtraIn1Port = this.Section.ExtraIn1Camera.Port;
                this.ExtraIn1UserName = this.Section.ExtraIn1Camera.UserName;
                this.ExtraIn1Password = this.Section.ExtraIn1Camera.Password;
                this.ExtraIn1Password = this.Section.ExtraIn1Camera.WayType;
                this.ExtraIn1Channel = this.Section.ExtraIn1Camera.Channel;
                if (this.Section.ExtraIn1Camera.ZoomFactor != null)
                    this.ExtraIn1Zoom = new ZoomFactor() { Factor = this.Section.ExtraIn1Camera.ZoomFactor.Factor, ZoomEnabled = false, ZoomX = this.Section.ExtraIn1Camera.ZoomFactor.ZoomX, ZoomY = this.Section.ExtraIn1Camera.ZoomFactor.ZoomY };
                else
                    this.ExtraIn1Zoom = new ZoomFactor() { Factor = 100, ZoomEnabled = false, ZoomX = 0, ZoomY = 0 };
                this.ExtraIn2Type = this.Section.ExtraIn2Camera.CameraType;
                this.ExtraIn2Camera = this.Section.ExtraIn2Camera.IP;
                this.ExtraIn2Port = this.Section.ExtraIn2Camera.Port;
                this.ExtraIn2UserName = this.Section.ExtraIn2Camera.UserName;
                this.ExtraIn2Password = this.Section.ExtraIn2Camera.Password;
                this.ExtraIn2Password = this.Section.ExtraIn2Camera.WayType;
                this.ExtraIn2Channel = this.Section.ExtraIn1Camera.Channel;
                if (this.Section.ExtraIn2Camera.ZoomFactor != null)
                    this.ExtraIn2Zoom = new ZoomFactor() { Factor = this.Section.ExtraIn2Camera.ZoomFactor.Factor, ZoomEnabled = false, ZoomX = this.Section.ExtraIn2Camera.ZoomFactor.ZoomX, ZoomY = this.Section.ExtraIn2Camera.ZoomFactor.ZoomY };
                else
                    this.ExtraIn2Zoom = new ZoomFactor() { Factor = 100, ZoomEnabled = false, ZoomX = 0, ZoomY = 0 };

            }
            if (Section.ExtraOut1Camera != null && Section.ExtraOut2Camera != null)
            {
                this.ExtraOut1Type = this.Section.ExtraOut1Camera.CameraType;
                this.ExtraOut1Camera = this.Section.ExtraOut1Camera.IP;
                this.ExtraOut1Port = this.Section.ExtraOut1Camera.Port;
                this.ExtraOut1UserName = this.Section.ExtraOut1Camera.UserName;
                this.ExtraOut1Password = this.Section.ExtraOut1Camera.Password;
                this.ExtraOut1Password = this.Section.ExtraOut1Camera.WayType;
                this.ExtraOut1Channel = this.Section.ExtraIn1Camera.Channel;
                if (this.Section.ExtraOut1Camera.ZoomFactor != null)
                    this.ExtraOut1Zoom = new ZoomFactor() { Factor = this.Section.ExtraOut1Camera.ZoomFactor.Factor, ZoomEnabled = false, ZoomX = this.Section.ExtraOut1Camera.ZoomFactor.ZoomX, ZoomY = this.Section.ExtraOut1Camera.ZoomFactor.ZoomY };
                else
                    this.ExtraOut1Zoom = new ZoomFactor() { Factor = 100, ZoomEnabled = false, ZoomX = 0, ZoomY = 0 };
                this.ExtraOut2Type = this.Section.ExtraOut2Camera.CameraType;
                this.ExtraOut2Camera = this.Section.ExtraOut2Camera.IP;
                this.ExtraOut2Port = this.Section.ExtraOut2Camera.Port;
                this.ExtraOut2UserName = this.Section.ExtraOut2Camera.UserName;
                this.ExtraOut2Password = this.Section.ExtraOut2Camera.Password;
                this.ExtraOut2Password = this.Section.ExtraOut2Camera.WayType;
                this.ExtraOut2Channel = this.Section.ExtraIn1Camera.Channel;
                if (this.Section.ExtraOut2Camera.ZoomFactor != null)
                    this.ExtraOut2Zoom = new ZoomFactor() { Factor = this.Section.ExtraOut2Camera.ZoomFactor.Factor, ZoomEnabled = false, ZoomX = this.Section.ExtraOut2Camera.ZoomFactor.ZoomX, ZoomY = this.Section.ExtraOut2Camera.ZoomFactor.ZoomY };
                else
                    this.ExtraOut2Zoom = new ZoomFactor() { Factor = 100, ZoomEnabled = false, ZoomX = 0, ZoomY = 0 };

            }
            IsInExtra = Section.IsInExtra;
            IsOutExtra = Section.IsOutExtra;
            _cardtypes = new ObservableCollection<string>();
            _cardtypes.Add("Serial");
            _cardtypes.Add("Ip Server");
            _cardtypes.Add("Ip Client");
            _cardtypes.Add("Ip Controller");
            _cardtypes.Add("Remode Card");
            _cardtypes.Add("Scannel");
            _cardtypes.Add("NFC");
            _cardtypes.Add("Proxies");
            _cardtypes.Add("ZKFarCard");
            _antennatypes = new ObservableCollection<string>();
            _antennatypes.Add("1");
            _antennatypes.Add("2");
            _antennatypes.Add("3");
            _antennatypes.Add("4");
            _doortypes = new ObservableCollection<CustomCombo>();
            _doortypes.Add(new CustomCombo() { Name = "-Chọn-", Value = "-1" });
            _doortypes.Add(new CustomCombo() { Name = "Door 1", Value = "1" });
            _doortypes.Add(new CustomCombo() { Name = "Door 2", Value = "2" });
			_doortypes.Add(new CustomCombo() { Name = "Door 3", Value = "3" });
			_doortypes.Add(new CustomCombo() { Name = "Door 4", Value = "4" });
			_readertypes = new ObservableCollection<CustomCombo>();
            _readertypes.Add(new CustomCombo() { Name = "-Chọn-", Value = "-1" });
            _readertypes.Add(new CustomCombo() { Name = "Reader 1", Value = "1" });
            _readertypes.Add(new CustomCombo() { Name = "Reader 2", Value = "2" });
			_readertypes.Add(new CustomCombo() { Name = "Reader 3", Value = "3" });
			_readertypes.Add(new CustomCombo() { Name = "Reader 4", Value = "4" });
			CurrentCardType = "Serial";
            CurrentAntennaType = "1";
            if (this.Section.ModWinsCards != null)
            {
                this.ModWinsCards = new ObservableCollection<IGreenCardReaderInfo>();
                foreach (var item in this.Section.ModWinsCards)
                    this.ModWinsCards.Add(item);
            }
            else
            {
                this.ModWinsCards = new ObservableCollection<IGreenCardReaderInfo>();
            }
            if (this.Section.TcpIpServerCards != null)
            {
                this.TcpIpServerCards = new ObservableCollection<IGreenCardReaderInfo>();
                foreach (var item in this.Section.TcpIpServerCards)
                    this.TcpIpServerCards.Add(item);
            }
            else
            {
                this.TcpIpServerCards = new ObservableCollection<IGreenCardReaderInfo>();
            }
            if (this.Section.TcpIpControllerCards != null)
            {
                this.TcpIpControllerCards = new ObservableCollection<IGreenCardReaderInfo>();
                foreach (var item in this.Section.TcpIpControllerCards)
                    this.TcpIpControllerCards.Add(item);
            }
            else
            {
                this.TcpIpControllerCards = new ObservableCollection<IGreenCardReaderInfo>();
            }
            if (this.Section.TcpIpClientCards != null)
            {
                this.TcpIpClientCards = new ObservableCollection<IGreenCardReaderInfo>();
                foreach (var item in this.Section.TcpIpClientCards)
                    this.TcpIpClientCards.Add(item);
            }
            else
            {
                this.TcpIpClientCards = new ObservableCollection<IGreenCardReaderInfo>();
            }
            if (this.Section.ScannelCards != null)
            {
                this.ScannelCards = new ObservableCollection<IGreenCardReaderInfo>();
                foreach (var item in this.Section.ScannelCards)
                    this.ScannelCards.Add(item);
            }
            else
            {
                this.ScannelCards = new ObservableCollection<IGreenCardReaderInfo>();
            }
            if (this.Section.TcpIpRemodeCards != null)
            {
                this.TcpIpRemoderCards = new ObservableCollection<IGreenCardReaderInfo>();
                foreach (var item in this.Section.TcpIpRemodeCards)
                    this.TcpIpRemoderCards.Add(item);
            }
            else
            {
                this.TcpIpRemoderCards = new ObservableCollection<IGreenCardReaderInfo>();
            }
            //this.Section.ReadyGreenCards();
            List<IGreenCardReaderInfo> lst = new List<IGreenCardReaderInfo>();
            List<string> allModWinsCard = ModWinsCardReader.ListModWinsCards();
            if (allModWinsCard == null)
                allModWinsCard = new List<string>();
            foreach (var item in allModWinsCard)
            {
                if (!_modwinscards.ToList().Exists(x => x.SerialNumber == item))
                {
                    lst.Add(new GreenCardReaderInfo() { Type = "ModWinsCard", SerialNumber = item });
                }
            }
            AvilableModWinsCards = new ObservableCollection<IGreenCardReaderInfo>();
            foreach (var item in lst)
            {
                AvilableModWinsCards.Add(item);
            }
            if (_avilablemodwinscards != null && _avilablemodwinscards.Count > 0)
                CurrentCardSelected = _avilablemodwinscards[0];

            this.NFCCards = new ObservableCollection<IGreenCardReaderInfo>();
            if (this.Section.NFCCards != null)
            {
                foreach (var item in this.Section.NFCCards)
                    this.NFCCards.Add(item);
            }

            this.ProxiesCards = new ObservableCollection<IGreenCardReaderInfo>();
            if (this.Section.ProxiesCards != null)
            {
                this.ProxiesCards = new ObservableCollection<IGreenCardReaderInfo>();
                foreach (var item in this.Section.ProxiesCards)
                    this.ProxiesCards.Add(item);
            }

            this.ZKFarCards = new ObservableCollection<IGreenCardReaderInfo>();
            if (this.Section.ZKFarCards != null)
            {
                this.ZKFarCards = new ObservableCollection<IGreenCardReaderInfo>();
                foreach (var item in this.Section.ZKFarCards)
                    this.ZKFarCards.Add(item);
            }

            LoadZoomList();
        }

        public void Init(ParameterKey key)
        {
            _server = Mvx.Resolve<IServer>();
        }

        public override void Start()
        {
            base.Start();
            IsDetecting = false;
            //_resourceLocatorService.RegisterSection(section, lane);
        }

        public void LaneNameChanged(string name)
        {
            LaneName = name;
        }

        public void GetCardReaderInfo()
        {
            AllCardReaders = new ObservableCollection<CardReaderWrapper>(_cardReaderService.GetCardReaders());
            AllCardReaders.Add(new CardReaderWrapper { SerialNumber = REMOTE_CARD_READER });
            CardReaders = Section.CardReaders != null
                ? new ObservableCollection<CardReaderWrapper>(Section.CardReaders)
                : new ObservableCollection<CardReaderWrapper>();

            AvailableCardReaders = new ObservableCollection<CardReaderWrapper>(
                AllCardReaders.Where(x => x != null
                    && !CardReaders.Any(card => card != null && card.SerialNumber.Equals(x.SerialNumber))));
        }

        public void GetWebcamInfo()
        {
            this.Webcams = WebcamFactoryService.WebCams.ToList();
        }

        private string _saveResultMessage;
        public string SaveResultMessage
        {
            get { return _saveResultMessage; }
            set { _saveResultMessage = value; RaisePropertyChanged(() => SaveResultMessage); }
        }

        MvxCommand _saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                _saveCommand = _saveCommand ?? new MvxCommand(() =>
                {
                    Save(null);
                    SaveResultMessage = GeneralConfigViewModel.msgSaved;
                });

                return _saveCommand;
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
        private void AddSiemens()
        {
            this.Section.BarrierBySiemensControl = this.BarrierBySiemensControl;
            if (this.Section != null && this.Section.BarrierBySiemensControl != null && !string.IsNullOrEmpty(this.Section.BarrierBySiemensControl.IP) && this.BarrierBySiemensControl.TypeIn != LogoTypeIn4.None)
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
        }
        public void Save(Action complete)
        {
            Dictionary<SectionPosition, Section> aaa = _userPreferenceService.SystemSettings.Sections;
            Section a1 = aaa[SectionPosition.Lane1];
            Section a2 = aaa[SectionPosition.Lane2];
            if (this.Section.Lane == null)
                this.Section.Lane = new Models.Lane();
            this.Section.OptionByLane = this.OptionByLane;
            this.Section.Lane.Name = this.LaneName;
            this.Section.Lane.VehicleTypeId = (int)SelectedVehicleType;
            this.Section.Lane.Direction = this.Direction;
            this.Section.Lane.Enabled = true;
            this.Section.Direction = this.Direction;
            this.Section.BarrierName = this.SelectedBarrier;
            this.Section.ComIctCashier = this.ComIctCashier;
            this.Section.ComIctCashierEnanble = this.ComIctCashierEnanble;
            this.Section.ComAlarm = this.ComAlarm;
            this.Section.AlarmSuccessKeys = this.AlarmSuccessKeys;
            this.Section.AlarmWarningKeys = this.AlarmWarningKeys;
            this.Section.ComPrint = this.ComPrint;
            this.Section.ComLed = this.ComLed;
            this.Section.LedIP = this.LedIP;
            this.Section.ComLedB = this.ComLedB;
            this.Section.UseBarrierIpController = this.UseBarrierIpController;
            this.Section.BarrierByInternetControl = this.BarrierByInternetControl;
            this.Section.BarrierByZKTekco = this.BarrierByZKTekco;
            this.Section.UseZKController = this.UseZKController;
            AddSiemens();
            this.Section.BarrierIpController = this.BarrierIpController;
            this.Section.BarrierPortController = this.BarrierPortController;
            this.Section.BarrierDoorsController = this.BarrierDoorsController;
            this.Section.BarrierHardButtonCode = this.BarrierHardButtonCode;
            this.Section.TimeTick = this.TimeTick;
            if (this.Door != null && (this.Door.Value == "1" || this.Door.Value == "2"))
                this.Section.Door = this.Door.Value;
            else
                this.Section.Door = null;
            if (this.Reader != null && (this.Reader.Value == "0" || this.Reader.Value == "1"))
                this.Section.Reader = this.Reader.Value;
            else
                this.Section.Reader = null;
            this.Section.LedOfKind = this.LedStyle;
            this.Section.BarrierPort = this.BarrierPort;
            this.Section.ModWinsCards = this.ModWinsCards.ToList();
            this.Section.TcpIpClientCards = this.TcpIpClientCards.ToList();
            this.Section.TcpIpServerCards = this.TcpIpServerCards.ToList();
            this.Section.ScannelCards = this.ScannelCards.ToList();
            this.Section.TcpIpControllerCards = this.TcpIpControllerCards.ToList();
            this.Section.TcpIpRemodeCards = this.TcpIpRemoderCards.ToList();
            this.Section.NFCCards = this.NFCCards.ToList();
            this.Section.ProxiesCards = this.ProxiesCards.ToList();
            this.Section.ZKFarCards = this.ZKFarCards.ToList();
            SaveCards();
            Section.IsInExtra = IsInExtra;
            Section.IsOutExtra = IsOutExtra;
            AttachDevices();
            AttachZoom();
            _userPreferenceService.SystemSettings.UpdateSection(this.Section as Section);
            _userPreferenceService.SystemSettings.MarkChanged();
            _userPreferenceService.SystemSettings.Save();
            if (complete != null) complete();
        }

        void SaveCards()
        {
            if (this.ModWinsCards != null)
            {
                foreach (var cif in this.ModWinsCards)
                {
                    CurrentListCardReader.AddCardInfo(cif);
                }
            }
            if (this.TcpIpServerCards != null)
            {
                foreach (var cif in this.TcpIpServerCards)
                {
                    CurrentListCardReader.AddCardInfo(cif);
                }
            }
            if (this.TcpIpControllerCards != null)
            {
                foreach (var cif in this.TcpIpControllerCards)
                {
                    CurrentListCardReader.AddCardInfo(cif);
                }
            }
            if (this.TcpIpClientCards != null)
            {
                foreach (var cif in this.TcpIpClientCards)
                {
                    CurrentListCardReader.AddCardInfo(cif);
                }
            }
            if (this.ScannelCards != null)
            {
                foreach (var cif in this.ScannelCards)
                {
                    CurrentListCardReader.AddCardInfo(cif);
                }
            }
            if (this.TcpIpRemoderCards != null)
            {
                foreach (var cif in this.TcpIpRemoderCards)
                {
                    CurrentListCardReader.AddCardInfo(cif);
                }
            }

            if (this.NFCCards != null)
            {
                foreach (var cif in this.NFCCards)
                {
                    CurrentListCardReader.AddCardInfo(cif);
                }
            }

            if (this.ProxiesCards != null)
            {
                foreach (var cif in this.ProxiesCards)
                {
                    CurrentListCardReader.AddCardInfo(cif);
                }
            }

            if (this.ZKFarCards != null)
            {
                foreach (var cif in this.ZKFarCards)
                {
                    CurrentListCardReader.AddCardInfo(cif);
                }
            }

            Task.Factory.StartNew(() => CurrentListCardReader.RefreshListCard());
        }
        void AttachZoom()
        {
            if (this.Section.FrontInCamera != null && this.Section.FrontInCamera.RawCamera != null)
                this.Section.FrontInCamera.ZoomFactor = this.Section.FrontInCamera.RawCamera.ZoomFactor;
            if (this.Section.FrontOutCamera != null && this.Section.FrontOutCamera.RawCamera != null)
                this.Section.FrontOutCamera.ZoomFactor = this.Section.FrontOutCamera.RawCamera.ZoomFactor;
            if (this.Section.BackInCamera != null && this.Section.BackInCamera.RawCamera != null)
                this.Section.BackInCamera.ZoomFactor = this.Section.BackInCamera.RawCamera.ZoomFactor;
            if (this.Section.BackOutCamera != null && this.Section.BackOutCamera.RawCamera != null)
                this.Section.BackOutCamera.ZoomFactor = this.Section.BackOutCamera.RawCamera.ZoomFactor;
            if (this.Section.ExtraIn1Camera != null && this.Section.ExtraIn1Camera.RawCamera != null)
                this.Section.ExtraIn1Camera.ZoomFactor = this.Section.ExtraIn1Camera.RawCamera.ZoomFactor;
            if (this.Section.ExtraIn2Camera != null && this.Section.ExtraIn2Camera.RawCamera != null)
                this.Section.ExtraIn2Camera.ZoomFactor = this.Section.ExtraIn2Camera.RawCamera.ZoomFactor;
            if (this.Section.ExtraOut1Camera != null && this.Section.ExtraOut1Camera.RawCamera != null)
                this.Section.ExtraOut1Camera.ZoomFactor = this.Section.ExtraOut1Camera.RawCamera.ZoomFactor;
            if (this.Section.ExtraOut2Camera != null && this.Section.ExtraOut2Camera.RawCamera != null)
                this.Section.ExtraOut2Camera.ZoomFactor = this.Section.ExtraOut2Camera.RawCamera.ZoomFactor;
        }
        public void AttachDevices()
        {
            this.Section.VehicleTypeId = (int)SelectedVehicleType;
            this.Section.AttachCamera(FrontInCamera, FrontInPort, FrontInUserName, FrontInPassword, FrontInChannel, LaneDirection.In, CameraPosition.Front, this.FrontInType, this.FrontInWayType);
            this.Section.AttachCamera(BackInCamera, BackInPort, BackInUserName, BackInPassword, BackInChannel, LaneDirection.In, CameraPosition.Back, this.BackInType, BackInWayType);
            this.Section.AttachCamera(FrontOutCamera, FrontOutPort, FrontOutUserName, FrontOutPassword, FrontOutChannel, LaneDirection.Out, CameraPosition.Front, this.FrontOutType, FrontOutWayType);
            this.Section.AttachCamera(BackOutCamera, BackOutPort, BackOutUserName, BackOutPassword, BackOutChannel, LaneDirection.Out, CameraPosition.Back, this.BackOutType, BackOutWayType);
            if (IsInExtra)
            {
                this.Section.AttachCamera(ExtraIn1Camera, ExtraIn1Port, ExtraIn1UserName, ExtraIn1Password, ExtraIn1Channel, LaneDirection.In, CameraPosition.Extra1, this.ExtraIn1Type, FrontInWayType);
                this.Section.AttachCamera(ExtraIn2Camera, ExtraIn2Port, ExtraIn2UserName, ExtraIn2Password, ExtraIn2Channel, LaneDirection.In, CameraPosition.Extra2, this.ExtraIn2Type, FrontInWayType);
            }
            if (IsOutExtra)
            {
                this.Section.AttachCamera(ExtraOut1Camera, ExtraOut1Port, ExtraOut1UserName, ExtraOut1Password, ExtraOut1Channel, LaneDirection.Out, CameraPosition.Extra1, this.ExtraOut1Type, FrontOutWayType);
                this.Section.AttachCamera(ExtraOut2Camera, ExtraOut2Port, ExtraOut2UserName, ExtraOut2Password, ExtraOut2Channel, LaneDirection.Out, CameraPosition.Extra2, this.ExtraOut2Type, FrontOutWayType);
            }
            if (Section.CardReaders != null)
                this.Section.CardReaders.Clear();
            ///old
            //foreach (var cardReader in CardReaders)
            //    this.Section.AttachCardReader(cardReader);
        }

        private MvxCommand _detectCardCommand;
        public ICommand DetectCardCommand
        {
            get
            {
                _detectCardCommand = _detectCardCommand ?? new MvxCommand(() =>
                {
                    if (!IsDetecting)
                    {
                        IsDetecting = true;
                        foreach (var item in this.AllCardReaders)
                        {
                            if (!item.SerialNumber.Equals(REMOTE_CARD_READER))
                                item.RawCardReader.ReadingCompleted += RawCardReader_ReadingCompleted;
                        }
                    }
                    else
                    {
                        IsDetecting = false;
                        foreach (var item in this.AllCardReaders)
                        {
                            if (!item.SerialNumber.Equals(REMOTE_CARD_READER))
                                item.RawCardReader.ReadingCompleted -= RawCardReader_ReadingCompleted;
                        }
                    }
                });

                return _detectCardCommand;
            }
        }
        private MvxCommand _addCurrentGreenCard;
        public ICommand AddCurrentGreenCard
        {
            get
            {
                return _addCurrentGreenCard ??
                    (_addCurrentGreenCard = new MvxCommand(() =>
                    {
                        switch (CurrentCardType)
                        {
                            case "Serial":
                                if (CurrentCardSelected != null && !_modwinscards.ToList().Exists(x => x.SerialNumber == CurrentCardSelected.SerialNumber))
                                {

                                    ModWinsCards.Add(new GreenCardReaderInfo() { Type = "ModWinsCard", SerialNumber = _currentcardselected.SerialNumber, CallName = "ModWins"+ $"(Farcard:{this.UsageAsTheSameFarCard})", UsageAsTheSameFarCard = this.UsageAsTheSameFarCard });
                                    var cc = AvilableModWinsCards.FirstOrDefault(x => x.SerialNumber == CurrentCardSelected.SerialNumber);
                                    _currentcardselected = null;

                                    if (cc != null)
                                    {
                                        AvilableModWinsCards.Remove(cc);
                                    }
                                    if (AvilableModWinsCards != null && AvilableModWinsCards.Count > 0)
                                        CurrentCardSelected = AvilableModWinsCards[0];
                                    else
                                        CurrentCardSelected = null;
                                }
                                break;
                            case "Ip Server":
                                if (!string.IsNullOrEmpty(CurrentIp) && !TcpIpServerCards.ToList().Exists(x => x.TcpIp == CurrentIp && x.Port == CurrentPort))
                                {

                                    TcpIpServerCards.Add(new GreenCardReaderInfo() { Type = "Tcp Ip Server", TcpIp = CurrentIp, Port = CurrentPort, CallName = "Tcp Server" + $"(Farcard:{this.UsageAsTheSameFarCard})", UsageAsTheSameFarCard = this.UsageAsTheSameFarCard });

                                }
                                break;
                            case "Ip Client":
                                if (!string.IsNullOrEmpty(CurrentIp) && !TcpIpClientCards.ToList().Exists(x => x.TcpIp == CurrentIp && x.Port == CurrentPort))
                                {
                                    TcpIpClientCards.Add(new GreenCardReaderInfo() { Type = "Tcp Ip Client", TcpIp = CurrentIp, Port = CurrentPort, CallName = "Tcp Client"+$"(Farcard:{this.UsageAsTheSameFarCard})", UsageAsTheSameFarCard = this.UsageAsTheSameFarCard });
                                }
                                break;
                            case "Scannel":
                                if (!string.IsNullOrEmpty(CurrentIp) && !ScannelCards.ToList().Exists(x => x.TcpIp == CurrentIp && x.Port == CurrentPort && x.Antenna == CurrentAntennaType))
                                {
                                    ScannelCards.Add(new GreenCardReaderInfo() { Type = "Scannel", TcpIp = CurrentIp, Port = CurrentPort, CallName = "Scannel"+ $"(Farcard:{this.UsageAsTheSameFarCard})", UsageAsTheSameFarCard = this.UsageAsTheSameFarCard, Antenna = CurrentAntennaType });
                                }
                                break;
                            case "Ip Controller":
                                if (!string.IsNullOrEmpty(CurrentIp) && Door != null && Reader != null && !TcpIpControllerCards.ToList().Exists(x => x.TcpIp == CurrentIp && x.Port == CurrentPort))
                                {
                                    TcpIpControllerCards.Add(new GreenCardReaderInfo() { Type = "Tcp Ip Controller", TcpIp = CurrentIp, Port = CurrentPort, CallName = "Controller:" + Door.Name + ", " + Reader.Name + $"(Farcard:{this.UsageAsTheSameFarCard})", UsageAsTheSameFarCard = this.UsageAsTheSameFarCard});
                                }
                                break;
                            case "Remode Card":
                                if (!string.IsNullOrEmpty(CurrentIp) && !TcpIpClientCards.ToList().Exists(x => x.TcpIp == CurrentIp && x.Port == CurrentPort))
                                {
                                    TcpIpRemoderCards.Add(new GreenCardReaderInfo() { Type = "Remode Card", TcpIp = CurrentIp, Port = CurrentPort, CallName = "Remode"+ $"(Farcard:{this.UsageAsTheSameFarCard})", UsageAsTheSameFarCard = this.UsageAsTheSameFarCard });
                                }
                                break;
                            case "NFC":
                                if (CurrentCardSelected != null && !_nfcCards.ToList().Exists(x => x.DeviceName == CurrentCardSelected.DeviceName))
                                {
                                    NFCCards.Add(new GreenCardReaderInfo() { Type = Constants.CardType.NFC.ToString(), DeviceName = CurrentCardSelected.DeviceName });
                                    AvailableNFCCards.Remove(_availableNFCCards.First(x => x.DeviceName == CurrentCardSelected.DeviceName));
                                    CurrentCardSelected = null;
                                }
                                break;
                            case "Proxies":
                                if (CurrentCardSelected != null && !_proxiesCards.ToList().Exists(x => x.DeviceName == CurrentCardSelected.DeviceName))
                                {
                                    ProxiesCards.Add(new GreenCardReaderInfo() { Type = Constants.CardType.Proxies.ToString(), DeviceName = CurrentCardSelected.DeviceName });
                                    AvailableProxiesCards.Remove(_availableProxiesCards.First(x => x.DeviceName == CurrentCardSelected.DeviceName));
                                    CurrentCardSelected = null;
                                }
                                break;
                            case "ZKFarCard":
                                if (!string.IsNullOrEmpty(CurrentIp) && CurrentPort > 0 && Reader != null &&
                                !_zkFarCards.ToList().Exists(x => x.TcpIp == CurrentIp && x.Port == CurrentPort && x.Reader == Reader.Value))
                                {
                                    ZKFarCards.Add(new GreenCardReaderInfo() { Type = Constants.CardType.ZKFarCard.ToString(), DeviceName = $"ZK:{CurrentIp}->{Reader.Value}(Farcard:{this.UsageAsTheSameFarCard})", TcpIp = CurrentIp, Port = CurrentPort, Reader = Reader.Value, UsageAsTheSameFarCard = this.UsageAsTheSameFarCard });
                                }
                                break;
                        }

                    }));
            }
        }
        private MvxCommand<IGreenCardReaderInfo> _removeGreenCard;
        public ICommand RemoveGreenCard
        {
            get
            {
                return _removeGreenCard ??
                       (_removeGreenCard = new MvxCommand<IGreenCardReaderInfo>(c =>
                       {
                           switch (c.Type)
                           {
                               case "ModWinsCard":
                                   if (ModWinsCards != null)
                                   {
                                       ModWinsCards.Remove(c);
                                       AvilableModWinsCards.Add(c);
                                   }
                                   break;
                               case "Tcp Ip Server":
                                   if (TcpIpServerCards != null)
                                       TcpIpServerCards.Remove(c);
                                   break;
                               case "Tcp Ip Controller":
                                   if (TcpIpControllerCards != null)
                                       TcpIpControllerCards.Remove(c);
                                   break;
                               case "Tcp Ip Client":
                                   if (TcpIpClientCards != null)
                                       TcpIpClientCards.Remove(c);
                                   break;
                               case "Scannel":
                                   if (ScannelCards != null)
                                       ScannelCards.Remove(c);
                                   break;
                               case "Remode Card":
                                   if (TcpIpRemoderCards != null)
                                       TcpIpRemoderCards.Remove(c);
                                   break;
                               case "NFC":
                                   if (NFCCards != null)
                                       NFCCards.Remove(c);
                                   break;
                               case "Proxis":
                                   if (ProxiesCards != null)
                                       ProxiesCards.Remove(c);
                                   break;
                               case "ZKFarCard":
                                   {
                                       if (ZKFarCards != null)
                                       {
                                           ZKFarCards.Remove(c);
                                       }
                                       break; 
                                   }
                           }
                       }));
            }
        }
        private MvxCommand _refreshModWInsCards;
        public ICommand RefreshModWInsCards
        {
            get
            {
                return _refreshModWInsCards ??
                       (_refreshModWInsCards = new MvxCommand(() =>
                       {
                           List<string> allModWinsCard = ModWinsCardReader.ListModWinsCards();
                           List<IGreenCardReaderInfo> lst = new List<IGreenCardReaderInfo>();
                           if (allModWinsCard == null)
                               allModWinsCard = new List<string>();
                           foreach (var item in allModWinsCard)
                           {
                               if (!_modwinscards.ToList().Exists(x => x.SerialNumber == item))
                               {
                                   lst.Add(new GreenCardReaderInfo() { Type = "ModWinsCard", SerialNumber = item });
                               }
                           }
                           AvilableModWinsCards = new ObservableCollection<IGreenCardReaderInfo>();
                           foreach (var item in lst)
                           {
                               AvilableModWinsCards.Add(item);
                           }

                       }));
            }
        }

        private void LoadCardData(string cardType)
        {
            switch (cardType)
            {
                case "NFC":
                    {
                        AvailableNFCCards = new ObservableCollection<IGreenCardReaderInfo>();
                        var cardReaders = NFCCardReader.GetReaders();
                        foreach (var item in cardReaders)
                        {
                            AvailableNFCCards.Add(new GreenCardReaderInfo() { Type = Constants.CardType.NFC.ToString(), DeviceName = item });
                        }
                        break;
                    }
                case "Proxies":
                    {
                        AvailableProxiesCards = new ObservableCollection<IGreenCardReaderInfo>();
                        var cardReaders = ProxiesCardReader.GetReaders();
                        foreach (var item in cardReaders)
                        {
                            AvailableProxiesCards.Add(new GreenCardReaderInfo() { Type = Constants.CardType.Proxies.ToString(), DeviceName = item });
                        }
                        break;
                    }
                case "ZKFarCard":
                    {
                        AvailableZKFarCards = new ObservableCollection<IGreenCardReaderInfo>();
                        var cardReaders = ZKFarCardReader.GetReaders();
                        foreach (var item in cardReaders)
                        {
                            AvailableZKFarCards.Add(new GreenCardReaderInfo() { Type = Constants.CardType.ZKFarCard.ToString(), DeviceName = item });
                        }
                        break;
                    }
                default:
                    break;
            }
        }


        private MvxCommand _detectBarrierCommand;
        public ICommand DetectBarrierCommand
        {
            get
            {
                _detectBarrierCommand = _detectBarrierCommand ?? new MvxCommand(() =>
                {
                    //var barrier = _barrierDeviceManager.GetDevice(SelectedBarrier, BarrierPort);
                    //if (barrier != null)
                    //	barrier.Open();
                    if (_userPreferenceService.OptionsSettings.IsSfactorsCom)
                    {
                        if (string.IsNullOrEmpty(SelectedBarrier))
                            return;
                        var index1 = SelectedBarrier.IndexOf("(");
                        var index2 = SelectedBarrier.IndexOf(")");
                        if (index1 == -1 || index2 == -1)
                            return;
                        var ComName = SelectedBarrier.Substring(index1 + 1, index2 - index1 - 1);
                        ComManagement sftCom = ComManagement.GetInstance();
                        if (BarrierPort.ToUpper().Contains("B1") && BarrierPort.ToUpper().Contains("B2"))
                        {
                            sftCom.AddCommand(new ComParameter()
                            {
                                ComName = ComName,
                                Description = "Kiểm tra cấu hình Barrier",
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
                                ComName = ComName,
                                Description = "Kiểm tra cấu hình Barrier",
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

                            if (BarrierPort.ToUpper().Contains("B1"))
                                sftCom.AddCommand(new ComParameter()
                                {
                                    ComName = ComName,
                                    Description = "Kiểm tra cấu hình Barrier",
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
                            else if (BarrierPort.ToUpper().Contains("B2"))
                                sftCom.AddCommand(new ComParameter()
                                {
                                    ComName = ComName,
                                    Description = "Kiểm tra cấu hình Barrier",
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
                    else
                    {

                        if (BarrierPort.ToUpper().Contains("B1") && BarrierPort.ToUpper().Contains("B2"))
                        {
                            var barrier = _barrierDeviceManager.GetDevice(SelectedBarrier, "B1");
                            if (barrier != null)
                            {
                                barrier.Open();
                                Thread.Sleep(100);
                            }
                            var barrier2 = _barrierDeviceManager.GetDevice(SelectedBarrier, "B2");
                            if (barrier2 != null)
                            {
                                barrier2.Open();
                                Thread.Sleep(100);
                            }
                        }
                        else
                        {
                            var barrier = _barrierDeviceManager.GetDevice(SelectedBarrier, BarrierPort);
                            if (barrier != null)
                                barrier.Open();
                        }
                    }
                });

                return _detectBarrierCommand; 
            }
        }
        private MvxCommand _detectBarrierIPController;
        public ICommand DetectBarrierCommandIPController
        {
            get
            {
                _detectBarrierIPController = _detectBarrierIPController ?? new MvxCommand(() =>
                {
                    if (UseBarrierIpController && !string.IsNullOrEmpty(BarrierIpController) && !string.IsNullOrEmpty(BarrierDoorsController))
                        CurrentListBarrierIp.OpenBarrier(BarrierIpController, BarrierPortController, BarrierDoorsController, TimeTick);
                    CurrentListBarrierIp.StoptHandButtonClick(BarrierIpController, BarrierPortController, HardButton_Clicked);
                    CurrentListBarrierIp.StartHandButtonClick(BarrierIpController, BarrierPortController, HardButton_Clicked);
                });
                return _detectBarrierIPController;
            }
        }

        private MvxCommand _detectBarrierZKController;
        public ICommand DetectBarrierZKController
        {
            get
            {
                _detectBarrierZKController = _detectBarrierZKController ?? new MvxCommand(() =>
                {
                    if (UseZKController && !string.IsNullOrEmpty(BarrierIpController) && !string.IsNullOrEmpty(BarrierDoorsController))
                    {
                        var zkController = ZKControllerProcessor.GetInstance(BarrierIpController, BarrierPortController);
                        zkController.SendOutputCommand(BarrierDoorsController);
                    }
                });
                return _detectBarrierZKController;
            }
        }

        //DetectBarrierCommandInternetControl
        private void HardButton_Clicked(object obj, GreenHandButtonEventArgs e)
        {
            if (e.ex == null)
            {
                BarrierHardButtonCode = e.EventType;
                CurrentListBarrierIp.StoptHandButtonClick(BarrierIpController, BarrierPortController, HardButton_Clicked);
            }
        }
        private MvxCommand _addCurrentCardReader;
        public ICommand AddCurrentCardReader
        {
            get
            {
                return _addCurrentCardReader ??
                    (_addCurrentCardReader = new MvxCommand(() =>
                    {
                        if (string.IsNullOrEmpty(CurrentCardReader)) return;
                        CardReaderWrapper cardReader = null;
                        if (!CurrentCardReader.Equals(REMOTE_CARD_READER))
                            cardReader = _cardReaderService.GetCardReader(CurrentCardReader);
                        else if (!string.IsNullOrWhiteSpace(CardReaderIP))
                        {
                            var comps = CardReaderIP.Split(':');
                            string ip = string.Empty;
                            string port = string.Empty;
                            if (comps.Length > 0)
                            {
                                ip = comps[0];
                                port = comps.Length > 1 ? comps[1] : "100";
                            }
                            cardReader = _cardReaderService.GetCardReader(ip, port);
                        }
                        if (cardReader == null) return;

                        for (int i = 0; i < AvailableCardReaders.Count; ++i)
                            if (AvailableCardReaders[i] != null &&
                                AvailableCardReaders[i].SerialNumber.Equals(cardReader.SerialNumber))
                            {
                                AvailableCardReaders.RemoveAt(i);
                                break;
                            }
                        CardReaders.Add(cardReader);
                    }));
            }
        }
        private MvxCommand<CardReaderWrapper> _removeCardReader;
        private string currentCardType;

        public ICommand RemoveCardReader
        {
            get
            {
                return _removeCardReader ??
                       (_removeCardReader = new MvxCommand<CardReaderWrapper>(c =>
                       {
                           CardReaders.Remove(c);
                           AvailableCardReaders.Insert(0, c);
                       }));
            }
        }

        public InternetControl BarrierByZKTekco { get; private set; }

        void RawCardReader_ReadingCompleted(object sender, CardReaderEventArgs e)
        {
            string serialNo = e.CardReader.CardReaderInfo.SerialNumber;
            var cardReader = AvailableCardReaders.FirstOrDefault(c => c.SerialNumber.Equals(serialNo));
            CurrentCardReader = cardReader != null ? cardReader.SerialNumber : null;
        }
    }
}
