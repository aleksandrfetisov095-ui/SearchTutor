using ClientTutor.Models;
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
    public partial class AddTeacherWindow : Window
    {
        public Teacher NewTeacher { get; private set; }

        public AddTeacherWindow()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (string.IsNullOrWhiteSpace(LastNameBox.Text) ||
                string.IsNullOrWhiteSpace(FirstNameBox.Text) ||
                string.IsNullOrWhiteSpace(SubjectBox.Text))
            {
                MessageBox.Show("Заполните обязательные поля (Фамилия, Имя, Предмет)",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(ExperienceBox.Text, out int exp))
            {
                MessageBox.Show("Стаж должен быть числом",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(PriceMinBox.Text, out decimal priceMin) ||
                !decimal.TryParse(PriceMaxBox.Text, out decimal priceMax))
            {
                MessageBox.Show("Цена должна быть числом",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            NewTeacher = new Teacher
            {
                LastName = LastNameBox.Text.Trim(),
                FirstName = FirstNameBox.Text.Trim(),
                MiddleName = string.IsNullOrWhiteSpace(MiddleNameBox.Text) ? null : MiddleNameBox.Text.Trim(),
                Subject = SubjectBox.Text.Trim(),
                Experience = exp,
                PriceMin = priceMin,
                PriceMax = priceMax,
                Education = string.IsNullOrWhiteSpace(EducationBox.Text) ? null : EducationBox.Text.Trim(),
                Description = string.IsNullOrWhiteSpace(DescriptionBox.Text) ? null : DescriptionBox.Text.Trim()
            };

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
