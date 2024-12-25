using OpenCvSharp;
using DirectShowLib;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfApp4.Models;
using System.Globalization;
using ZXing;
using ZXing.Common;
using OpenCvSharp.Extensions;
using ZXing.QrCode;

namespace WpfApp4.ViewModels
{
    public class QRScannerViewModel : ViewModelBase
    {
        private VideoCapture? _videoCapture;
        private BitmapSource? _currentFrame;
        private bool _isStreaming;
        private ObservableCollection<CameraDevice>? _availableCameras;
        private CameraDevice? _selectedCamera;
        private bool _isProcessing;
        private string _qrCodeContent = string.Empty;
        public QRScannerViewModel()
        {
            LoadAvailableCameras();
            StartStreamCommand = new ViewModelCommand(ExecuteStartStream, CanExecuteStartStream);
            StopStreamCommand = new ViewModelCommand(ExecuteStopStream, CanExecuteStopStream);
        }


        public ObservableCollection<CameraDevice>? AvailableCameras {
            get => _availableCameras;
            set => SetProperty(ref _availableCameras, value);
        }

        public CameraDevice? SelectedCamera {
            get => _selectedCamera;
            set
            {
                if (SetProperty(ref _selectedCamera, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
        public bool IsStreaming {
            get => _isStreaming;
            set
            {
                if (SetProperty(ref _isStreaming, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public string QRCodeContent  // Property to store and bind QR code content to the view
        {
            get => _qrCodeContent;
            set => SetProperty(ref _qrCodeContent, value);
        }

        public BitmapSource? CurrentFrame {
            get => _currentFrame;
            set => SetProperty(ref _currentFrame, value);
        }

        public ICommand StartStreamCommand { get; }
        public ICommand StopStreamCommand { get; }

        private void LoadAvailableCameras()
        {
            AvailableCameras = new ObservableCollection<CameraDevice>();
            var cameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            for (int i = 0; i < cameras.Length; i++)
            {
                //AvailableCameras.Add(new CameraDevice
                //{
                //    CameraName = cameras[i].Name.ToString(),
                //    CameraIndex = i,
                //});
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

        private bool CanExecuteStartStream(object? parameter)
        {
            return SelectedCamera != null && !_isStreaming;
        }

        private bool CanExecuteStopStream(object? parameter)
        {
            return _isStreaming;
        }

        private async void ExecuteStartStream(object? parameter)
        {
            if (SelectedCamera == null) return;

            try
            {
                _videoCapture = new VideoCapture(SelectedCamera.CameraIndex);
                if (!_videoCapture.IsOpened())
                {
                    MessageBox.Show("Failed to open camera");
                    return;
                }
                _isStreaming = true;
                CommandManager.InvalidateRequerySuggested();

                await Task.Run(() => ProcessCameraStream());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting camera: {ex.Message}");
                _isStreaming = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void ExecuteStopStream(object? parameter)
        {
            if (_videoCapture != null)
            {
                _isStreaming = false;
                _videoCapture.Dispose();
                _videoCapture = null;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        
        private async Task ProcessCameraStream()
        {
            var qrCodeDectector = new QRCodeDetector();
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
                    string qrCodeData = qrCodeDectector.DetectAndDecode(frame,out points);
                    if(!string.IsNullOrEmpty(qrCodeData))
                    {
                        QRCodeContent = qrCodeData;
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


                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CurrentFrame = frame.ToBitmapSource();
                    });
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"Error processing frame: {ex.Message}");
                    });
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


