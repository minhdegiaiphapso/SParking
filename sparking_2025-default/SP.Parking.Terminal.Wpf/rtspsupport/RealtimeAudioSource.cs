using System;
using System.Collections.Generic;
using RtspClientSharp.RawFrames;
using RtspClientSharp.RawFrames.Audio;
using  SP.Parking.Terminal.Wpf.RtspSupport.RawFramesDecoding;
using  SP.Parking.Terminal.Wpf.RtspSupport.RawFramesDecoding.DecodedFrames;
using  SP.Parking.Terminal.Wpf.RtspSupport.RawFramesDecoding.FFmpeg;
using  SP.Parking.Terminal.Wpf.RtspSupport.RawFramesReceiving;

namespace  SP.Parking.Terminal.Wpf.RtspSupport
{
    class RealtimeAudioSource : IAudioSource
    {
        private IRawFramesSource _rawFramesSource;

        private readonly Dictionary<FFmpegAudioCodecId, FFmpegAudioDecoder> _audioDecodersMap =
            new Dictionary<FFmpegAudioCodecId, FFmpegAudioDecoder>();

        public event EventHandler<IDecodedAudioFrame> FrameReceived;

        public void SetRawFramesSource(IRawFramesSource rawFramesSource)
        {
            if (_rawFramesSource != null)
                _rawFramesSource.FrameReceived -= OnFrameReceived;

            _rawFramesSource = rawFramesSource;

            if (rawFramesSource == null)
                return;

            rawFramesSource.FrameReceived += OnFrameReceived;
        }

        private void OnFrameReceived(object sender, RawFrame rawFrame)
        {
            if (_rawFramesSource == null || !_rawFramesSource.IsStart)
                return;
            if (!(rawFrame is RawAudioFrame rawAudioFrame))
                return;

            FFmpegAudioDecoder decoder = GetDecoderForFrame(rawAudioFrame);
            if (_rawFramesSource == null || !_rawFramesSource.IsStart)
                return;
            if (!decoder.TryDecode(rawAudioFrame))
                return;

            IDecodedAudioFrame decodedFrame = decoder.GetDecodedFrame(new AudioConversionParameters() {OutBitsPerSample = 16});

            if (_rawFramesSource == null || !_rawFramesSource.IsStart)
                return;

            FrameReceived?.Invoke(this, decodedFrame);
        }

        private FFmpegAudioDecoder GetDecoderForFrame(RawAudioFrame audioFrame)
        {
            FFmpegAudioCodecId codecId = DetectCodecId(audioFrame);

            if (!_audioDecodersMap.TryGetValue(codecId, out FFmpegAudioDecoder decoder))
            {
                int bitsPerCodedSample = 0;

                if (audioFrame is RawG726Frame g726Frame)
                    bitsPerCodedSample = g726Frame.BitsPerCodedSample;

                decoder = FFmpegAudioDecoder.CreateDecoder(codecId, bitsPerCodedSample);
                _audioDecodersMap.Add(codecId, decoder);
            }

            return decoder;
        }

        private FFmpegAudioCodecId DetectCodecId(RawAudioFrame audioFrame)
        {
            if (audioFrame is RawAACFrame)
                return FFmpegAudioCodecId.AAC;
            if (audioFrame is RawG711AFrame)
                return FFmpegAudioCodecId.G711A;
            if (audioFrame is RawG711UFrame)
                return FFmpegAudioCodecId.G711U;
            if (audioFrame is RawG726Frame)
                return FFmpegAudioCodecId.G726;

            throw new ArgumentOutOfRangeException(nameof(audioFrame));
        }
    }
}
