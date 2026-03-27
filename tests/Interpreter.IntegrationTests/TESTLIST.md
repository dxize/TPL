# Список интеграционных тестов Interpreter

## Точка входа
- [x] Пустая функция:` func int main() { return 0; } `
- [x] Главная функция с телом из нескольких инструкций: `func int main() { print(1); return 0; }`
- [x] Главная функция с объявлением переменной: `func int main() { int x = 10; return 0; }`
### Ошибки (точка входа, синтаксис)
- [x] Пропущен main: ` func int start() { return 0; }` => UnexpectedLexemeException
- [x] Пропущен int: `  func string main() { return 0; }` => UnexpectedLexemeException
- [x] После программы есть лишний код: `func int main() { return 0; } func int other() { return 0; }`=> UnexpectedLexemeException
### Ошибки (точка входа, семантика)
- [x] Отсутствует return: `func int main() { print(1); }` => SemanticException
- [x] Возвращение не int: `func int main() { return "dea"; }` => SemanticException

## (далее все тесты будут описаны в сокращенном варианте явно не показывая: func int main() { return 0; })

## Литералы + вывод
- [x] Целое число: `print(2025);`
- [x] Вещественное число: `print(3.14);`
- [x] Строка в двойных кавычках: `print("hello dea");`
- [x] Пустая строка: `print("");`
### Ошибки (Литералы + вывод, синтаксис)
- [x] Вывод без аргументов: `print();` => UnexpectedLexemeException
- [ ] Пропущена закрывающая скобка: `print(1;` => UnexpectedLexemeException
- [ ] Лишняя запятая: `print(1,)` => UnexpectedLexemeException

## Ввод
- [ ] Ввод строки в переменную: `string name; input(name);`
### Ошибки (Ввод)
- [ ] Ввод вызывается не с идентификатором: `input(1);` => UnexpectedLexemeException
### Ошибки (Ввод, семантика)
- [ ] Ввод вызывается с `const`: `const string name = "dea"; input(name);` => SemanticException
- [ ] Ввод вызывается с необъявленной переменной: `input(name);` => SemanticException

`Expressions`

## Арифметические операции над числами
- [x] Сложение целых чисел: `print(2 + 3);`
- [x] Вычитание целых чисел: `print(7 - 2);`
- [x] Умножение: `print(3 * 4);`
- [x] Деление: `print(8 / 2);`
- [x] Целочисленное деление: `print(7 // 2);`
- [x] Остаток от деления: `print(7 % 3);`
- [x] Возведение в степень: `print(2 ^ 3);`
- [x] Унарный минус: `print(-5);`
- [x] Унарный плюс: `print(+5);`

### Приоритеты и скобки
- [x] Приоритет умножения над сложением: `print(2 + 3 * 4);`
- [x] Скобки меняют приоритет: `print((2 + 3) * 4);`
- [x] Вложенные скобки: `print((1 + (2 * 3)));`

### Ассоциативность
- [x] Левая ассоциативность вычитания: `print(10 - 3 - 2);`
- [x] Левая ассоциативность деления: `print(16 / 4 / 2);`
- [x] Ассоциативность степени: `print(2 ^ 3 ^ 2);`
- [x] Приоритет унарного минуса и степени: `print(-2 ^ 3 ^ 2);`

### Ошибки (Арифметические, синтаксис)
- [x] Пропущен правый/левый операнд: `print(1 +);` => UnexpectedLexemeException
- [x] Пропущен правый/левый операнд: `print(* 2);` => UnexpectedLexemeException
- [x] Пропущена скобка: `print((1 + 2);` => UnexpectedLexemeException
- [x] Пустые скобки в выражении: `print(());` => UnexpectedLexemeException

## Операции над строками
### Конкатенация
- [x] Конкатенация двух строк: `print("dea" + "lang");`
- [x] Конкатенация строки и переменной: `string s = "dea"; print(s + "lang");`

`TypesSemantic`

### Ошибки (Арифметические, семантика)
- [x] Нельзя складывать число и строку: `print(10 + "5");` => SemanticException
- [x] Нельзя умножать строку на число: `print("cat" * 2);` => SemanticException
- [x] Нельзя делить строку на число: `print("ten" / 2);` => SemanticException
- [x] Нельзя вычитать число из строки: `print("10" - 5);` => SemanticException
- [x] Нельзя применять унарный минус к строке: `print(-"one");` => SemanticException
- [x] Нельзя использовать `//` для `num`: `print(7.5 // 2.0);` => SemanticException
- [x] Нельзя использовать `%` для `num`: `print(7.5 % 2.0);` => SemanticException
- [x] Нельзя конкатенировать строку с числом: `print("dea" + 1);` => SemanticException

`BuiltInFunctions`

### Built-in функции (строки)
### Длина строки
- [x] Длина непустой строки: `print(len("dea"));`
- [x] Длина пустой строки: `print(len(""));`
- [x] Длина строки в переменной: `string s = "hello"; print(len(s));`

### Декомпозиция строки
- [x] Подстрока из строки: `print(substr("dealang", 0, 3));`
- [x] Подстрока из переменной: `string s = "dealang"; print(substr(s, 3, 4));`
- [x] Вложенный вызов строковых функций: `string s = "dea"; print(substr(s, 0, len(s)));`

### Ошибки (Built-in операции над строками, семантика)
- [x] `len` без аргументов: `print(len());` => SemanticException
- [x] `len` с лишним аргументом: `print(len("dea", "x"));` => SemanticException
- [x] `len` от значения неверного типа: `print(len(10));` => SemanticException
- [x] `substr` с недостаточным числом аргументов: `print(substr("dea"));` => SemanticException
- [x] `substr` с аргументами неверных типов: `print(substr("dea", "x", 1));` => SemanticException
- [x] `substr` с неверным типом третьего аргумента: `print(substr("dea", 0, "x"));` => SemanticException

### Built-in функции (числа)
- [x] abs от целого числа: `print(abs(-5));`
- [x] abs от вещественного числа: `print(abs(-3.14));`
- [x] min от двух целых чисел: `print(min(3, 1));`
- [x] min от нескольких целых чисел: `print(min(5, 2, 7, 1));`
- [x] min от двух вещественных чисел: `print(min(3.5, 1.2));`
- [x] max от двух целых чисел: `print(max(3, 1));`
- [x] max от нескольких целых чисел: `print(max(5, 2, 7, 1));`
- [x] max от двух вещественных чисел: `print(max(3.5, 1.2));`

### Ошибки (Built-in функции чисел, семантика)
- [x] abs без аргументов: `print(abs());` => SemanticException
- [x] abs с лишним аргументом: `print(abs(1, 2));` => SemanticException
- [x] min без аргументов: `print(min());` => SemanticException
- [x] min с одним аргументом: `print(min(1));` => SemanticException
- [x] max без аргументов: `print(max());` => SemanticException
- [x] max с одним аргументом: `print(max(1));` => SemanticException
- [x] abs от строки: `print(abs("dea"));` => SemanticException
- [x] min с аргументами разных типов: `print(min(1, 1.5));` => SemanticException
- [x] max с аргументами разных типов: `print(max(1.5, 1));` => SemanticException