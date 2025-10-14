using System;
using RtspClientSharp.RawFrames;

namespace  SP.Parking.Terminal.Wpf.RtspSupport.RawFramesReceiving
{
    interface IRawFramesSource
    {
        EventHandler<RawFrame> FrameReceived { get; set; }
        EventHandler<string> ConnectionStatusChanged { get; set; }
        bool IsStart { get; set; }
        void Start();
        void Stop();
    }
}