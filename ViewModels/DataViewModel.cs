using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp4.ViewModels;
using WpfApp4.Models;
using WpfApp4.Repositories;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Win32;
using OfficeOpenXml;
using System.IO;
using System.Windows;
using WpfApp4.Helper;

namespace WpfApp4.ViewModels
{
    public class DataViewModel:ViewModelBase
    {
        //Fields
        private readonly QRCodeRepository? _qrCodeRepository;
        private ObservableCollection<QRCodeModel>? _qrCodeListFromSQL;
        private ObservableCollection<QRCodeModel>? _qrCodeListFromSQLFiltered;
        private string? _currentUserAccountDisplayName;
        private bool _isLoading;
        private string? _searchText = string.Empty;
        private bool _isFiltered;
        private bool _isFilterPopUpOpen;
        private DateTime? _startDateFilter;
        private DateTime? _endDateFilter;
        private TimeSpan? _startTimeFilter;
        private TimeSpan? _endTimeFilter;
        private string? _picFilter;

        //Properties
        public bool ISFilterPopUpOpen {
            get { return _isFilterPopUpOpen; }
            set
            {
                _isFilterPopUpOpen = value;
                OnPropertyChanged(nameof(ISFilterPopUpOpen));
            }
        }
        public DateTime? StartDateFilter {
            get { return _startDateFilter; }
            set
            {
                _startDateFilter = value;
                OnPropertyChanged(nameof(StartDateFilter));
            }
        }
        public DateTime? EndDateFilter {
            get { return _endDateFilter; }
            set
            {
                _endDateFilter = value;
                OnPropertyChanged(nameof(EndDateFilter));
            }
        }
        public TimeSpan? StartTimeFilter {
            get { return _startTimeFilter; }
            set
            {
                _startTimeFilter = value;
                OnPropertyChanged(nameof(StartTimeFilter));
            }
        }
        public TimeSpan? EndTimeFilter {
            get { return _endTimeFilter; }
            set
            {
                _endTimeFilter = value;
                OnPropertyChanged(nameof(EndTimeFilter));
            }
        }
        public string? PICFilter {
            get { return _picFilter; }
            set
            {
                _picFilter = value;
                OnPropertyChanged(nameof(PICFilter));
            }
        }
        public string FilterIconName {
            get { return IsFiltered ? "Filter" : "FilterCircleXmark"; }
            set
            {
                OnPropertyChanged(nameof(FilterIconName));
            }
        }
        public bool IsFiltered {
            get { return _isFiltered; }
            set
            {
                _isFiltered = value;
                OnPropertyChanged(nameof(IsFiltered));
                OnPropertyChanged(nameof(FilterIconName));
            }
        }
        public ObservableCollection<QRCodeModel> QRCodeListFromSQL {
            get { return _qrCodeListFromSQL ??= new ObservableCollection<QRCodeModel>(); }
            set
            {
                _qrCodeListFromSQL = value;
                OnPropertyChanged(nameof(QRCodeListFromSQL));
            }
        }

