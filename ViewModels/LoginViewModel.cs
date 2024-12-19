using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Input;
using WpfApp4.Models;
using WpfApp4.Repositories;

namespace WpfApp4.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        //Fields
        private string _username = string.Empty;
        private SecureString _password;
        private string _errorMessage = string.Empty;
        private bool _isViewVisible = true;
        private IUserRepository userRepository;

        //Properties
        public string Username { get { return _username; } set { _username = value; OnPropertyChanged(nameof(Username)); } }
        public SecureString Password { get { return _password; } set { _password = value; OnPropertyChanged(nameof(Password)); } }
        public string ErrorMessage { get { return _errorMessage; } set { _errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); } }
        public bool IsViewVisible { get { return _isViewVisible; } set { _isViewVisible = value; OnPropertyChanged(nameof(IsViewVisible)); } }

        //Commands
        public ICommand LoginCommand { get; }
        public ICommand RecoverPasswordCommand { get; }
        public ICommand ShowPasswordCommand { get; }
        public ICommand RemenberPasswordCommand { get; }

        //Constructor
        public LoginViewModel()
        {
            userRepository = new UserRepository();
            LoginCommand = new ViewModelCommand(ExecuteLoginCommand, CanExecuteLoginCommand);
            RecoverPasswordCommand = new ViewModelCommand(p=>ExecuteRecoverPasswordCommand("", ""));
        }
        
        private bool CanExecuteLoginCommand(object obj)
        {
            bool validData;
            if (string.IsNullOrWhiteSpace(Username) || Username.Length < 3 || Password == null || Password.Length < 3) validData = false; else validData = true;
            return validData;
        }
        private void ExecuteLoginCommand(object obj)
        {
            var isValidUser = userRepository.AuthenticatesUser(new NetworkCredential(Username, Password));
            if (isValidUser)
            {
                Thread.CurrentPrincipal = new GenericPrincipal(
                    new GenericIdentity(Username), null);
                IsViewVisible = false;
            }
            else
            {
                ErrorMessage = "* Invalid username or password";
            }
        }

        private void ExecuteRecoverPasswordCommand(string username, string email)
        {
            throw new NotImplementedException();
        }
    }
}
