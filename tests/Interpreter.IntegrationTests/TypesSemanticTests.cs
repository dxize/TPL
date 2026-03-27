using Semantics.Exceptions;
using TestsLibrary;

using DeaInterpreter = Interpreter.Interpreter;

namespace Interpreter.IntegrationTests;

public class TypesSemanticTests
{
    [Theory]
    [MemberData(nameof(GetExpressionsWithTypeErrorsData))]
    public void Rejects_expressions_with_type_errors(string code)
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);

        // TODO переделать на конкретную ошибку
        Assert.Throws<SemanticException>(() => interpreter.Execute(code));
    }

    public static TheoryData<string> GetExpressionsWithTypeErrorsData()
    {
        return new TheoryData<string>
        {
            // Нельзя проводить арифметические операции со строками
            {
                """
                func int main() {
                    print(10 + "5");
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    print("cat" * 2);
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    print("ten" / 2);
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    print("10" - 5);
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    print(-"one");
                    return 0;
                }
                """
            },

            // Нельзя использовать операции // и % для num
            {
                """
                func int main() {
                    print(7.5 // 2.0);
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    print(7.5 % 2.0);
                    return 0;
                }
                """
            },

            // Нельзя конкатенировать string с не-string
            {
                """
                func int main() {
                    print("dea" + 1);
                    return 0;
                }
                """
            },
        };
    }
}