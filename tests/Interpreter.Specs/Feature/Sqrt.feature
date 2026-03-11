# language: ru-RU
Функциональность: Вычисление квадратного корня методом Ньютона

    Сценарий: Квадратный корень из 25
        Дано я запустил программу:
            """
            func sqrt(number) {
                const epsilon = 0.00001;
                
                if (number == 0 || number < 0)
                {
                    return 0;
                } 
                else 
                {
                    var guess = number / 2;
                    var previousGuess = number + 1;

                    while (abs(guess - previousGuess) > epsilon) {
                        previousGuess = guess;
                        guess = (guess + number / guess) / 2;
                    }
                    
                    return guess;
                }
            }

            var value;
            input(value);
            var root = sqrt(value);
            print(root);
            """
        И я установил входные данные:
            | Число |
            | 25    |
        Когда я выполняю программу
        Тогда я получаю результаты:
            | Результат |
            | 5         |
