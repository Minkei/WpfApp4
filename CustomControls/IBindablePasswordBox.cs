using System.Security;

namespace WpfApp4.CustomControls
{
    public interface IBindablePasswordBox
    {
        SecureString Password { get; set; }

        void InitializeComponent();
    }
}