## Эпик 1
### Arithmetic
```
func int main()
{
    int x;
    int y;
    print("Enter two integers: ");
    input(x);
    input(y);

    const int k = 2;
    int sum = x + y;
    int diff = x - y;
    int prod = x * y;
    num quot = x / y;
    int intDiv = x // y;
    int rem = x % y;
    num avg = (x + y) / k;
    int xSquared = x ^ 2;

    print("Sum: ", sum, "\n");
    print("Difference: ", diff, "\n");
    print("Product: ", prod, "\n");
    print("Real quotient: ", quot, "\n");
    print("Integer division: ", intDiv, "\n");
    print("Remainder: ", rem, "\n");
    print("Average: ", avg, "\n");
    print("x squared: ", xSquared, "\n");

    return 0;
}
```

### Strings
```
func int main()
{
    string s;
    print("Enter a word: ");
    input(s);

    const string punct = "!";
    string message = "You entered: " + s + punct + "\n";
    print(message);

    int length = len(s);
    print("length: ", length, "\n");

    string first = substr(s, 1, 1);
    string last = substr(s, length, 1);
    print("First character: ", first, "\n");
    print("Last character: ", last, "\n");

    string combined = first + " and " + last;
    print("Combined: ", combined, "\n");

    return 0;
}
```
## Эпик 2
### Prime
```
func bool isPrime(int n)
{
    if (n <= 1)
    {
        return false;
    }
    if (n == 2 || n == 3)
    {
        return true;
    }
    if (n % 2 == 0)
    {
        return false;
    }
    bool prime = true;
    int i = 3;
    while (i * i <= n)
    {
        if (n % i == 0)
        {
            prime = false;
            break;
        }
        i = i + 2;
    }
    return prime;
}

func int nextPrime(int n)
{
    int next = n + 1;
    while (!isPrime(next))
    {
        next = next + 1;
    }
    return next;
}

proc printResult(int n, bool prime, int next)
{
    if (prime)
    {
        print(n, " is prime. Next prime is ", next, "\n");
    }
    else
    {
        print(n, " is not prime. Next prime is ", next, "\n");
    }
}

func int main()
{
    int num;
    print("Enter an integer: ");
    input(num);
    bool prime = isPrime(num);
    int next = nextPrime(num);
    printResult(num, prime, next);
    return 0;
}
```

### MultiplicationTable
```
proc printMultiplicationTable(int N)
{
    int i;
    for (i = 1 to N)
    {
        int j;
        for (j = 1 to N)
        {
            int prod = i * j;
            print(i, " * ", j, " = ", prod, "\n");
        }
        print("----------------\n");
    }
}

func int main()
{
    int n;
    print("Enter N: ");
    input(n);
    printMultiplicationTable(n);
    return 0;
}
```