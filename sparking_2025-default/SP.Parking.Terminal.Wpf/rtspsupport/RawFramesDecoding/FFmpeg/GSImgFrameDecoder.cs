using Emgu.CV;
using Emgu.CV.Structure;
using SP.Parking.Terminal.Wpf.RtspSupport.RawFramesDecoding.DecodedFrames;
using SP.Parking.Terminal.Wpf.RtspSupport.RawFramesReceiving;
using RtspClientSharp.RawFrames;
using RtspClientSharp.RawFrames.Video;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Wpf.RtspSupport.RawFramesDecoding.FFmpeg
{
    class GSImgRealtimeSource : IDisposable
    {
        private readonly Dictionary<FFmpegVideoCodecId, GSFFmpegVideoDecoder> _videoDecodersMap =
           new Dictionary<FFmpegVideoCodecId, GSFFmpegVideoDecoder>();
        public event EventHandler<Image<Bgr, byte>> DataReceiveHandler;
        
        public void OnOriginFrameReceive(object sender, RawFrame rawFrame)
        {
            if(DataReceiveHandler==null) return;
            if (!(rawFrame is RawVideoFrame rawVideoFrame))
                return;
            GSFFmpegVideoDecoder decoder = GetDecoderForFrame(rawVideoFrame);
         
            var decodeFrame = decoder.TryDecode(rawVideoFrame);
            if (decodeFrame != null)
            {
                var bmp = decoder.PrepareImg;
                var tp = decoder.TransformParameters;
                if(bmp != null && tp != null)
                {
                    System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(
                                new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                                System.Drawing.Imaging.ImageLockMode.WriteOnly, bmp.PixelFormat);
                    decodeFrame.TransformTo(bmpData.Scan0, bmpData.Stride, tp); 
                    bmp.UnlockBits(bmpData);
                    var data = bmp.ToImage<Bgr, byte>();
                    if(data != null)
                        DataReceiveHandler?.Invoke(this, data);
                    bmp?.Dispose();
                }
            }         
        }
        private GSFFmpegVideoDecoder GetDecoderForFrame(RawVideoFrame videoFrame)
        {
            FFmpegVideoCodecId codecId = DetectCodecId(videoFrame);
            if (!_videoDecodersMap.TryGetValue(codecId, out GSFFmpegVideoDecoder decoder))
            {
                decoder = GSFFmpegVideoDecoder.CreateDecoder(codecId);
                _videoDecodersMap.Add(codecId, decoder);
            }

            return decoder;
        }
        private FFmpegVideoCodecId DetectCodecId(RawVideoFrame videoFrame)
        {
            if (videoFrame is RawJpegFrame)
                return FFmpegVideoCodecId.MJPEG;
            if (videoFrame is RawH264Frame)
                return FFmpegVideoCodecId.H264;

            throw new ArgumentOutOfRangeException(nameof(videoFrame));
        }
        public void Dispose()
        {
            DropAllVideoDecoders();
        }
        private void DropAllVideoDecoders()
        {
            foreach (GSFFmpegVideoDecoder decoder in _videoDecodersMap.Values)
                decoder.Dispose();
            _videoDecodersMap.Clear();
        }
    }
    class GSFFmpegVideoDecoder
    {
        private readonly IntPtr _decoderHandle;
        private readonly FFmpegVideoCodecId _videoCodecId;

        private DecodedVideoFrameParameters _currentFrameParameters =
            new DecodedVideoFrameParameters(0, 0, FFmpegPixelFormat.None);

        private readonly Dictionary<TransformParameters, FFmpegDecodedVideoScaler> _scalersMap =
            new Dictionary<TransformParameters, FFmpegDecodedVideoScaler>();

        private byte[] _extraData = new byte[0];
        private bool _disposed;
        public Bitmap PrepareImg { get; set; }
        public TransformParameters TransformParameters { get; set; }
        private GSFFmpegVideoDecoder(FFmpegVideoCodecId videoCodecId, IntPtr decoderHandle)
        {
            _videoCodecId = videoCodecId;
            _decoderHandle = decoderHandle;
        }

        ~GSFFmpegVideoDecoder()
        {
            Dispose();
        }

        public static GSFFmpegVideoDecoder CreateDecoder(FFmpegVideoCodecId videoCodecId)
        {
            int resultCode = FFmpegVideoPInvoke.CreateVideoDecoder(videoCodecId, out IntPtr decoderPtr);

            if (resultCode != 0)
                throw new DecoderException(
                    $"An error occurred while creating video decoder for {videoCodecId} codec, code: {resultCode}");

            return new GSFFmpegVideoDecoder(videoCodecId, decoderPtr);
        }
        [HandleProcessCorruptedStateExceptionsAttribute]
        public unsafe IDecodedVideoFrame TryDecode(RawVideoFrame rawVideoFrame)
        {
            try
            {
                if (Istransfrom)
                    return null;
                fixed (byte* rawBufferPtr = &rawVideoFrame.FrameSegment.Array[rawVideoFrame.FrameSegment.Offset])
                {
                    int resultCode;
                    Istransfrom = true;
                    if (rawVideoFrame is RawH264IFrame rawH264IFrame)
                    {
                        if (rawH264IFrame.SpsPpsSegment.Array != null &&
                            !_extraData.SequenceEqual(rawH264IFrame.SpsPpsSegment))
                        {
                            if (_extraData.Length != rawH264IFrame.SpsPpsSegment.Count)
                                _extraData = new byte[rawH264IFrame.SpsPpsSegment.Count];

                            Buffer.BlockCopy(rawH264IFrame.SpsPpsSegment.Array, rawH264IFrame.SpsPpsSegment.Offset,
                                _extraData, 0, rawH264IFrame.SpsPpsSegment.Count);

                            fixed (byte* initDataPtr = &_extraData[0])
                            {
                                resultCode = FFmpegVideoPInvoke.SetVideoDecoderExtraData(_decoderHandle,
                                    (IntPtr)initDataPtr, _extraData.Length);

                                if (resultCode != 0)
                                    return null;
                                //throw new DecoderException(
                                //    $"An error occurred while setting video extra data, {_videoCodecId} codec, code: {resultCode}");
                            }
                        }
                    }
                    resultCode = FFmpegVideoPInvoke.DecodeFrame(_decoderHandle, (IntPtr)rawBufferPtr,
                        rawVideoFrame.FrameSegment.Count,
                        out int width, out int height, out FFmpegPixelFormat pixelFormat);

                    if (resultCode != 0)
                        return null;

                    if (_currentFrameParameters.Width != width || _currentFrameParameters.Height != height ||
                        _currentFrameParameters.PixelFormat != pixelFormat)
                    {
                        _currentFrameParameters = new DecodedVideoFrameParameters(width, height, pixelFormat);
                        DropAllVideoScalers();
                    }
                    Istransfrom = false;
                    PrepareImg = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                    TransformParameters = new TransformParameters(RectangleF.Empty,
                    new System.Drawing.Size(width, height),
                    ScalingPolicy.Stretch, PixelFormat.Bgra32, ScalingQuality.FastBilinear);
                    return new DecodedVideoFrame(TransformTo);
                }
            }
            catch (Exception)
            {
                return null;
            }

        }

        public void Dispose()
        {
            while (Istransfrom)
                ;
            if (_disposed)
                return;
            _disposed = true;
            FFmpegVideoPInvoke.RemoveVideoDecoder(_decoderHandle);
            DropAllVideoScalers();
            GC.SuppressFinalize(this);
        }

        private void DropAllVideoScalers()
        {
            foreach (var scaler in _scalersMap.Values)
                scaler.Dispose();

            _scalersMap.Clear();
        }
        bool Istransfrom = false;
        [HandleProcessCorruptedStateExceptionsAttribute]
        private void TransformTo(IntPtr buffer, int bufferStride, TransformParameters parameters)
        {
            if (!Istransfrom)
            {
                try
                {
                    {
                        Istransfrom = true;
                        if (!_scalersMap.TryGetValue(parameters, out FFmpegDecodedVideoScaler videoScaler))
                        {
                            videoScaler = FFmpegDecodedVideoScaler.Create(_currentFrameParameters, parameters);
                            _scalersMap.Add(parameters, videoScaler);
                        }
                        int resultCode = FFmpegVideoPInvoke.ScaleDecodedVideoFrame(_decoderHandle, videoScaler.Handle, buffer, bufferStride);
                        if (resultCode != 0)
                        {
                            Istransfrom = false;
                            return;
                            //throw new DecoderException($"An error occurred while converting decoding video frame, {_videoCodecId} codec, code: {resultCode}");
                        }
                    }
                }
                catch
                {
                    ;
                }
                finally
                {
                    Istransfrom = false;
                }
            }
        }
    }
}
