using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneComplier
{

    public class NumLiteral : ASTLeaf
    {
        // 整型字面量
        public NumLiteral(Token t) : base(t)
        {
            if (t.Type != TokenType.Number)
                throw new StoneException("Token is not number");
        }

        public int Value
        {
            get
            {
                return token.GetNumber();
            }
        }
    }

    public class StringLiteral : ASTLeaf
    {
        // 整型字面量
        public StringLiteral(Token t) : base(t)
        {
            if (t.Type != TokenType.String)
                throw new StoneException("Token is not string");
        }

        public string Value
        {
            get
            {
                return token.GetText();
            }
        }
    }


    public class DefName : ASTLeaf
    {
        // 变量名、类名、函数名
        public DefName(Token t) : base(t)
        {
            if (t.Type != TokenType.Identifier)
                throw new StoneException("Token is not identifier");
        }

        public string GetValue()
        {
            return token.GetText();
        }
    }

    public class BinaryOp : ASTList
    {
        // 二元运算
        public BinaryOp(List<ASTree> list) : base(list)
        {
            if(list.Count != 3)
                throw new StoneException("BinaryOp need 3 elements");
        }

        public ASTree Left
        {
            get
            {
                return Children[0];
            }
        }

        // 运算符也是一个leaf node，存在于token中
        public string Operator
        {
            get
            {
                return ((ASTLeaf)Children[1]).ToString();
            }
        }

        public ASTree Right
        {
            get
            {
                return Children[2];
            }
        }
    }

    public class PrimaryExpr : ASTList
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

    public class NegativeExpr : ASTList
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

    public class BlockStatement : ASTList
    {
        public BlockStatement(List<ASTree> list) : base(list)
        {

        }
    }

    public class IfStatement : ASTList
    {
        public IfStatement(List<ASTree> list) : base(list)
        {

        }

        public ASTree Condition()
        {
            return Children[0];
        }

        public ASTree ThenBlock()
        {
            return Children[1];
        }

        public ASTree ElseBlock()
        {
            if (Children.Count > 2)
                return Children[2];
            else
                return null;
        }

        public override string ToString()
        {
            return "(if" + Condition().ToString() + " then " + ThenBlock().ToString() + "else" + ElseBlock().ToString() + ")";
        }
    }


    public class WhileStatement : ASTList
    {
        public WhileStatement(List<ASTree> list) : base(list)
        {

        }

        public ASTree Condition()
        {
            return Children[0];
        }

        public ASTree Body()
        {
            return Children[1];
        }

        public override string ToString()
        {
            return "(while" + Condition() + " do " + Body() + ")";
        }
    }
    public class NullStatement : ASTList
    {
        public NullStatement(List<ASTree> list) : base(list)
        {

        }
    }
}
