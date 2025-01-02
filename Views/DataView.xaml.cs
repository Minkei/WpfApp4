using System;
using System.Collections.Generic;
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
using WpfApp4.ViewModels;


namespace WpfApp4.Views
{
    /// <summary>
    /// Interaction logic for DataView.xaml
    /// </summary>
    public partial class DataView : UserControl
    {
        private readonly DataViewModel? _viewModel;
        public DataView()
        {
            InitializeComponent();
            // Ensure the DataContext is set to an instance of DataViewModel
            _viewModel = new DataViewModel();
            this.DataContext = _viewModel;

            // Subscribe to the Loaded event programmatically
            this.Loaded += DataView_Loaded;
        }

        // Handle the Loaded event
        private void DataView_Loaded(object sender, RoutedEventArgs e)
        {
            // Ensure _viewModel is not null
            if (_viewModel != null)
            {
                // Execute the LoadDataCommand when the DataView is loaded
                _viewModel.LoadDataCommand.Execute(null);
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //when textbox text changed, check if it is empty, if yes, show the placeholder text
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                PlaceholderText.Visibility = Visibility.Visible;
            }
            else
            {
                PlaceholderText.Visibility = Visibility.Collapsed;
            }
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            //when textbox got focus, hide the placeholder text
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                PlaceholderText.Visibility = Visibility.Visible;
            }
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            //when textbox lost focus, check if it is empty, if yes, show the placeholder text
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                PlaceholderText.Visibility = Visibility.Visible;
            }
            else
            {
                PlaceholderText.Visibility = Visibility.Collapsed;
            }
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            //when user press enter key, execute the search command
            if (e.Key == Key.Enter)
            {
                if (_viewModel != null)
                {
                    _viewModel.SearchCommand.Execute(null);
                }
            }
        }

        private void txStartTimeFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (_viewModel != null)
                {
                    _viewModel.ApplyFilterCommand.Execute(null);
                }
            }
        }

        private void txEndTimeFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                if (_viewModel != null)
                {
                    _viewModel.ApplyFilterCommand.Execute(null);
                }
            }
        }

        private void txPICFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                if (_viewModel != null)
                {
                    _viewModel.ApplyFilterCommand.Execute(null);
                }
            }
        }
    }

    public static class TextBoxHelper
    {
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.RegisterAttached(
                "Placeholder",
                typeof(string),
                typeof(TextBoxHelper),
                new FrameworkPropertyMetadata(string.Empty, OnPlaceholderChanged));

        public static string GetPlaceholder(DependencyObject obj) => (string)obj.GetValue(PlaceholderProperty);
        public static void SetPlaceholder(DependencyObject obj, string value) => obj.SetValue(PlaceholderProperty, value);

        private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                textBox.GotFocus -= RemovePlaceholder;
                textBox.LostFocus -= ShowPlaceholder;

                if (!string.IsNullOrEmpty((string)e.NewValue))
                {
                    textBox.GotFocus += RemovePlaceholder;
                    textBox.LostFocus += ShowPlaceholder;
                    ShowPlaceholder(textBox, null); // Initialize placeholder
                }
            }
        }

        private static void ShowPlaceholder(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Text = GetPlaceholder(textBox);
                textBox.Foreground = Brushes.Gray;
            }
        }

        private static void RemovePlaceholder(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Text == GetPlaceholder(textBox))
            {
                textBox.Text = string.Empty;
                textBox.Foreground = Brushes.Black;
            }
        }
    }
}

