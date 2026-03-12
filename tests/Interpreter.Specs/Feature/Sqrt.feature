# language: ru-RU
Функциональность: Вычисление квадратного корня методом Ньютона

    Сценарий: Квадратный корень из 25
        Дано я запустил программу:
            """
            func num sqrt(num number) {
                const num epsilon = 0.00001;
                
                if (number == 0 || number < 0)
                {
                    return 0;
                } 
                else 
                {
                    num guess = number / 2;
                    num previousGuess = number + 1;

                    while (abs(guess - previousGuess) > epsilon) {
                        previousGuess = guess;
                        guess = (guess + number / guess) / 2;
                    }
                    
                    return guess;
                }
            }

            num value;
            input(value);
            num root = sqrt(value);
            print(root);
            """
        И я установил входные данные:
            | Число |
            | 25    |
        Когда я выполняю программу
        Тогда я получаю результаты:
            | Результат |
            | 5         |
