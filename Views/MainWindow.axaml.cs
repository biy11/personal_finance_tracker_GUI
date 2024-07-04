using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Newtonsoft.Json;
using PersonalFinanceTracker.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace PersonalFinanceTracker.Views
{
    public partial class MainWindow : Window
    {
        private static ObservableCollection<Transaction> transactions = new ObservableCollection<Transaction>(LoadTransactions());
        private ObservableCollection<Transaction> filteredTransactions;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            filteredTransactions = new ObservableCollection<Transaction>(transactions);
            TransactionsListBox.ItemsSource = filteredTransactions;

            HideAllSections();
            MainMenuSection.IsVisible = true;
        }

        private void ShowTransactionsMenu_Click(object sender, RoutedEventArgs e)
        {
            HideAllSections();
            TransactionsMenuSection.IsVisible = true;
            RefreshTransactionsList();
        }

        private async void ShowAddTransactionDialog_Click(object sender, RoutedEventArgs e)
        {
            var addTransactionDialog = new AddTransactionDialog();
            await addTransactionDialog.ShowDialog(this);

            if (addTransactionDialog.NewTransaction != null)
            {
                transactions.Add(addTransactionDialog.NewTransaction);
                SaveTransactions(transactions);
                RefreshTransactionsList();
            }
        }

        private void ViewTransactions_Click(object sender, RoutedEventArgs e)
        {
            HideAllSections();
            TransactionsMenuSection.IsVisible = true;
            RefreshTransactionsList();
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

        private void OnDeleteSelectedClick(object sender, RoutedEventArgs e)
        {
            var selectedTransactions = TransactionsListBox.SelectedItems.Cast<Transaction>().ToList();
            if (selectedTransactions.Any())
            {
                foreach (var transaction in selectedTransactions)
                {
                    transactions.Remove(transaction);
                }
                SaveTransactions(transactions);
                RefreshTransactionsList();
            }
            else
            {
                ShowError("No transaction selected");
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
            MainMenuSection.IsVisible = true;
        }

        private void BackToTransactionsMenu_Click(object sender, RoutedEventArgs e)
        {
            HideAllSections();
            TransactionsMenuSection.IsVisible = true;
        }

        private void HideAllSections()
        {
            MainMenuSection.IsVisible = false;
            TransactionsMenuSection.IsVisible = false;
            SummaryReportSection.IsVisible = false;
        }

        private void RefreshTransactionsList()
        {
            filteredTransactions = new ObservableCollection<Transaction>(transactions);
            TransactionsListBox.ItemsSource = filteredTransactions;
        }

        private static async void SaveTransactions(ObservableCollection<Transaction> transactions)
        {
            try
            {
                var json = JsonConvert.SerializeObject(transactions, Formatting.Indented);
                await File.WriteAllTextAsync("transactions.json", json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving transactions: {ex.Message}");
            }
        }

        private static ObservableCollection<Transaction> LoadTransactions()
        {
            try
            {
                if (File.Exists("transactions.json"))
                {
                    var json = File.ReadAllText("transactions.json");
                    return new ObservableCollection<Transaction>(JsonConvert.DeserializeObject<List<Transaction>>(json) ?? new List<Transaction>());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading transactions: {ex.Message}");
            }
            return new ObservableCollection<Transaction>();
        }

        private void SortByDateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            var selectedItem = comboBox?.SelectedItem as ComboBoxItem;

            if (selectedItem != null)
            {
                if (selectedItem.Content.ToString() == "Newest First")
                {
                    filteredTransactions = new ObservableCollection<Transaction>(transactions.OrderByDescending(t => t.Date));
                }
                else if (selectedItem.Content.ToString() == "Oldest First")
                {
                    filteredTransactions = new ObservableCollection<Transaction>(transactions.OrderBy(t => t.Date));
                }
                TransactionsListBox.ItemsSource = filteredTransactions;
            }
        }

        private void SortByAmountComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            var selectedItem = comboBox?.SelectedItem as ComboBoxItem;

            if (selectedItem != null)
            {
                if (selectedItem.Content.ToString() == "Highest to Lowest")
                {
                    filteredTransactions = new ObservableCollection<Transaction>(transactions.OrderByDescending(t => t.Amount));
                }
                else if (selectedItem.Content.ToString() == "Lowest to Highest")
                {
                    filteredTransactions = new ObservableCollection<Transaction>(transactions.OrderBy(t => t.Amount));
                }
                TransactionsListBox.ItemsSource = filteredTransactions;
            }
        }

        private void SearchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            var searchText = (sender as TextBox)?.Text.ToLower();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filteredTransactions = new ObservableCollection<Transaction>(transactions.Where(t => t.Description.ToLower().Contains(searchText) || t.Category.ToLower().Contains(searchText)));
            }
            else
            {
                filteredTransactions = new ObservableCollection<Transaction>(transactions);
            }
            TransactionsListBox.ItemsSource = filteredTransactions;
        }
    }
}
