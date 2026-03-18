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
using System.Windows.Shapes;

namespace ClientTutor
{
    
    public partial class AdminPanel : Window
    {
        private readonly TutorClientService _service;
        private List<Teacher> _teachers = new List<Teacher>();
        private List<Student> _students = new List<Student>();
        private List<Review> _reviews = new List<Review>();

        public AdminPanel()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;

            _service = new TutorClientService("127.0.0.1", 5555);

            
            Loaded += async (s, e) =>
            {
                await LoadTeachersAsync();
                await LoadStudentsAsync();
                await LoadReviewsAsync();
            };
        }


        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TeachersPanel.Visibility = TeachersTab.IsSelected ? Visibility.Visible : Visibility.Collapsed;
            StudentsPanel.Visibility = StudentsTab.IsSelected ? Visibility.Visible : Visibility.Collapsed;
            ReviewsPanel.Visibility = ReviewsTab.IsSelected ? Visibility.Visible : Visibility.Collapsed;
        }

        //Учителя

        private async void RefreshTeachers_Click(object sender, RoutedEventArgs e)
        {
            await LoadTeachersAsync(ShowDeletedTeachersCheck.IsChecked == true);
        }

        private async void ShowDeletedTeachers_Changed(object sender, RoutedEventArgs e)
        {
            await LoadTeachersAsync(ShowDeletedTeachersCheck.IsChecked == true);
        }

        private async Task LoadTeachersAsync(bool showDeleted = false)
        {
            try
            {
                StatusText.Text = "Загрузка учителей...";
                LoadingProgressBar.Visibility = Visibility.Visible;

                
                _teachers = await _service.GetAllTeachersAsync(showDeleted);

                TeachersGrid.ItemsSource = null;
                TeachersGrid.ItemsSource = _teachers;

                StatusText.Text = $"Загружено {_teachers.Count} учителей";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoadingProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        private async void AddTeacher_Click(object sender, RoutedEventArgs e)
        {
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
                        await LoadTeachersAsync(ShowDeletedTeachersCheck.IsChecked == true);
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

        private async void DeleteTeacher_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int teacherId)
            {
                var result = MessageBox.Show("Удалить учителя? (мягкое удаление)",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        StatusText.Text = "Удаление...";
                        LoadingProgressBar.Visibility = Visibility.Visible;

                        bool deleted = await _service.DeleteTeacherAsync(teacherId);

                        if (deleted)
                        {
                            StatusText.Text = "Учитель удален";
                            await LoadTeachersAsync(ShowDeletedTeachersCheck.IsChecked == true);
                        }
                        else
                        {
                            MessageBox.Show("Не удалось удалить учителя", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {
                        LoadingProgressBar.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private async void RestoreTeacher_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int teacherId)
            {
                try
                {
                    StatusText.Text = "Восстановление...";
                    LoadingProgressBar.Visibility = Visibility.Visible;

                    bool restored = await _service.RestoreTeacherAsync(teacherId);

                    if (restored)
                    {
                        StatusText.Text = "Учитель восстановлен";
                        await LoadTeachersAsync(ShowDeletedTeachersCheck.IsChecked == true);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось восстановить учителя", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка восстановления: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    LoadingProgressBar.Visibility = Visibility.Collapsed;
                }
            }
        }

        //Ученики

        private async void RefreshStudents_Click(object sender, RoutedEventArgs e)
        {
            await LoadStudentsAsync(ShowDeletedStudentsCheck.IsChecked == true);
        }

        private async void ShowDeletedStudents_Changed(object sender, RoutedEventArgs e)
        {
            await LoadStudentsAsync(ShowDeletedStudentsCheck.IsChecked == true);
        }

        private async Task LoadStudentsAsync(bool showDeleted = false)
        {
            try
            {
                StatusText.Text = "Загрузка учеников...";
                LoadingProgressBar.Visibility = Visibility.Visible;

                _students = await _service.GetAllStudentsAsync(showDeleted);

                StudentsGrid.ItemsSource = null;
                StudentsGrid.ItemsSource = _students;

                StatusText.Text = $"Загружено {_students.Count} учеников";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoadingProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        private async void DeleteStudent_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int studentId)
            {
                var result = MessageBox.Show("Удалить ученика? (мягкое удаление)",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        StatusText.Text = "Удаление...";
                        LoadingProgressBar.Visibility = Visibility.Visible;

                        bool deleted = await _service.DeleteStudentAsync(studentId);

                        if (deleted)
                        {
                            StatusText.Text = "Ученик удален";
                            await LoadStudentsAsync(ShowDeletedStudentsCheck.IsChecked == true);
                        }
                        else
                        {
                            MessageBox.Show("Не удалось удалить ученика", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {
                        LoadingProgressBar.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private async void RestoreStudent_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int studentId)
            {
                try
                {
                    StatusText.Text = "Восстановление...";
                    LoadingProgressBar.Visibility = Visibility.Visible;

                    bool restored = await _service.RestoreStudentAsync(studentId);

                    if (restored)
                    {
                        StatusText.Text = "Ученик восстановлен";
                        await LoadStudentsAsync(ShowDeletedStudentsCheck.IsChecked == true);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось восстановить ученика", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка восстановления: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    LoadingProgressBar.Visibility = Visibility.Collapsed;
                }
            }
        }

        private async void DeactivateStudent_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int studentId)
            {
                try
                {
                    StatusText.Text = "Деактивация...";
                    LoadingProgressBar.Visibility = Visibility.Visible;

                    bool deactivated = await _service.DeactivateStudentAsync(studentId);

                    if (deactivated)
                    {
                        StatusText.Text = "Ученик деактивирован";
                        await LoadStudentsAsync(ShowDeletedStudentsCheck.IsChecked == true);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось деактивировать ученика", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка деактивации: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    LoadingProgressBar.Visibility = Visibility.Collapsed;
                }
            }
        }

        private async void ActivateStudent_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int studentId)
            {
                try
                {
                    StatusText.Text = "Активация...";
                    LoadingProgressBar.Visibility = Visibility.Visible;

                    bool activated = await _service.ActivateStudentAsync(studentId);

                    if (activated)
                    {
                        StatusText.Text = "Ученик активирован";
                        await LoadStudentsAsync(ShowDeletedStudentsCheck.IsChecked == true);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось активировать ученика", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка активации: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    LoadingProgressBar.Visibility = Visibility.Collapsed;
                }
            }
        }

        //Отзывы

        private async void RefreshReviews_Click(object sender, RoutedEventArgs e)
        {
            await LoadReviewsAsync(ShowModeratedCheck.IsChecked == true);
        }

        private async void ShowModerated_Changed(object sender, RoutedEventArgs e)
        {
            await LoadReviewsAsync(ShowModeratedCheck.IsChecked == true);
        }

        private async Task LoadReviewsAsync(bool showModerated = false)
        {
            try
            {
                StatusText.Text = "Загрузка отзывов...";
                LoadingProgressBar.Visibility = Visibility.Visible;

                
                _reviews = new List<Review>
                {
                    new Review { Id = 1, FromName = "Иван Петров", ToName = "Мария Иванова", Rating = 5, Comment = "Отличный учитель!", IsModerated = true, CreatedAt = DateTime.Now.AddDays(-5) },
                    new Review { Id = 2, FromName = "Анна Сидорова", ToName = "Петр Смирнов", Rating = 4, Comment = "Хороший подход", IsModerated = false, CreatedAt = DateTime.Now.AddDays(-2) }
                };

                ReviewsGrid.ItemsSource = null;
                ReviewsGrid.ItemsSource = _reviews;

                StatusText.Text = $"Загружено {_reviews.Count} отзывов";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки отзывов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoadingProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        private async void ApproveReview_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int reviewId)
            {
                try
                {
                    StatusText.Text = "Одобрение отзыва...";
                    LoadingProgressBar.Visibility = Visibility.Visible;

                    
                    await Task.Delay(500);

                    StatusText.Text = "Отзыв одобрен";
                    await LoadReviewsAsync(ShowModeratedCheck.IsChecked == true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка одобрения: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    LoadingProgressBar.Visibility = Visibility.Collapsed;
                }
            }
        }

        private async void DeleteReview_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int reviewId)
            {
                var result = MessageBox.Show("Удалить отзыв?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        StatusText.Text = "Удаление отзыва...";
                        LoadingProgressBar.Visibility = Visibility.Visible;

                       
                        await Task.Delay(500);

                        StatusText.Text = "Отзыв удален";
                        await LoadReviewsAsync(ShowModeratedCheck.IsChecked == true);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
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
