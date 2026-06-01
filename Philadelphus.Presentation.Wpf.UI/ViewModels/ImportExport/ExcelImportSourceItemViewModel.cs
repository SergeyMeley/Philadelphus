using Philadelphus.Infrastructure.ImportExport.Excel;
using Philadelphus.Presentation.Wpf.UI.ViewModels;
using System;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ImportExport
{
    public class ExcelImportSourceItemViewModel : ViewModelBase
    {
        private readonly Action<ExcelImportSourceItemViewModel, bool> _includedChanged;
        private bool _isIncluded;
        private string _statusText = string.Empty;

        public ExcelImportSourceItemViewModel(
            ExcelImportSheetSchema sheet,
            ExcelPreviewSourceInfo sourceInfo,
            Action<ExcelImportSourceItemViewModel, bool> includedChanged)
        {
            Sheet = sheet;
            SourceInfo = sourceInfo;
            _includedChanged = includedChanged;
            _isIncluded = sheet.IsEnabled;
            _statusText = sourceInfo.StatusText;
        }

        public ExcelImportSheetSchema Sheet { get; }

        public ExcelPreviewSourceInfo SourceInfo { get; }

        public string Name => SourceInfo.Name;

        public int TotalRowCount => SourceInfo.TotalRowCount;

        public bool IsIncluded
        {
            get => _isIncluded;
            set
            {
                if (SetProperty(ref _isIncluded, value))
                {
                    _includedChanged(this, value);
                }
            }
        }

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public void SetIncludedSilently(bool value)
        {
            if (_isIncluded == value)
                return;

            _isIncluded = value;
            OnPropertyChanged(nameof(IsIncluded));
        }
    }
}
