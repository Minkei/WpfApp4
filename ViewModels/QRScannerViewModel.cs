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
using System.Windows.Media.Media3D;
using WpfApp4.Services;
using System.Windows.Controls;

namespace WpfApp4.ViewModels
{
    public class QRScannerViewModel : ViewModelBase
    {
        private readonly QRScannerService _qrScannerService = new();
        private string _qrCodeContent = string.Empty;
        private BitmapSource? _currentFrame;

        public QRScannerViewModel(QRScannerService qrScannerService)
        {
            //_qrScannerService = qrScannerService;
            _qrScannerService.QRCodeDetected += OnQRCodeDetected;
            _qrScannerService.FrameCaptured += OnFrameCaptured;
            _qrScannerService.LoadAvailableCameras();
            StartStreamCommand = new ViewModelCommand(ExecuteStartStream, CanExecuteStartStream);
            StopStreamCommand = new ViewModelCommand(ExecuteStopStream, CanExecuteStopStream);
        }


        public ObservableCollection<CameraDevice>? AvailableCameras {
            get => _qrScannerService.AvailableCameras;
            set
            {
                if (_qrScannerService.AvailableCameras != value)
                {
                    _qrScannerService.AvailableCameras = value;
                    OnPropertyChanged(nameof(AvailableCameras));
                }
            }
        }

        public CameraDevice? SelectedCamera {
            get => _qrScannerService.SelectedCamera;
            set
            {
                if (_qrScannerService.SelectedCamera != value)
                {
                    _qrScannerService.SelectedCamera = value;
                    OnPropertyChanged(nameof(SelectedCamera));
                }
            }
        }

        public bool IsStreaming {
            get => _qrScannerService.IsStreaming;
            set
            {
                if (_qrScannerService.IsStreaming != value)
                {
                    _qrScannerService.IsStreaming = value;
                    OnPropertyChanged(nameof(IsStreaming)); 
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



        private bool CanExecuteStartStream(object? parameter)
        {
            return SelectedCamera != null && !IsStreaming;
        }

        private bool CanExecuteStopStream(object? parameter)
        {
            return !IsStreaming;
        }


        private void ExecuteStartStream(object? parameter)
        {
            if (SelectedCamera == null) return;

            try
            {
                _qrScannerService.StartCamera(SelectedCamera.CameraIndex);
                CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting camera: {ex.Message}");
            }
        }

        private void ExecuteStopStream(object? parameter)
        {
            _qrScannerService.StopCamera();
            CommandManager.InvalidateRequerySuggested();
        }
        private void OnQRCodeDetected(object? sender, string qrCodeData)
        {
            QRCodeContent = qrCodeData;
        }
        private void OnFrameCaptured(object? sender, BitmapSource frame)
        {
            CurrentFrame = frame;
        }

    }
}


