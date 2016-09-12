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
    }
}
