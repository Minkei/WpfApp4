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

namespace WpfApp4.ViewModels
{
    public class DataViewModel:ViewModelBase
    {
        //Fields
        private readonly QRCodeRepository? _qrCodeRepository;
        private ObservableCollection<QRCodeModel>? _qrCodeListFromSQL;
        private ObservableCollection<QRCodeModel>? _qrCodeListFromSQLFiltered;
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
