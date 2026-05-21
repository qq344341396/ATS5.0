using System.Windows.Controls;
using ATS5.Application.DataQuery;
using ATS5.Modules.DataQuery.ViewModels;

namespace ATS5.Modules.DataQuery.Views
{
    public partial class DataQueryView : UserControl
    {
        public DataQueryView()
        {
            InitializeComponent();
        }

        private void RecordsGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(DataContext is DataQueryViewModel viewModel))
            {
                return;
            }

            viewModel.SelectedUploadRecords.Clear();
            foreach (var selectedItem in RecordsGrid.SelectedItems)
            {
                if (selectedItem is TestRecord record)
                {
                    viewModel.SelectedUploadRecords.Add(record);
                }
            }
        }
    }
}
