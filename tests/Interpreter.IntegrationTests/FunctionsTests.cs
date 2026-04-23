using Semantics.Exceptions;
using TestsLibrary;

using DeaInterpreter = Interpreter.Interpreter;

namespace Interpreter.IntegrationTests;

public class FunctionsTests
{
    [Theory]
    [MemberData(nameof(GetFunctionsData))]
    public void Can_evaluate_functions(string code, string expectedOutput)
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);

        interpreter.Execute(code);

        Assert.Equal(expectedOutput, environment.Output);
    }

    [Theory]
    [MemberData(nameof(GetInvalidFunctionsSemanticsData))]
    public void Rejects_invalid_functions_semantics(string code)
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);

        Assert.ThrowsAny<SemanticException>(() => interpreter.Execute(code));
    }

    public static TheoryData<string, string> GetFunctionsData()
    {
        return new TheoryData<string, string>
        {
            // Определение и вызов
            {
                """
                func int getFive() { return 5; }
                func int main() { print(getFive()); return 0; }
                """,
                "5"
            },
            {
                """
                func int square(int x) { return x * x; }
                func int main() { print(square(5) + 10); return 0; }
                """,
                "35"
            },
            {
                """
                func string repeat(string s, int n) { 
                    if (n <= 1) { return s; }
                    return s + repeatManual(s, n - 1); 
                }
                func string repeatManual(string s, int n) { return s; } 
                func int main() { print(repeat("a", 2)); return 0; }
                """,
                "aa"
            },
            {
                """
                proc sayHi() { print("hi"); }
                func int main() { sayHi(); return 0; }
                """,
                "hi"
            },
            {
                """
                func int getFive() { return 5; }
                func int square(int x) { return x * x; }
                func int main() { print(square(getFive())); return 0; }
                """,
                "25"
            },

            // Области видимости
            {
                """
                func int work() { int x = 10; return x; }
                func int main() { int x = 5; work(); print(x); return 0; }
                """,
                "5"
            },
            {
                "int x = 10; func int getX() { return x; } func int main() { print(getX()); return 0; }",
                "10"
            },
            {
                """
                proc testVal(int x) { x = 20; }
                func int main() { int a = 5; testVal(a); print(a); return 0; }
                """,
                "5"
            },
            {
                """
                int x = 1;
                func int shadow(int x) { return x; }
                func int main() { print(shadow(10)); return 0; }
                """,
                "10"
            },

            // Неявное приведение в return
            {
                """
                func bool retInt() { return 1; }

                func int main() {
                    print(retInt());
                    return 0;
                }
                """,
                "true"
            },
            {
                """
                func bool retNum() { return 1.5; }

                func int main() {
                    print(retNum());
                    return 0;
                }
                """,
                "true"
            },
            {
                """
                func bool retStr() { return "ok"; }

                func int main() {
                    print(retStr());
                    return 0;
                }
                """,
                "true"
            },
            {
                """
                func bool retEmpty() { return ""; }

                func int main() {
                    print(retEmpty());
                    return 0;
                }
                """,
                "false"
            },
        };
    }

    public static TheoryData<string> GetInvalidFunctionsSemanticsData()
    {
        return new TheoryData<string>
        {
            // Вызов до объявления
            { "func int main() { f(); return 0; } func int f() { return 1; }" },

            // Прямая рекурсия
            { "func int f(int n) { return f(n); } func int main() { return 0; }" },

            // Взаимная рекурсия
            { "func int f1() { return f2(); } func int f2() { return f1(); } func int main() { return 0; }" },

            // Переменная с именем параметра
            { "func int f(int x) { int x = 10; return x; } func int main() { return 0; }" },

            // Ошибка в количестве аргументов
            { "func int f(int a) { return a; } func int main() { f(1, 2); return 0; }" },

            // Ошибка в типах аргументов
            { "func int f(int a) { return a; } func int main() { f(\"str\"); return 0; }" },

            // Несоответствие возвращаемого типа (string к int нельзя привести)
            { "func int f() { return \"hi\"; } func int main() { return 0; }" },

            // Отсутствие return в func
            { "func int f() { print(1); } func int main() { return 0; }" },

            // Повторное объявление функции
            { "func int f() { return 1; } func int f() { return 2; } func int main() { return 0; }" },

            // Доступ к локальной переменной функции из main
            { "func int f() { int secret = 42; return 0; } func int main() { f(); print(secret); return 0; }" },
        };
    }
}