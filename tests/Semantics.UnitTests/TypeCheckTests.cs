using Semantics;
using Semantics.Exceptions;
using Xunit;

using DeaParser = global::Parser.Parser;

namespace Semantics.UnitTests;

/// <summary>
/// Отрицательные семантические тесты, проверяющие ошибки типов, выявляются SemanticsChecker,
/// и не передаются виртуальной машине.
/// </summary>
public class TypeCheckTests
{
    [Theory]
    [MemberData(nameof(GetBinaryTypeMismatchData))]
    public void Rejects_binary_expressions_with_type_mismatch(string code)
    {
        DeaParser parser = new(code);
        Ast.ProgramNode program = parser.ParseProgram();
        SemanticsChecker checker = new();

        SemanticException ex = Assert.ThrowsAny<SemanticException>(() => checker.Check(program));

        Assert.IsType<TypeErrorException>(ex);
    }

    [Theory]
    [MemberData(nameof(GetUnaryTypeMismatchData))]
    public void Rejects_unary_expressions_with_type_mismatch(string code)
    {
        DeaParser parser = new(code);
        Ast.ProgramNode program = parser.ParseProgram();
        SemanticsChecker checker = new();

        SemanticException ex = Assert.ThrowsAny<SemanticException>(() => checker.Check(program));

        Assert.IsType<TypeErrorException>(ex);
    }

    [Theory]
    [MemberData(nameof(GetAssignmentTypeMismatchData))]
    public void Rejects_assignment_with_type_mismatch(string code)
    {
        DeaParser parser = new(code);
        Ast.ProgramNode program = parser.ParseProgram();
        SemanticsChecker checker = new();

        SemanticException ex = Assert.ThrowsAny<SemanticException>(() => checker.Check(program));

        Assert.IsType<TypeErrorException>(ex);
    }

    [Theory]
    [MemberData(nameof(GetBuiltinTypeMismatchData))]
    public void Rejects_builtin_calls_with_type_mismatch(string code)
    {
        DeaParser parser = new(code);
        Ast.ProgramNode program = parser.ParseProgram();
        SemanticsChecker checker = new();

        SemanticException ex = Assert.ThrowsAny<SemanticException>(() => checker.Check(program));

        Assert.IsType<TypeErrorException>(ex);
    }

    public static TheoryData<string> GetBinaryTypeMismatchData()
    {
        return new TheoryData<string>
        {
            // int + string
            {
                """
                func int main() {
                    print(1 + "hello");
                    return 0;
                }
                """
            },

            // string + int
            {
                """
                func int main() {
                    print("hello" + 1);
                    return 0;
                }
                """
            },

            // string * int
            {
                """
                func int main() {
                    print("hello" * 2);
                    return 0;
                }
                """
            },

            // int * string
            {
                """
                func int main() {
                    print(2 * "hello");
                    return 0;
                }
                """
            },

            // string - int
            {
                """
                func int main() {
                    print("hello" - 1);
                    return 0;
                }
                """
            },

            // string / int
            {
                """
                func int main() {
                    print("hello" / 2);
                    return 0;
                }
                """
            },

            // string // int
            {
                """
                func int main() {
                    print("hello" // 2);
                    return 0;
                }
                """
            },

            // string % int
            {
                """
                func int main() {
                    print("hello" % 2);
                    return 0;
                }
                """
            },

            // num // num
            {
                """
                func int main() {
                    print(7.5 // 2.0);
                    return 0;
                }
                """
            },

            // num % num
            {
                """
                func int main() {
                    print(7.5 % 2.0);
                    return 0;
                }
                """
            },

            // num // int (смешанные типы)
            {
                """
                func int main() {
                    num x = 7.5;
                    int y = 2;
                    print(x // y);
                    return 0;
                }
                """
            },

            // string ^ int
            {
                """
                func int main() {
                    print("2" ^ 3);
                    return 0;
                }
                """
            },

            // через переменные: string + int
            {
                """
                func int main() {
                    string s = "hello";
                    int n = 5;
                    print(s + n);
                    return 0;
                }
                """
            },
        };
    }

    public static TheoryData<string> GetUnaryTypeMismatchData()
    {
        return new TheoryData<string>
        {
            // унарный минус для строкового литерала
            {
                """
                func int main() {
                    print(-"hello");
                    return 0;
                }
                """
            },

            // унарный плюс для строкового литерала
            {
                """
                func int main() {
                    print(+"hello");
                    return 0;
                }
                """
            },

            // унарный минус для строковой переменной
            {
                """
                func int main() {
                    string s = "hello";
                    print(-s);
                    return 0;
                }
                """
            },
        };
    }

    public static TheoryData<string> GetAssignmentTypeMismatchData()
    {
        return new TheoryData<string>
        {
            // присваивание строки в int-переменную
            {
                """
                func int main() {
                    int x;
                    x = "hello";
                    return 0;
                }
                """
            },

            // присваивание int в string-переменную
            {
                """
                func int main() {
                    string s;
                    s = 42;
                    return 0;
                }
                """
            },

            // присваивание num в int-переменную
            {
                """
                func int main() {
                    int x;
                    x = 3.14;
                    return 0;
                }
                """
            },

            // присваивание int в num-переменную
            {
                """
                func int main() {
                    num x;
                    x = 42;
                    return 0;
                }
                """
            },

            // инициализация int-переменной строкой
            {
                """
                func int main() {
                    int x = "hello";
                    return 0;
                }
                """
            },

            // инициализация string-переменной числом
            {
                """
                func int main() {
                    string s = 42;
                    return 0;
                }
                """
            },

            // инициализация const int строкой
            {
                """
                func int main() {
                    const int c = "hello";
                    return 0;
                }
                """
            },
        };
    }

    public static TheoryData<string> GetBuiltinTypeMismatchData()
    {
        return new TheoryData<string>
        {

            // len с num
            {
                """
                func int main() {
                    print(len(3.14));
                    return 0;
                }
                """
            },

            // substr: первый аргумент не строка
            {
                """
                func int main() {
                    print(substr(42, 0, 1));
                    return 0;
                }
                """
            },

            // substr: второй аргумент не int
            {
                """
                func int main() {
                    print(substr("hello", 0.5, 1));
                    return 0;
                }
                """
            },

            // substr: третий аргумент не int
            {
                """
                func int main() {
                    print(substr("hello", 0, 1.5));
                    return 0;
                }
                """
            },
        };
    }

}