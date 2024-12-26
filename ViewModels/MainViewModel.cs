using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
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
    public class MainViewModel:ViewModelBase
    {
        //Fields
        private readonly QRScannerService _qrScannerService = new();
        private UserAccountModel? _currentUserAccount;
        private IUserRepository userRepository;
        private ViewModelBase? _currentChildView;
        private string? _caption;
        private IconChar _icon;
        private string _qrCodeContent = string.Empty;
        private BitmapSource? _currentFrame;
        public string QRCodeContent  // Property to store and bind QR code content to the view
        {
            get => _qrCodeContent;
            set => SetProperty(ref _qrCodeContent, value);
        }

        public BitmapSource? CurrentFrame {
            get => _currentFrame;
            set => SetProperty(ref _currentFrame, value);
        }

        public UserAccountModel? CurrentUserAccount
        {
            get { return _currentUserAccount; } 
            set { _currentUserAccount = value; OnPropertyChanged(nameof(CurrentUserAccount)); } 
        }

        public ViewModelBase? CurrentChildView { get => _currentChildView; set { _currentChildView = value; OnPropertyChanged(nameof(CurrentChildView)); } }
        public string? Caption { get => _caption; set { _caption = value; OnPropertyChanged(nameof(Caption)); } }
        public IconChar Icon { get => _icon; set { _icon = value; OnPropertyChanged(nameof(Icon)); } }
        public ICommand ShowQRScannerView { get; }
        public ICommand ShowDataView { get; }  
        public ICommand ShowReportView { get; }
        public ICommand ShowSettingView { get; }    
        public ICommand ShowSupportView { get; }    
        public MainViewModel()
        {

            userRepository = new UserRepository();
            CurrentUserAccount = new UserAccountModel();
            //Initialize commands
            ShowQRScannerView = new ViewModelCommand(ExecuteShowQRScannerViewCommand);
            ShowDataView = new ViewModelCommand(ExecuteShowDataViewCommand);
            ShowReportView = new ViewModelCommand(ExecuteShowReportViewCommand);
            ShowSettingView = new ViewModelCommand(ExecuteShowSettingViewCommand);
            ShowSupportView = new ViewModelCommand(ExecuteShowSupportViewCommand);
            //Default view
            ExecuteShowQRScannerViewCommand(new object());
            LoadCurrentUserData();
            //
            _qrScannerService.QRCodeDetected += OnQRCodeDetected;
            _qrScannerService.FrameCaptured += OnFrameCaptured;
        }

        private void ExecuteShowSupportViewCommand(object? obj)
        {
            CurrentChildView = new SupportViewModel();
            Caption = "Support";
            Icon = IconChar.Headset;
        }

        private void ExecuteShowSettingViewCommand(object? obj)
        {
            CurrentChildView = new SettingViewModel();
            Caption = "Setting";
            Icon = IconChar.Gear;
        }

        private void ExecuteShowReportViewCommand(object? obj)
        {
            CurrentChildView = new ReportViewModel();
            Caption = "Report";
            Icon = IconChar.PieChart;
        }

        private void ExecuteShowDataViewCommand(object? obj)
        {
            CurrentChildView = new DataViewModel();
            Caption = "Data";
            Icon = IconChar.Database;
        }

        private void ExecuteShowQRScannerViewCommand(object? obj)
        {
            CurrentChildView = new QRScannerViewModel(_qrScannerService);
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
