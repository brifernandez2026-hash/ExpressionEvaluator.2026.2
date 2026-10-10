using Backend;

// Mathematical expressions to test the calculator.
string[] expressions =
{
    "4*5/(4+6)",
    "4*(5+6-(8/2^3)-7)-1",
    "4*7^(1/3)*7*((1+9)/3*7^4)",
    "144^(1/2)",
    "12+3*2",
    "25.5+4.5",
    "-3+5",
    "(10+5)*2"
};

// Evaluate each expression and show the result.
foreach (string infix in expressions)
{
    try
    {
        double result = ExpressionEvaluator.Evalute(infix);

        Console.WriteLine(
            $"Infix = {infix}, Result = {result:N5}"
        );
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            $"Infix = {infix}, Error = {ex.Message}"
        );
    }
}