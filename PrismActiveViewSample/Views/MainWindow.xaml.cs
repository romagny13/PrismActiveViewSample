using Prism.Regions;
using PrismActiveViewSample.ViewModels;
using System.Windows;

namespace PrismActiveViewSample.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            (DataContext as MainWindowViewModel).InitializeRegion();
        }
    }
}
