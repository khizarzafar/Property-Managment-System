using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualBasic;

namespace SocietyManagmentSystem
{
    public partial class finance_managepayments_addpayment : Window

    {
        public finance_managepayments_addpayment()
        {
            InitializeComponent();
        }

        private void SavePayment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string residentId = ResidentIdTextBox.Text;
                string monthYear = MonthYearTextBox.Text;
                decimal amount = decimal.Parse(AmountTextBox.Text); // Consider using TryParse for safety
                DateTime? dueDate = DueDatePicker.SelectedDate;
                string status = ((ComboBoxItem)StatusComboBox.SelectedItem)?.Content.ToString();
                string paymentMethod = ((ComboBoxItem)PaymentMethodComboBox.SelectedItem)?.Content.ToString();

                MessageBox.Show($"Payment Added:\nResident ID: {residentId}\nMonth/Year: {monthYear}\nAmount: {amount:C}\nDue Date: {dueDate:dd MMM yyyy}\nStatus: {status}\nPayment Method: {paymentMethod}");

                var managePaymentsWindow = new finance_managepayments();
                managePaymentsWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving payment: {ex.Message}");
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder for Dashboard navigation
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                var managePaymentsWindow = new finance_managepayments();
                managePaymentsWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Manage Payments window: {ex.Message}");
            }
        }


        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                var paymentReportsWindow = new finance_paymentreports();
                paymentReportsWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Payment Reports window: {ex.Message}");
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
                MessageBox.Show($"Error opening Login window: {ex.Message}");
            }
        }
    }
}