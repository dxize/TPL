# Список интеграционных тестов Interpreter

`EntryPoint`

## Точка входа
- [x] Пустая функция:` func int main() { return 0; } `
- [x] Главная функция с телом из нескольких инструкций: `func int main() { print(1); return 0; }`
- [x] Главная функция с объявлением переменной: `func int main() { int x = 10; return 0; }`
- [x] В главной функции должен быть достижимый return:
  `func int main() { 
      if (false) {return 1; }
      print("skipped");
      return 0;
  }`
### Негативные тесты (точка входа, синтаксис)
- [x] Пропущен main: ` func int start() { return 0; }` => UnexpectedLexemeException
- [x] Пропущен int: `  func string main() { return 0; }` => UnexpectedLexemeException
- [x] После программы есть лишний код: `func int main() { return 0; } func int other() { return 0; }`=> UnexpectedLexemeException
### Негативные тесты (точка входа, семантика)
- [x] Отсутствует return: `func int main() { print(1); }` => SemanticException
- [x] Возвращение не int: `func int main() { return "dea"; }` => SemanticException
- [x] Не достижимый return: `func int main() {if (false) { return 0;} }`  => SemanticException

## (далее все тесты будут описаны в сокращенном варианте явно не показывая: func int main() {return 0;})

`InputOutput`

## Вывод
- [x] Пустая строка: `print("");`
- [x] Вывод нескольких аргументов разных типов: `print(1, 2.5, "dea");`
### Негативные тесты (Вывод, синтаксис)
- [x] Вывод без аргументов: `print();` => UnexpectedLexemeException
- [x] Лишняя запятая: `print(1,)` => UnexpectedLexemeException

## Ввод
- [x] Ввод строки в переменную: `string name; input(name);`
- [x] Ввод целого числа в переменную: `int x; input(x);`
- [x] Ввод вещественного числа в переменную: `num x; input(x);`
- [x] Ввод строки и последующий вывод: `string name; input(name); print(name);`
### Негативные тесты (Ввод)
- [x] Ввод вызывается не с идентификатором: `input(1);` => UnexpectedLexemeException
### Негативные тесты (Ввод, семантика)
- [x] Ввод вызывается с `const`: `const string name = "dea"; input(name);` => SemanticException
- [x] Ввод вызывается с необъявленной переменной: `input(name);` => SemanticException

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

### Негативные тесты (Арифметические, синтаксис)
- [x] Пропущен правый/левый операнд: `print(1 +);` => UnexpectedLexemeException
- [x] Пропущен правый/левый операнд: `print(* 2);` => UnexpectedLexemeException
- [x] Пропущена скобка: `print((1 + 2);` => UnexpectedLexemeException
- [x] Пустые скобки в выражении: `print(());` => UnexpectedLexemeException

### Конкатенация строк
- [x] Конкатенация двух строк: `print("dea" + "lang");`
- [x] Конкатенация строки и переменной: `string s = "dea"; print(s + "lang");`

### Операторы сравнения
- [x] Сравнение чисел (int/num): 
  `print(5 > 3);`
  `print(5.5 >= 3);`
  `print(5.0 == 5);`
  `print(5 == 5.0);`
  `print(2.5 <= 2.5);`
  `print(2.5 < 2.5);`
- [x] Сравнение строк:
  `print("apple" == "apple");`
  `print("apple" != "apple");`
- [x] Сравнение булевых значений: `print(true != false);`
- [x] Логическое И (AND): 
  `print(true && false);`
  `print(1 && 0);`
  `print("abc" && "");`
- [x] Логическое ИЛИ (OR): 
  `print(true || false);`
  `print(1 || 0);`
  `print("abc" || "");`
- [x] Отрицание:
  `print(!true);`
  `print(!!false);`
  `print(!10);`
  `print(!"");`

### Short-circuit (вычисления по короткой схеме)
- [x] AND не вычисляет правую часть, если левая false: `false && (1 / 0 == 0);`
- [x] OR не вычисляет правую часть, если левая true: `true || (1 / 0 == 0);`

`TypesSemantic`

### Негативные тесты (Арифметические, семантика)
- [x] Нельзя складывать число и строку: `print(10 + "5");` => SemanticException
- [x] Нельзя умножать строку на число: `print("cat" * 2);` => SemanticException
- [x] Нельзя делить строку на число: `print("ten" / 2);` => SemanticException
- [x] Нельзя вычитать число из строки: `print("10" - 5);` => SemanticException
- [x] Нельзя применять унарный минус к строке: `print(-"one");` => SemanticException
- [x] Нельзя использовать `//` для `num`: `print(7.5 // 2.0);` => SemanticException
- [x] Нельзя использовать `%` для `num`: `print(7.5 % 2.0);` => SemanticException
- [x] Нельзя конкатенировать строку с числом: `print("dea" + 1);` => SemanticException
- [x] Нельзя возводить строку в степень: `print("2" ^ 3);` => SemanticException

