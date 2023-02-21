using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneComplier
{
    public partial class NumLiteral : ASTLeaf
    {
        // 整型字面量
        public NumLiteral(Token t) : base(t)
        {
            if (t.Type != TokenType.Number)
                throw new StoneException("Token is not number");
        }

        public int Value => token.GetNumber();
    }

    public partial class StringLiteral : ASTLeaf
    {
        // 整型字面量
        public StringLiteral(Token t) : base(t)
        {
            if (t.Type != TokenType.String)
                throw new StoneException("Token is not string");
        }

        public string Value => token.GetText();
    }


    public partial class DefName : ASTLeaf
    {
        // 变量名、类名、函数名
        public DefName(Token t) : base(t)
        {
            if (t.Type != TokenType.Identifier)
                throw new StoneException("Token is not identifier");
        }

        public string Value => token.GetText();
    }

    public partial class BinaryOp : ASTList
    {
        // 二元运算
        public BinaryOp(List<ASTree> list) : base(list)
        {
            if (list.Count != 3)
                throw new StoneException("BinaryOp need 3 elements");
        }

        public ASTree Left => Children[0];

        // 运算符也是一个leaf node，存在于token中
        public string Operator => Children[1].ToString();

        public ASTree Right => Children[2];
    }

    public partial class PrimaryExpr : ASTList
    {
        // 语法模式 非终结符
        public PrimaryExpr(List<ASTree> list) : base(list)
        {

        }

        public static ASTree Create(List<ASTree> list)
        {
            if (list.Count == 1)
                return list[0];
            else
                return new PrimaryExpr(list);
        }
    }

    public partial class NegativeExpr : ASTList
    {
        public NegativeExpr(List<ASTree> list) : base(list)
        {

        }

        public ASTree Operand()
        {
            return Children[0];
        }

        public override string ToString()
        {
            return "-" + Operand().ToString();
        }
    }

    public partial class BlockStatement : ASTList
    {
        public BlockStatement(List<ASTree> list) : base(list)
        {

        }
    }

    public partial class IfStatement : ASTList
    {
        public IfStatement(List<ASTree> list) : base(list)
        {

        }

        public ASTree Condition => Children[0];

        public ASTree ThenBlock => Children[1];

        public ASTree ElseBlock => (Children.Count > 2) ? Children[2] : null;

        public override string ToString()
        {
            return "(if" + Condition.ToString() + " then " + ThenBlock.ToString() + "else" + ElseBlock.ToString() + ")";
        }
    }


    public partial class WhileStatement : ASTList
    {
        public WhileStatement(List<ASTree> list) : base(list)
        {

        }

        public ASTree Condition => Children[0];

        public ASTree Body => Children[1];

        public override string ToString()
        {
            return "(while" + Condition.ToString() + " do " + Body.ToString() + ")";
        }
    }
    public partial class NullStatement : ASTList
    {
        public NullStatement(List<ASTree> list) : base(list)
        {

        }
    }

    public partial class DefStatement : ASTList
    {
        public DefStatement(List<ASTree> list) : base(list)
        {

        }

        public string FuncName => ((ASTLeaf)Children[0]).ToString();

        public ParameterList ParameterList => (ParameterList)Children[1];

        public BlockStatement Body => (BlockStatement)Children[2];

        public override string ToString()
        {
            return "(def" + FuncName + " " + ParameterList.ToString() + " " + Body.ToString() + ")";
        }
    }

    public partial class ParameterList : ASTList
    {
        public ParameterList(List<ASTree> list) : base(list)
        {

        }

        public string GetParamName(int i)
        {
            return ((ASTLeaf)Children[i]).ToString();
        }

        public int Size => Children.Count;
    }

    public partial class Arguments : ASTList
    {
        public Arguments(List<ASTree> list) : base(list)
        {

        }

        public int Size => Children.Count;
    }
}
