using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace CraftAssist
{
    /// <summary>
    /// Interaction logic for FilePicker.xaml
    /// </summary>
    public partial class FilePicker : UserControl
    {
        public FilePicker()
        {
            InitializeComponent();
        }
        
        public delegate void FileSelectedEventHandler(object sender, FileSelectedEventArgs e);

        public event FileSelectedEventHandler FileSelected;

        private void BrowseForFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                this.FilePath.Text = openFileDialog.FileName;
                this.FileSelected?.Invoke(sender, new FileSelectedEventArgs() { FilePath = openFileDialog.FileName });
            }
        }
    }
}
