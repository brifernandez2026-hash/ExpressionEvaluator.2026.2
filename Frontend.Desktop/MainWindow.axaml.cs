using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Backend;

namespace Frontend.Desktop;

public partial class MainWindow : Window
{
    private string expression = "";
    private bool justCalculated = false;

    public MainWindow()
    {
        InitializeComponent();
    }

    // Update the calculator screen.
    private void UpdateDisplay()
    {
        Display.Text = expression.Length == 0 ? "0" : expression;
    }

    // Handle calculator buttons.
    private void OnButtonClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        string value = button.Content?.ToString() ?? "";

        // Clear the entire expression.
        if (value == "Clear")
        {
            expression = "";
            justCalculated = false;
            UpdateDisplay();
            return;
        }

        // Delete the last character.
        if (value == "Delete")
        {
            if (expression.Length > 0)
            {
                expression = expression[..^1];
            }

            justCalculated = false;
            UpdateDisplay();
            return;
        }

        // Calculate using the stack evaluator.
        if (value == "=")
        {
            if (expression.Length == 0)
            {
                return;
            }

            try
            {
                double result = ExpressionEvaluator.Evalute(expression);

                if (double.IsNaN(result) || double.IsInfinity(result))
                {
                    throw new ArithmeticException();
                }

                string answer = result.ToString(
                    "G15",
                    CultureInfo.InvariantCulture
                );

                Display.Text = expression + "=" + answer;
                expression = answer;
            }
            catch (Exception)
            {
                Display.Text = "Error";
                expression = "";
            }

            justCalculated = true;
            return;
        }

        if (value.Length == 0)
        {
            return;
        }

        // Start a new calculation after showing a result.
        if (justCalculated)
        {
            if (char.IsDigit(value[0]) ||
                value == "." ||
                value == "(")
            {
                expression = "";
            }

            justCalculated = false;
        }

        // Handle decimal numbers.
        if (value == ".")
        {
            int lastOperator = expression.LastIndexOfAny(
                new[] { '+', '-', '*', '/', '^', '(', ')' }
            );

            string number = expression[(lastOperator + 1)..];

            if (number.Contains('.'))
            {
                return;
            }

            if (expression.Length == 0 ||
                !char.IsDigit(expression[^1]))
            {
                expression += "0";
            }
        }

        // Avoid unnecessary leading zeros.
        if (expression == "0" && char.IsDigit(value[0]))
        {
            expression = value;
        }
        else
        {
            expression += value;
        }

        UpdateDisplay();
    }
}