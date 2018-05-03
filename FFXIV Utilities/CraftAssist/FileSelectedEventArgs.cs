namespace CraftAssist
{
    using System.Windows;

    public class FileSelectedEventArgs : RoutedEventArgs
    {
        public string FilePath { get; set; }
    }
}
