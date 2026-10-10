using System.Globalization;

namespace Backend;

public static class ExpressionEvaluator
{
    public static double Evalute(string infix)
    {
        return EvalutePostfix(ToPostfix(infix));
    }

    // Convert an infix expression to postfix notation.
    private static string ToPostfix(string infix)
    {
        if (string.IsNullOrWhiteSpace(infix))
        {
            throw new ArgumentException("The expression cannot be empty.");
        }

        var postfix = new List<string>();
        var stack = new Stack<char>();
        bool expectOperand = true;

        for (int i = 0; i < infix.Length; i++)
        {
            char item = infix[i];

            // Ignore spaces.
            if (char.IsWhiteSpace(item))
            {
                continue;
            }

            // Read complete numbers, including decimals.
            if (char.IsDigit(item) || item == '.')
            {
                if (!expectOperand)
                {
                    throw new ArgumentException("Invalid expression.");
                }

                int start = i;
                bool hasDecimal = false;
                bool hasDigit = false;

                while (i < infix.Length &&
                       (char.IsDigit(infix[i]) || infix[i] == '.'))
                {
                    if (infix[i] == '.')
                    {
                        if (hasDecimal)
                        {
                            throw new ArgumentException("Invalid number.");
                        }

                        hasDecimal = true;
                    }
                    else
                    {
                        hasDigit = true;
                    }

                    i++;
                }

                string number = infix[start..i];

                if (!hasDigit ||
                    !double.TryParse(
                        number,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out _))
                {
                    throw new ArgumentException("Invalid number.");
                }

                postfix.Add(number);
                i--;
                expectOperand = false;
                continue;
            }

            // Save opening parentheses in the stack.
            if (item == '(')
            {
                if (!expectOperand)
                {
                    throw new ArgumentException("Invalid expression.");
                }

                stack.Push(item);
                expectOperand = true;
                continue;
            }

            // Remove operators until the opening parenthesis.
            if (item == ')')
            {
                if (expectOperand)
                {
                    throw new ArgumentException("Invalid parentheses.");
                }

                while (stack.Count > 0 && stack.Peek() != '(')
                {
                    postfix.Add(stack.Pop().ToString());
                }

                if (stack.Count == 0)
                {
                    throw new ArgumentException("Invalid parentheses.");
                }

                stack.Pop();
                expectOperand = false;
                continue;
            }

            // Process mathematical operators.
            if (IsOperator(item))
            {
                // Handle positive and negative signs.
                if (expectOperand)
                {
                    if (item == '-')
                    {
                        stack.Push('~');
                        continue;
                    }

                    if (item == '+')
                    {
                        stack.Push('@');
                        continue;
                    }

                    throw new ArgumentException("Invalid operator.");
                }

                // Compare operator priorities.
                while (stack.Count > 0 &&
                       stack.Peek() != '(' &&
                       (Priority(stack.Peek()) > Priority(item) ||
                       (Priority(stack.Peek()) == Priority(item) &&
                        item != '^')))
                {
                    postfix.Add(stack.Pop().ToString());
                }

                stack.Push(item);
                expectOperand = true;
                continue;
            }

            throw new ArgumentException("Invalid character.");
        }

        if (expectOperand)
        {
            throw new ArgumentException("Incomplete expression.");
        }

        // Remove the remaining operators from the stack.
        while (stack.Count > 0)
        {
            char op = stack.Pop();

            if (op == '(')
            {
                throw new ArgumentException("Invalid parentheses.");
            }

            postfix.Add(op.ToString());
        }

        return string.Join(" ", postfix);
    }

    // Define the priority of each operator.
    private static int Priority(char op) => op switch
    {
        '^' => 4,
        '~' => 3,
        '@' => 3,
        '*' => 2,
        '/' => 2,
        '+' => 1,
        '-' => 1,
        _ => 0
    };

    // Check if a character is a mathematical operator.
    private static bool IsOperator(char item)
    {
        return item == '+' ||
               item == '-' ||
               item == '*' ||
               item == '/' ||
               item == '^';
    }

    // Evaluate a postfix expression using a stack.
    private static double EvalutePostfix(string postfix)
    {
        var stack = new Stack<double>();

        string[] items = postfix.Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries
        );

        foreach (string item in items)
        {
            // Add numbers to the stack.
            if (double.TryParse(
                    item,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double number))
            {
                stack.Push(number);
                continue;
            }

            // Apply positive or negative signs.
            if (item == "~" || item == "@")
            {
                if (stack.Count < 1)
                {
                    throw new ArgumentException("Invalid expression.");
                }

                double value = stack.Pop();

                stack.Push(item == "~" ? -value : value);
                continue;
            }

            // Get two numbers from the stack.
            if (item.Length != 1 ||
                !IsOperator(item[0]) ||
                stack.Count < 2)
            {
                throw new ArgumentException("Invalid expression.");
            }

            double ope2 = stack.Pop();
            double ope1 = stack.Pop();

            // Calculate and save the result.
            double result = Calculate(ope1, ope2, item[0]);

            stack.Push(result);
        }

        if (stack.Count != 1)
        {
            throw new ArgumentException("Invalid expression.");
        }

        return stack.Pop();
    }

    // Calculate the result of two numbers.
    private static double Calculate(
        double ope1,
        double ope2,
        char item)
    {
        if (item == '/' && ope2 == 0)
        {
            throw new DivideByZeroException("Division by zero.");
        }

        return item switch
        {
            '+' => ope1 + ope2,
            '-' => ope1 - ope2,
            '*' => ope1 * ope2,
            '/' => ope1 / ope2,
            '^' => Math.Pow(ope1, ope2),
            _ => throw new ArgumentException("Invalid operator.")
        };
    }
}