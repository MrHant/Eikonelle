using System.Windows;

namespace Eikonelle;

public partial class MainMenu : System.Windows.Controls.UserControl
{
    public MainMenu() => InitializeComponent();

    private void Settings_Click(object sender, RoutedEventArgs e) =>
        ((App)System.Windows.Application.Current).ShowSettings();
}
