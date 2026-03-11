using Execution;

namespace Parser.UnitTests;

public class ParserTests
{
	[Theory]
	[MemberData(nameof(GetExpressionTestData))]
	public void Can_parse_expressions(string code, object expected)
	{
		FakeEnvironment env = new();
		Context context = new();
		Parser parser = new(context, code, env);
		Row res = parser.EvaluateExpression();

		if (expected == null)
			Assert.Null(res[0]);
		else if (expected is double expectedDouble)
			Assert.Equal(expectedDouble, (double)res[0]);
		else if (expected is bool expectedBool)
			Assert.Equal(expectedBool ? 1.0 : 0.0, (double)res[0]);
		else
			Assert.Equal(expected, res[0]);
	}

	public static TheoryData<string, object> GetExpressionTestData()
	{
		return new TheoryData<string, object>
		{
			// Числовые литералы
			{ "2025", 2025.0 },
			{ "3.14", 3.14 },
			{ "0", 0.0 },
			{ "123.456", 123.456 },

			// Унарные операторы
			{ "-5", -5.0 },
			{ "+5", 5.0 },
			{ "-3.14", -3.14 },
			{ "+42", 42.0 },

			// Логическое НЕ
			{ "!1", false },
			{ "!0", true },

			// Арифметические операторы
			{ "1 + 2", 3.0 },
			{ "5 - 3", 2.0 },
			{ "2 * 3", 6.0 },
			{ "6 / 2", 3.0 },
			{ "7 // 2", 3.0 },
			{ "7 % 3", 1.0 },
			{ "2 ^ 3", 8.0 },
			{ "2 ^ 3 ^ 2", 512.0 },
			{ "(-2) ^ 3", -8.0 },
			{ "4 ^ 0.5", 2.0 }, 

			// Операторы сравнения
			{ "3 < 5", true },
			{ "5 < 5", false },
			{ "6 < 5", false },
			{ "3 > 5", false },
			{ "5 > 5", false },
			{ "6 > 5", true },
			{ "3 <= 5", true },
			{ "5 <= 5", true },
			{ "6 <= 5", false },
			{ "3 >= 5", false },
			{ "5 >= 5", true },
			{ "6 >= 5", true },

			// Операторы равенства/неравенства
			{ "5 == 5", true },
			{ "5 == 3", false },
			{ "5 != 3", true },
			{ "5 != 5", false },

			// Логические операторы
			{ "1 && 1", true },
			{ "1 && 0", false },
			{ "0 && 1", false },
			{ "0 && 0", false },
			{ "1 || 1", true },
			{ "1 || 0", true },
			{ "0 || 1", true },
			{ "0 || 0", false },

			// Приоритет операторов
			{ "2 + 3 * 4", 14.0 },
			{ "10 - 3 - 2", 5.0 },
			{ "12 / 3 / 2", 2.0 },
			{ "-3 + 2", -1.0 },
			{ "3 + 2 * 3", 9.0 },
			{ "1 + 2 > 2", true },
			{ "1 < 2 == 5 > 4", true },
			{ "2 ^ 3 * 2", 16.0 },
			{ "2 * 3 ^ 2", 18.0 },

			// Скобки
			{ "(1 + 2) * 3", 9.0 },
			{ "2 * (3 + 4)", 14.0 },
			{ "((1 + 2) * 3) + 4", 13.0 },

			// Встроенные функции
			{ "abs(-5)", 5.0 },
			{ "abs(5)", 5.0 },
			{ "abs(-3.14)", 3.14 },
			{ "min(7, 3, 5)", 3.0 },
			{ "min(1)", 1.0 },
			{ "min(5, 2)", 2.0 },
			{ "max(2, 8, 4)", 8.0 },
			{ "max(1)", 1.0 },
			{ "max(5, 2)", 5.0 },

			// Комбинированные выражения
			{ "abs(-5) + 3", 8.0 },
			{ "min(10, 20) * 2", 20.0 },
			{ "max(1, 2, 3) ^ 2", 9.0 },
			{ "(1 + 2) * abs(-3)", 9.0 },
		};
	}

}