using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading; 
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfApp4.Models;
using WpfApp4.Repositories;
using WpfApp4.Views;

namespace WpfApp4.ViewModels
{
    public class MainViewModel:ViewModelBase
    {
        //Fields
        private UserAccountModel _currentUserAccount;
        private IUserRepository userRepository;
        private ViewModelBase _currentChildView;
        private string _caption;
        private IconChar _icon;

        public UserAccountModel CurrentUserAccount
        {
            get { return _currentUserAccount; } 
            set { _currentUserAccount = value; OnPropertyChanged(nameof(CurrentUserAccount)); } 
        }

        public ViewModelBase CurrentChildView { get => _currentChildView; set { _currentChildView = value; OnPropertyChanged(nameof(CurrentChildView)); } }
        public string Caption { get => _caption; set { _caption = value; OnPropertyChanged(nameof(Caption)); } }
        public IconChar Icon { get => _icon; set { _icon = value; OnPropertyChanged(nameof(Icon)); } }
        public ICommand ShowQRScannerView { get; }
        public ICommand ShowDataView { get; }  
        public MainViewModel()
        {
            userRepository = new UserRepository();
            CurrentUserAccount = new UserAccountModel();
            //Initialize commands
            ShowQRScannerView = new ViewModelCommand(ExecuteShowQRScannerViewCommand);
            ShowDataView = new ViewModelCommand(ExecuteShowDataViewCommand);
            //Default view
            ExecuteShowQRScannerViewCommand(null);
            LoadCurrentUserData();
        }

        private void ExecuteShowDataViewCommand(object obj)
        {
            CurrentChildView = new DataViewModel();
            Caption = "Data";
            Icon = IconChar.Database;
        }

        private void ExecuteShowQRScannerViewCommand(object obj)
        {
            CurrentChildView = new QRScannerViewModel();
            Caption = "QR Scanner";
            Icon = IconChar.Qrcode;
        }

        private void LoadCurrentUserData()
        {
            var user = userRepository.GetByUsername(Thread.CurrentPrincipal.Identity.Name);
            if (user != null)
            {
                CurrentUserAccount.Username = user.UserName;
                CurrentUserAccount.DisplayName = $"{user.LastName} { user.Name}";
                CurrentUserAccount.ProfilePicture = null;
            }
            else
            {
                CurrentUserAccount.DisplayName = $"Invalid user, not logged in";
            }
        }
    }
}