### Негативные тесты (Логика и сравнения, семантика)
- [x] Нельзя сравнивать разные типы (int и string): `print(1 == "asd");` => SemanticException
- [x] Нельзя сравнивать строки операциями (<, >, <=, >=): `print("abc" < "def");` => SemanticException

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

### Негативные тесты (Built-in операции над строками, семантика)
- [x] `len` без аргументов: `print(len());` => SemanticException
- [x] `len` с лишним аргументом: `print(len("dea", "x"));` => SemanticException
- [x] `len` от значения неверного типа: `print(len(10));` => SemanticException
- [x] `substr` с недостаточным числом аргументов: `print(substr("dea"));` => SemanticException
- [x] `substr` с аргументами неверных типов: 
`print(substr(8, 0, 1));` => SemanticException
`print(substr("dea", "x", 1));` => SemanticException
`print(substr("dea", 1, "x"));` => SemanticException

### Built-in функции (числа)
- [x] abs от целого числа: `print(abs(-5));`
- [x] abs от вещественного числа: `print(abs(-3.14));`
- [x] min от двух целых чисел: `print(min(3, 1));`
- [x] min от нескольких целых чисел: `print(min(5, 2, 7, 1));`
- [x] min от двух вещественных чисел: `print(min(3.5, 1.2));`
- [x] max от двух целых чисел: `print(max(3, 1));`
- [x] max от нескольких целых чисел: `print(max(5, 2, 7, 1));`
- [x] max от двух вещественных чисел: `print(max(3.5, 1.2));`

### Негативные тесты (Built-in функции чисел, семантика)
- [x] abs без аргументов: `print(abs());` => SemanticException
- [x] abs с лишним аргументом: `print(abs(1, 2));` => SemanticException
- [x] abs не с числами: `print(abs("42"));` => SemanticException
- [x] min без аргументов: `print(min());` => SemanticException
- [x] min с одним аргументом: `print(min(1));` => SemanticException
- [x] max без аргументов: `print(max());` => SemanticException
- [x] max с одним аргументом: `print(max(1));` => SemanticException
- [x] abs от строки: `print(abs("dea"));` => SemanticException
- [x] min с аргументами разных типов: `print(min(1, 1.5));` => SemanticException
- [x] max с аргументами разных типов: `print(max(1.5, 1));` => SemanticException
- [x] min не с числами: `print(min("dea", "deae"));` => SemanticException
- [x] max не с числами: `print(max("dea", "deae"));` => SemanticException

`Variables`

## Переменные и константы
- [x] Объявление и инициализация переменной типа int: `int x = 10;`
- [x] Объявление и инициализация переменной типа num: `num x = 3.14;`
- [x] Объявление и инициализация переменной типа string: `string s = "dea";`
- [x] Объявление и инициализация переменной типа bool: `bool b = true; print(b);`
- [x] Инициализация bool c неявным приведением: 
  `bool b = 1;`
  `bool b = 1.5;`
  `bool b = "dea";`
- [x] Объявление переменной без инициализации: `string name;`
- [x] Присваивание переменной значения того же типа: `int x = 1; x = 2;`
- [x] Использование переменной в выражении: `int x = 2; int y = 3; print(x + y);`
- [x] Использование переменной в print: `int x = 10; print(x);`
- [x] Объявление и использование константы типа int: `const int x = 10; print(x);`
- [x] Объявление и использование константы типа num: `const num pi = 3.14; print(pi);`
- [x] Объявление и использование константы типа string: `const string name = "dea"; print(name);`
- [x] Объявление и использование константы типа bool: `const bool flag = false; print(flag);`
- [x] Использование константы в выражении: `const int x = 10; print(x + 5);`
- [x] Использование глобальной переменной в main: `int x = 10; func int main() { print(x); return 0; }`
- [x] Использование глобальной константы в main: `const int x = 10; func int main() { print(x); return 0; }`

### Негативные тесты (синтаксические ошибки, связанные с переменными)
- [x] Присваивание не является выражением: `x = y = 0;` => UnexpectedLexemeException
- [x] Слева в присваивании должен быть идентификатор: `10 = x;` => UnexpectedLexemeException
- [x] Константа должна быть инициализирована при объявлении: `const int x;` => UnexpectedLexemeException

