using ClientTutor.Models;
using ClientTutor.Services;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClientTutor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly TutorClientService _service;
        private List<Teacher> _teachers = new List<Teacher>();
        private List<Student> _students = new List<Student>();
        private List<Subject> _subjects = new List<Subject>();

        public MainWindow()
        {
            InitializeComponent();

            _service = new TutorClientService("127.0.0.1", 5555);

            // Загрузка данных
            Loaded += async (s, e) =>
            {
                await LoadSubjectsAsync();
                await LoadTeachersAsync();
            };

            UpdateUIBasedOnAuth();
        }

        private void UpdateUIBasedOnAuth()
        {
            if (SessionManager.IsLoggedIn)
            {
                UserInfoText.Text = $"{SessionManager.CurrentUserRole}: {SessionManager.CurrentUserId}";
                LoginButton.Visibility = Visibility.Collapsed;
                RegisterButton.Visibility = Visibility.Collapsed;
                LogoutButton.Visibility = Visibility.Visible;

                // Кнопка админа видна только администратору
                AdminButton.Visibility = SessionManager.IsAdmin ? Visibility.Visible : Visibility.Collapsed;

                // Кнопка добавления учителя только для админа
                AddTeacherButton.Visibility = SessionManager.IsAdmin ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                UserInfoText.Text = "";
                LoginButton.Visibility = Visibility.Visible;
                RegisterButton.Visibility = Visibility.Visible;
                LogoutButton.Visibility = Visibility.Collapsed;
                AdminButton.Visibility = Visibility.Collapsed;
                AddTeacherButton.Visibility = Visibility.Collapsed;
            }
        }

        private async Task LoadSubjectsAsync()
        {
            try
            {
                _subjects = await _service.GetSubjectsAsync();

                SubjectFilterCombo.ItemsSource = _subjects;
                SubjectFilterCombo.DisplayMemberPath = "Name";

                StudentSubjectFilterCombo.ItemsSource = _subjects;
                StudentSubjectFilterCombo.DisplayMemberPath = "Name";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Ошибка загрузки предметов: {ex.Message}";
            }
        }

        private async Task LoadTeachersAsync(string subject = null, int? exp = null, decimal? price = null)
        {
            try
            {
                StatusText.Text = "Загрузка учителей...";
                LoadingProgressBar.Visibility = Visibility.Visible;

                _teachers = await _service.GetTeachersAsync(subject, exp, price);
                ResultsList.ItemsSource = _teachers;

                CounterText.Text = $"Найдено учителей: {_teachers.Count}";
                StatusText.Text = $"Загружено {_teachers.Count} учителей";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Ошибка: {ex.Message}";
                MessageBox.Show($"Ошибка загрузки учителей: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoadingProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        private async Task LoadStudentsAsync(string subject = null)
        {
            try
            {
                StatusText.Text = "Загрузка учеников...";
                LoadingProgressBar.Visibility = Visibility.Visible;

                // Получаем учеников через сервис
                var students = await _service.GetStudentsAsync(subject);

                // Обновляем список
                _students = students;
                ResultsList.ItemsSource = _students;

                // Обновляем счетчик
                CounterText.Text = $"Найдено учеников: {_students.Count}";
                StatusText.Text = $"Загружено {_students.Count} учеников";

                if (_students.Count == 0)
                {
                    MessageBox.Show("Нет учеников для отображения. Добавьте учеников через регистрацию.",
                        "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Ошибка: {ex.Message}";
                MessageBox.Show($"Ошибка загрузки учеников: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoadingProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TeachersTab.IsSelected)
            {
                ShowTeachersPanel();
            }
            else if (StudentsTab.IsSelected)
            {
                ShowStudentsPanel();
            }
        }

        private void ShowTeachersPanel()
        {
            TeachersFilterPanel.Visibility = Visibility.Visible;
            StudentsFilterPanel.Visibility = Visibility.Collapsed;
            _ = LoadTeachersAsync();
        }

        private void ShowStudentsPanel()
        {
            TeachersFilterPanel.Visibility = Visibility.Collapsed;
            StudentsFilterPanel.Visibility = Visibility.Visible;
            _ = LoadStudentsAsync();
        }

        // Поиск учителей
        private async void SearchTeachers_Click(object sender, RoutedEventArgs e)
        {
            string subject = SubjectFilterCombo.Text;
            int? exp = int.TryParse(ExperienceFilterBox.Text, out int eVal) ? eVal : (int?)null;
            decimal? price = decimal.TryParse(PriceFilterBox.Text, out decimal pVal) ? pVal : (decimal?)null;

            await LoadTeachersAsync(
                string.IsNullOrWhiteSpace(subject) ? null : subject,
                exp,
                price
            );
        }

        private void ResetTeachersFilter_Click(object sender, RoutedEventArgs e)
        {
            SubjectFilterCombo.Text = "";
            ExperienceFilterBox.Text = "";
            PriceFilterBox.Text = "";
            _ = LoadTeachersAsync();
        }

        // Поиск учеников
        private async void SearchStudents_Click(object sender, RoutedEventArgs e)
        {
            string subject = StudentSubjectFilterCombo.Text;
            await LoadStudentsAsync(string.IsNullOrWhiteSpace(subject) ? null : subject);
        }

        private void ResetStudentsFilter_Click(object sender, RoutedEventArgs e)
        {
            StudentSubjectFilterCombo.Text = "";
            _ = LoadStudentsAsync();
        }

        // Кнопки авторизации
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            if (loginWindow.ShowDialog() == true)
            {
                UpdateUIBasedOnAuth();
                StatusText.Text = $"Добро пожаловать!";

                // Обновляем данные в зависимости от роли
                if (TeachersTab.IsSelected)
                    _ = LoadTeachersAsync();
                else
                    _ = LoadStudentsAsync();
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterStudentWindow();
            registerWindow.ShowDialog();
        }

        private void AdminButton_Click(object sender, RoutedEventArgs e)
        {
            if (SessionManager.IsAdmin)
            {
                var adminPanel = new AdminPanel();
                adminPanel.ShowDialog();

                // Обновляем данные после закрытия админки
                if (TeachersTab.IsSelected)
                    _ = LoadTeachersAsync();
                else
                    _ = LoadStudentsAsync();
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.Logout();
            UpdateUIBasedOnAuth();

            if (TeachersTab.IsSelected)
                _ = LoadTeachersAsync();
            else
                _ = LoadStudentsAsync();

            StatusText.Text = "Вы вышли из системы";
        }

        // Добавление учителя (админ)
        private async void AddTeacherButton_Click(object sender, RoutedEventArgs e)
        {
            if (!SessionManager.IsAdmin)
            {
                MessageBox.Show("Только администратор может добавлять учителей", "Доступ запрещен",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var addWindow = new AddTeacherWindow();
            if (addWindow.ShowDialog() == true)
            {
                try
                {
                    StatusText.Text = "Добавление учителя...";
                    LoadingProgressBar.Visibility = Visibility.Visible;

                    int newId = await _service.AddTeacherAsync(addWindow.NewTeacher);

                    if (newId > 0)
                    {
                        MessageBox.Show("Учитель успешно добавлен!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadTeachersAsync();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    LoadingProgressBar.Visibility = Visibility.Collapsed;
                }
            }
        }

        // Добавление отзыва
        private async void AddReview_Click(object sender, RoutedEventArgs e)
        {
            if (!SessionManager.IsStudent)
            {
                MessageBox.Show("Только ученики могут оставлять отзывы", "Доступ запрещен",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (sender is Button btn && btn.Tag is int teacherId)
            {
                var teacher = _teachers.Find(t => t.Id == teacherId);
                if (teacher == null) return;

                var reviewWindow = new AddReviewWindow(teacher.FullName);
                if (reviewWindow.ShowDialog() == true)
                {
                    try
                    {
                        StatusText.Text = "Отправка отзыва...";
                        LoadingProgressBar.Visibility = Visibility.Visible;

                        bool added = await _service.AddReviewAsync(teacherId, reviewWindow.Rating, reviewWindow.Comment);

                        if (added)
                        {
                            MessageBox.Show("Отзыв успешно добавлен!", "Спасибо!",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            await LoadTeachersAsync(); // Обновляем рейтинг
                        }
                        else
                        {
                            MessageBox.Show("Вы уже оставляли отзыв этому учителю",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {
                        LoadingProgressBar.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }
    }
}
