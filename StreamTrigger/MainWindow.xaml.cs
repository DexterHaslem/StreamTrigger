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

        private void OnClick(object sender, RoutedEventArgs e)
        {
            vm.OnFindScriptFile();
        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            vm.OnWindowClosing();
        }
    }
}
