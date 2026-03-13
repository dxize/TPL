# Грамматика языка DEA

## 1. Операторы

### 1.1. Арифметические операторы

| Символы | Операция                                            |
| ------: | --------------------------------------------------- |
|     `+` | Сложение или унарный `+` (для строк — конкатенация) |
|     `-` | Вычитание или унарный `-`                           |
|     `*` | Умножение                                           |
|     `/` | Деление (вещественное)                              |
|    `//` | Целочисленное деление                               |
|     `%` | Остаток от деления                                  |
|     `^` | Возведение в степень                                |

### 1.2. Логические операторы

| Символы | Операция       |
| ------: | -------------- |
|    `&&` | Логическое И   |
|     `!` | Логическое НЕ  |
|  `\|\|` | Логическое ИЛИ |

### 1.3. Операторы сравнения

| Символы | Операция         |
| ------: | ---------------- |
|    `!=` | Неравенство      |
|    `==` | Равенство        |
|    `<=` | Меньше или равно |
|    `>=` | Больше или равно |
|     `<` | Меньше           |
|     `>` | Больше           |

### 1.4. Оператор присваивания

| Символы | Операция     |
| ------: | ------------ |
|     `=` | Присваивание |

## 2. Приоритет и ассоциативность операторов

| Приоритет (по убыванию) | Операторы              | Ассоциативность |
| ----------------------: | ---------------------- | --------------- |
|                       8 | `^`                    | правая          |
|                       7 | унарные: `+`, `-`, `!` | правая          |
|                       6 | `*`, `/`, `//`, `%`    | левая           |
|                       5 | `+`, `-`               | левая           |
|                       4 | `<`, `<=`, `>`, `>=`   | левая           |
|                       3 | `==`, `!=`             | левая           |
|                       2 | `&&`                   | левая           |
|                       1 | `\|\|`                 | левая           |

## 3. Полная грамматика в нотации ISO EBNF

```ebnf
(* Программа находится в одном файле и имеет одну точку входа *)
program = { top_level_declaration }, main_function_declaration ;

(* Верхнеуровневое объявление *)
top_level_declaration =
      function_declaration
      | procedure_declaration
      | variable_declaration, ";"
      | constant_declaration, ";" ;

(* Типы данных *)
type = "int" | "num" | "string" | "bool" ;

(* Обязательная точка входа *)
main_function_declaration =
      "func", "int", "main", "(", ")", block ;

(* Объявление функции *)
function_declaration =
      "func", type, identifier, "(", [ parameter_list ], ")", block ;

(* Объявление процедуры *)
procedure_declaration =
      "proc", identifier, "(", [ parameter_list ], ")", block ;

(* Параметры функции или процедуры *)
parameter_list = parameter, { ",", parameter } ;
parameter = type, identifier ;

(* Блок инструкций *)
block = "{", { statement }, "}" ;

(* Инструкция: простая (с ';') или составная (без ';') *)
statement = simple_statement, ";" | compound_statement ;

(* Простые инструкции *)
simple_statement =
      assignment_statement
      | variable_declaration
      | constant_declaration
      | input_statement
      | print_statement
      | procedure_call
      | return_statement ;

(* Составные инструкции *)
compound_statement =
      if_statement
      | while_statement
      | for_statement ;

(* Присваивание *)
assignment_statement = identifier, "=", expression ;

(* Объявление переменной *)
variable_declaration = type, identifier, [ "=", expression ] ;

(* Объявление константы *)
constant_declaration = "const", type, identifier, "=", expression ;

(* Ввод *)
input_statement = "input", "(", identifier, ")" ;

(* Вывод *)
print_statement = "print", "(", argument_list, ")" ;

(* Возврат из функции или процедуры *)
return_statement = "return", [ expression ] ;

if_statement =
    "if", "(", expression, ")", block, [ "else", block ] ;

(* Инструкции break/continue допускаются только внутри loop_block *)
break_statement = "break" ;
continue_statement = "continue" ;

loop_block = "{", { loop_statement }, "}" ;

loop_statement =
      statement
      | break_statement, ";"
      | continue_statement, ";" ;

(* Цикл while *)
while_statement = "while", "(", expression, ")", loop_block ;

(* Цикл for *)
for_statement =
      "for", "(", assignment_statement, ( "to" | "downto" ), expression, ")", loop_block ;

(* Вызов процедуры *)
procedure_call = identifier, "(", [ argument_list ], ")" ;

(* Список аргументов функции/процедуры *)
argument_list = expression, { ",", expression } ;


(* Грамматика выражений *)

expression = logical_or ;

logical_or = logical_and, { "||", logical_and } ;
logical_and = equality, { "&&", equality } ;

equality = comparison, { ( "==" | "!=" ), comparison } ;
comparison = additive, { ( "<" | "<=" | ">" | ">=" ), additive } ;

additive = multiplicative, { ( "+" | "-" ), multiplicative } ;
multiplicative = unary, { ( "*" | "/" | "//" | "%" ), unary } ;

unary =
      ( "+" | "-" | "!" ), unary
      | power ;

power = primary, [ "^", power ] ;

(* Первичные выражения — числа, строковые и булевы литералы, идентификаторы, функции или подвыражения в скобках *)
primary =
      number
      | string_literal
      | bool_literal
      | identifier
      | function_call_expression
      | "(", expression, ")" ;

function_call_expression = (built_in_function | identifier), "(", [ argument_list ], ")" ;

(* Имена встроенных функций *)
built_in_function = "abs"| "min"| "max"| "len"| "substr" ; 
```