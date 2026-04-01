using Avalonia.Controls;
using DidiApp.ViewModels;

namespace DidiApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
  
        DataContext = new MainViewModel();
    }
}
