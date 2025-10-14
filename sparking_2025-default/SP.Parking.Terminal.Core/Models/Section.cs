using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using Newtonsoft.Json;
using SP.Parking.Terminal.Core.Services;
using Green.Devices.CardReader;
using Green.Devices.Dal;
using System;
using System.Collections.Generic;
using System.Linq;


namespace SP.Parking.Terminal.Core.Models
{
    public class DisplayedInfo
    {
        public DisplayedPosition DisplayedPosition { get; set; }
        public bool ShouldBeDisplayed { get; set; }
    }

    public enum DisplayedPosition
    {
        Left = 0,
        Right,
		Middle,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        None
	}

    public enum SectionPosition
    {
        Lane1 = 0,
        Lane2,
        Lane3,
        Lane4,
        Admin = 999,
    }
    public enum LedStyle
    {
        Standard=0,//Used from GetWay apply
        SGCT=1,
        VietTell=2,
        Matrixs = 3
    }

    public interface ISection
    {
        InternetControl BarrierByInternetControl { get; set; }
        SiemensControl BarrierBySiemensControl { get; set; }
        bool UseBarrierIpController { get; set; }
        bool UseZKController { get; set; }
        string BarrierIpController { get; set; }
        ushort BarrierPortController { get; set; }
        string BarrierDoorsController { get; set; }
        string BarrierPort { get; set; }
        byte BarrierHardButtonCode { get; set; }
        int TimeTick { get; set; }
        string PrintAddressTitle { get; set; }
        string PrintCallTile { get; set; }
        
        //string PrintLogoPath { get; set; }
        bool PrintComActive { get; set; }
        string ComIctCashier { get; set; }
        bool ComIctCashierEnanble { get; set; }
        string ComAlarm { get; set; }
        string AlarmWarningKeys { get; set; }
        string AlarmSuccessKeys { get; set; }
        string ComPrint { get; set; }
        string ComLed { get; set; }
        string LedIP { get; set; }
        string ComCash { get; set; }
        string ComLedB { get; set; }
        string Door { get; set; }
        string Reader { get; set; }
        LedStyle LedOfKind { get; set; }
        IUserService UserService { get; }
        SectionPosition Id { get; set; }
        //DisplayedInfo DisplayedInfo { get; set; }
        bool ShouldBeDisplayed { get; set; }
        DisplayedPosition DisplayedPosition { get; set; }
		OptionByLane OptionByLane {  get; set; }
		string LaneName { get; set; }
        List<Camera> Cameras { get; set; }
        bool IsConfigured { get; set; }
        List<CardReaderWrapper> CardReaders { get; set; }
        //VehicleType VehicleType { get; set; }
        int VehicleTypeId { get; set; }
        IBarrierDevice Barrier { get; set; }
        LaneDirection Direction { get; set; }
        LaneDirection TemporaryDirection { get; set; }
        Camera FrontInCamera { get; }
        Camera FrontOutCamera { get; }
        Camera BackInCamera { get; }
        Camera BackOutCamera { get; }
        //Camera FrontCamera { get; }
        //Camera BackCamera { get; }
        Lane Lane { get; set; }
        /// <summary>
        /// Cấu hình lại đầu đọc và thêm camera mở rộng - 2018Jul25
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="direction"></param>
        /// <param name="cameraPosition"></param>
        bool IsInExtra { get; set; }
        bool IsOutExtra { get; set; }
        Camera ExtraIn1Camera { get; }
        Camera ExtraOut1Camera { get; }
        Camera ExtraIn2Camera { get; }
        Camera ExtraOut2Camera { get; }
        List<IGreenCardReaderInfo> ModWinsCards
        {
            get; set;
        }
        List<IGreenCardReaderInfo> TcpIpServerCards
        {
            get; set;
        }
        List<IGreenCardReaderInfo> TcpIpClientCards
        {
            get; set;
        }
        List<IGreenCardReaderInfo> ScannelCards
        {
            get; set;
        }
        List<IGreenCardReaderInfo> TcpIpControllerCards
        {
            get; set;
        }
        List<IGreenCardReaderInfo> TcpIpRemodeCards
        {
            get; set;
        }

