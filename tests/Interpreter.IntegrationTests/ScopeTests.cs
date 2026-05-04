using Semantics.Exceptions;
using TestsLibrary;

using DeaInterpreter = Interpreter.Interpreter;

namespace Interpreter.IntegrationTests;

public class ScopeTests
{
    [Theory]
    [MemberData(nameof(GetIfElseScopeData))]
    public void Scopes_work_correctly_in_if_else(string code, string expectedOutput)
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);
        interpreter.Execute(code);
        Assert.Equal(expectedOutput, environment.Output);
    }

    [Theory]
    [MemberData(nameof(GetLoopScopeData))]
    public void Scopes_work_correctly_in_loops(string code, string expectedOutput)
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);
        interpreter.Execute(code);
        Assert.Equal(expectedOutput, environment.Output);
    }

    [Theory]
    [MemberData(nameof(GetNestedMixedScopeData))]
    public void Scopes_work_correctly_in_mixed_nesting(string code, string expectedOutput)
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);
        interpreter.Execute(code);
        Assert.Equal(expectedOutput, environment.Output);
    }

    [Theory]
    [MemberData(nameof(GetScopeLeakData))]
    public void Variable_does_not_leak_outside_block(string code)
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);
        Assert.ThrowsAny<SemanticException>(() => interpreter.Execute(code));
    }

    public static TheoryData<string, string> GetIfElseScopeData()
    {
        return new TheoryData<string, string>
        {
            {
                """
                func int main() {
                    int x = 10;
                    if (true) {
                        int y = 20;
                        print(x);
                        print(y);
                    }
                    print(x);
                    return 0;
                }
                """,
                "102010"
            },
            {
                """
                func int main() {
                    if (true) {
                        int a = 1;
                        print(a);
                    } else {
                        int a = 2;
                        print(a);
                    }
                    return 0;
                }
                """,
                "1"
            },
            {
                """
                func int main() {
                    if (false) {
                        int a = 1;
                        print(a);
                    } else {
                        int a = 2;
                        print(a);
                    }
                    return 0;
                }
                """,
                "2"
            },
            {
                """
                func int main() {
                    int x = 0;
                    if (true) {
                        int y = 5;
                        if (y > 3) {
                            int z = y + 1;
                            print(z);
                        }
                        print(y);
                    }
                    print(x);
                    return 0;
                }
                """,
                "650"
            },
            {
                """
                func int main() {
                    int count = 0;
                    if (true) { count = count + 1; }
                    if (true) { count = count + 1; }
                    if (true) { count = count + 1; }
                    print(count);
                    return 0;
                }
                """,
                "3"
            },
            {
                """
                func int main() {
                    int result = 0;
                    if (true) {
                        int tmp = 10;
                        result = result + tmp;
                    }
                    if (true) {
                        int tmp = 20;
                        result = result + tmp;
                    }
                    print(result);
                    return 0;
                }
                """,
                "30"
            },
            {
                """
                func int main() {
                    int a = 1;
                    if (a == 1) {
                        int b = 2;
                        if (b == 2) {
                            int c = 3;
                            if (c == 3) {
                                print(a);
                                print(b);
                                print(c);
                            }
                        }
                    }
                    return 0;
                }
                """,
                "123"
            },
            {
                """
                func int main() {
                    int i = 0;
                    int total = 0;
                    while (i < 4) {
                        if (i < 2) {
                            int side = 1;
                            total = total + side;
                        } else {
                            int side = 10;
                            total = total + side;
                        }
                        i = i + 1;
                    }
                    print(total);
                    return 0;
                }
                """,
                "22"
            },
        };
    }

    public static TheoryData<string, string> GetLoopScopeData()
    {
        return new TheoryData<string, string>
        {
            {
                """
                func int main() {
                    int i = 0;
                    while (i < 3) {
                        int x = i * 2;
                        print(x);
                        i = i + 1;
                    }
                    return 0;
                }
                """,
                "024"
            },
            {
                """
                func int main() {
                    int i;
                    for (i = 1 to 4) {
                        int sq = i * i;
                        print(sq);
                    }
                    return 0;
                }
                """,
                "14916"
            },
            {
                """
                func int main() {
                    int i;
                    int total = 0;
                    for (i = 1 to 3) {
                        int local = 10;
                        total = total + local;
                    }
                    print(total);
                    return 0;
                }
                """,
                "30"
            },
            {
                """
                func int main() {
                    int i = 0;
                    while (i < 3) {
                        string s = "x";
                        print(s);
                        i = i + 1;
                    }
                    return 0;
                }
                """,
                "xxx"
            },
            {
                """
                func int main() {
                    int i;
                    for (i = 1 to 2) {
                        int outer = i * 10;
                        int j;
                        for (j = 1 to 2) {
                            int inner = outer + j;
                            print(inner);
                        }
                    }
                    return 0;
                }
                """,
                "11122122"
            },
            {
                """
                func int main() {
                    int i = 0;
                    int result = 0;
                    while (i < 4) {
                        int step = i + 1;
                        result = result + step;
                        i = i + 1;
                    }
                    print(result);
                    return 0;
                }
                """,
                "10"
            },
            {
                """
                func int main() {
                    int i;
                    for (i = 3 downto 1) {
                        int val = i * i;
                        print(val);
                    }
                    return 0;
                }
                """,
                "941"
            },
            {
                """
                func int main() {
                    int i;
                    for (i = 1 to 5) {
                        bool even = (i == 2);
                        if (even) {
                            print(i);
                        }
                    }
                    return 0;
                }
                """,
                "2"
            },
            {
                """
                func int main() {
                    int i = 1;
                    while (i <= 3) {
                        int val = i;
                        print(val);
                        i = i + 1;
                    }
                    return 0;
                }
                """,
                "123"
            },
        };
    }

    public static TheoryData<string, string> GetNestedMixedScopeData()
    {
        return new TheoryData<string, string>
        {
            {
                """
                func int main() {
                    int i;
                    for (i = 1 to 4) {
                        if (i == 2) {
                            int special = 99;
                            print(special);
                        } else {
                            print(i);
                        }
                    }
                    return 0;
                }
                """,
                "19934"
            },
            {
                """
                func int main() {
                    int x = 3;
                    if (x > 0) {
                        int count = 0;
                        while (count < x) {
                            int tmp = count + 1;
                            count = tmp;
                        }
                        print(count);
                    }
                    return 0;
                }
                """,
                "3"
            },
            {
                """
                func int main() {
                    bool flag = true;
                    if (flag) {
                        int sum = 0;
                        int i;
                        for (i = 1 to 5) {
                            int cur = i;
                            sum = sum + cur;
                        }
                        print(sum);
                    }
                    return 0;
                }
                """,
                "15"
            },
            {
                """
                func int main() {
                    int i;
                    for (i = 1 to 3) {
                        if (i < 3) {
                            int msg = i * 10;
                            print(msg);
                        } else {
                            int msg = i * 100;
                            print(msg);
                        }
                    }
                    return 0;
                }
                """,
                "10 20 300".Replace(" ", "")
            },
            {
                """
                func int main() {
                    int i = 1;
                    int sum = 0;
                    while (i <= 5) {
                        if (i <= 3) {
                            int add = i;
                            sum = sum + add;
                        } else {
                            int sub = i - 3;
                            sum = sum - sub;
                        }
                        i = i + 1;
                    }
                    print(sum);
                    return 0;
                }
                """,
                "3"
            },
            {
                """
                func int main() {
                    int i;
                    for (i = 1 to 10) {
                        int doubled = i * 2;
                        if (doubled > 8) {
                            break;
                        }
                        print(doubled);
                    }
                    return 0;
                }
                """,
                "2468"
            },
            {
                """
                func int main() {
                    int i = 0;
                    while (i < 6) {
                        i = i + 1;
                        if (i == 3) {
                            int skip = 1;
                            continue;
                        }
                        int val = i * i;
                        print(val);
                    }
                    return 0;
                }
                """,
                "14162536"
            },
            {
                """
                func int main() {
                    int i;
                    for (i = 1 to 2) {
                        int j = 0;
                        while (j < 2) {
                            j = j + 1;
                            if (j == 1) {
                                int tag = i * 100 + j;
                                print(tag);
                            }
                        }
                    }
                    return 0;
                }
                """,
                "101201"
            },
            {
                """
                func int main() {
                    int total = 0;
                    int i;
                    for (i = 1 to 3) {
                        int a = i;
                        if (a > 1) {
                            int b = a * 2;
                            total = total + b;
                        } else {
                            total = total + a;
                        }
                    }
                    print(total);
                    return 0;
                }
                """,
                "11"
            },
        };
    }

    public static TheoryData<string> GetScopeLeakData()
    {
        return new TheoryData<string>
        {
            {
                """
                func int main() {
                    if (true) {
                        int x = 1;
                    }
                    print(x);
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    if (false) {
                        int x = 1;
                    } else {
                        int y = 2;
                    }
                    print(y);
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    int i = 0;
                    while (i < 3) {
                        int inner = i;
                        i = i + 1;
                    }
                    print(inner);
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    int i;
                    for (i = 1 to 3) {
                        int val = i * 2;
                    }
                    print(val);
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    int x = 5;
                    if (x > 0) {
                        int inner = x + 1;
                    }
                    print(inner);
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    int i;
                    for (i = 1 to 2) {
                        int j;
                        for (j = 1 to 2) {
                            int deep = i + j;
                        }
                        print(deep);
                    }
                    return 0;
                }
                """
            },
        };
    }
}