### Негативные тесты (семантические ошибки, связанные с переменными)
- [x] Нельзя использовать необъявленную переменную: `print(x);` => SemanticException
- [x] Нельзя инициализировать переменную значением другого типа: `int x = "dea";` => SemanticException
- [x] Нельзя присвоить переменной значение другого типа: `int x = 10; x = "dea";` => SemanticException
- [x] Нельзя изменять константу: `const int x = 10; x = 20;` => SemanticException
- [x] Нельзя повторно объявлять переменную с тем же именем в одной области видимости: `int x = 1; int x = 2;` => SemanticException
- [x] Нельзя повторно объявлять константу с тем же именем в одной области видимости: `const int x = 1; const int x = 2;` => SemanticException
- [x] Нельзя объявлять локальную переменную с именем глобальной переменной: `int x = 10; func int main() { int x = 20; return 0; }` => SemanticException
- [x] Нельзя объявлять локальную переменную с именем глобальной константы: `const int x = 10; func int main() { int x = 20; return 0; }` => SemanticException
- [x] Нельзя использовать имя встроенной функции как имя переменной: `int len = 10;` => SemanticException
- [x] Нельзя использовать имя встроенной функции как имя константы: `const int len = 10;` => SemanticException

### Негативные тесты выполнения, связанные с переменными
- [x] Нельзя читать значение неинициализированной переменной: `int x; print(x);` => RuntimeException

`IfElseTests`

### Ветвления (if..else)
- [x] Одиночный if: `if (true) { print(1); }`
- [x] Конструкция if..else: `if (5 > 10) { print(1); } else { print(2); }`
- [x] Вложенные if: `if (true) { if (false) { print(1); } else { print(2); } }`
- [x] Переменные внутри блоков (область видимости): `if (true) { int x = 5; print(x); }`
### Ветвления с неявным приведением
- [x] Число в условии: `if (10) { print(1); }`
- [x] Ноль в условии: `if (0) { print(1); } else { print(2); }`
- [x] Строка в условии: `if ("abc") { print(1); }`

### Негативные тесты (if, семантика/синтаксис)
- [x] Переменная из if недоступна снаружи: `if (true) { int x=1; } print(x);` => SemanticException
- [x] Ошибка синтаксиса (пропущены скобки): `if true { print(1); }` => UnexpectedLexemeException
- [x] Ошибка синтаксиса (пропущены фигурные скобки): `if (true) print(1);` => UnexpectedLexemeException

`FunctionsTests`
### Определение и вызов
- [x] Функция без аргументов: `func int getFive() { return 5; }`
- [x] Функция с одним аргументом: `func int square(int x) { return x * x; }`
- [x] Несколько аргументов разных типов: `func string repeat(string s, int n) { ... }`
- [x] Процедура (без возвращаемого значения): `proc sayHi() { print("hi"); }`
- [x] Вызов функции в выражении: `print(square(5) + 10);`
- [x] Вложенные вызовы: `print(square(getGive()));`
- [x] Гарантированный return из всех веток if/else

### Области видимости
- [x] Локальные переменные функции не пересекаются с main.
- [x] Функция видит глобальные переменные и константы.
- [x] Передача по значению: присваивание параметру внутри функции не меняет переменную снаружи.
- [x] Shadowing: Параметры функции перекрывают глобальные переменные с тем же именем.

### Неявное приведение в return
- [x] Функция bool возвращает 1 (станет true).
- [x] Функция bool возвращает 1.5 (приводится к true).
- [x] Функция bool возвращает "" (станет false).

### Негативные тесты(функции, семантика)
- [x] Вызов до объявления: => SemanticException
- [x] Прямая рекурсия: => SemanticException
- [x] Взаимная рекурсия: => SemanticException
- [x] Внутри функции объявлена переменная с именем параметра: => SemanticException
- [x] Ошибка в количестве аргументов: => SemanticException
- [x] Ошибка в несовпадении типов передаваемых аргументов:  => SemanticException
- [x] Несоответствие возвращаемого типа: => SemanticException
- [x] Отсутствие return в func: => SemanticException
- [x] Повторное объявление функции: => SemanticException
- [x] Доступ к локальной переменной функции из main: => SemanticException
- [x] Функция не может использовать return без значения
- [x] Процедура не может использовать return со значением
- [x] Пользовательская функция вызвана с неправильным числом параметров(избыток/недостаток).

`LoopsTests`

### Цикл while
- [x] while не выполняется если условие изначально ложно: `while (i > 10) { ... }`
- [x] while выполняется пока условие истинно: `while (i <= 3) { print(i); i = i + 1; }`
- [x] while с декрементом: `while (i > 0) { print(i); i = i - 1; }`