        List<IGreenCardReaderInfo> NFCCards
        {
            get; set;
        }

        List<IGreenCardReaderInfo> ProxiesCards
        {
            get; set;
        }

         List<IGreenCardReaderInfo> ZKFarCards { get; set; }

        void AttachCamera(string ip, string port, string username, string password, int channel, LaneDirection direction, CameraPosition cameraPosition, CameraType cameraType, string waytype);
        void AttachCardReader(CardReaderWrapper cardReader);
        void AttachCardReader(string serialNo);
        void SetupDevice(LaneDirection direction);
        void SetupCameras(LaneDirection direction);
        void SetupCardReader();
        bool StopCardReader(CardReaderEventHandler read, CardReaderEventHandler takeoff);
        bool StartCardReader(CardReaderEventHandler read, CardReaderEventHandler takeoff);
        void StartCameras(LaneDirection direction);
        void StopDevices();
        void savecamconfig();
        void StopDevices(LaneDirection direction);
        void savekeysmap();
        KeyMap KeyMap { get; set; }
    }

    public class Section : MvxNotifyPropertyChanged, ISection
    {
        public string PrintAddressTitle { get; set; }
        public string PrintCallTile { get; set; }
        //public string PrintLogoPath { get; set; }
        public bool PrintComActive { get; set; }
        public string ComAlarm { get; set; }
        public string ComIctCashier { get; set; }
        public bool ComIctCashierEnanble { get; set; }
        public string AlarmWarningKeys { get; set; }
        public string AlarmSuccessKeys { get; set; }
        public string ComPrint { get; set; }
        public string ComLed { get; set; }
        public string LedIP { get; set; }
        public string ComCash { get; set; }
        public string ComLedB { get; set; }
        public string Door { get; set; }
        public string Reader { get; set; }
        public bool UseBarrierIpController { get; set; }
        public string BarrierIpController { get; set; }
        public ushort BarrierPortController { get; set; }
        public string BarrierDoorsController { get; set; }
        public byte BarrierHardButtonCode { get; set; }
        public int TimeTick { get; set; }
        public LedStyle LedOfKind { get; set; }
        IUserPreferenceService _userPreferenceService;
        IHostSettings _hostSettings;
        IResourceLocatorService _resourceLocatorService;
        IResourceLocatorService ResourceLocatorService
        {
            get 
            {
                return _resourceLocatorService = _resourceLocatorService ?? Mvx.Resolve<IResourceLocatorService>();
            }
        }

        [JsonIgnore]
        public IUserService UserService { get { return Mvx.Resolve<IUserServiceLocator>().GetUserService(this.Id); } }
        public OptionByLane OptionByLane {  get; set; }
		public string LaneName
        {
            get { return Lane != null ? Lane.Name : null; }
            set
            {
                if (Lane != null)
                    Lane.Name = value;
            }
        }
        public SectionPosition Id { get; set; }
        //public DisplayedInfo DisplayedInfo { get; set; }
        public DisplayedPosition DisplayedPosition { get; set; }
        public bool ShouldBeDisplayed { get; set; }
        public bool IsConfigured { get; set; }

        public List<CardReaderWrapper> CardReaders { get; set; }

        public Lane Lane { get; set; }

        [JsonIgnore]
        private List<Camera> _cameras;
        public List<Camera> Cameras
        {
            get
            {
                if(_cameras == null)
                    _cameras = new List<Camera>();
                return _cameras;
            }
            set
            {
                if (_cameras == value) return;
                _cameras = value;
            }
        }

        //public VehicleType VehicleType
        //{
        //    get { return Lane != null ? Lane.VehicleType : VehicleType.Bike; }
        //    set
        //    {
        //        if (Lane != null)
        //            Lane.VehicleType = value;
        //    }
        //}

        [JsonIgnore]
        public int VehicleTypeId
        {
            get { return Lane != null ? Lane.VehicleTypeId : 2; }
            set
            {
                if (Lane != null)
                    Lane.VehicleTypeId = value;
            }
        }


        public float TotalAmount { get; set; }
		public string BarrierName { get; set; }
        public string BarrierPort { get; set; }
        public string BarrierPort2 { get; set; }

