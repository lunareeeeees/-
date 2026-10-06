using System;
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
            try
            {
                var database = new Database();
                string role = database.Login(LoginTextBox.Text, PasswordTextBox.Password);

                if (role == null)
                {
                    MessageBox.Show("Неверный логин или пароль.");
                    return;
                }

                var window = new MainWindow(role);
                window.Show();
                Close();
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message, "Ошибка подключения");
            }
        }
    }
}
