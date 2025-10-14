using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SP.Parking.Terminal.Core.Models;
using Green.Devices.Dal;
using Green.Devices.Vivotek;
using Cirrious.CrossCore;
using SP.Parking.Terminal.Core.ViewModels;

namespace SP.Parking.Terminal.Core.Services
{
    //public class SectionManager
    //{
    //    static SectionManager _instance;
    //    public static SectionManager instance
    //    {
    //        get
    //        {
    //            if (_instance == null)
    //                _instance = new SectionManager();

    //            return _instance;
    //        }
    //    }
        

    //    Dictionary<SectionPosition, Section> Sections { get; set; }

    //    IBaseSettings _systemSettings;

    //    public SectionManager()
    //    {
    //        _systemSettings = Mvx.Resolve<ISystemSettings>();
    //    }

    //    public void ChangeLaneDirection(SectionPosition sectionId, LaneDirection direction)
    //    {
    //        Sections[sectionId].Direction = direction;
    //        _systemSettings.MarkChanged();
    //    }

    //    public void AddSection(Section section)
    //    {
    //        Sections.Add(section.Id, section);
    //        _systemSettings.MarkChanged();
    //    }

    //    public void UpdateSection(Section section)
    //    {
    //        if (!Sections.ContainsKey(section.Id))
    //            return;

    //        Sections[section.Id] = section;
    //        _systemSettings.MarkChanged();
    //    }

    //    public List<Section> GetAllSections()
    //    {
    //        if (Sections.Count == 0)
    //        {
    //            Section sec = new Section();
    //            Lane lane = new Lane();
    //            sec.Lane = lane;
    //            sec.Direction = LaneDirection.In;
    //            sec.Id = SectionPosition.Lane1;
    //            Sections.Add(SectionPosition.Lane1, sec);
    //        }

    //        return Sections.Where(sec => sec.Value.IsConfigured == true).Select(sec => sec.Value).ToList();
    //    }

    //    public List<Section> GetAllSections(DisplayedPosition pos)
    //    {
    //        return Sections.Where(sec => sec.Value.DisplayedPosition == pos).Select(sec => sec.Value).ToList();
    //    }

    //    public void ChangeDisplayedPositionLane(SectionPosition sectionId, DisplayedPosition pos)
    //    {
    //        Sections[sectionId].Direction = direction;
    //        _systemSettings.MarkChanged();
    //    }
    //}

    public class SystemSettingsData : BaseSettingsData
    {
        public Dictionary<SectionPosition, Section> Sections { get; set; }
        public string OtherTerminalIPs { get; set; }
        public int CameraPort { get; set; }
    }

    public interface ISystemSettings : IBaseSettings
    {
        int NumberOfDisplayedLane { get; }
        Dictionary<SectionPosition, Section> Sections { get; set; }
        string OtherTerminalIPs { get; set; }
        int CameraPort { get; set; }
        void ChangeLaneDirection(SectionPosition sectionId, LaneDirection direction);
        void AddSection(Section section);
        void UpdateSection(Section section);
        List<Section> GetAllSections();
        List<Section> GetAllSections(DisplayedPosition pos);
        void ChangeDisplayedPositionLane(SectionPosition sectionId, DisplayedPosition pos);
    }

    public class SystemSettings : BaseSettings<SystemSettingsData>, ISystemSettings
    {
        public int NumberOfDisplayedLane { get { return 2; } }

        public Dictionary<SectionPosition, Section> Sections
        {
            get { return _data.Sections; }
            set
            {
                //try
                {
                    _data.Sections = value;
                    //MarkChanged();
                }
                //catch (Exception ex)
                //{
                    
                //    throw ex;
                //}
            }
        }

        public string OtherTerminalIPs
        {
            get { return _data.OtherTerminalIPs; }
            set {
                _data.OtherTerminalIPs = value; //MarkChanged();
            }
        }

        public int CameraPort
        {
            get { return _data.CameraPort; }
            set {
                _data.CameraPort = value;
                //MarkChanged();
            }
        }

