# language: ru-RU
Функциональность: Факториал числа

    Сценарий: Вычисление факториала числа N
        Дано я запустил программу:
            """
            func int factorial(int n) {
                int result = 1;
                int i = 2;
                
                while (i <= n) {
                    result = result * i;
                    i = i + 1;
                }
                
                return result;
            }

            int number;
            input(number);
            int fact = factorial(number);
            print(fact);
            """
        И я установил входные данные:
            | Число |
            | 5     |
        Когда я выполняю программу
        Тогда я получаю результаты:
            | Результат |
            | 120        |
