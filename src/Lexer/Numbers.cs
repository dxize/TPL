namespace Lexer;

public static class Numbers
{
    public const double Tolerance = 0.001;

    public static bool AreEqual(double a, double b)
    {
        return Math.Abs(a - b) < Tolerance;
    }

    public static bool AreNotEqual(double a, double b)
    {
        return Math.Abs(a - b) > Tolerance;
    }

    public static bool IsLessThan(double a, double b)
    {
        return a < b && !AreEqual(a, b);
    }

    public static bool IsGreaterThan(double a, double b)
    {
        return a > b && !AreEqual(a, b);
    }

    public static bool IsLessOrEqual(double a, double b)
    {
        return a < b || AreEqual(a, b);
    }

    public static bool IsGreaterOrEqual(double a, double b)
    {
        return a > b || AreEqual(a, b);
    }
}