# language: ru-RU
Функциональность: Факториал числа

    Сценарий: Вычисление факториала числа N
        Дано я запустил программу:
            """
            func factorial(n) {
                var result = 1;
                var i = 2;
                
                while (i <= n) {
                    result = result * i;
                    i = i + 1;
                }
                
                return result;
            }

            var number;
            input(number);
            var fact = factorial(number);
            print(fact);
            """
        И я установил входные данные:
            | Число |
            | 5     |
        Когда я выполняю программу
        Тогда я получаю результаты:
            | Результат |
            | 120        |