### Цикл for
- [x] for to базовый: `for (i = 1 to 3) { print(i); }` => "123"
- [x] for downto базовый: `for (i = 3 downto 1) { print(i); }` => "321"
- [x] for с одной итерацией: `for (i = 5 to 5) { print(i); }` => "5"
- [x] for to без итераций (start > end): `for (i = 5 to 3) { ... }`
- [x] for downto без итераций (start < end): `for (i = 1 downto 3) { ... }`
- [x] for с выражением в качестве end: `num end = 3.0; for (i = 1 to end) { ... }`
- [x] for с арифметикой в теле: `for (i = 0 to 5) { print(i + 1); }` => "123456"
- [x] for с аккумулятором: `int sum = 0; for (i = 1 to 5) { sum = sum + i; }`

### break и continue
- [x] break в while: `while (...) { if (i == 3) { break; } ... }`
- [x] break в for: `for (...) { if (i == 5) { break; } ... }`
- [x] continue в while: `while (...) { if (i == 3) { continue; } ... }`
- [x] continue в for: `for (...) { if (i == 3) { continue; } ... }`
- [x] break в for с точкой с запятой

### Вложенные конструкции
- [x] if внутри while: `while (...) { if (...) { ... } else { ... } }`
- [x] while внутри if: `if (...) { while (...) { ... } }`
- [x] вложенные if-else: `if (...) { if (...) { ... } else { ... } } else { ... }`
- [x] вложенные for: `for (i = 1 to 3) { for (j = 1 to 2) { ... } }`
- [x] for внутри while
- [x] while внутри for
- [x] break во вложенном цикле выходит только из внутреннего
- [x] continue во вложенном цикле

### Негативные тесты (циклы, синтаксис)
- [x] пропущены скобки в while: `while i < 3 { ... }` => UnexpectedLexemeException
- [x] пропущены скобки в for: `for i = 1 to 3 { ... }` => UnexpectedLexemeException

### Негативные тесты (циклы, семантика)
- [x] break вне цикла: `break;` => SemanticException
- [x] continue вне цикла: `continue;` => SemanticException
- [x] переменная цикла for не объявлена: `for (i = 1 to 3) { ... }` => SemanticException
- [x] переменная цикла for не int (num): `num i; for (i = 1 to 3) { ... }` => SemanticException
- [x] break внутри if но вне цикла: `if (true) { break; }` => SemanticException

`ScopeTests`

### Области видимости в if/else
- [x] Переменная внутри if видна только там; внешняя видна внутри: `if (true) { int y = 20; print(x); print(y); }`
- [x] Одинаковые имена переменных в разных ветках — не конфликтуют: `if (true) { int a = 1; } else { int a = 2; }`
- [x] Вложенные if: переменные на каждом уровне
- [x] Внешняя переменная мутируется внутри нескольких if
- [x] Одноимённые временные переменные в последовательных if — без конфликтов
- [x] Тройная вложенность if
- [x] Переменные в ветке else с одинаковыми именами в обеих ветках

### Области видимости в циклах
- [x] while: переменная внутри пересоздаётся каждую итерацию
- [x] for: переменная внутри пересоздаётся каждую итерацию
- [x] Одноимённая переменная может объявляться заново каждую итерацию
- [x] while: string-переменная пересоздаётся и не накапливается
- [x] for + вложенный for: переменные на обоих уровнях
- [x] Аккумулятор снаружи, шаг объявлен внутри
- [x] for downto: переменная внутри
- [x] bool-переменная внутри for
- [x] Повторное объявление переменной с одним именем в разных итерациях while

### Смешанная вложенность
- [x] if внутри for: переменная из if не мешает следующей итерации
- [x] while внутри if
- [x] for внутри if с накопителем
- [x] Одинаковые имена в if/else разных итераций for
- [x] while с if/else — аккумулятор снаружи
- [x] break внутри if внутри for
- [x] continue внутри if внутри while
- [x] Тройная вложенность for > while > if
- [x] Временные переменные во вложенных блоках + аккумулятор снаружи

### Негативные тесты (области видимости)
- [x] Переменная из if недоступна после блока => SemanticException
- [x] Переменная из else недоступна после блока => SemanticException
- [x] Переменная из while недоступна после цикла => SemanticException
- [x] Переменная из for недоступна после цикла => SemanticException
- [x] Переменная внутреннего if недоступна снаружи => SemanticException
- [x] Переменная внутреннего for недоступна в теле внешнего for => SemanticException