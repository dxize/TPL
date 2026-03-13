using Ast.Declarations;
using Ast.Expressions;

namespace Ast;

public interface IAstVisitor
{
    void Visit(ProgramNode p);

    void Visit(FunctionDeclaration d);

    void Visit(LiteralExpression e);

    void Visit(PrintExpression e);

    void Visit(ReturnExpression e);
}