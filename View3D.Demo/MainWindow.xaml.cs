using System.Windows;

namespace View3D.Demo;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var container = new Example();
        MainViewControl.Load(container);
    }
}