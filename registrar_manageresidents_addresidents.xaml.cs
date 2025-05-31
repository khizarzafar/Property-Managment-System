using System;
using System.Windows;
using System.Windows.Controls;

namespace SocietyManagmentSystem
{
    public partial class registrar_manageresidents_addresidents : Window
    {
        public registrar_manageresidents_addresidents()
        {
            InitializeComponent();
        }

        private void SaveResident_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string residentId = ResidentIdTextBox.Text;
                string name = NameTextBox.Text;
                string status = (StatusComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                string email = EmailTextBox.Text;
                string phone = PhoneTextBox.Text;
                string moveInDate = MoveInDatePicker.SelectedDate?.ToString("yyyy-MM-dd");

                // Save to database logic here
                MessageBox.Show($"Resident Added:\nID: {residentId}\nName: {name}\nStatus: {status}\nEmail: {email}\nPhone: {phone}\nMove-In Date: {moveInDate}");

                // Return to manageresidents window
                var manageWindow = new registrar_manageresidents();
                manageWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving resident: {ex.Message}");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder for Dashboard
            // Example:
            // var dashboard = new DashboardWindow();
            // dashboard.Show();
            // this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                var manageresidents = new registrar_manageresidents();
                manageresidents.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error navigating to Manage Residents: {ex.Message}");
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                var handleComplaints = new registrar_handlecomplaints();
                handleComplaints.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error navigating to Handle Complaints: {ex.Message}");
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var loginWindow = new login();
                loginWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error navigating to Login: {ex.Message}");
            }
        }
    }
}
