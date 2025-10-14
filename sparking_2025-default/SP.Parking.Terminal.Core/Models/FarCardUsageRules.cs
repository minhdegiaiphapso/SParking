using Cirrious.MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Models
{
	public class FarCardUsageRules: MvxNotifyPropertyChanged
	{
		private bool _checkWithAvalableCardFirst;
		private bool _allowCollectFarCard;
		private bool _combileWithRecognitePlateCheck;
		private bool _checkMultiResultAsTheSameTime;
		private bool _showPoupForChoseWhenMultiResult;
		private bool _ignoreWhenMultiResult;
		private int _cycleTimeForCheckResult = 1;
		private int _timoutForPopupResult = 5;
		private int _ignoredDurationForProcessedTags = 5;
		public bool CheckWithAvalableCardFirst
		{
			get { return _checkWithAvalableCardFirst; }
			set
			{
				if (_checkWithAvalableCardFirst == value)
					return;
				_checkWithAvalableCardFirst = value;
				RaisePropertyChanged(() => CheckWithAvalableCardFirst);
			}
		}

		public bool AllowCollectFarCard
		{
			get { return _allowCollectFarCard; }
			set
			{
				if (_allowCollectFarCard == value)
					return;
				_allowCollectFarCard = value;
				RaisePropertyChanged(() => AllowCollectFarCard);
			}
		}

		public bool CombileWithRecognitePlateCheck
		{
			get { return _combileWithRecognitePlateCheck; }
			set
			{
				if (_combileWithRecognitePlateCheck == value)
					return;
				_combileWithRecognitePlateCheck = value;
				RaisePropertyChanged(() => CombileWithRecognitePlateCheck);
			}
		}
		public bool IgnoreWhenMultiResult
		{
			get { return _ignoreWhenMultiResult; }
			set
			{
				if (_ignoreWhenMultiResult == value)
					return;
				_ignoreWhenMultiResult = value;
				RaisePropertyChanged(() => IgnoreWhenMultiResult);
				RaisePropertyChanged(() => CheckMultiResultAsTheSameTime);
				RaisePropertyChanged(() => ShowPoupForChoseWhenMultiResult);
			}
		}
		public bool CheckMultiResultAsTheSameTime
		{
			get { return _checkMultiResultAsTheSameTime && !_ignoreWhenMultiResult; }
			set
			{
				if (_checkMultiResultAsTheSameTime == value)
					return;
				_checkMultiResultAsTheSameTime = value;
				RaisePropertyChanged(() => CheckMultiResultAsTheSameTime);
			}
		}

		public bool ShowPoupForChoseWhenMultiResult
		{
			get { return _showPoupForChoseWhenMultiResult && !_ignoreWhenMultiResult; }
			set
			{
				if (_showPoupForChoseWhenMultiResult == value)
					return;
				_showPoupForChoseWhenMultiResult = value;
				RaisePropertyChanged(() => ShowPoupForChoseWhenMultiResult);
			}
		}

		public int CycleTimeForCheckResult 
		{
			get { return _cycleTimeForCheckResult; }
			set
			{
				if (_cycleTimeForCheckResult == value)
					return;
				_cycleTimeForCheckResult = value;
				RaisePropertyChanged(() => CycleTimeForCheckResult);
			}
		}
		public int TimoutForPopupResult
		{
			get { return _timoutForPopupResult; }
			set
			{
				if (_timoutForPopupResult == value)
					return;
				_timoutForPopupResult = value;
				RaisePropertyChanged(() => TimoutForPopupResult);
			}
		}

		public int IgnoredDurationForProcessedTags
		{
			get { return _ignoredDurationForProcessedTags; }
			set
			{
				if (_ignoredDurationForProcessedTags == value)
					return;
				_ignoredDurationForProcessedTags = value;
				RaisePropertyChanged(() => IgnoredDurationForProcessedTags);
			}
		}
	}
}
