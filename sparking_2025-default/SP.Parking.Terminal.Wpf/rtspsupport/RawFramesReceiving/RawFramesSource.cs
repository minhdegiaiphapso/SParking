using System;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using RtspClientSharp;
using RtspClientSharp.RawFrames;
using RtspClientSharp.Rtsp;

namespace  SP.Parking.Terminal.Wpf.RtspSupport.RawFramesReceiving
{

    class RawFramesSource : IRawFramesSource
    {
        private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);
        private readonly ConnectionParameters _connectionParameters;
        private Task _workTask = Task.CompletedTask;
        private CancellationTokenSource _cancellationTokenSource;

        public EventHandler<RawFrame> FrameReceived { get; set; }
        public EventHandler<string> ConnectionStatusChanged { get; set; }

        public RawFramesSource(ConnectionParameters connectionParameters)
        {
            _connectionParameters =
                connectionParameters ?? throw new ArgumentNullException(nameof(connectionParameters));
        }
        public bool IsStart { get; set; } = false;
        public void Start()
        {
            if (!IsStart && (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested))
            {
               
                _cancellationTokenSource = new CancellationTokenSource();

                CancellationToken token = _cancellationTokenSource.Token;
                IsStart = true;
                _workTask = _workTask.ContinueWith(async p =>
                {
                    
                    await ReceiveAsync(token);
                    
                }, token);
            }
        }

        public void Stop()
        {
            IsStart = false;
            _cancellationTokenSource.Cancel();
           
        }

        private async Task ReceiveAsync(CancellationToken token)
        {
            try
            {
                using (var rtspClient = new RtspClient(_connectionParameters))
                {
                    rtspClient.FrameReceived += RtspClientOnFrameReceived;

                    while (IsStart)
                    {
                        OnStatusChanged("Đang kết nối...");

                        try
                        {
                            if (!IsStart)
                                break;
                            await rtspClient.ConnectAsync(token);
                        }
                        catch (InvalidCredentialException)
                        {
                            if (!IsStart)
                                break;
                            OnStatusChanged("Sai thông tin đăng nhập!!!");
                            await Task.Delay(RetryDelay, token);
                            continue;
                        }
                        catch (RtspClientException e)
                        {
                            if (!IsStart)
                                break;
                            OnStatusChanged(e.ToString());
                            await Task.Delay(RetryDelay, token);
                            continue;
                        }

                        OnStatusChanged("Đang tạo khung ảnh...");

                        try
                        {
                            if (!IsStart)
                                break;
                            await rtspClient.ReceiveAsync(token);
                        }
                        catch (RtspClientException e)
                        {
                            if (!IsStart)
                                break;
                            OnStatusChanged(e.ToString());
                            await Task.Delay(RetryDelay, token);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void RtspClientOnFrameReceived(object sender, RawFrame rawFrame)
        {
            if (IsStart && (_cancellationTokenSource!= null && !_cancellationTokenSource.IsCancellationRequested))
                FrameReceived?.Invoke(this, rawFrame);
        }

        private void OnStatusChanged(string status)
        {
            ConnectionStatusChanged?.Invoke(this, status);
        }
    }
}