		[JsonIgnore]
		public IBarrierDevice Barrier { get; set; }
        
        [JsonIgnore]
        public LaneDirection Direction
        {
            get { return Lane != null ? Lane.Direction : LaneDirection.In; }
            set
            {
                if (Lane != null)
                {
                    Lane.Direction = value;
                    TemporaryDirection = value;
                }
            }
        }

        private LaneDirection _temporaryDirection;
        [JsonIgnore]
        public LaneDirection TemporaryDirection
        {
            get
            {
                if (_temporaryDirection == LaneDirection.Unknown) return this.Direction;
                else return _temporaryDirection;
            }
            set
            {
                _temporaryDirection = value;
            }
        }

        [JsonIgnore]
        public Camera FrontInCamera
        {
            get { return Cameras.Where(camera => camera.Direction == LaneDirection.In && camera.Position == CameraPosition.Front).FirstOrDefault(); }
        }

        [JsonIgnore]
        public Camera FrontOutCamera
        {
            get { return Cameras.Where(camera => camera.Direction == LaneDirection.Out && camera.Position == CameraPosition.Front).FirstOrDefault(); }
        }

        [JsonIgnore]
        public Camera BackInCamera
        {
            get { return Cameras.Where(camera => camera.Direction == LaneDirection.In && camera.Position == CameraPosition.Back).FirstOrDefault(); }
        }

        [JsonIgnore]
        public Camera BackOutCamera
        {
            get { return Cameras.Where(camera => camera.Direction == LaneDirection.Out && camera.Position == CameraPosition.Back).FirstOrDefault(); }
        }
        /// <summary>
        /// Mở rộng Camera và đầu đọc tầm xa - 2018Jul25
        /// </summary>
        
        
        public bool IsInExtra { get; set; }
   
        public bool IsOutExtra { get; set; }
 
        public List<IGreenCardReaderInfo> ModWinsCards
        {
            get; set;
        }
      
        public List<IGreenCardReaderInfo> TcpIpServerCards
        {
            get; set;
        }
    
        public List<IGreenCardReaderInfo> TcpIpClientCards
        {
            get; set;
        }
  
        public List<IGreenCardReaderInfo> TcpIpRemodeCards
        {
            get; set;
        }
        public List<IGreenCardReaderInfo> TcpIpControllerCards
        {
            get; set;
        }
        public List<IGreenCardReaderInfo> ScannelCards
        {
            get; set;
        }

        [JsonIgnore]
        public Camera ExtraIn1Camera
        {
            get { return Cameras.Where(camera => camera.Direction == LaneDirection.In && camera.Position == CameraPosition.Extra1).FirstOrDefault(); }
        }
        [JsonIgnore]
        public Camera ExtraIn2Camera
        {
            get { return Cameras.Where(camera => camera.Direction == LaneDirection.In && camera.Position == CameraPosition.Extra2).FirstOrDefault(); }
        }
        [JsonIgnore]
        public Camera ExtraOut1Camera
        {
            get { return Cameras.Where(camera => camera.Direction == LaneDirection.Out && camera.Position == CameraPosition.Extra1).FirstOrDefault(); }
        }
        [JsonIgnore]
        public Camera ExtraOut2Camera
        {
            get { return Cameras.Where(camera => camera.Direction == LaneDirection.Out && camera.Position == CameraPosition.Extra2).FirstOrDefault(); }
        }
        public Camera GetExtra1Camera(LaneDirection direction)
        {
            return Cameras.Where(camera => camera.Direction == direction && camera.Position == CameraPosition.Extra1).FirstOrDefault();
        }

        public Camera GetExtra2Camera(LaneDirection direction)
        {
            return Cameras.Where(camera => camera.Direction == direction && camera.Position == CameraPosition.Extra2).FirstOrDefault();
        }
        /// <summary>
        /// 2018Jul25
        /// </summary>
       
        public Camera GetFrontCamera(LaneDirection direction)
        {
            return Cameras.Where(camera => camera.Direction == direction && camera.Position == CameraPosition.Front).FirstOrDefault();
        }

