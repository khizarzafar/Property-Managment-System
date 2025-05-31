using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace SocietyManagmentSystem
{
    /// <summary>
    /// Interaction logic for admin_financereports.xaml
    /// </summary>
    public partial class admin_financereports : Window
    {
        private string connectionString = "Server=localhost;Database=zephyr;Uid=root;Pwd=root;"; 
        private List<FinanceReportModel> allReports;

        public admin_financereports()
        {
            InitializeComponent();
            LoadFinanceReports();
        }

        private void LoadFinanceReports()
        {
            try
            {
                allReports = new List<FinanceReportModel>();

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
                    WHERE res.status = 'Active'
                    ORDER BY r.due_date DESC, r.rent_id DESC";

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                allReports.Add(new FinanceReportModel
                                {
                                    RentId = reader.GetInt32("rent_id"),
                                    ResidentName = reader.GetString("resident_name"),
                                    Phone = reader.IsDBNull("phone") ? "N/A" : reader.GetString("phone"),
                                    MonthYear = reader.GetString("month_year"),
                                    Amount = reader.GetDecimal("amount"),
                                    DueDate = reader.GetDateTime("due_date"),
                                    Status = reader.GetString("status"),
                                    PaymentMethod = reader.IsDBNull("payment_method") ? "N/A" : reader.GetString("payment_method"),
                                    PaymentDate = reader.IsDBNull("payment_date") ? (DateTime?)null : reader.GetDateTime("payment_date")
                                });
                            }
                        }
                    }
                }

                FinanceReportsDataGrid.ItemsSource = allReports;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading finance reports: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (allReports == null) return;

            var selectedItem = (ComboBoxItem)StatusFilterComboBox.SelectedItem;
            string selectedStatus = selectedItem.Content.ToString();

            if (selectedStatus == "All")
            {
                FinanceReportsDataGrid.ItemsSource = allReports;
            }
            else
            {
                var filteredReports = allReports.FindAll(r => r.Status.Equals(selectedStatus, StringComparison.OrdinalIgnoreCase));
                FinanceReportsDataGrid.ItemsSource = filteredReports;
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadFinanceReports();
            StatusFilterComboBox.SelectedIndex = 0; // Reset filter to "All"
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dashboardWindow = new admin();
                dashboardWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Dashboard: {ex.Message}");
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                var registrarWindow = new admin_registrarreports();
                registrarWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Registrar Reports: {ex.Message}");
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            // Already on this page, optionally refresh data
            RefreshButton_Click(sender, e);
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
                MessageBox.Show($"Error logging out: {ex.Message}");
            }
        }
    }

    // Data model for Finance Reports
    public class FinanceReportModel
    {
        public int RentId { get; set; }
        public string ResidentName { get; set; }
        public string Phone { get; set; }
        public string MonthYear { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
}