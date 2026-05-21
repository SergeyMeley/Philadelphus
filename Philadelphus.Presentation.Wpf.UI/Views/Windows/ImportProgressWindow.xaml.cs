using System.Windows;

namespace Philadelphus.Presentation.Wpf.UI.Views.Windows
{
    public partial class ImportProgressWindow : Window
    {
        public ImportProgressWindow()
        {
            InitializeComponent();
        }

        public void Initialize(string header, string status)
        {
            TxtHeader.Text = header;
            TxtStatus.Text = status;
        }

        public void UpdateStatus(string status)
        {
            if (IsLoaded == false)
                return;

            TxtStatus.Text = status;
        }

        public void Complete(string status)
        {
            if (IsLoaded == false)
                return;

            TxtStatus.Text = status;
            ProgressOperation.IsIndeterminate = false;
            ProgressOperation.Value = 100;
            BtnClose.IsEnabled = true;
        }

        public void Fail(string status)
        {
            if (IsLoaded == false)
                return;

            TxtStatus.Text = status;
            ProgressOperation.IsIndeterminate = false;
            ProgressOperation.Value = 0;
            BtnClose.IsEnabled = true;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