        public SystemSettings(ArgumentParameter argParams = null)
            : base(argParams)
        {
            if (!HasLocal)
            {
                Sections = new Dictionary<SectionPosition, Section>();
                SectionPosition[] items = (SectionPosition[])Enum.GetValues(typeof(SectionPosition));
                foreach (SectionPosition pos in items)
                {
                    Section section = new Section(pos);
                    //Section section = Mvx.IocConstruct<Section>();
                    //section.Id = pos;
                    
                    //int iPos = (int)pos;
                    //if (iPos % 2 == 0)
                    //    section.DisplayedPosition = DisplayedPosition.Left;
                    //else
                    //    section.DisplayedPosition = DisplayedPosition.Right;

                    //section.Lane = new Lane();
                    //section.KeyMap = new KeyMap(pos);
                    ////section.Lane.Direction = section.Direction;
                    //section.Lane.Enabled = true;
                    ////section.Lane.VehicleType = VehicleType.Bike;
                    //section.Lane.Name = null;
                    //if (pos == SectionPosition.Lane1 || pos == SectionPosition.Lane2 || pos == SectionPosition.Lane3)
                    //{
                    //    section.IsConfigured = true;
                    //    if (pos != SectionPosition.Lane3)
                    //        section.ShouldBeDisplayed = true;
                        
                    //}
                    Sections.Add(pos, section);
                }
            }
        }

        //public SystemSettings()
        //    : base()
        //{
        //    if (!HasLocal)
        //    {
        //        Sections = new Dictionary<SectionPosition, Section>();
        //        SectionPosition[] items = (SectionPosition[])Enum.GetValues(typeof(SectionPosition));
        //        foreach (SectionPosition pos in items)
        //        {
        //            Section section = Mvx.IocConstruct<Section>();
        //            section.Id = pos;
        //            section.Lane = new Lane();
        //            //section.Lane.Direction = section.Direction;
        //            section.Lane.Enabled = true;
        //            //section.Lane.VehicleType = VehicleType.Bike;
        //            section.Lane.Name = null;
        //            if (pos == SectionPosition.Lane1 || pos == SectionPosition.Lane2)
        //                section.IsConfigured = true;
        //            Sections.Add(pos, section);
        //        }
        //    }
        //}

        public void ChangeLaneDirection(SectionPosition sectionId, LaneDirection direction)
        {
            Sections[sectionId].Direction = direction;
            //MarkChanged();
        }

        public void AddSection(Section section)
        {
            Sections.Add(section.Id, section);
            //MarkChanged();
        }

        public void UpdateSection(Section section)
        {
            if (!Sections.ContainsKey(section.Id))
                return;

            Sections[section.Id] = section;
            if (section.Id == SectionPosition.Lane1 || section.Id == SectionPosition.Lane2)
                section.ShouldBeDisplayed = true;
            else
                section.ShouldBeDisplayed = false;
           //MarkChanged();
        }

        public List<Section> GetAllSections()
        {
            if (Sections.Count == 0)
            {
                Section sec = new Section();
                Lane lane = new Lane();
                sec.Lane = lane;
                sec.Direction = LaneDirection.In;
                sec.Id = SectionPosition.Lane1;
                Sections.Add(SectionPosition.Lane1, sec);
            }

            return Sections.Where(sec => sec.Value.IsConfigured == true).Select(sec => sec.Value).ToList();
        }

        public List<Section> GetAllSections(DisplayedPosition pos)
        {
            return Sections.Where(sec => sec.Value.DisplayedPosition == pos).Select(sec => sec.Value).ToList();
        }

        public void ChangeDisplayedPositionLane(SectionPosition sectionId, DisplayedPosition pos)
        {
            Section section = Sections[sectionId];
            // khi doi lan
            // phai co 1 lan o vi tri cu duoc mo?
            
            var sections = Sections.Where(s => s.Value.DisplayedPosition == section.DisplayedPosition && s.Value.Id != section.Id).ToDictionary(s => s.Key, s => s.Value);
            var filtered = sections.Where(s => s.Value.ShouldBeDisplayed == true);
            if (filtered.Count() == 0)
            {
                if (sections.Count > 0)
                    sections.FirstOrDefault().Value.ShouldBeDisplayed = true;
            }

            section.DisplayedPosition = pos;
            section.ShouldBeDisplayed = false;

            if (pos == Models.DisplayedPosition.Left)
            {
                section.KeyMap = new KeyMap(SectionPosition.Lane1);
            }
            else if (pos == Models.DisplayedPosition.Right)
            {
                section.KeyMap = new KeyMap(SectionPosition.Lane2);
            }

            //MarkChanged();
        }

        protected override string GetSettingName()
        {
            if (_argParams.Mode == RunMode.Testing)
                return this.GetType().Name + "Test";
            else
                return base.GetSettingName();
        }

        public override void ForceSave()
        {
            this.Sections.Values.ToList().ForEach(item => item.SaveStates());
            MarkChanged();

            base.ForceSave();
        }
    } 
}
