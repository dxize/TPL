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

		Assert.Equal(expected, res[0]);
	}

	public static TheoryData<string, object> GetExpressionTestData()
	{
		return new TheoryData<string, object>
		{
			// Числовые литералы
			{ "2025", 2025 },
			{ "3.14", 3.14 },
			{ "0", 0 },
			{ "123.456", 123.456 },

			// Унарные операторы
			{ "-5", -5 },
			{ "+5", 5 },
			{ "-3.14", -3.14 },
			{ "+42", 42 },

			// Логическое НЕ
			{ "!1", false },
			{ "!0", true },

			// Арифметические операторы
			{ "1 + 2", 3 },
			{ "5 - 3", 2 },
			{ "2 * 3", 6 },
			{ "6 / 2", 3.0 },
			{ "7 // 2", 3 },
			{ "7 % 3", 1 },
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
			{ "2 + 3 * 4", 14 },
			{ "10 - 3 - 2", 5 },
			{ "12 / 3 / 2", 2.0 },
			{ "-3 + 2", -1 },
			{ "3 + 2 * 3", 9 },
			{ "1 + 2 > 2", true },
			{ "1 < 2 == 5 > 4", true },
			{ "2 ^ 3 * 2", 16.0 },
			{ "2 * 3 ^ 2", 18.0 },

			// Скобки
			{ "(1 + 2) * 3", 9 },
			{ "2 * (3 + 4)", 14 },
			{ "((1 + 2) * 3) + 4", 13 },

			// Встроенные функции
			{ "abs(-5)", 5 },
			{ "abs(5)", 5 },
			{ "abs(-3.14)", 3.14 },
			{ "min(7, 3, 5)", 3 },
			{ "min(1)", 1 },
			{ "min(5, 2)", 2 },
			{ "max(2, 8, 4)", 8 },
			{ "max(1)", 1 },
			{ "max(5, 2)", 5 },

			// Комбинированные выражения
			{ "abs(-5) + 3", 8 },
			{ "min(10, 20) * 2", 20 },
			{ "max(1, 2, 3) ^ 2", 9.0 },
			{ "(1 + 2) * abs(-3)", 9 },

            // String operations
            { "\"hello\" + \" world\"", "hello world" },
            { "len(\"abc\")", 3 },
            { "substr(\"hello\", 1, 3)", "ell" },
		};
	}

}