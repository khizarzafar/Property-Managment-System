using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient; // Ensure you have this using statement and the MySql.Data NuGet package

namespace SocietyManagmentSystem
{
    /// <summary>
    /// Interaction logic for finance_paymentreports.xaml
    /// </summary>
    public partial class finance_paymentreports : Window
    {
        private string connectionString = "Server=localhost;Database=zephyr;Uid=root;Pwd=root;"; // Your MySQL connection string
        private List<PaymentReportModel> allPaymentReports;

        public finance_paymentreports()
        {
            InitializeComponent();
            LoadPaymentReports();
        }

        private void LoadPaymentReports()
        {
            try
            {
                allPaymentReports = new List<PaymentReportModel>();

                // SQL query to fetch rent payment details joined with resident information
                string query = @"
                    SELECT 
                        r.rent_id,
                        res.name AS resident_name,
                        res.phone,
                        r.month_year,
                        r.amount,
                        r.due_date,
                        r.status,
                        r.payment_method,
                        r.payment_date
                    FROM rent r
                    INNER JOIN residents res ON r.resident_id = res.resident_id
                    ORDER BY r.due_date DESC, r.rent_id DESC";
                // If you only want to show reports for 'Active' residents, add:
                // WHERE res.status = 'Active'
                // before the ORDER BY clause.

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                allPaymentReports.Add(new PaymentReportModel
                                {
                                    RentId = reader.GetInt32("rent_id"),
                                    ResidentName = reader.GetString("resident_name"),
                                    Phone = reader.IsDBNull(reader.GetOrdinal("phone")) ? "N/A" : reader.GetString("phone"),
                                    MonthYear = reader.GetString("month_year"),
                                    Amount = reader.GetDecimal("amount"),
                                    DueDate = reader.GetDateTime("due_date"),
                                    Status = reader.GetString("status"),
                                    PaymentMethod = reader.IsDBNull(reader.GetOrdinal("payment_method")) ? "N/A" : reader.GetString("payment_method"),
                                    PaymentDate = reader.IsDBNull(reader.GetOrdinal("payment_date")) ? (DateTime?)null : reader.GetDateTime("payment_date")
                                });
                            }
                        }
                    }
                }

                PaymentsReportDataGrid.ItemsSource = allPaymentReports;
                if (StatusFilterComboBox.ItemsSource == null) // Populate ComboBox items if not already done (e.g., from XAML)
                {
                    // This part is typically defined in XAML, but can be added here if needed.
                    // For your XAML, it's already defined.
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading payment reports: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (allPaymentReports == null || StatusFilterComboBox.SelectedItem == null) return;

            var selectedItem = (ComboBoxItem)StatusFilterComboBox.SelectedItem;
            string selectedStatus = selectedItem.Content.ToString();

            if (selectedStatus == "All")
            {
                PaymentsReportDataGrid.ItemsSource = allPaymentReports;
            }
            else
            {
                // Using LINQ to filter the reports
                var filteredReports = allPaymentReports.Where(r => r.Status.Equals(selectedStatus, StringComparison.OrdinalIgnoreCase)).ToList();
                PaymentsReportDataGrid.ItemsSource = filteredReports;
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadPaymentReports();
            StatusFilterComboBox.SelectedIndex = 0; // Reset filter to "All"
        }

        // Navigation Button Event Handlers

        private void Button_Click(object sender, RoutedEventArgs e) // Dashboard Button
        {
            try
            {
                var financeWindow = new finance(); // Assuming 'finance' is your dashboard window
                financeWindow.Show();
                this.Close();
            }
            catch (Exception ex) { MessageBox.Show($"Error navigating to Dashboard: {ex.Message}"); }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) // Manage Payments Button
        {
            try
            {
                var managePaymentsWindow = new finance_managepayments(); // Navigates to your manage payments window
                managePaymentsWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Manage Payments: {ex.Message}", "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e) // Payment Reports Button (Current Page)
        {
            // Already on this page, so refresh the data.
            RefreshButton_Click(sender, e);
            MessageBox.Show("Payment reports refreshed.", "Payment Reports", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show("Are you sure you want to logout?", "Confirm Logout",
                                           MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    var loginWindow = new login(); // Navigates to your login window
                    loginWindow.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error logging out: {ex.Message}", "Logout Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    // Data model for Payment Reports (matches the DataGrid columns and your database structure)
    public class PaymentReportModel
    {
        public int RentId { get; set; }
        public string ResidentName { get; set; }
        public string Phone { get; set; }
        public string MonthYear { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime? PaymentDate { get; set; } // Nullable DateTime for payment_date
    }
}