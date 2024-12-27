//using OpenCvSharp;
//using System.Collections.ObjectModel;
//using System.ComponentModel;
//using DirectShowLib;
//using OpenCvSharp.WpfExtensions;
//using ZXing;
//using ZXing.Common;
//using ZXing.QrCode; 
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Media.Imaging;
//using WpfApp4.Models;
//using WpfApp4.ViewModels;
//using System.Windows;
//using System.Windows.Input;

//namespace WpfApp4.Services
//{
//    public class QRScannerService: ViewModelBase
//    {
//        //Fileds    
//        private VideoCapture? _videoCapture;
//        public bool _isStreaming;
//        public bool _isProcessing;
//        public CameraDevice? _selectedCamera;
//        public ObservableCollection<CameraDevice>? _availableCameras;

//        public bool IsStreaming {
//            get => _isStreaming;
//            set
//            {
//                if (_isStreaming != value)
//                {
//                    _isStreaming = value;
//                    OnPropertyChanged(nameof(IsStreaming));
//                }
//            }
//        }
//        public ObservableCollection<CameraDevice>? AvailableCameras {
//            get => _availableCameras;
//            set
//            {
//                if (_availableCameras != value)
//                {
//                    _availableCameras = value;
//                    OnPropertyChanged(nameof(AvailableCameras));
//                }
//            }
//        }

//        public CameraDevice? SelectedCamera {
//            get => _selectedCamera;
//            set
//            {
//                if (_selectedCamera != value)
//                {
//                    _selectedCamera = value;
//                    OnPropertyChanged(nameof(SelectedCamera));
//                }
//            }
//        }

//        public event EventHandler<string>? QRCodeDetected;
//        public event EventHandler<BitmapSource>? FrameCaptured;
//        public event EventHandler<string>? StreamingStatusChanged;


//        public void LoadAvailableCameras()
//        {
//            AvailableCameras = new ObservableCollection<CameraDevice>();
//            var cameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

//            for (int i = 0; i < cameras.Length; i++)
//            {
//                using (var capture = new VideoCapture(i))
//                {
//                    if (capture.IsOpened())
//                    {
//                        int width = (int)capture.Get(OpenCvSharp.VideoCaptureProperties.FrameWidth);
//                        int height = (int)capture.Get(OpenCvSharp.VideoCaptureProperties.FrameHeight);

//                        AvailableCameras.Add(new CameraDevice
//                        {
//                            CameraName = cameras[i].Name,
//                            CameraIndex = i,
//                            CameraWidth = width,
//                            CameraHeight = height
//                        });
//                    }
//                }
//            }
//        }

//        public void StartCamera(int cameraIndex)
//        {
//            if (_isStreaming) return;

//            _videoCapture = new VideoCapture(cameraIndex);
//            if (!_videoCapture.IsOpened())
//            {
//                throw new Exception("Failed to open camera");
//            }

//            _isStreaming = true;
//            StreamingStatusChanged?.Invoke(this, "Streaming");
//            Application.Current.Dispatcher.Invoke(() => IsStreaming = true);
//            Task.Run(() => ProcessCameraStream());
//        }

//        public void StopCamera()
//        {
//            if (!_isStreaming) return;

//            _isStreaming = false;
//            StreamingStatusChanged?.Invoke(this, "Stoped");
//            Application.Current.Dispatcher.Invoke(() => IsStreaming = false);
//            _videoCapture?.Dispose();
//            _videoCapture = null;
//        }

//        private async Task ProcessCameraStream()
//        {
//            var qrCodeDetector = new QRCodeDetector();

//            while (_isStreaming)
//            {
//                if (_isProcessing)
//                    continue;

//                _isProcessing = true;

//                try
//                {
//                    using var frame = new Mat();
//                    _videoCapture?.Read(frame);

//                    if (frame.Empty())
//                        continue;

//                    /// Detect and decode 
//                    Point2f[] points;
//                    string qrCodeData = qrCodeDetector.DetectAndDecode(frame, out points);
//                    if (!string.IsNullOrEmpty(qrCodeData))
//                    {
//                        QRCodeDetected?.Invoke(this, qrCodeData);
//                    }
//                    /// Draw the bounding box on the frame
//                    if (points.Length == 4)
//                    {
//                        // Draw bounding box around QR code
//                        for (int i = 0; i < points.Length; i++)
//                        {
//                            var pt1 = points[i];
//                            var pt2 = points[(i + 1) % points.Length]; // Connect the last point with the first
//                            var pt1Int = new OpenCvSharp.Point((int)pt1.X, (int)pt1.Y);
//                            var pt2Int = new OpenCvSharp.Point((int)pt2.X, (int)pt2.Y);
//                            Cv2.Line(frame, pt1Int, pt2Int, new Scalar(255, 0, 0), 3); // Draw red lines
//                        }
//                    }
//                    var frameBitmap = frame.ToBitmapSource();
//                    frameBitmap.Freeze();
//                    Application.Current.Dispatcher.Invoke(() =>
//                    {
//                        FrameCaptured?.Invoke(this, frameBitmap);
//                    });
//                }
//                catch (Exception ex)
//                {
//                    // Handle error (logging, rethrowing, etc.)
//                    Console.WriteLine($"Error processing frame: {ex.Message}");
//                    break;
//                }
//                finally
//                {
//                    _isProcessing = false;
//                }

//                await Task.Delay(30); // Limit frame rate
//            }
//        }
//    }
//}


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
            set
            {
                if (_isStreaming != value)
                {
                    _isStreaming = value;
                    OnPropertyChanged(nameof(IsStreaming));
                }
            }
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
        public event EventHandler<string>? StreamingStatusChanged;
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
            StreamingStatusChanged?.Invoke(this, "Streaming");
            CameraOverviewChanged?.Invoke(this, SelectedCamera?.CameraOverview ?? "Unknown");

            Application.Current.Dispatcher.Invoke(() => OnPropertyChanged(nameof(IsStreaming)));

            Task.Run(() => ProcessCameraStream(token), token);
        }
        public void StopCamera()
        {
            if (!IsStreaming) return;

            IsStreaming = false;
            StreamingStatusChanged?.Invoke(this, "Stopped");
            CameraOverviewChanged?.Invoke(this, "Unknown");
            Application.Current.Dispatcher.Invoke(() => OnPropertyChanged(nameof(IsStreaming)));

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
