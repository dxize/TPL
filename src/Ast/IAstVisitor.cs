using Ast.Declarations;
using Ast.Expressions;

namespace Ast;

public interface IAstVisitor
{
    void Visit(ProgramNode p);

    void Visit(FunctionDeclaration d);

    void Visit(VariableDeclarationExpression d);

    void Visit(ConstantDeclarationExpression d);

    void Visit(LiteralExpression e);

    void Visit(IdentifierExpression e);

    void Visit(UnaryExpression e);

    void Visit(BinaryExpression e);

    void Visit(CallExpression e);

    void Visit(ProcedureCallStatement s);

    void Visit(AssignmentExpression e);

    void Visit(InputExpression e);

    void Visit(PrintExpression e);

    void Visit(ReturnExpression e);

    void Visit(IfStatement s);
}