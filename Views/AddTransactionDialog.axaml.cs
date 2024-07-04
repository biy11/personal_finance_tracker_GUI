//AddTransactionDialog.axaml.cs
#nullable enable

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using PersonalFinanceTracker.Models;
using System;

namespace PersonalFinanceTracker.Views
{
    public partial class AddTransactionDialog : Window
    {
        public Transaction? NewTransaction { get; private set; }

        public AddTransactionDialog()
        {
            InitializeComponent();
        }

        private void OnAddClick(object sender, RoutedEventArgs e)
        {
            var description = this.FindControl<TextBox>("DescriptionTextBox")?.Text;
            var categoryComboBox = this.FindControl<ComboBox>("CategoryComboBox");
            var selectedCategoryItem = categoryComboBox?.SelectedItem as ComboBoxItem;
            var category = selectedCategoryItem?.Content?.ToString();
            var amountText = this.FindControl<TextBox>("AmountTextBox")?.Text;
            var isIncome = this.FindControl<RadioButton>("IncomeRadioButton")?.IsChecked ?? false;
            var isExpense = this.FindControl<RadioButton>("ExpenseRadioButton")?.IsChecked ?? false;

            if (string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(category))
            {
                ShowError("Description and Category cannot be empty!");
                return;
            }

            if (description.Length > 50)
            {
                ShowError("Description cannot exceed 50 characters!");
                return;
            }

            if (category.Length > 30)
            {
                ShowError("Category cannot exceed 30 characters!");
                return;
            }

            if (!decimal.TryParse(amountText, out var amount))
            {
                ShowError("Invalid amount entered.");
                return;
            }

            if (!isIncome && !isExpense)
            {
                ShowError("Please select whether the transaction \nis an income or an expense.");
                return;
            }

            if (isExpense)
            {
                amount = -Math.Abs(amount);
            }

            NewTransaction = new Transaction
            {
                Id = Guid.NewGuid().GetHashCode(),
                Description = description,
                Category = category,
                Amount = amount,
                Date = DateTime.Now,
                IsIncome = isIncome
            };

            Close();
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
    }
}
