using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Automation;
using System.Windows.Interop;
using WpfApp4.ViewModels;
using WpfApp4.Views;
using System.Windows.Threading;
using Microsoft.VisualBasic;

namespace WpfApp4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : Window
    {
        private DispatcherTimer? _timer;
        public MainView()
        {
            InitializeComponent();
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            StartClock();
        }

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWndm, int wMsg, int wParam, IntPtr lParam);

        private void pnlControlBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SendMessage(helper.Handle, 161, 2, 0);
        }

        private void pnlControlBar_MouseEnter(object sender, MouseEventArgs e)
        {
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if(this.WindowState==WindowState.Normal)
                this.WindowState = WindowState.Maximized;
            else this.WindowState = WindowState.Normal;
        }




        private void StartClock()
        {
            //Initialize the dispatcher timer
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += OnTimerTick;
            _timer.Start();
            //Display the initialize time
            UpdateClock();
            UpdateDate();

        }

        private void UpdateDate()
        {
            TextBlockDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            UpdateClock();
        }

        private void UpdateClock()
        {
            TextBlockClock.Text = DateAndTime.TimeString;
        }
    }
}