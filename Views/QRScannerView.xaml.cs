using Microsoft.VisualBasic;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WpfApp4.Models;
using WpfApp4.Services;
using WpfApp4.ViewModels;

namespace WpfApp4.Views
{
    public partial class QRScannerView : UserControl
    {
        //private DispatcherTimer? _timer;
        public QRScannerView()
        {
            InitializeComponent();
            /*StartClock()*/;
        }

        //private void StartClock()
        //{
        //    //Initialize the dispatcher timer
        //    _timer = new DispatcherTimer
        //    {
        //        Interval = TimeSpan.FromSeconds(1)
        //    };
        //    _timer.Tick += OnTimerTick;
        //    _timer.Start();
        //    //Display the initialize time
        //    UpdateClock();
        //    UpdateDate();

        //}

        //private void UpdateDate()
        //{
        //    TextBlockDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
        //}

        //private void OnTimerTick(object? sender, EventArgs e)
        //{
        //    UpdateClock();
        //}

        //private void UpdateClock()
        //{
        //    TextBlockClock.Text = DateAndTime.TimeString;
        //}
    }
}
