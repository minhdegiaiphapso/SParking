using SP.Parking.Terminal.Core.Models;
using Green.Devices.Dal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Services
{
    public interface IResourceLocatorService
    {
        List<CardReaderWrapper> GetCardReaders(SectionPosition pos);

        //Camera GetFrontCamera(SectionPosition pos);

        //Camera GetBackCamera(SectionPosition pos);

		IBarrierDevice GetBarrier(SectionPosition pos);

        //void SetupCamera(Section section, string ip, LaneDirection direction, CameraPosition cameraPosition);

        CardReaderWrapper SetupCardReader(string serialNumber);

        CardReaderWrapper SetupCardReader(string ip, string port);

        void SetupBarrier(Section section, string barrierName, string devicePort);
    }

    public class ResourceLocatorService : IResourceLocatorService
    {
        public List<Section> Sections { get; set; }

        IUserPreferenceService _preferenceService;

        ICardReaderService _cardReaderService;

		IBarrierDeviceManager _barrierService;

        public ResourceLocatorService(IUserPreferenceService preferenceService
			, ICardReaderService rfidService
			, IBarrierDeviceManager barrierService)
        {
            _preferenceService = preferenceService;
            _cardReaderService = rfidService;
            _barrierService = barrierService;
            Sections = _preferenceService.SystemSettings.GetAllSections();
        }

        //public Camera GetFrontCamera(SectionPosition sectionPos)
        //{
        //    Section section = _preferenceService.SystemSettings.Sections[sectionPos];
        //    return section.FrontCamera;
        //}

        //public Camera GetBackCamera(SectionPosition sectionPos)
        //{
        //    Section section = _preferenceService.SystemSettings.Sections[sectionPos];
        //    return section.BackCamera;
        //}

        public List<CardReaderWrapper> GetCardReaders(SectionPosition pos)
        {
            Section section = _preferenceService.SystemSettings.Sections[pos];
            return section.CardReaders;
        }

        public IBarrierDevice GetBarrier(SectionPosition pos)
		{
			Section section = _preferenceService.SystemSettings.Sections[pos];
			return section.Barrier;
		}

        //public void SetupCamera(Section section, string ip, LaneDirection direction, CameraPosition cameraPosition)
        //{
        //    Camera camera = section.Cameras.Where(c => c.Direction == direction & c.Position == cameraPosition).FirstOrDefault();
        //    if (camera == null)
        //    {
        //        camera = new Camera();
        //        section.Cameras.Add(camera);
        //    }
        //    camera.IP = ip;
        //    camera.LaneId = section.Lane.Id;
        //    camera.Position = cameraPosition;
        //    camera.Direction = direction;
        //    camera.Setup(ip);
        //}

        public CardReaderWrapper SetupCardReader(string serialNumber)
        {
            return _cardReaderService.GetCardReader(serialNumber);
        }

        public CardReaderWrapper SetupCardReader(string ip, string port)
        {
            return _cardReaderService.GetCardReader(ip, port);
        }

        public void SetupBarrier(Section section, string barrierName, string devicePort)
        {
            section.Barrier = _barrierService.GetDevice(barrierName, devicePort);
        }
    }
}