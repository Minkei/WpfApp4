using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading; 
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WpfApp4.Models;
using WpfApp4.Repositories;
using WpfApp4.Services;
using WpfApp4.Views;

namespace WpfApp4.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        //Fields
        public readonly QRScannerService _qrScannerService = new();
        private string _qrCodeContent = string.Empty;
        private BitmapSource? _currentFrame;
        private string _streamingStatus = String.Empty;
        private string _cameraOverview = string.Empty;
        public CameraDevice? _selectedCamera;
        public ObservableCollection<CameraDevice>? _availableCameras;
        private UserAccountModel? _currentUserAccount;
        private IUserRepository userRepository;
        private ViewModelBase? _currentChildView;
        private string? _caption;
        private IconChar _icon;
        private QRScannerViewModel _qrScannerViewModel;
        private bool _isQRScannerViewVisible = true;
        private bool _isDataViewVisible = false;
        private bool _isReportViewVisible = false;
        private bool _isSettingViewVisible = false;
        private bool _isSupportViewVisible = false;

        //Properties
        public string QRCodeContent {
            get => _qrCodeContent;
            set => SetProperty(ref _qrCodeContent, value);
        }
        public BitmapSource? CurrentFrame {
            get => _currentFrame;
            set => SetProperty(ref _currentFrame, value);
        }
        public UserAccountModel? CurrentUserAccount {
            get { return _currentUserAccount; }
            set { _currentUserAccount = value; OnPropertyChanged(nameof(CurrentUserAccount)); }
        }
        public ViewModelBase? CurrentChildView { get => _currentChildView; set { _currentChildView = value; OnPropertyChanged(nameof(CurrentChildView)); } }
        public string? Caption { get => _caption; set { _caption = value; OnPropertyChanged(nameof(Caption)); } }
        public IconChar Icon { get => _icon; set { _icon = value; OnPropertyChanged(nameof(Icon)); } }
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
        public string? CameraOverview {
            get => _qrScannerService.CameraOverview;
            set
            {
                if (_qrScannerService.CameraOverview != value)
                {
                    _qrScannerService.CameraOverview = value;
                    OnPropertyChanged(nameof(CameraOverview));
                }
            }
        }
        public string StreamingStatus {
            get => _streamingStatus;
            set
            {
                _streamingStatus = value;
                OnPropertyChanged(nameof(StreamingStatus));
            }
        }
        public QRScannerViewModel QRScannerViewModel {
            get => _qrScannerViewModel;
            set
            {
                if (_qrScannerViewModel != value)
                {
                    _qrScannerViewModel = value;
                    OnPropertyChanged(nameof(QRScannerViewModel));
                }
            }
        }
        public bool IsQRScannerViewVisible {
            get { return _isQRScannerViewVisible; }
            set { _isQRScannerViewVisible = value; OnPropertyChanged(nameof(IsQRScannerViewVisible)); }
        }
        public bool IsDataViewVisible {
            get { return _isDataViewVisible; }
            set { _isDataViewVisible = value; OnPropertyChanged(nameof(IsDataViewVisible)); }
        }
        public bool IsSettingViewVisible {
            get { return _isSettingViewVisible; }
            set { _isSettingViewVisible = value; OnPropertyChanged(nameof(IsSettingViewVisible)); }
        }
        public bool IsReportViewVisible {
            get { return _isReportViewVisible; }
            set { _isReportViewVisible = value; OnPropertyChanged(nameof(IsReportViewVisible)); }
        }
        public bool IsSupportViewVisible {
            get { return _isSupportViewVisible; }
            set { _isSupportViewVisible = value; OnPropertyChanged(nameof(IsSupportViewVisible)); }
        }


        //Commands
        public ICommand ShowQRScannerView { get; }
        public ICommand ShowDataView { get; }
        public ICommand ShowReportView { get; }
        public ICommand ShowSettingView { get; }
        public ICommand ShowSupportView { get; }
        public ICommand StartStreamCommand { get; }
        public ICommand StopStreamCommand { get; }


        //Constructor
        public MainViewModel()
        {
            //Initialize
            _qrScannerViewModel = new QRScannerViewModel();
            userRepository = new UserRepository();
            CurrentUserAccount = new UserAccountModel();
            _qrScannerService = new QRScannerService();
            ShowQRScannerView = new ViewModelCommand(ExecuteShowQRScannerViewCommand);
            ShowDataView = new ViewModelCommand(ExecuteShowDataViewCommand);
            ShowReportView = new ViewModelCommand(ExecuteShowReportViewCommand);
            ShowSettingView = new ViewModelCommand(ExecuteShowSettingViewCommand);
            ShowSupportView = new ViewModelCommand(ExecuteShowSupportViewCommand);

            //Default view
            ExecuteShowQRScannerViewCommand(new object());
            LoadCurrentUserData();

            //QR Scanner
            if (_qrScannerService.SelectedCamera == null) _qrScannerService.LoadAvailableCameras();
            _qrScannerService.QRCodeDetected += OnQRCodeDetected;
            _qrScannerService.FrameCaptured += OnFrameCaptured;
            _qrScannerService.StreamingStatusChanged += OnStreamingStatusChanged;
            _qrScannerService.CameraOverviewChanged += CameraOverviewChanged;

            //Commands
            StartStreamCommand = new ViewModelCommand(ExecuteStartStream, CanExecuteStartStream);
            StopStreamCommand = new ViewModelCommand(ExecuteStopStream, CanExecuteStopStream);
        }

        //Methods
        private void ExecuteShowSupportViewCommand(object? obj)
        {
            IsQRScannerViewVisible = false;
            IsDataViewVisible = false;
            IsReportViewVisible = false;
            IsSettingViewVisible = false;
            IsSupportViewVisible = true;

            Caption = "Support";
            Icon = IconChar.Headset;
        }
        private void ExecuteShowSettingViewCommand(object? obj)
        {
            IsQRScannerViewVisible = false;
            IsDataViewVisible = false;
            IsReportViewVisible = false;
            IsSettingViewVisible = true;
            IsSupportViewVisible = false;

            Caption = "Setting";
            Icon = IconChar.Gear;
        }
        private void ExecuteShowReportViewCommand(object? obj)
        {
            IsQRScannerViewVisible = false;
            IsDataViewVisible = false;
            IsReportViewVisible = true;
            IsSettingViewVisible = false;
            IsSupportViewVisible = false;

            Caption = "Report";
            Icon = IconChar.PieChart;
        }
        private void ExecuteShowDataViewCommand(object? obj)
        {
            IsQRScannerViewVisible = false;
            IsDataViewVisible = true;
            IsReportViewVisible = false;
            IsSettingViewVisible = false;
            IsSupportViewVisible = false;

            Caption = "Data";
            Icon = IconChar.Database;
        }
        private void ExecuteShowQRScannerViewCommand(object? obj)
        {
            IsQRScannerViewVisible = true;
            IsDataViewVisible = false;
            IsReportViewVisible = false;
            IsSettingViewVisible = false;
            IsSupportViewVisible = false;

            if (StreamingStatus == "Stopped")
            {
                QRScannerViewModel = new QRScannerViewModel();
            }
            Caption = "QR Scanner";
            Icon = IconChar.Qrcode;
        }
        private void LoadCurrentUserData()
        {
            var identity = Thread.CurrentPrincipal?.Identity;
            if (identity != null && !string.IsNullOrEmpty(identity.Name))
            {
                var user = userRepository.GetByUsername(identity.Name);
                if (user != null)
                {
                    CurrentUserAccount.Username = user?.UserName ?? string.Empty;
                    CurrentUserAccount.DisplayName = $"{user?.LastName ?? string.Empty} {user?.Name ?? string.Empty}";
                    CurrentUserAccount.ProfilePicture = Array.Empty<byte>();
                }
                else
                {
                    CurrentUserAccount.DisplayName = $"Invalid user, not logged in";
                }
            }
            else
            {
                CurrentUserAccount.DisplayName = $"Invalid user, not logged in";
            }
        }
        private bool CanExecuteStartStream(object? parameter)
        {
            return SelectedCamera != null && !IsStreaming;
        }
        private bool CanExecuteStopStream(object? parameter)
        {
            return IsStreaming;
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

        //Events
        private void OnQRCodeDetected(object? sender, string qrCodeData)
        {
            QRCodeContent = qrCodeData;
        }
        private void OnFrameCaptured(object? sender, BitmapSource frame)
        {
            CurrentFrame = frame;
        }
        private void OnStreamingStatusChanged(object? sender, string status)
        {
            StreamingStatus = status;
        }
        private void CameraOverviewChanged(object? sender, string cameraOverview)
        {
            CameraOverview = cameraOverview;
        }
    }
}
