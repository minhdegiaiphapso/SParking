using System;
using  SP.Parking.Terminal.Wpf.RtspSupport.RawFramesDecoding.DecodedFrames;

namespace  SP.Parking.Terminal.Wpf.RtspSupport
{
    interface IAudioSource
    {
        event EventHandler<IDecodedAudioFrame> FrameReceived;
    }
}
