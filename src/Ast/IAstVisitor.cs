using Ast.Declarations;
using Ast.Expressions;

namespace Ast;

public interface IAstVisitor
{
    public void Visit(BinaryOperationExpression e);

    public void Visit(UnaryOperationExpression e);

    public void Visit(LiteralExpression e);

    public void Visit(VariableExpression e);

    public void Visit(FunctionCall e);

    public void Visit(VariableScopeExpression e);

    public void Visit(AssignmentExpression e);

    public void Visit(PrintExpression e);

    public void Visit(InputExpression e);

    public void Visit(IfExpression e);

    public void Visit(IfElseExpression e);

    public void Visit(WhileExpression e);

    public void Visit(ForLoopExpression e);

    public void Visit(BreakExpression e);

    public void Visit(ContinueExpression e);

    public void Visit(ReturnExpression e);

    public void Visit(VariableDeclaration d);

    public void Visit(ConstantDeclaration d);

    public void Visit(FunctionDeclaration d);
}