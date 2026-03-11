namespace Lexer;

public enum TokenType
{
    /// <summary>
    /// Ключевое слово const
    /// </summary>
    Const,

    /// <summary>
    /// Ключевое слово func
    /// </summary>
    Func,

    /// <summary>
    /// Ключевое слово proc
    /// </summary>
    Proc,

    /// <summary>
    /// Ключевое слово return
    /// </summary>
    Return,

    /// <summary>
    /// Ключевое слово if
    /// </summary>
    If,

    /// <summary>
    /// Ключевое слово else
    /// </summary>
    Else,

    /// <summary>
    /// Ключевое слово while
    /// </summary>
    While,

    /// <summary>
    /// Ключевое слово for
    /// </summary>
    For,

    /// <summary>
    /// Ключевое слово to
    /// </summary>
    To,

    /// <summary>
    /// Ключевое слово downto
    /// </summary>
    Downto,

    /// <summary>
    /// Ключевое слово break
    /// </summary>
    Break,

    /// <summary>
    /// Ключевое слово continue
    /// </summary>
    Continue,

    /// <summary>
    /// Ключевое слово int
    /// </summary>
    Int,

    /// <summary>
    /// Ключевое слово num
    /// </summary>
    Num,

    /// <summary>
    /// Ключевое слово string
    /// </summary>
    String,

    /// <summary>
    /// Ключевое слово bool
    /// </summary>
    Bool,

    /// <summary>
    /// Ключевое слово true
    /// </summary>
    True,

    /// <summary>
    /// Ключевое слово false
    /// </summary>
    False,

    /// <summary>
    /// Ключевое слово input
    /// </summary>
    Input,

    /// <summary>
    /// Ключевое слово print
    /// </summary>
    Print,

    // Идентификаторы и литералы

    /// <summary>
    /// Идентификатор
    /// </summary>
    Identifier,

    /// <summary>
    /// Целочисленный литерал
    /// </summary>
    IntegerLiteral,

    /// <summary>
    /// Числовой литерал
    /// </summary>
    NumLiteral,

    /// <summary>
    /// Строковый литерал
    /// </summary>
    StringLiteral,

    // Операторы

    /// <summary>
    /// Оператор присваивания (=)
    /// </summary>
    Assign,

    /// <summary>
    /// Оператор неравенства (!=)
    /// </summary>
    NotEqual,

    /// <summary>
    /// Оператор равенства (==)
    /// </summary>
    Equal,

    /// <summary>
    /// Оператор меньше или равно (<=)
    /// </summary>
    LessOrEqual,

    /// <summary>
    /// Оператор больше или равно (>=)
    /// </summary>
    GreaterOrEqual,

    /// <summary>
    /// Оператор меньше (<)
    /// </summary>
    Less,

    /// <summary>
    /// Оператор больше (>)
    /// </summary>
    Greater,

    /// <summary>
    /// Оператор умножения (*)
    /// </summary>
    Multiply,

    /// <summary>
    /// Оператор деления (/)
    /// </summary>
    Divide,

    /// <summary>
    /// Оператор целочисленного деления (//)
    /// </summary>
    IntegerDivide,

    /// <summary>
    /// Оператор остатка (%)
    /// </summary>
    Modulo,

    /// <summary>
    /// Оператор вычитания или унарный минус (-)
    /// </summary>
    Minus,

    /// <summary>
    /// Оператор сложения или унарный плюс (+)
    /// </summary>
    Plus,

    /// <summary>
    /// Оператор возведения в степень (^)
    /// </summary>
    Power,

    /// <summary>
    /// Логическое И (&&)
    /// </summary>
    And,

    /// <summary>
    /// Логическое ИЛИ (||)
    /// </summary>
    Or,

    /// <summary>
    /// Логическое НЕ (!)
    /// </summary>
    Not,

    // Разделители

    /// <summary>
    /// Точка с запятой (;)
    /// </summary>
    Semicolon,

    /// <summary>
    /// Запятая (,)
    /// </summary>
    Comma,

    /// <summary>
    /// Открывающая круглая скобка
    /// </summary>
    OpenParenthesis,

    /// <summary>
    /// Закрывающая круглая скобка
    /// </summary>
    CloseParenthesis,

    /// <summary>
    /// Открывающая фигурная скобка
    /// </summary>
    OpenBrace,

    /// <summary>
    /// Закрывающая фигурная скобка
    /// </summary>
    CloseBrace,

    // Служебные

    /// <summary>
    /// Конец файла
    /// </summary>
    EndOfFile,

    /// <summary>
    /// Ошибка лексического анализа
    /// </summary>
    Error
}