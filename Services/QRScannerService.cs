using OpenCvSharp;
using System.Collections.ObjectModel;
using System.ComponentModel;
using DirectShowLib;
using OpenCvSharp.WpfExtensions;
using ZXing;
using ZXing.Common;
using ZXing.QrCode; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using WpfApp4.Models;
using WpfApp4.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace WpfApp4.Services
{
    public class QRScannerService: ViewModelBase
    {
        //Fileds    
        private VideoCapture? _videoCapture;
        public bool _isStreaming;
        public bool _isProcessing;
        public CameraDevice? _selectedCamera;
        public ObservableCollection<CameraDevice>? _availableCameras;

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
            set
            {
                if (_availableCameras != value)
                {
                    _availableCameras = value;
                    OnPropertyChanged(nameof(AvailableCameras));
                }
            }
        }

        public CameraDevice? SelectedCamera {
            get => _selectedCamera;
            set
            {
                if (_selectedCamera != value)
                {
                    _selectedCamera = value;
                    OnPropertyChanged(nameof(SelectedCamera));
                }
            }
        }

        public event EventHandler<string>? QRCodeDetected;
        public event EventHandler<BitmapSource>? FrameCaptured;


        public void LoadAvailableCameras()
        {
            AvailableCameras = new ObservableCollection<CameraDevice>();
            var cameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            for (int i = 0; i < cameras.Length; i++)
            {
                using (var capture = new VideoCapture(i))
                {
                    if (capture.IsOpened())
                    {
                        int width = (int)capture.Get(OpenCvSharp.VideoCaptureProperties.FrameWidth);
                        int height = (int)capture.Get(OpenCvSharp.VideoCaptureProperties.FrameHeight);

                        AvailableCameras.Add(new CameraDevice
                        {
                            CameraName = cameras[i].Name,
                            CameraIndex = i,
                            CameraWidth = width,
                            CameraHeight = height
                        });
                    }
                }
            }
        }

        public void StartCamera(int cameraIndex)
        {
            if (_isStreaming) return;

            _videoCapture = new VideoCapture(cameraIndex);
            if (!_videoCapture.IsOpened())
            {
                throw new Exception("Failed to open camera");
            }

            _isStreaming = true;
            Application.Current.Dispatcher.Invoke(() => IsStreaming = true);
            Task.Run(() => ProcessCameraStream());
            Console.WriteLine("Camera started");
            Console.WriteLine(IsStreaming);
        }

        public void StopCamera()
        {
            if (!_isStreaming) return;

            _isStreaming = false;
            Application.Current.Dispatcher.Invoke(() => IsStreaming = false);
            _videoCapture?.Dispose();
            _videoCapture = null;
            Console.WriteLine("Camera stopped");
            Console.WriteLine(IsStreaming);
        }

        private async Task ProcessCameraStream()
        {
            var qrCodeDetector = new QRCodeDetector();

            while (_isStreaming)
            {
                if (_isProcessing)
                    continue;

                _isProcessing = true;

                try
                {
                    using var frame = new Mat();
                    _videoCapture?.Read(frame);

                    if (frame.Empty())
                        continue;

                    /// Detect and decode 
                    Point2f[] points;
                    string qrCodeData = qrCodeDetector.DetectAndDecode(frame, out points);
                    if (!string.IsNullOrEmpty(qrCodeData))
                    {
                        QRCodeDetected?.Invoke(this, qrCodeData);
                    }
                    /// Draw the bounding box on the frame
                    if (points.Length == 4)
                    {
                        // Draw bounding box around QR code
                        for (int i = 0; i < points.Length; i++)
                        {
                            var pt1 = points[i];
                            var pt2 = points[(i + 1) % points.Length]; // Connect the last point with the first
                            var pt1Int = new OpenCvSharp.Point((int)pt1.X, (int)pt1.Y);
                            var pt2Int = new OpenCvSharp.Point((int)pt2.X, (int)pt2.Y);
                            Cv2.Line(frame, pt1Int, pt2Int, new Scalar(255, 0, 0), 3); // Draw red lines
                        }
                    }
                    var frameBitmap = frame.ToBitmapSource();
                    frameBitmap.Freeze();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        FrameCaptured?.Invoke(this, frameBitmap);
                    });
                }
                catch (Exception ex)
                {
                    // Handle error (logging, rethrowing, etc.)
                    Console.WriteLine($"Error processing frame: {ex.Message}");
                    break;
                }
                finally
                {
                    _isProcessing = false;
                }

                await Task.Delay(30); // Limit frame rate
            }
        }
    }
}
