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
    /// <summary>
    /// Interaction logic for AddReviewWindow.xaml
    /// </summary>
    public partial class AddReviewWindow : Window
    {
        public int Rating { get; private set; }
        public string Comment => CommentBox.Text;

        public AddReviewWindow(string teacherName)
        {
            InitializeComponent();
            TeacherNameText.Text = teacherName;
            Owner = Application.Current.MainWindow;
            SetRating(0);
        }

        private void SetRating(int rating)
        {
            Rating = rating;
            Star1.Content = rating >= 1 ? "⭐" : "☆";
            Star2.Content = rating >= 2 ? "⭐" : "☆";
            Star3.Content = rating >= 3 ? "⭐" : "☆";
            Star4.Content = rating >= 4 ? "⭐" : "☆";
            Star5.Content = rating >= 5 ? "⭐" : "☆";
        }

        private void Star_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Tag.ToString(), out int rating))
            {
                SetRating(rating);
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (Rating == 0)
            {
                MessageBox.Show("Поставьте оценку", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(CommentBox.Text))
            {
                MessageBox.Show("Напишите комментарий", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
