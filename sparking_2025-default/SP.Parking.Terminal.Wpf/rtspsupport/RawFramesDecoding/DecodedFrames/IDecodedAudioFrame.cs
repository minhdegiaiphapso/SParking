using System;

namespace  SP.Parking.Terminal.Wpf.RtspSupport.RawFramesDecoding.DecodedFrames
{
    public interface IDecodedAudioFrame
    {
        DateTime Timestamp { get; }
        ArraySegment<byte> DecodedBytes { get; }
        AudioFrameFormat Format { get; }
    }
}