        public Camera GetBackCamera(LaneDirection direction)
        {
            return Cameras.Where(camera => camera.Direction == direction && camera.Position == CameraPosition.Back).FirstOrDefault();
        }
        
        [JsonIgnore]
        KeyMap _keyMap;
        public KeyMap KeyMap
        {
            get { return _keyMap=  _keyMap ?? new KeyMap(this.Id); }
            set { _keyMap = value; }
        }

        public InternetControl BarrierByInternetControl { get ; set; }
        public SiemensControl BarrierBySiemensControl { get; set; }
        public InternetControl BarrierByZKTekco { get; set; }
        public List<IGreenCardReaderInfo> NFCCards { get; set; }
        public List<IGreenCardReaderInfo> ProxiesCards { get; set; }
        public List<IGreenCardReaderInfo> ZKFarCards { get; set; }
        public bool UseZKController { get; set; }

        public Section()
        {
            this.TemporaryDirection = LaneDirection.Unknown;
			
			//DisplayedInfo = new DisplayedInfo();
		}

        public Section(SectionPosition pos)
            : this()
        {
			if (_hostSettings == null)
				_hostSettings = Mvx.Resolve<IHostSettings>();

			Id = pos;

			int iPos = (int)pos;
			if (iPos % 2 == 0)
				DisplayedPosition = DisplayedPosition.Left;
			else
				DisplayedPosition = DisplayedPosition.Right;

			Lane = new Lane();
			KeyMap = new KeyMap(pos);
			Lane.Enabled = true;
			Lane.Name = null;// 
			if (pos == SectionPosition.Lane1 || pos == SectionPosition.Lane2 || pos == SectionPosition.Lane3 || pos == SectionPosition.Lane4)
			// 
			{
				IsConfigured = true;
				if (pos == SectionPosition.Lane3)
				{
					KeyMap = new KeyMap(SectionPosition.Lane1);
				}
				else if (pos == SectionPosition.Lane4)
				{
					KeyMap = new KeyMap(SectionPosition.Lane2);
				}
				else
				{
					ShouldBeDisplayed = true;
				}
			}
			else
			{
				DisplayedPosition = DisplayedPosition.None;
			}
		}

