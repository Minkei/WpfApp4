using OpenCvSharp;
using System.Collections.ObjectModel;
using System.ComponentModel;
using DirectShowLib;
using OpenCvSharp.WpfExtensions;
using ZXing;
using ZXing.Common;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using WpfApp4.Models;
using WpfApp4.ViewModels;
using System.Windows;
using System.Threading;
using System.Media;
using System.Windows.Documents;

namespace WpfApp4.Services
{
    public class QRScannerService : ViewModelBase
    {
        // Fields
        private VideoCapture? _videoCapture;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isStreaming;
        private bool _isProcessing;
        private string? _cameraOverview;
        private CameraDevice? _selectedCamera;
        private ObservableCollection<CameraDevice>? _availableCameras;

        // Properties
        public bool IsStreaming {
            get => _isStreaming;
            set => SetProperty(ref _isStreaming, value);
        }
        public ObservableCollection<CameraDevice>? AvailableCameras {
            get => _availableCameras;
            set => SetProperty(ref _availableCameras, value);
        }
        public CameraDevice? SelectedCamera {
            get => _selectedCamera;
            set => SetProperty(ref _selectedCamera, value);
        }
        public string? CameraOverview {
            get => _cameraOverview;
            set => SetProperty(ref _cameraOverview, value);
        }

        //Events
        public event EventHandler<string>? QRCodeDetected;
        public event EventHandler<BitmapSource>? FrameCaptured;
        public event EventHandler<string>? StreamingStatusStringChanged;
        public event EventHandler<bool>? StreamingStatusBoolChanged;
        public event EventHandler<string>? CameraOverviewChanged;

        // Methods
        public void LoadAvailableCameras()
        {
            AvailableCameras = new ObservableCollection<CameraDevice>();
            var cameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            foreach (var camera in cameras.Select((value, index) => new { value, index }))
            {
                try
                {
                    using (var capture = new VideoCapture(camera.index))
                    {
                        if (capture.IsOpened())
                        {
                            var camera_name = camera.value.Name;
                            var camera_index = camera.index;
                            var camera_width = (int)capture.Get(OpenCvSharp.VideoCaptureProperties.FrameWidth);
                            var camera_height = (int)capture.Get(OpenCvSharp.VideoCaptureProperties.FrameHeight);
                            var camera_fps = (int)capture.Get(OpenCvSharp.VideoCaptureProperties.Fps);

                            AvailableCameras.Add(new CameraDevice
                            {
                                CameraName = camera_name,
                                CameraIndex = camera_index,
                                CameraWidth = camera_width,
                                CameraHeight = camera_height,
                                CameraOverview = $"via by {camera_name} {camera_width}x{camera_height} {camera_fps}fps"
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading camera {camera.index}: {ex.Message}");
                }
            }
        }
        public void StartCamera(int cameraIndex)
        {
            if (IsStreaming) return;

            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            _videoCapture = new VideoCapture(cameraIndex);
            if (!_videoCapture.IsOpened())
            {
                throw new Exception("Failed to open camera.");
            }

            IsStreaming = true;
            StreamingStatusStringChanged?.Invoke(this, "Streaming");
            StreamingStatusBoolChanged?.Invoke(this, IsStreaming);
            CameraOverviewChanged?.Invoke(this, SelectedCamera?.CameraOverview ?? "Unknown");


            Task.Run(() => ProcessCameraStream(token), token);
        }
        public void StopCamera()
        {
            if (!IsStreaming) return;

            IsStreaming = false;
            StreamingStatusStringChanged?.Invoke(this, "Stopped");
            StreamingStatusBoolChanged?.Invoke(this, IsStreaming);
            CameraOverviewChanged?.Invoke(this, "Unknown");

            _cancellationTokenSource?.Cancel();
            _videoCapture?.Release();
            _videoCapture = null;
        }
        
        private async Task ProcessCameraStream(CancellationToken token)
        {
            var qrCodeDetector = new QRCodeDetector();

            while (IsStreaming && !token.IsCancellationRequested)
            {
                if (_isProcessing) continue;

                _isProcessing = true;

                try
                {
                    using var frame = new Mat();
                    if (_videoCapture == null || !_videoCapture.Read(frame) || frame.Empty())
                        continue;

                    // Detect QR code
                    Point2f[] points;
                    string qrCodeData = qrCodeDetector.DetectAndDecode(frame, out points);

                    if (!string.IsNullOrEmpty(qrCodeData))
                    {
                        QRCodeDetected?.Invoke(this, qrCodeData);
                    }

                    // Draw bounding box if QR code is detected
                    if (points.Length == 4)
                    {
                        for (int i = 0; i < points.Length; i++)
                        {
                            var pt1 = points[i];
                            var pt2 = points[(i + 1) % points.Length];
                            var pt1Int = new OpenCvSharp.Point((int)pt1.X, (int)pt1.Y);
                            var pt2Int = new OpenCvSharp.Point((int)pt2.X, (int)pt2.Y);
                            Cv2.Line(frame, pt1Int, pt2Int, new Scalar(255, 0, 0), 3);
                        }
                    }

                    // Convert frame to BitmapSource and update UI
                    var frameBitmap = frame.ToBitmapSource();
                    frameBitmap.Freeze(); // Freeze for thread-safety
                    Application.Current.Dispatcher.Invoke(() => FrameCaptured?.Invoke(this, frameBitmap));
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing frame: {ex.Message}");
                    break;
                }
                finally
                {
                    _isProcessing = false;
                }

                // Control frame rate (adjust delay for performance)
                await Task.Delay(30, token).ConfigureAwait(false); // Adjust as needed for smooth performance
            }
        }
    }
}
