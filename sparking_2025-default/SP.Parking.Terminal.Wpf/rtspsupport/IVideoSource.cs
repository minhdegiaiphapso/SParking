using System;
using  SP.Parking.Terminal.Wpf.RtspSupport.RawFramesDecoding.DecodedFrames;

namespace  SP.Parking.Terminal.Wpf.RtspSupport
{
    public interface IVideoSource
    {
        event EventHandler<IDecodedVideoFrame> FrameReceived;
    }
}