        private void InitActualTwo(SectionPosition pos)
        {
			Id = pos;

			int iPos = (int)pos;
			if (iPos % 2 == 0)
				DisplayedPosition = DisplayedPosition.Left;
			else
				DisplayedPosition = DisplayedPosition.Right;

			Lane = new Lane();
			KeyMap = new KeyMap(pos);
			Lane.Enabled = true;
			Lane.Name = null;// 
			if (pos == SectionPosition.Lane1 || pos == SectionPosition.Lane2 || pos == SectionPosition.Lane3 || pos == SectionPosition.Lane4)
			// 
			{
				IsConfigured = true;
				if (pos == SectionPosition.Lane3)
				{
					KeyMap = new KeyMap(SectionPosition.Lane1);
				}
				else if (pos == SectionPosition.Lane4)
				{
					KeyMap = new KeyMap(SectionPosition.Lane2);
				}
				else
				{
					ShouldBeDisplayed = true;
				}
			}
			else
			{
				DisplayedPosition = DisplayedPosition.None;
			}
		}
		private void InitActualThree(SectionPosition pos)
		{
			Id = pos;
			int iPos = (int)pos;
            if(iPos == 0)
            {
				DisplayedPosition = DisplayedPosition.Left;
			}
            else if(iPos == 1)
            {
				DisplayedPosition = DisplayedPosition.Middle;
			}   
            else if(iPos == 2)
            {
                DisplayedPosition = DisplayedPosition.Right;
            }
            else
            {
                DisplayedPosition = DisplayedPosition.None;
            }
			Lane = new Lane();
			KeyMap = new KeyMap(pos);
			Lane.Enabled = true;
			Lane.Name = null;// 
			if (pos == SectionPosition.Lane1 || pos == SectionPosition.Lane2 || pos == SectionPosition.Lane3 || pos == SectionPosition.Lane4)
			// 
			{
				IsConfigured = true;
				if (pos == SectionPosition.Lane4)
				{
					KeyMap = new KeyMap(SectionPosition.Lane2);
				}
				else
				{
					ShouldBeDisplayed = true;
				}
			}
			else
			{
				DisplayedPosition = DisplayedPosition.None;
			}
		}
		private void InitActualFour(SectionPosition pos)
		{
			Id = pos;

			int iPos = (int)pos;
            if (iPos == 0)
            {
                DisplayedPosition = DisplayedPosition.TopLeft;
            }
            else if (iPos == 1)
            {
                DisplayedPosition = DisplayedPosition.TopRight;
            }
            else if (iPos == 2) {
                DisplayedPosition = DisplayedPosition.BottomLeft;
            }
            else if (iPos == 3){
                DisplayedPosition = DisplayedPosition.BottomRight;
            }
            else
            {
                DisplayedPosition = DisplayedPosition.None;
            }
			
			Lane = new Lane();
			KeyMap = new KeyMap(pos);
			Lane.Enabled = true;
			Lane.Name = null;// 
			if (pos == SectionPosition.Lane1 || pos == SectionPosition.Lane2 || pos == SectionPosition.Lane3 || pos == SectionPosition.Lane4)
			{
				IsConfigured = true;
				ShouldBeDisplayed = true;
			}
			else
			{
				DisplayedPosition = DisplayedPosition.None;
			}
		}
		public void AttachCamera(string ip, string port, string username, string password, int channel, LaneDirection direction, CameraPosition cameraPosition, CameraType cameratype, string waytype)
        {
            Camera camera = Cameras.Where(c => c.Direction == direction && c.Position == cameraPosition).FirstOrDefault();

            if (camera == null)
            {
                camera = new Camera();
                this.Cameras.Add(camera);
            }
            camera.IP = ip;
            camera.Port = port;
            camera.UserName = username;
            camera.Password = password;
            camera.LaneId = this.Lane.Id;
            camera.Position = cameraPosition;
            camera.Direction = direction;
            camera.CameraType = cameratype;
            camera.WayType = waytype;
            camera.Channel = channel;
        }

