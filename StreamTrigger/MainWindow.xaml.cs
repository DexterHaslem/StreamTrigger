using System.Windows;

namespace StreamTrigger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ViewModel vm;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = vm = new ViewModel(this);
        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            vm.OnWindowClosing();
        }

        private void OnFindOfflineFileClick(object sender, RoutedEventArgs e)
        {
            vm.OnFindExecutableFile(true);
        }

        private void OnFindOnlineFileClick(object sender, RoutedEventArgs e)
        {
            vm.OnFindExecutableFile(false);
        }

        private void OnShowAbout(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(this, "May Markniisaan never mangle his body in a work accident \r\nand be slave to the bluecollar for life\r\nOR perhaps go to collage and get educated", "About");
        }

        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
