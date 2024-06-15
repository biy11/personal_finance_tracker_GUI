// Views/AddTransactionDialog.axaml.cs
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
        public Transaction? NewTransaction { get; private set; } // Make NewTransaction nullable

        public AddTransactionDialog()
        {
            InitializeComponent();
        }

        private void OnAddClick(object sender, RoutedEventArgs e)
        {
            var description = this.FindControl<TextBox>("DescriptionTextBox")?.Text;
            var category = this.FindControl<TextBox>("CategoryTextBox")?.Text;
            var amountText = this.FindControl<TextBox>("AmountTextBox")?.Text;
            var isIncome = this.FindControl<CheckBox>("IsIncomeCheckBox")?.IsChecked ?? false;

            if (string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(category))
            {
                ShowError("Description and Category cannot be empty!");
                return;
            }

            if (decimal.TryParse(amountText, out var amount))
            {
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
            else
            {
                ShowError("Invalid amount entered.");
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
                Background = new SolidColorBrush(Colors.White) // Ensure background is white for readability
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
