using Ast;
using Ast.Declarations;
using Ast.Expressions;

namespace Semantics.Passes;

/// <summary>
/// Базовый проход по AST. По умолчанию просто обходит дочерние узлы.
/// </summary>
public abstract class AbstractPass : IAstVisitor
{
    public virtual void Visit(ProgramNode p)
    {
        p.MainFunction.Accept(this);
    }

    public virtual void Visit(FunctionDeclaration d)
    {
        foreach (AstNode node in d.Body)
        {
            node.Accept(this);
        }
    }

    public virtual void Visit(LiteralExpression e)
    {
    }

    public virtual void Visit(IdentifierExpression e)
    {
    }

    public virtual void Visit(UnaryExpression e)
    {
        e.Operand.Accept(this);
    }

    public virtual void Visit(BinaryExpression e)
    {
        e.Left.Accept(this);
        e.Right.Accept(this);
    }

    public virtual void Visit(CallExpression e)
    {
        foreach (Expression argument in e.Arguments)
        {
            argument.Accept(this);
        }
    }

    public virtual void Visit(VariableDeclarationExpression e)
    {
        e.Initializer?.Accept(this);
    }

    public virtual void Visit(ConstantDeclarationExpression e)
    {
        e.Initializer.Accept(this);
    }

    public virtual void Visit(AssignmentExpression e)
    {
        e.Value.Accept(this);
    }

    public virtual void Visit(InputExpression e)
    {
    }

    public virtual void Visit(PrintExpression e)
    {
        foreach (Expression argument in e.Arguments)
        {
            argument.Accept(this);
        }
    }

    public virtual void Visit(ReturnExpression e)
    {
        e.Value.Accept(this);
    }
}