using ClientTutor.Services;
using ClientTutor.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    
    public partial class RegisterStudentWindow : Window
    {
        public bool IsSuccess { get; private set; }

        public RegisterStudentWindow()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (string.IsNullOrWhiteSpace(LastNameBox.Text) ||
                string.IsNullOrWhiteSpace(FirstNameBox.Text))
            {
                MessageBox.Show("Заполните фамилию и имя",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            
            if (string.IsNullOrWhiteSpace(EmailBox.Text))
            {
                MessageBox.Show("Введите email",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            
            if (!ValidationHelper.IsValidEmailSimple(EmailBox.Text.Trim()))
            {
                MessageBox.Show(ValidationHelper.GetEmailErrorMessage(),
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                EmailBox.Focus();
                return;
            }

            
            if (PasswordBox.Password.Length < 3)
            {
                MessageBox.Show("Пароль должен быть не менее 3 символов",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                PasswordBox.Focus();
                return;
            }

            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Пароли не совпадают",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                ConfirmPasswordBox.Focus();
                return;
            }

            try
            {
                var client = new TutorClientService("127.0.0.1", 5555);
                var (success, studentId, error) = await client.RegisterStudentAsync(
                    LastNameBox.Text.Trim(),
                    FirstNameBox.Text.Trim(),
                    MiddleNameBox.Text.Trim(),
                    EmailBox.Text.Trim().ToLower(), 
                    PasswordBox.Password,
                    GoalsBox.Text.Trim(),
                    SubjectsBox.Text.Trim()
                );

                if (success)
                {
                    MessageBox.Show(" Регистрация успешна! Теперь вы можете войти.",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    IsSuccess = true;
                    DialogResult = true;
                }
                else
                {
                    string message;
                    if (error == "EMAIL_EXISTS")
                    {
                        message = "Пользователь с таким email уже существует";
                    }
                    else
                    {
                        message = "Ошибка регистрации";
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

        
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            LastNameBox.Text = "";
            FirstNameBox.Text = "";
            MiddleNameBox.Text = "";
            EmailBox.Text = "";
            PasswordBox.Password = "";
            ConfirmPasswordBox.Password = "";
            GoalsBox.Text = "";
            SubjectsBox.Text = "";
            LastNameBox.Focus();
        }
    }
}
