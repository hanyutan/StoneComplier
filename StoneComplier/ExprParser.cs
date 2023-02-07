using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneComplier
{
/* 四则运算表达式的语法规则
 * factor:     NUMBER | "(" expression ")"
 * term:       factor { ( "*" | "/" ) factor}
 * expression: term   { ( "+" | "-" ) term  }
 */

    class ExprParser
    {
        private Lexer lexer;

        public ExprParser(Lexer p)
        {
            lexer = p;
        }

        public ASTree Expression()
        {
            // 代表expression非终结符
            ASTree left = Term();
            while(IsToken("+") || IsToken("-"))  // while循环表示{}匹配0或多次
            {
                ASTree op = new ASTLeaf(lexer.Read());
                ASTree right = Term();
                left = new BinaryOp(new List<ASTree> { left, op, right });
            }
            return left;
        }

        public ASTree Term()
        {
            // 代表term非终结符
            ASTree left = Factor();
            while (IsToken("*") || IsToken("/"))
            {
                ASTree op = new ASTLeaf(lexer.Read());
                ASTree right = Factor();
                left = new BinaryOp(new List<ASTree> { left, op, right });
            }
            return left;
        }

        public ASTree Factor()
        {
            // 代表factor非终结符
            if(IsToken("("))
            {
                DropToken();
                ASTree expression = Expression();
                VerifyToken(")");
                return expression;
            }
            else
            {
                Token token = lexer.Read();
                if(token.Type == TokenType.Number)
                {
                    NumLiteral numLiteral = new NumLiteral(token);
                    return numLiteral;
                }
                else
                {
                    throw new StoneException("[parse failed] expected num token in factor");
                }
            }
        }

        bool IsToken(string name)
        {
            // 预读，判断语法规则分支方向
            Token token = lexer.Peek(0);
            if (token.Type == TokenType.Identifier && token.GetText() == name)
                return true;
            else
                return false;
        }

        void DropToken()
        {
            // 扔掉语法树中不需要的符号，如用于表达计算顺序的括号
            lexer.Read();
        }

        void VerifyToken(string name)
        {
            // 核实，然后扔掉
            Token token = lexer.Read();
            if (!(token.Type == TokenType.Identifier && token.GetText() == name))
                throw new StoneException($"[parse failed] expected token: {token.GetText()}");
        }
    }
}
