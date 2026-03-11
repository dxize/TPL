using System.Globalization;

using Execution;

using Reqnroll;

namespace Interpreter.Specs.Steps;

[Binding]
public class InterpreterSteps
{
    private const int Precision = 5;
    private static readonly double Tolerance = Math.Pow(0.1, Precision);

    private Context? _context;
    private FakeEnvironment? _environment;
    private string? _currentProgram;

    [Given(@"я запустил программу:")]
    public void GivenЯЗапустилПрограмму(string program)
    {
        _context = new Context();
        _environment = new FakeEnvironment();
        _currentProgram = program;
    }

    [Given(@"я установил входные данные:")]
    public void GivenЯУстановилВходныеДанные(Table table)
    {
        if (_environment == null)
        {
            throw new InvalidOperationException("Сначала нужно запустить программу");
        }

        List<double> inputs = table.Rows.Select(r => double.Parse(r["Число"], CultureInfo.InvariantCulture)).ToList();

        _environment = new FakeEnvironment(inputs.ToArray());
    }

    [When(@"я выполняю программу")]
    public void WhenЯВыполняюПрограмму()
    {
        if (_context == null || _environment == null || _currentProgram == null)
        {
            throw new InvalidOperationException("Сначала нужно запустить программу и установить входные данные");
        }

        Parser.Parser parser = new(_context, _currentProgram, _environment);
        parser.ParseProgram();
    }

    [Then(@"я получаю результаты:")]
    public void ThenЯПолучаюРезультаты(Table table)
    {
        if (_environment == null)
        {
            throw new InvalidOperationException("Программа не была выполнена");
        }

        List<double> expected = table.Rows.Select(r => double.Parse(r["Результат"], CultureInfo.InvariantCulture)).ToList();
        IReadOnlyList<double> actual = _environment.Results;

        for (int i = 0, iMax = Math.Min(expected.Count, actual.Count); i < iMax; ++i)
        {
            if (Math.Abs(expected[i] - actual[i]) >= Tolerance)
            {
                Assert.Fail($"Expected does not match actual at index {i}: {expected[i]} != {actual[i]}");
            }
        }

        if (expected.Count != actual.Count)
        {
            Assert.Fail(
                $"Actual results count does not match expected. Expected: {expected.Count}, Actual: {actual.Count}."
            );
        }
    }
}