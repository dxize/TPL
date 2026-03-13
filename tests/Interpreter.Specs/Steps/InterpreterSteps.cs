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

        List<object> inputs = [];
        foreach (DataTableRow row in table.Rows)
        {
            string val = row["Число"];
            if (int.TryParse(val, out int i))
            {
                inputs.Add(i);
            }
            else if (double.TryParse(val, CultureInfo.InvariantCulture, out double d))
            {
                inputs.Add(d);
            }
            else
            {
                inputs.Add(val);
            }
        }

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

        List<string> expectedStrings = table.Rows.Select(r => r["Результат"]).ToList();
        IReadOnlyList<object> actual = _environment.Results;

        Assert.Equal(expectedStrings.Count, actual.Count);

        for (int i = 0; i < actual.Count; i++)
        {
            object actVal = actual[i];
            string expStr = expectedStrings[i];

            if (actVal is double d)
            {
                double expVal = double.Parse(expStr, CultureInfo.InvariantCulture);
                if (Math.Abs(expVal - d) >= Tolerance)
                {
                    Assert.Fail($"Expected does not match actual at index {i}: {expVal} != {d}");
                }
            }
            else if (actVal is int iVal)
            {
                int expVal = int.Parse(expStr, CultureInfo.InvariantCulture);
                Assert.Equal(expVal, iVal);
            }
            else if (actVal is bool b)
            {
                bool expVal = bool.Parse(expStr);
                Assert.Equal(expVal, b);
            }
            else
            {
                Assert.Equal(expStr, actVal.ToString());
            }
        }
    }
}