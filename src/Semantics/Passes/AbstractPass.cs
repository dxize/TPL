using Ast;
using Ast.Declarations;
using Ast.Expressions;

namespace Semantics.Passes;

public abstract class AbstractPass : IAstVisitor
{
    public virtual void Visit(ProgramNode p)
    {
        foreach (Declaration decl in p.GlobalDeclarations)
        {
            decl.Accept(this);
        }

        foreach (FunctionDeclaration function in p.UserFunctions)
        {
            function.Accept(this);
        }

        p.MainFunction.Accept(this);
    }

    public virtual void Visit(FunctionDeclaration d)
    {
        foreach (AstNode node in d.Body)
        {
            node.Accept(this);
        }
    }

    public virtual void Visit(VariableDeclarationExpression d)
    {
        d.Initializer?.Accept(this);
    }

    public virtual void Visit(ConstantDeclarationExpression d)
    {
        d.Initializer.Accept(this);
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

    public virtual void Visit(ProcedureCallStatement s)
    {
        foreach (Expression argument in s.Arguments)
        {
            argument.Accept(this);
        }
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
        e.Value?.Accept(this);
    }

    public virtual void Visit(IfStatement s)
    {
        s.Condition.Accept(this);

        foreach (AstNode node in s.ThenBody)
        {
            node.Accept(this);
        }

        if (s.ElseBody is null)
        {
            return;
        }

        foreach (AstNode node in s.ElseBody)
        {
            node.Accept(this);
        }
    }

    public virtual void Visit(WhileStatement s)
    {
        s.Condition.Accept(this);

        foreach (AstNode node in s.Body)
        {
            node.Accept(this);
        }
    }

    public virtual void Visit(ForStatement s)
    {
        s.Start.Accept(this);
        s.End.Accept(this);

        foreach (AstNode node in s.Body)
        {
            node.Accept(this);
        }
    }

    public virtual void Visit(BreakStatement s)
    {
    }

    public virtual void Visit(ContinueStatement s)
    {
    }
}