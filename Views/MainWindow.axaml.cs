//Views/MainWindow.axaml.cs
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Newtonsoft.Json;
using PersonalFinanceTracker.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Views
{
    public partial class MainWindow : Window
    {
        private static List<Transaction> transactions = LoadTransactions();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            HideAllSections();
            MainMenuHeader.IsVisible = true;
            MainMenuSection.IsVisible = true;
            }

        private void ShowTransactionsMenu_Click(object sender, RoutedEventArgs e)
        {
            HideAllSections();
            TransactionsMenuSection.IsVisible = true;
        }

        private void AddTransaction_Click(object sender, RoutedEventArgs e)
        {
            HideAllSections();
            AddTransactionSection.IsVisible = true;
        }

        private void ViewTransactions_Click(object sender, RoutedEventArgs e)
        {
            HideAllSections();
            ViewTransactionsSection.IsVisible = true;
            RefreshTransactionsList();
        }

        private void DeleteTransaction_Click(object sender, RoutedEventArgs e)
        {
            HideAllSections();
            DeleteTransactionSection.IsVisible = true;
            RefreshTransactionsListForDelete();
        }

        private void GenerateSummaryReport_Click(object sender, RoutedEventArgs e)
        {
            HideAllSections();
            SummaryReportSection.IsVisible = true;

            var totalIncome = transactions.Where(t => t.IsIncome).Sum(t => t.Amount);
            var totalExpenses = transactions.Where(t => !t.IsIncome).Sum(t => t.Amount);
            var balance = totalIncome - totalExpenses;

            var biggestIncome = transactions.Where(t => t.IsIncome).OrderByDescending(t => t.Amount).FirstOrDefault();
            var biggestExpense = transactions.Where(t => !t.IsIncome).OrderByDescending(t => t.Amount).FirstOrDefault();

            SummaryReportSection.Children.Clear();
            var backButton = new Button
            {
                Content = "Back",
                Margin = new Thickness(0, 0, 0, 10),
                Background = new SolidColorBrush(Color.Parse("#2196F3")),
                Foreground = new SolidColorBrush(Colors.White),
                Padding = new Thickness(10),
                Width = 200,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };
            backButton.Click += BackToMenu_Click;
            SummaryReportSection.Children.Add(backButton);

            SummaryReportSection.Children.Add(new TextBlock { Text = "Financial Summary Report", FontSize = 20, FontWeight = FontWeight.Bold, Margin = new Thickness(0, 0, 0, 20), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center });
            SummaryReportSection.Children.Add(new TextBlock { Text = $"Total Income: ${totalIncome}", Foreground = new SolidColorBrush(Colors.Black) });
            SummaryReportSection.Children.Add(new TextBlock { Text = $"Total Expenses: ${totalExpenses}", Foreground = new SolidColorBrush(Colors.Black) });
            SummaryReportSection.Children.Add(new TextBlock { Text = $"Balance: ${balance}", Foreground = new SolidColorBrush(Colors.Black) });

            if (biggestIncome != null)
            {
                SummaryReportSection.Children.Add(new TextBlock { Text = $"Biggest Income: {biggestIncome.Description} - {biggestIncome.Category}: ${biggestIncome.Amount} on {biggestIncome.Date}", Foreground = new SolidColorBrush(Colors.Black) });
            }

            if (biggestExpense != null)
            {
                SummaryReportSection.Children.Add(new TextBlock { Text = $"Biggest Expense: {biggestExpense.Description} - {biggestExpense.Category}: ${biggestExpense.Amount} on {biggestExpense.Date}", Foreground = new SolidColorBrush(Colors.Black) });
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnAddClick(object sender, RoutedEventArgs e)
        {
            var descriptionTextBox = this.FindControl<TextBox>("DescriptionTextBox");
            var categoryTextBox = this.FindControl<TextBox>("CategoryTextBox");
            var amountTextBox = this.FindControl<TextBox>("AmountTextBox");
            var isIncomeCheckBox = this.FindControl<CheckBox>("IsIncomeCheckBox");

            var description = descriptionTextBox?.Text;
            var category = categoryTextBox?.Text;
            var amountText = amountTextBox?.Text;
            var isIncome = isIncomeCheckBox?.IsChecked ?? false;

            if (string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(category))
            {
                ShowError("Description and Category cannot be empty!");
                return;
            }

            if (decimal.TryParse(amountText, out var amount))
            {
                if (amount <= 0)
                {
                    ShowError("Amount must be positive.");
                    return;
                }

                var newTransaction = new Transaction
                {
                    Id = Guid.NewGuid().GetHashCode(),
                    Description = description,
                    Category = category,
                    Amount = amount,
                    Date = DateTime.Now,
                    IsIncome = isIncome
                };

                transactions.Add(newTransaction);
                SaveTransactions(transactions);
                RefreshTransactionsList();
                HideAllSections();
                TransactionsMenuSection.IsVisible = true;

                // Clear the input fields
                descriptionTextBox.Text = string.Empty;
                categoryTextBox.Text = string.Empty;
                amountTextBox.Text = string.Empty;
                isIncomeCheckBox.IsChecked = false;
            }
            else
            {
                ShowError("Invalid amount entered.");
            }
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            var selectedTransaction = TransactionsListBoxForDelete.SelectedItem as string;
            if (selectedTransaction != null)
            {
                var transactionId = int.Parse(selectedTransaction.Split('.')[0]);
                var transactionToRemove = transactions.FirstOrDefault(t => t.Id == transactionId);
                if (transactionToRemove != null)
                {
                    transactions.Remove(transactionToRemove);
                    SaveTransactions(transactions);
                    RefreshTransactionsListForDelete();
                }
            }
        }

        private void ShowError(string message)
        {
            var messageBox = new Window
            {
                Title = "Error",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = new SolidColorBrush(Colors.White)
            };

            var stackPanel = new StackPanel { Margin = new Thickness(10) };

            var textBlock = new TextBlock { Text = message, Foreground = new SolidColorBrush(Colors.Black), Margin = new Thickness(0, 0, 0, 20) };
            stackPanel.Children.Add(textBlock);

            var button = new Button { Content = "OK", Width = 80, Height = 30 };
            button.Click += (s, ev) => messageBox.Close();
            stackPanel.Children.Add(button);

            messageBox.Content = stackPanel;
            messageBox.ShowDialog(this);
        }

        private void BackToMenu_Click(object sender, RoutedEventArgs e)
        {
            HideAllSections();
            MainMenuHeader.IsVisible = true;
            MainMenuSection.IsVisible = true;
        }

        private void BackToTransactionsMenu_Click(object sender, RoutedEventArgs e)
        {
            HideAllSections();
            TransactionsMenuSection.IsVisible = true;
        }

        private void HideAllSections()
        {
            MainMenuHeader.IsVisible = false;
            MainMenuSection.IsVisible = false;
            TransactionsMenuSection.IsVisible = false;
            AddTransactionSection.IsVisible = false;
            ViewTransactionsSection.IsVisible = false;
            DeleteTransactionSection.IsVisible = false;
            SummaryReportSection.IsVisible = false;
        }

        private void RefreshTransactionsList()
        {
            TransactionsListBox.ItemsSource = transactions;
        }

        private void RefreshTransactionsListForDelete()
        {
            TransactionsListBoxForDelete.ItemsSource = transactions;
        }

        private static async void SaveTransactions(List<Transaction> transactions)
        {
            try
            {
                var json = JsonConvert.SerializeObject(transactions, Formatting.Indented);
                await File.WriteAllTextAsync("transaction.json", json);
            }
            catch
            {
                // Handle exception (e.g., log the error, show a message to the user)
            }
        }

        private static List<Transaction> LoadTransactions()
        {
            try
            {
                if (File.Exists("transaction.json"))
                {
                    var json = File.ReadAllText("transaction.json");
                    return JsonConvert.DeserializeObject<List<Transaction>>(json) ?? new List<Transaction>();
                }
            }
            catch
            {
                // Handle exception (e.g., log the error, show a message to the user)
            }
            return new List<Transaction>();
        }
    }
}