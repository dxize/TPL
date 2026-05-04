using Ast;
using Ast.Declarations;
using Ast.Expressions;

using Semantics.Exceptions;
using Semantics.Symbols;

namespace Semantics.Passes;

public sealed class ResolveNamesPass : AbstractPass
{
    private readonly SymbolsTable _globalSymbols;
    private SymbolsTable _symbols;

    public ResolveNamesPass(SymbolsTable globalSymbols)
    {
        _globalSymbols = globalSymbols;
        _symbols = globalSymbols;
    }

    public override void Visit(ProgramNode p)
    {
        foreach (Declaration decl in p.GlobalDeclarations)
        {
            decl.Accept(this);
        }

        foreach (FunctionDeclaration function in p.UserFunctions)
        {
            VisitTopLevelFunction(function);
        }

        VisitTopLevelFunction(p.MainFunction);
    }

    public override void Visit(VariableDeclarationExpression e)
    {
        EnsureNotBuiltinName(e.Name);

        base.Visit(e);

        if (_symbols.Parent is not null && _symbols.Parent.ContainsVariable(e.Name))
        {
            throw new DuplicateIdentifierException($"Identifier '{e.Name}' is already declared in global scope.");
        }

        _symbols.DeclareVariable(e.Name, e.Type, isConst: false);
    }

    public override void Visit(ConstantDeclarationExpression e)
    {
        EnsureNotBuiltinName(e.Name);

        base.Visit(e);

        if (_symbols.Parent is not null && _symbols.Parent.ContainsVariable(e.Name))
        {
            throw new DuplicateIdentifierException($"Identifier '{e.Name}' is already declared in global scope.");
        }

        _symbols.DeclareVariable(e.Name, e.Type, isConst: true);
    }

    public override void Visit(IdentifierExpression e)
    {
        SymbolInfo? symbol = _symbols.ResolveVariable(e.Name);
        if (symbol is null)
        {
            throw new UnknownIdentifierException($"Identifier '{e.Name}' is not declared.");
        }

        e.Symbol = symbol;
    }

    public override void Visit(AssignmentExpression e)
    {
        SymbolInfo? symbol = _symbols.ResolveVariable(e.Name);
        if (symbol is null)
        {
            throw new UnknownIdentifierException($"Identifier '{e.Name}' is not declared.");
        }

        if (symbol.IsConst)
        {
            throw new InvalidAssignmentException($"Cannot assign to constant '{e.Name}'.");
        }

        e.Symbol = symbol;
        base.Visit(e);
    }

    public override void Visit(InputExpression e)
    {
        SymbolInfo? symbol = _symbols.ResolveVariable(e.VariableName);
        if (symbol is null)
        {
            throw new UnknownIdentifierException($"Cannot read input into undeclared variable '{e.VariableName}'.");
        }

        if (symbol.IsConst)
        {
            throw new InvalidAssignmentException($"Cannot read input into constant '{e.VariableName}'.");
        }

        e.Symbol = symbol;
    }

    public override void Visit(CallExpression e)
    {
        base.Visit(e);

        if (_symbols.ResolveBuiltin(e.Name) is BuiltinInfo builtin)
        {
            e.Builtin = builtin;
            return;
        }

        FunctionDeclaration? function = _globalSymbols.ResolveFunction(e.Name);
        if (function is null)
        {
            throw new UnknownIdentifierException($"Unknown function '{e.Name}'.");
        }

        e.Function = function;
    }

    public override void Visit(ProcedureCallStatement s)
    {
        base.Visit(s);

        FunctionDeclaration? procedure = _globalSymbols.ResolveFunction(s.Name);
        if (procedure is null)
        {
            throw new UnknownIdentifierException($"Unknown procedure '{s.Name}'.");
        }

        s.Procedure = procedure;
    }

    public override void Visit(IfStatement s)
    {
        s.Condition.Accept(this);

        VisitNestedBlock(s.ThenBody);

        if (s.ElseBody is not null)
        {
            VisitNestedBlock(s.ElseBody);
        }
    }

    public override void Visit(ForStatement s)
    {
        SymbolInfo? symbol = _symbols.ResolveVariable(s.VariableName);
        if (symbol is null)
        {
            throw new UnknownIdentifierException($"Loop variable '{s.VariableName}' is not declared.");
        }

        if (symbol.Type != DataType.Int)
        {
            throw new InvalidAssignmentException($"Loop variable '{s.VariableName}' must be of type int.");
        }

        s.VariableSymbol = symbol;

        s.Start.Accept(this);
        s.End.Accept(this);

        VisitNestedBlock(s.Body);
    }

    public override void Visit(WhileStatement s)
    {
        s.Condition.Accept(this);

        VisitNestedBlock(s.Body);
    }

    private void VisitTopLevelFunction(FunctionDeclaration function)
    {
        EnsureNotBuiltinName(function.Name);

        if (function.Name != "main" && _globalSymbols.ResolveFunction(function.Name) is not null)
        {
            throw new DuplicateIdentifierException($"Identifier '{function.Name}' is already declared.");
        }

        SymbolsTable functionScope = new(_globalSymbols);
        SymbolsTable previous = _symbols;
        _symbols = functionScope;

        try
        {
            foreach (ParameterDeclaration parameter in function.Parameters)
            {
                functionScope.DeclareVariable(parameter.Name, parameter.Type, isConst: false);
            }

            foreach (AstNode node in function.Body)
            {
                node.Accept(this);
            }
        }
        finally
        {
            _symbols = previous;
        }

        if (function.Name != "main")
        {
            _globalSymbols.DeclareFunction(function);
        }
    }

    private void VisitNestedBlock(List<AstNode> statements)
    {
        SymbolsTable previous = _symbols;
        _symbols = new SymbolsTable(_symbols);

        try
        {
            foreach (AstNode node in statements)
            {
                node.Accept(this);
            }
        }
        finally
        {
            _symbols = previous;
        }
    }

    private void EnsureNotBuiltinName(string name)
    {
        if (_globalSymbols.ResolveBuiltin(name) is not null)
        {
            throw new DuplicateIdentifierException($"Identifier '{name}' is a reserved builtin name.");
        }
    }
}