using System.Windows;

namespace ShopSimpleWpf
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            bool isAdmin = RoleComboBox.SelectedIndex == 0;
            var mainWindow = new MainWindow(isAdmin);
            mainWindow.Show();
            Close();
        }
    }
}