        void camera_OnZoomReceived(object sender, ZoomEventArgs e)
        {
            //if (_userPreferenceService == null)
            //    _userPreferenceService = Mvx.Resolve<IUserPreferenceService>();
            //_userPreferenceService.SystemSettings.Save();
        }
        public void savecamconfig()
        {	
			AttachZoom();
			AttachTracker();
			if (_userPreferenceService == null)
                _userPreferenceService = Mvx.Resolve<IUserPreferenceService>();
            _userPreferenceService.SystemSettings.UpdateSection(this);
            _userPreferenceService.SystemSettings.MarkChanged();
            _userPreferenceService.SystemSettings.Save();
        }
        public void savekeysmap()
        {
            if (_userPreferenceService == null)
                _userPreferenceService = Mvx.Resolve<IUserPreferenceService>();
           
            _userPreferenceService.SystemSettings.MarkChanged();
            _userPreferenceService.SystemSettings.Save();
        }
		void AttachTracker()
		{
			if (this.FrontInCamera != null && this.FrontInCamera.RawCamera != null)
				this.FrontInCamera.TrackerFactor = this.FrontInCamera.RawCamera.TrackerFactor;
			if (this.FrontOutCamera != null && this.FrontOutCamera.RawCamera != null)
				this.FrontOutCamera.TrackerFactor = this.FrontOutCamera.RawCamera.TrackerFactor;
			if (this.BackInCamera != null && this.BackInCamera.RawCamera != null)
				this.BackInCamera.TrackerFactor = this.BackInCamera.RawCamera.TrackerFactor;
			if (this.BackOutCamera != null && this.BackOutCamera.RawCamera != null)
				this.BackOutCamera.TrackerFactor = this.BackOutCamera.RawCamera.TrackerFactor;
			if (this.ExtraIn1Camera != null && this.ExtraIn1Camera.RawCamera != null)
				this.ExtraIn1Camera.TrackerFactor = this.ExtraIn1Camera.RawCamera.TrackerFactor;
			if (this.ExtraIn2Camera != null && this.ExtraIn2Camera.RawCamera != null)
				this.ExtraIn2Camera.TrackerFactor = this.ExtraIn2Camera.RawCamera.TrackerFactor;
			if (this.ExtraOut1Camera != null && this.ExtraOut1Camera.RawCamera != null)
				this.ExtraOut1Camera.TrackerFactor = this.ExtraOut1Camera.RawCamera.TrackerFactor;
			if (this.ExtraOut2Camera != null && this.ExtraOut2Camera.RawCamera != null)
				this.ExtraOut2Camera.TrackerFactor = this.ExtraOut2Camera.RawCamera.TrackerFactor;
		}
		void AttachZoom()
        {
            if (this.FrontInCamera != null && this.FrontInCamera.RawCamera != null)
                this.FrontInCamera.ZoomFactor = this.FrontInCamera.RawCamera.ZoomFactor;
            if (this.FrontOutCamera != null && this.FrontOutCamera.RawCamera != null)
                this.FrontOutCamera.ZoomFactor = this.FrontOutCamera.RawCamera.ZoomFactor;
            if (this.BackInCamera != null && this.BackInCamera.RawCamera != null)
                this.BackInCamera.ZoomFactor = this.BackInCamera.RawCamera.ZoomFactor;
            if (this.BackOutCamera != null && this.BackOutCamera.RawCamera != null)
                this.BackOutCamera.ZoomFactor = this.BackOutCamera.RawCamera.ZoomFactor;
            if (this.ExtraIn1Camera != null && this.ExtraIn1Camera.RawCamera != null)
                this.ExtraIn1Camera.ZoomFactor = this.ExtraIn1Camera.RawCamera.ZoomFactor;
            if (this.ExtraIn2Camera != null && this.ExtraIn2Camera.RawCamera != null)
                this.ExtraIn2Camera.ZoomFactor = this.ExtraIn2Camera.RawCamera.ZoomFactor;
            if (this.ExtraOut1Camera != null && this.ExtraOut1Camera.RawCamera != null)
                this.ExtraOut1Camera.ZoomFactor = this.ExtraOut1Camera.RawCamera.ZoomFactor;
            if (this.ExtraOut2Camera != null && this.ExtraOut2Camera.RawCamera != null)
                this.ExtraOut2Camera.ZoomFactor = this.ExtraOut2Camera.RawCamera.ZoomFactor;
        }
        public void AttachCardReader(CardReaderWrapper cardReader)
        {
            if (CardReaders == null)
                CardReaders = new List<CardReaderWrapper>();
            if (cardReader != null)
                CardReaders.Add(cardReader);
        }

        public void AttachCardReader(string serialNo)
        {
            if (CardReaders == null)
                CardReaders = new List<CardReaderWrapper>();

            var cardReader = ResourceLocatorService.SetupCardReader(serialNo);
            CardReaders.Add(cardReader);
        }

        public void AttachCardReader(string ip, string port)
        {
            if (CardReaders == null)
                CardReaders = new List<CardReaderWrapper>();

            var cardReader = ResourceLocatorService.SetupCardReader(ip, port);
            CardReaders.Add(cardReader);
        }

