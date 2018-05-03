using System.Windows;
using System.Windows.Controls;

namespace CraftAssist
{
    /// <summary>
    /// Interaction logic for SettingsControl.xaml
    /// </summary>
    public partial class SettingsControl : UserControl
    {
        public SettingsControl()
        {
            InitializeComponent();
        }
        
        public void VerboseLogFile_FileSelected(object sender, FileSelectedEventArgs e)
        {
            var viewModel = this.DataContext as CraftAssistViewModel;
            if (viewModel != null)
            {
                viewModel.VerboseLogFile = e.FilePath;
            }
        }

        public void FilteredLogFile_FileSelected(object sender, FileSelectedEventArgs e)
        {
            var viewModel = this.DataContext as CraftAssistViewModel;
            if (viewModel != null)
            {
                viewModel.FilteredLogFile = e.FilePath;
            }
        }

        public void VerboseLogFile_Loaded(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as CraftAssistViewModel;
            if (viewModel != null)
            {
                this.VerboseLogFile.FilePath.Text = viewModel.VerboseLogFile;
            }
        }

        public void FilteredLogFile_Loaded(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as CraftAssistViewModel;
            if (viewModel != null)
            {
                this.FilteredLogFile.FilePath.Text = viewModel.FilteredLogFile;
            }
        }
    }
}
