using Cirrious.MvvmCross.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Models
{
    public class FarCards : MvxNotifyPropertyChanged
    {
        private string _cardId;
        [JsonProperty("cardId")]
        public string CardId
        {
            get { return _cardId; }
            set
            {
                _cardId = value;
                RaisePropertyChanged(() => CardId);
            }
        }
    }
    //public class Region : MvxNotifyPropertyChanged
    //{
    //    private int _regionId;
    //    [JsonProperty("RegionId")]
    //    public int RegionId
    //    {
    //        get { return _regionId; }
    //        set
    //        {
    //            _regionId = value;
    //            RaisePropertyChanged(() => RegionId);
    //        }
    //    }
    //    private int? _parentId;
    //    [JsonProperty("ParentId")]
    //    public int? ParentId
    //    {
    //        get { return _parentId; }
    //        set
    //        {
    //            _parentId = value;
    //            RaisePropertyChanged(() => ParentId);
    //        }
    //    }
    //    private int _inOutClosed;
    //    [JsonProperty("InOutClosed")]
    //    public int InOutClosed
    //    {
    //        get { return _inOutClosed; }
    //        set
    //        {
    //            _inOutClosed = value;
    //            RaisePropertyChanged(() => InOutClosed);
    //        }
    //    }
    //    private int _indirect;
    //    [JsonProperty("Indirect")]
    //    public int Indirect
    //    {
    //        get { return _indirect; }
    //        set
    //        {
    //            _indirect = value;
    //            RaisePropertyChanged(() => Indirect);
    //        }
    //    }
    //    private string _regionName;
    //    [JsonProperty("RegionName")]
    //    public string RegionName
    //    {
    //        get { return _regionName; }
    //        set
    //        {
    //            _regionName = value;
    //            RaisePropertyChanged(() => RegionName);
    //        }
    //    }
    //    private string _parentName;
    //    [JsonProperty("ParentName")]
    //    public string ParentName
    //    {
    //        get { return _parentName; }
    //        set
    //        {
    //            _parentName = value;
    //            RaisePropertyChanged(() => ParentName);
    //        }
    //    }
    //    private string _note;
    //    [JsonProperty("Note")]
    //    public string Note
    //    {
    //        get { return _note; }
    //        set
    //        {
    //            _note = value;
    //            RaisePropertyChanged(() => Note);
    //        }
    //    }
    //}
    public class BlackNumber : MvxNotifyPropertyChanged
    {
        private string _number;
        [JsonProperty("vehicle_number")]
        public string Number
        {
            get { return _number; }
            set
            {
                _number = value;
                RaisePropertyChanged(() => Number);
            }
        }
    }

}