        public void StartCameras(LaneDirection direction)
        {
            try
            {
                GetFrontCamera(direction).Start();
                GetBackCamera(direction).Start();
                var c1 = GetExtra1Camera(direction);
                if (c1 != null)
                    c1.Start();
                var c2 = GetExtra2Camera(direction);
                if (c2 != null)
                    c2.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public bool StopCardReader(CardReaderEventHandler read, CardReaderEventHandler takeoff)
        {
            if (CardReaders == null || CardReaders.Count == 0)
            {
                Console.WriteLine("dont have card reader");
                return false;
            }

            foreach (var cardReader in CardReaders)
                if (cardReader != null)
                {
                    cardReader.ReadingCompleted -= read;
                    cardReader.TakingOffCompleted -= takeoff;
                }

            return true;
        }

        public bool StartCardReader(CardReaderEventHandler read, CardReaderEventHandler takeoff)
        {
            if (CardReaders == null || CardReaders.Count == 0)
            {
                Console.WriteLine("dont have card reader");
                return false;
            }

            foreach (var cardReader in CardReaders)
                if (cardReader != null)
                {
                    cardReader.ReadingCompleted += read;
                    cardReader.TakingOffCompleted += takeoff;
                }

            return true;
        }

        public void StopDevices()
        {
            try
            {
                StopDevices(this.Direction);
                //FrontCamera.Stop();
                //BackCamera.Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void StopDevices(LaneDirection direction)
        {
            try
            {
                GetFrontCamera(direction).Stop();
                GetBackCamera(direction).Stop();
                var c1 = GetExtra1Camera(direction);
                if (c1 != null)
                    c1.Stop();
                var c2 = GetExtra2Camera(direction);
                if (c2 != null)
                    c2.Stop();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        CameraType _preCameraType;
        public void SetupCameras(LaneDirection direction)
        {
            IOptionsSettings _optionSettings = Mvx.Resolve<IOptionsSettings>();  
            Camera frontCamera = GetFrontCamera(direction);
            if (frontCamera != null)
            {
                //frontCamera.Setup(_optionSettings.CameraType, _optionSettings.Zoomable, _optionSettings.CameraType != _preCameraType);
                //frontCamera.Setup(frontCamera.CameraType, _optionSettings.Zoomable, frontCamera.CameraType != _preCameraType);
                frontCamera.OnZoomReceived += camera_OnZoomReceived;
                frontCamera.Zoomable = _optionSettings.Zoomable;
            }

            Camera backCamera = GetBackCamera(direction);
            if (backCamera != null)
            {
                //backCamera.Setup(_optionSettings.CameraType, _optionSettings.Zoomable, _optionSettings.CameraType != _preCameraType);
                //backCamera.Setup(backCamera.CameraType, _optionSettings.Zoomable, backCamera.CameraType != _preCameraType);
                backCamera.OnZoomReceived += camera_OnZoomReceived;
                backCamera.Zoomable = _optionSettings.Zoomable;
            }
            Camera extra1Camera = GetExtra1Camera(direction);
            if (extra1Camera != null)
            {
                //extra1Camera.Setup(_optionSettings.CameraType, _optionSettings.Zoomable, _optionSettings.CameraType != _preCameraType);
                //extra1Camera.Setup(extra1Camera.CameraType, _optionSettings.Zoomable, extra1Camera.CameraType != _preCameraType);
                extra1Camera.OnZoomReceived += camera_OnZoomReceived;
                extra1Camera.Zoomable = _optionSettings.Zoomable;
            }
            Camera extra2Camera = GetExtra2Camera(direction);
            if (extra2Camera != null)
            {
                //extra2Camera.Setup(_optionSettings.CameraType, _optionSettings.Zoomable, _optionSettings.CameraType != _preCameraType);
                //extra2Camera.Setup(extra2Camera.CameraType, _optionSettings.Zoomable, extra2Camera.CameraType != _preCameraType);
                extra2Camera.OnZoomReceived += camera_OnZoomReceived;
                extra2Camera.Zoomable = _optionSettings.Zoomable;
            }
            _preCameraType = _optionSettings.CameraType;
        }

        public void SetupCardReader()
        {
            if (CardReaders != null)
            {
                for (int i = 0; i < CardReaders.Count; ++i)
                    if (CardReaders[i] != null)
                    {
                        var info = CardReaders[i].CardReaderInfo as ProlificCardReaderInfo;
                        if (info != null)
                            CardReaders[i] = ResourceLocatorService.SetupCardReader(info.IP, info.Port);
                        else
                            CardReaders[i] = ResourceLocatorService.SetupCardReader(CardReaders[i].SerialNumber);
                    }
            }
        }

		public void SetupBarrier()
		{
            //if (!string.IsNullOrEmpty(BarrierName))
            ResourceLocatorService.SetupBarrier(this, BarrierName, BarrierPort);
		}

        public void SetupDevice(LaneDirection direction)
        {
            this.SetupCameras(direction);
            ///old
            //this.SetupCardReader();
			this.SetupBarrier();
        }

        public void SaveStates()
        {
            foreach (var cam in this.Cameras)
                cam.SaveStates();
        }
    }
}
