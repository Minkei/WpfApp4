using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Media;
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
		private DataViewModel? _dataViewModel;
		private BitmapSource? _currentFrame;
		public CameraDevice? _selectedCamera;
		private IconChar _icon;
		public ObservableCollection<QRCodeModel>? _qrCodeList = new ObservableCollection<QRCodeModel>();
		private UserAccountModel? _currentUserAccount;
		private IUserRepository userRepository;
		private QRCodeRepository _qrCodeRepository;
		private ViewModelBase? _currentViewModel;

		private string? _streamingStatusString = String.Empty;
		private string? _cameraOverview = string.Empty;
		private string? _qrCodeContent = string.Empty;
		private string? _lastQrCodeContent = string.Empty;
		private string? _caption;

		private bool _isQRScannerViewVisible = true;
		private bool _isDataViewVisible = false;
		private bool _isReportViewVisible = false;
		private bool _isSettingViewVisible = false;
		private bool _isSupportViewVisible = false;
		private bool _streamingStatusBool = false;
		private bool _isSilentMode = false;

		//Properties
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

		public ObservableCollection<QRCodeModel>? QRCodeListFromUI {
			get => _qrCodeList;
			set
			{
				_qrCodeList = value;
				OnPropertyChanged(nameof(QRCodeListFromUI));
			}
		}


		public BitmapSource? CurrentFrame {
			get => _currentFrame;
			set => SetProperty(ref _currentFrame, value);
		}
		public UserAccountModel? CurrentUserAccount {
			get { return _currentUserAccount; }
			set { _currentUserAccount = value; OnPropertyChanged(nameof(CurrentUserAccount)); }
		}
		public ViewModelBase? CurrentViewModel {
			get { return _currentViewModel; }
			set { _currentViewModel = value; OnPropertyChanged(nameof(CurrentViewModel)); }
		}
		public DataViewModel? DataViewModel {
			get { return _dataViewModel; }
			set { _dataViewModel = value; OnPropertyChanged(nameof(DataViewModel)); }
		}

		public string? QRCodeContent {
			get => _qrCodeContent;
			set => SetProperty(ref _qrCodeContent, value);
		}
		public string? Caption { get => _caption; set { _caption = value; OnPropertyChanged(nameof(Caption)); } }
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
		public string? StreamingStatusString {
			get => _streamingStatusString;
			set
			{
				_streamingStatusString = value;
				OnPropertyChanged(nameof(StreamingStatusString));
			}
		}
		public bool IsSilentMode {
			get => _isSilentMode;
			set
			{
				_isSilentMode = value;
				OnPropertyChanged(nameof(IsSilentMode));
				OnPropertyChanged(nameof(BellIcon));
            }
		}
		public string BellIcon {
            get { return IsSilentMode ? "BellSlash" : "Bell"; }
        }
        public bool StreamingStatusBool {
			get => _streamingStatusBool;
			set
			{
				_streamingStatusBool = value;
				OnPropertyChanged(nameof(StreamingStatusBool));
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
		public ICommand ToggleSilentModeCommand { get; }


        //Constructor
        public MainViewModel()
		{
			//Initialize
			_qrCodeRepository = new QRCodeRepository();
			userRepository = new UserRepository();
			CurrentUserAccount = new UserAccountModel();
			_qrScannerService = new QRScannerService();
			_dataViewModel = new DataViewModel();

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
			_qrScannerService.StreamingStatusStringChanged += OnStreamingStatusStringChanged;
			_qrScannerService.StreamingStatusBoolChanged += OnStreamingStatusBoolChanged;
			_qrScannerService.CameraOverviewChanged += CameraOverviewChanged;

			//Commands
			StartStreamCommand = new ViewModelCommand(ExecuteStartStream, CanExecuteStartStream);
			StopStreamCommand = new ViewModelCommand(ExecuteStopStream, CanExecuteStopStream);
            ToggleSilentModeCommand = new ViewModelCommand(ExecuteSilentModeCommand);

        }

        private void ExecuteSilentModeCommand(object? obj)
        {
            IsSilentMode = !IsSilentMode;
        }

        //Methods
        private void ExecuteShowSupportViewCommand(object? obj)
		{
			IsQRScannerViewVisible = false;
			IsDataViewVisible = false;
			IsReportViewVisible = false;
			IsSettingViewVisible = false;
			IsSupportViewVisible = true;
			CurrentViewModel = new SupportViewModel();

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
			CurrentViewModel = new SettingViewModel();

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
			CurrentViewModel = new ReportViewModel();

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
			CurrentViewModel = new DataViewModel();

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
			return SelectedCamera != null && !StreamingStatusBool;
		}
		private bool CanExecuteStopStream(object? parameter)
		{
			return StreamingStatusBool;
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
		private void PlaySoundQRCodeDetected()
		{
			if(IsSilentMode)
			{
				Console.WriteLine("Silent mode is ON. Sound will not be played");
				return;
			}
			try
			{
				//string soundPath = "Audio/short-beep-tone-47916.wav";
				string soundPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Audio", "short-beep-tone-47916.wav");
				if (System.IO.File.Exists(soundPath))
				{
					using var player = new SoundPlayer(soundPath);
					player.PlaySync();
				}
				else
				{
					Console.WriteLine($"Sound file not found: {soundPath}");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error playing system sound: {ex.Message}");
			}
		}


		//Events
		private async void OnQRCodeDetected(object? sender, string qrCodeData)
		{
			if (qrCodeData != _lastQrCodeContent)
			{
				PlaySoundQRCodeDetected();
				_lastQrCodeContent = qrCodeData;
				//Display the QR Code content 
				QRCodeContent = qrCodeData;

				//Save the QR Code content
				var qrCodeModel = new QRCodeModel()
				{
					QRCodeValue = qrCodeData,
					ScanDate = DateTime.Now.Date,
					ScanTime = DateTime.Now.TimeOfDay,
					PIC = CurrentUserAccount?.DisplayName
				};
				await _qrCodeRepository.AddQRCodeAsync(qrCodeModel);
				Application.Current.Dispatcher.Invoke(() => QRCodeListFromUI.Add(qrCodeModel));
			}
		}
		private void OnFrameCaptured(object? sender, BitmapSource frame)
		{
			CurrentFrame = frame;
		}
		private void OnStreamingStatusStringChanged(object? sender, string is_streaming_string)
		{
			StreamingStatusString = is_streaming_string;
		}
		private void OnStreamingStatusBoolChanged(object? sender, bool is_streaming_bool)
		{
			StreamingStatusBool = is_streaming_bool;
		}
		private void CameraOverviewChanged(object? sender, string cameraOverview)
		{
			CameraOverview = cameraOverview;
		}
	}
}
