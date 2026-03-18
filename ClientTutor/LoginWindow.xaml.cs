using ClientTutor.Services;
using ClientTutor.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.WebPages;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ClientTutor
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public bool IsSuccess { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка заполнения email
            if (string.IsNullOrWhiteSpace(EmailBox.Text))
            {
                MessageBox.Show("Введите email", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                EmailBox.Focus();
                return;
            }

            // Проверка корректности email
            if (!Helpers.ValidationHelper.IsValidEmailSimple(EmailBox.Text.Trim()))
            {
                MessageBox.Show(Helpers.ValidationHelper.GetEmailErrorMessage(),
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                EmailBox.Focus();
                return;
            }

            // Проверка пароля
            if (PasswordBox.Password.Length == 0)
            {
                MessageBox.Show("Введите пароль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                PasswordBox.Focus();
                return;
            }

            try
            {
                var client = new TutorClientService("127.0.0.1", 5555);
                var (success, userId, role, error) = await client.LoginAsync(
                    EmailBox.Text.Trim().ToLower(),
                    PasswordBox.Password
                );

                if (success)
                {
                    SessionManager.Login(userId, role);
                    IsSuccess = true;
                    DialogResult = true;
                }
                else
                {
                    string message;
                    if (error == "INVALID_CREDENTIALS")
                    {
                        message = "Неверный email или пароль";
                    }
                    else
                    {
                        message = "Ошибка подключения к серверу";
                    }
                    MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        // Очистка полей при открытии
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            EmailBox.Text = "";
            PasswordBox.Password = "";
            EmailBox.Focus();
        }
    }
}
