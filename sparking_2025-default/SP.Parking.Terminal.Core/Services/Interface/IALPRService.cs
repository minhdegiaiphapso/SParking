using System;

namespace SP.Parking.Terminal.Core.Services
{
    public interface IALPRService
    {
        void RecognizeLicensePlate(byte[] image, Action<string, Exception> complete);
        void RecognizeLicensePlate(byte[] image,int VehicleType, Action<string, Exception> complete);
    }
}
