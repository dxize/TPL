# Список тестов парсера

#### Точка входа
- [x] Пустая функция:` func int main() { return 0; } `
#### Ошибки (точка входа)
- [x] Пропущен main: ` func int start() { return 0; }` => UnexpectedLexemeException
- [x] Пропущен int: `  func string main() { return 0; }` => UnexpectedLexemeException
- [x] Возврат не int в return: `func string main() { return "dea"; }` => UnexpectedLexemeException

## Выражения (expressions-grammar.md)
## Инструкции верхнего уровня (top-level-grammar.md)

#### Литералы + вывод
- [x] Целое число: `print(2025)` => 2025
- [x] Вещественное число: `print(3.14)` => 3.14
- [x] Строка в двойных кавычках: `print("hello dea")` => "hello dea"
- [x] Пустая строка: `print("")` => ""
- [ ] Вывод нескольких аргументов: `print(1, 2.5, "dea");` => 1, 2.5, "dea"

## Ошибки (Инструкции верхнего уровня)
- [x] Вывод без аргументов: `print();` => UnexpectedLexemeException