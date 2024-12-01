using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace algos_base
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Start : Page
    {
        public Start()
        {
            InitializeComponent();
        }

        private void OpenTask1(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Task01());
        }

        private void OpenTask2(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Task02());
        }

        private void OpenTask3(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Task03());
        }
    }
}