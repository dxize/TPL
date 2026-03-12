# language: ru-RU
Функциональность: Сумма квадратов чисел от 1 до N

    Сценарий: Сумма квадратов от 1 до 5
        Дано я запустил программу:
            """
            func int sumSquares(int n) {
                if (n < 1) 
                {
                    return 0;
                } 
                else 
                {
                    int sum = 0;
                    int i;
                    
                    for (i = 1 to n) {
                        sum = sum + i * i;
                    }
                    
                    return sum;
                }
            }

            int n;
            input(n);
            print(sumSquares(n));
            """
        И я установил входные данные:
            | Число |
            | 5     |
        Когда я выполняю программу
        Тогда я получаю результаты:
            | Результат |
            | 55        |