        public ObservableCollection<QRCodeModel> QRCodeListFromSQLFiltered {
            get { return _qrCodeListFromSQLFiltered ??= new ObservableCollection<QRCodeModel>(); }
            set
            {
                _qrCodeListFromSQLFiltered = value;
                OnPropertyChanged(nameof(QRCodeListFromSQLFiltered));
            }
        }
        public string? SearchText {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    SearchCommand?.Execute(null); // Automatically trigger search when text changes
                }
            }
        }

        public bool IsLoading {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        //Commands
        public ICommand LoadDataCommand { get; }
        public ICommand ShowFilterPopUpCommand { get; } 
        public ICommand SearchCommand { get; }
        public ICommand ApplyFilterCommand { get; }
        public ICommand ResetFilterCommand { get; }
        public ICommand ExportFileCommand { get; }

        //Constructor
        public DataViewModel()
        {
            _searchText = string.Empty;
            _qrCodeRepository = new QRCodeRepository();
            QRCodeListFromSQL = new ObservableCollection<QRCodeModel>();
            LoadDataCommand = new ViewModelCommand(async _ => await LoadDataAsync());
            QRCodeListFromSQLFiltered = new ObservableCollection<QRCodeModel>();
            SearchCommand = new ViewModelCommand(_ => FilterQRCodeList(SearchText));
            ShowFilterPopUpCommand = new ViewModelCommand(_ => ToggleFilterPopup());
            ApplyFilterCommand = new ViewModelCommand(_ => ApplyFilter());
            ResetFilterCommand = new ViewModelCommand(_ => ResetFilter());
            ExportFileCommand = new ViewModelCommand(_ => ExportData());

        }
        //Methods
        public string GetCurrentUserAccountDisplayName()
        {
            var UserAccountModel = Mediator.Get<UserAccountModel>("CurrentUserAccount");
            return UserAccountModel?.DisplayName ?? "Default User";
        }
        private void ExportData()
        {
            try
            {
                // Get the current date and time to create the file name
                string dateTime = DateTime.Now.ToString("yyyy.MM.dd_HH.mm.ss");
                string currentUser = GetCurrentUserAccountDisplayName();

                // Create the file name following the format [QRCodeData] [Date] [Time] [PIC]
                string fileName = $"QRCodeData_{dateTime}_{currentUser}.xlsx";

                // Show the SaveFileDialog to allow the user to choose the file path and name
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    FileName = fileName
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;

                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                    // Use "using" to ensure the resource is disposed properly
                    using (var package = new ExcelPackage())
                    {
                        var worksheet = package.Workbook.Worksheets.Add("QRCodeData");

                        // Add headers for the columns
                        worksheet.Cells[1, 1].Value = "RowNumber";
                        worksheet.Cells[1, 2].Value = "QRCodeValue";
                        worksheet.Cells[1, 3].Value = "ScanDate";
                        worksheet.Cells[1, 4].Value = "ScanTime";
                        worksheet.Cells[1, 5].Value = "PIC";

                        // Apply header styles
                        using (var range = worksheet.Cells[1, 1, 1, 5])
                        {
                            range.Style.Font.Bold = true;
                            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                            range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        }

                        // Loop through the QRCodeList to add data to the Excel file
                        int row = 2; // Start from the second row after the header
                        foreach (var qrCode in QRCodeListFromSQLFiltered)
                        {
                            worksheet.Cells[row, 1].Value = qrCode.RowNumber;
                            worksheet.Cells[row, 2].Value = qrCode.QRCodeValue;

                            // Format ScanDate as yyyy-MM-dd
                            worksheet.Cells[row, 3].Value = qrCode.ScanDate;
                            worksheet.Cells[row, 3].Style.Numberformat.Format = "yyyy-MM-dd";

                            // Format ScanTime as HH:mm:ss
                            worksheet.Cells[row, 4].Value = qrCode.ScanTime;
                            worksheet.Cells[row, 4].Style.Numberformat.Format = "HH:mm:ss";

                            worksheet.Cells[row, 5].Value = qrCode.PIC;

                            row++;
                        }

                        // Auto-fit all columns
                        worksheet.Cells.AutoFitColumns();

                        // Save the Excel file
                        var fi = new FileInfo(filePath);
                        package.SaveAs(fi);

                        // Show a success message after export
                        MessageBox.Show("Data exported successfully to Excel!", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle errors during the export process
                MessageBox.Show($"Error exporting data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        public void ToggleFilterPopup()
        {
            ISFilterPopUpOpen = !ISFilterPopUpOpen;
        }
        public void ApplyFilter()
        {
            var filteredList = QRCodeListFromSQL.AsEnumerable();
            if(StartDateFilter.HasValue)
            {
                filteredList = filteredList.Where(qr => qr.ScanDate >= StartDateFilter);
            }
            if (EndDateFilter.HasValue)
            {
                filteredList = filteredList.Where(qr => qr.ScanDate <= EndDateFilter);
            }
            if (StartTimeFilter.HasValue)
            {
                filteredList = filteredList.Where(qr => qr.ScanTime >= StartTimeFilter);
            }
            if (EndTimeFilter.HasValue)
            {
                filteredList = filteredList.Where(qr => qr.ScanTime <= EndTimeFilter);
            }
            if (!string.IsNullOrEmpty(PICFilter))
            {
                filteredList = filteredList.Where(qr => qr.PIC.Contains(PICFilter, StringComparison.OrdinalIgnoreCase));
            }
            QRCodeListFromSQLFiltered = new ObservableCollection<QRCodeModel>(filteredList);
            IsFiltered = true;
            SearchText = string.Empty;
            if (ISFilterPopUpOpen)
            {
                ISFilterPopUpOpen = false;
            }
        }

        public void ResetFilter()
        {
            StartDateFilter = null;
            EndDateFilter = null;
            StartTimeFilter = null;
            EndTimeFilter = null;
            PICFilter = string.Empty;
            IsFiltered = false;
            ISFilterPopUpOpen = false;
            QRCodeListFromSQLFiltered = new ObservableCollection<QRCodeModel>(QRCodeListFromSQL);
        }
        public void FilterQRCodeList(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                // If search text is empty, show all data
                QRCodeListFromSQLFiltered = new ObservableCollection<QRCodeModel>(QRCodeListFromSQL);
            }
            else
            {
                var filteredList = QRCodeListFromSQL
                    .Where(qr =>
                        qr.QRCodeValue.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                        qr.ScanDate.ToString("dd/MM/yyyy").Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                         qr.ScanTime.ToString("hh\\:mm\\:ss").Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                        qr.PIC.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                QRCodeListFromSQLFiltered = new ObservableCollection<QRCodeModel>(filteredList);
            }
        }


        private async Task LoadDataAsync()
        {
            if (_qrCodeRepository == null)
            {
                throw new InvalidOperationException("QRCodeRepository is not initialized.");
            }

            IsLoading = true;
            try
            {
                var qrCodeListFromSQL = await _qrCodeRepository.GetAllQRCodesAsync();
                int rowNumber = 1;
                QRCodeListFromSQL.Clear();
                foreach (var qrCode in qrCodeListFromSQL)
                {
                    qrCode.RowNumber = rowNumber++;
                    QRCodeListFromSQL.Add(qrCode);
                }
                if(string.IsNullOrEmpty(SearchText))
                {
                    FilterQRCodeList(SearchText);
                }
                
                if (IsFiltered)
                {
                    ApplyFilter();
                }

            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
