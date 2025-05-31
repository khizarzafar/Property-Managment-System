using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace SocietyManagmentSystem
{
    public partial class finance_managepayments : Window
    {
        private string connectionString = "Server=localhost;Database=zephyr;Uid=root;Pwd=root;";
        private int selectedRentId = 0;

        public finance_managepayments()
        {
            InitializeComponent();
            LoadPaymentsData();
            UpdateStatusCombo.SelectionChanged += UpdateStatusCombo_SelectionChanged;
        }

        private void LoadPaymentsData(string filterStatus = "")
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    // CORRECTED: Using res.name for resident_name as per your schema
                    // ADDED: Filter capability
                    string query = @"SELECT r.rent_id, r.resident_id, r.month_year, r.amount,
                                            r.due_date, r.status, r.payment_method, r.payment_date,
                                            res.name as resident_name 
                                       FROM rent r
                                       LEFT JOIN residents res ON r.resident_id = res.resident_id";

                    if (!string.IsNullOrEmpty(filterStatus))
                    {
                        query += " WHERE r.status = @status";
                    }
                    query += " ORDER BY r.due_date DESC, r.rent_id DESC";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    if (!string.IsNullOrEmpty(filterStatus))
                    {
                        command.Parameters.AddWithValue("@status", filterStatus);
                    }

                    MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    PaymentsDataGrid.ItemsSource = dataTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading payment data: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowAll_Click(object sender, RoutedEventArgs e)
        {
            LoadPaymentsData(); // Load all
        }

        private void ShowPendingOnly_Click(object sender, RoutedEventArgs e)
        {
            LoadPaymentsData("Pending"); // Load only Pending
        }

        private void ShowPaidOnly_Click(object sender, RoutedEventArgs e)
        {
            LoadPaymentsData("Paid"); // Load only Paid
        }

        private void AddRent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var addRentWindow = new finance_managepayments_addrent();
                addRentWindow.Owner = this; // Optional: set owner
                var dialogResult = addRentWindow.ShowDialog();
                if (dialogResult == true) // Assuming addRentWindow sets DialogResult
                {
                    LoadPaymentsData(); // Refresh data if changes were made
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Add Rent Form: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdatePayment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var dataRowView = button?.DataContext as DataRowView;

                if (dataRowView != null)
                {
                    selectedRentId = Convert.ToInt32(dataRowView["rent_id"]);
                    string currentStatus = dataRowView["status"].ToString();
                    string currentPaymentMethod = dataRowView["payment_method"]?.ToString();
                    DateTime? currentPaymentDate = dataRowView["payment_date"] != DBNull.Value ? Convert.ToDateTime(dataRowView["payment_date"]) : (DateTime?)null;

                    UpdateRentIdText.Text = selectedRentId.ToString();
                    UpdateCurrentStatusText.Text = currentStatus;

                    // Set current status in combo box
                    UpdateStatusCombo.SelectedItem = UpdateStatusCombo.Items.Cast<ComboBoxItem>()
                                                     .FirstOrDefault(item => item.Content.ToString() == currentStatus);

                    // If status is Paid, show details and populate them
                    if (currentStatus == "Paid")
                    {
                        PaymentDetailsPanel.Visibility = Visibility.Visible;
                        PaymentMethodCombo.Text = currentPaymentMethod; // For editable ComboBox
                        PaymentDatePicker.SelectedDate = currentPaymentDate;
                    }
                    else
                    {
                        PaymentDetailsPanel.Visibility = Visibility.Collapsed;
                        PaymentMethodCombo.Text = string.Empty; // Clear for non-paid or if becoming pending
                        PaymentDatePicker.SelectedDate = null;
                    }
                    UpdatePaymentDialog.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening update dialog: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateStatusCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = sender as ComboBox;
            var selectedItem = combo?.SelectedItem as ComboBoxItem;

            if (selectedItem?.Content.ToString() == "Paid")
            {
                PaymentDetailsPanel.Visibility = Visibility.Visible;
                // Only default payment date if it's not already set from existing record
                if (PaymentDatePicker.SelectedDate == null)
                {
                    PaymentDatePicker.SelectedDate = DateTime.Now;
                }
                // Do not clear PaymentMethodCombo.Text here, let UpdatePayment_Click populate it if existing
            }
            else
            {
                PaymentDetailsPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void SaveUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedStatusItem = UpdateStatusCombo.SelectedItem as ComboBoxItem;
                if (selectedStatusItem == null)
                {
                    MessageBox.Show("Please select a status.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                string newStatus = selectedStatusItem.Content.ToString();

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query;
                    MySqlCommand command;

                    if (newStatus == "Paid")
                    {
                        string paymentMethod = PaymentMethodCombo.Text.Trim(); // Get text from editable ComboBox
                        if (string.IsNullOrEmpty(paymentMethod) || PaymentDatePicker.SelectedDate == null)
                        {
                            MessageBox.Show("Please enter payment method and select payment date for 'Paid' status.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        // **IMPORTANT ENUM CAVEAT**: If 'payment_method' column in DB is an ENUM,
                        // the 'paymentMethod' string MUST be one of the allowed ENUM values ('Cash', 'Cheque', 'Online Transfer').
                        // Otherwise, this SQL update will fail. Consider adding client-side validation or changing DB column type.

                        DateTime paymentDate = PaymentDatePicker.SelectedDate.Value;
                        query = @"UPDATE rent SET status = @status, payment_method = @paymentMethod, 
                                        payment_date = @paymentDate WHERE rent_id = @rentId";
                        command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@status", newStatus);
                        command.Parameters.AddWithValue("@paymentMethod", paymentMethod);
                        command.Parameters.AddWithValue("@paymentDate", paymentDate);
                        command.Parameters.AddWithValue("@rentId", selectedRentId);
                    }
                    else // Status is "Pending"
                    {
                        query = @"UPDATE rent SET status = @status, payment_method = NULL, 
                                        payment_date = NULL WHERE rent_id = @rentId";
                        command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@status", newStatus);
                        command.Parameters.AddWithValue("@rentId", selectedRentId);
                    }

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Payment status updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        CloseAndUpdateDialog();
                        LoadPaymentsData();
                    }
                    else
                    {
                        MessageBox.Show("No records were updated. The data might be the same or the record not found.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (MySqlException mex) // Catch specific MySQL errors
            {
                // Example: Check for data truncation errors if ENUM value is invalid
                if (mex.Number == 1265) // Error code for "Data truncated for column"
                {
                    MessageBox.Show($"Error updating payment: Invalid payment method provided. Please use 'Cash', 'Cheque', or 'Online Transfer' if the database column is restricted.", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show($"Error updating payment: {mex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating payment: {ex.Message}", "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseAndUpdateDialog()
        {
            UpdatePaymentDialog.Visibility = Visibility.Collapsed;
            UpdateStatusCombo.SelectedIndex = -1;
            PaymentMethodCombo.Text = string.Empty; // Clear text for editable ComboBox
            PaymentDatePicker.SelectedDate = null;
            PaymentDetailsPanel.Visibility = Visibility.Collapsed;
            selectedRentId = 0;
        }


        private void CancelUpdate_Click(object sender, RoutedEventArgs e)
        {
            CloseAndUpdateDialog();
        }

        private void DeletePayment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var dataRowView = button?.DataContext as DataRowView;

                if (dataRowView != null)
                {
                    int rentId = Convert.ToInt32(dataRowView["rent_id"]);
                    // CORRECTED: Using resident_name which comes from res.name
                    string residentName = dataRowView["resident_name"]?.ToString() ?? "N/A";
                    string monthYear = dataRowView["month_year"]?.ToString() ?? "N/A";

                    var result = MessageBox.Show($"Are you sure you want to delete rent record for {residentName} ({monthYear})?",
                                                 "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        using (MySqlConnection connection = new MySqlConnection(connectionString))
                        {
                            connection.Open();
                            string query = "DELETE FROM rent WHERE rent_id = @rentId";
                            MySqlCommand command = new MySqlCommand(query, connection);
                            command.Parameters.AddWithValue("@rentId", rentId);
                            int rowsAffected = command.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Rent record deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                LoadPaymentsData();
                            }
                            else
                            {
                                MessageBox.Show("No records were deleted.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting payment: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Navigation methods
        private void Button_Click(object sender, RoutedEventArgs e) // Dashboard
        {
            try
            {
                var financeWindow = new finance(); // Assuming 'finance' is your dashboard window
                financeWindow.Show();
                this.Close();
            }
            catch (Exception ex) { MessageBox.Show($"Error navigating to Dashboard: {ex.Message}"); }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) // Manage Payments (Refresh)
        {
            LoadPaymentsData();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e) // Payment Reports
        {
            try
            {
                var reportsWindow = new finance_paymentreports(); // Assuming this window exists
                reportsWindow.Show();
                this.Close();
            }
            catch (Exception ex) { MessageBox.Show($"Error navigating to Payment Reports: {ex.Message}"); }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var loginWindow = new login(); // Assuming 'login' is your login window
                loginWindow.Show();
                this.Close();
            }
            catch (Exception ex) { MessageBox.Show($"Error logging out: {ex.Message}"); }
        }
    }
}