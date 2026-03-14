# Список тестов парсера

#### Точка входа
- [x] Пустая функция:` func int main() { return 0; } `
#### Ошибки (точка входа)
- [x] Пропущен main: ` func int start() { return 0; }` => UnexpectedLexemeException
- [x] Пропущен int: `  func string main() { return 0; }` => UnexpectedLexemeException
- [x] Возврат не int в return: `func int main() { return "dea"; }` => UnexpectedLexemeException

#### Литералы + вывод
- [x] Целое число: `print(2025)` => 2025
- [x] Вещественное число: `print(3.14)` => 3.14
- [x] Строка в двойных кавычках: `print("hello dea")` => "hello dea"
- [x] Пустая строка: `print("")` => ""
- [x] Вывод нескольких аргументов: `print(1, 2.5, "dea");` => 1, 2.5, "dea"

#### Ошибки (Литералы + вывод)
- [x] Вывод без аргументов: `print();` => UnexpectedLexemeException