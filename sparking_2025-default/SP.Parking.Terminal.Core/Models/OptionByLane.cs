using Cirrious.MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Models
{
	public enum UsageCameraTrackerMethod
	{
		Unusage = 0,
		AllowInOutWhenTrackerCamBackOnRedAndBlue = 1,
		AllowInOutWhenTrackerCamBackOnBlue = 2,
		AllowInOutWhenTrackerCamFrontOnRedAndBlue = 3,
		AllowInOutWhenTrackerCamFrontOnBlue = 4,
	}
	public class OptionByLane: MvxNotifyPropertyChanged
	{
		private bool _autoInByVehicleRecognition;
		private bool _autoOutByVehicleRecognition;
		private bool _allowConfirmToCollectVisitorPlate;
		private bool _compareWithOnlySerialNumberOfPlate;

		private int _amountSecondsDelayForNext = 2;
		private int _ignoredDurationForProcessedPlate= 5;
		

		private UsageCameraTrackerMethod _methodTracker;

		public bool AutoInByVehicleRecognition
		{
			get { return _autoInByVehicleRecognition; }
			set
			{
				if (_autoInByVehicleRecognition == value)
					return;
				_autoInByVehicleRecognition = value;
				RaisePropertyChanged(() => AutoInByVehicleRecognition);
				RaisePropertyChanged(() => AllowConfirmToCollectVisitorPlate);
			}
		}

		public bool AutoOutByVehicleRecognition
		{
			get { return _autoOutByVehicleRecognition; }
			set
			{
				if (_autoOutByVehicleRecognition == value)
					return;
				_autoOutByVehicleRecognition = value;
				RaisePropertyChanged(() => AutoOutByVehicleRecognition);
			}
		}

		public bool CompareWithOnlySerialNumberOfPlate
		{
			get { return _compareWithOnlySerialNumberOfPlate; }
			set
			{
				if (_compareWithOnlySerialNumberOfPlate == value)
					return;
				_compareWithOnlySerialNumberOfPlate = value;
				RaisePropertyChanged(() => CompareWithOnlySerialNumberOfPlate);
			}
		}

		public bool AllowConfirmToCollectVisitorPlate
		{
			get { return _autoInByVehicleRecognition && _allowConfirmToCollectVisitorPlate; }
			set
			{
				if (_allowConfirmToCollectVisitorPlate == value)
					return;
				_allowConfirmToCollectVisitorPlate = value;
				RaisePropertyChanged(() => AllowConfirmToCollectVisitorPlate);
			}
		}

		public int IgnoredDurationForProcessedPlate
		{
			get { return _ignoredDurationForProcessedPlate; }
			set
			{
				if (_ignoredDurationForProcessedPlate == value)
					return;
				_ignoredDurationForProcessedPlate = value;
				RaisePropertyChanged(() => IgnoredDurationForProcessedPlate);
			}
		}

		public int AmountSecondsDelayForNext
		{
			get { return _amountSecondsDelayForNext; }
			set
			{
				if (_amountSecondsDelayForNext == value)
					return;
				_amountSecondsDelayForNext = value;
				RaisePropertyChanged(() => AmountSecondsDelayForNext);
			}
		}
		public UsageCameraTrackerMethod MethodTracker
		{
			get { return _methodTracker; }
			set
			{
				if (_methodTracker == value)
					return;
				_methodTracker = value;
				RaisePropertyChanged(() => MethodTracker);
			}
		}
		public List<UsageCameraTrackerMethod> TrackerMethods
		{
			get { return Enum.GetValues(typeof(UsageCameraTrackerMethod)).Cast<UsageCameraTrackerMethod>().ToList<UsageCameraTrackerMethod>(); }
		}
	}
}
