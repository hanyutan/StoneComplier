using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneComplier
{
    public class BasicParser
    {
        protected static List<string> reserved = new List<string>();   // 不会作为变量名使用
        protected static Operators operators = new Operators();

        // 首先定义了大量Parser类型字段，将BNF语法规则转换为c#语言程序
        // 这些Parser对象能够根据各种类型的非终结符模式来执行语法分析，返回一颗抽象语法树

        // 用于简化写法
        protected static Parser R => Parser.Rule();
        protected static Parser RT(Type type)
        {
            return Parser.Rule(type);
        }

        protected static Parser expr0 = R;      // 语法规则的定义是递归的，先占个位，不然后面为null
        protected static Parser statement0 = R;

        protected static Parser primary = RT(typeof(PrimaryExpr))
            .Or(R.Sep("(").Ast(expr0).Sep(")"),
                R.Number(typeof(NumLiteral)),
                R.Identifier(reserved, typeof(IdName)),
                R.String(typeof(StringLiteral)));
        protected static Parser factor = R
            .Or(RT(typeof(NegativeExpr)).Sep("-").Ast(primary),
                primary);
        protected static Parser expr = expr0.Expression(factor, operators, typeof(BinaryOp));
        protected static Parser block = RT(typeof(BlockStatement))
            .Sep("{")
            .Option(statement0)
            .Repeat(R.Sep(";", Token.EOL).Option(statement0))
            .Sep("}");
        protected static Parser simple = RT(typeof(PrimaryExpr)).Ast(expr0);
        protected static Parser statement = statement0
            .Or(RT(typeof(IfStatement))
                    .Sep("if")
                    .Ast(expr)
                    .Ast(block)
                    .Option(R.Sep("else").Ast(block)),
                RT(typeof(WhileStatement))
                    .Sep("while")
                    .Ast(expr0)
                    .Ast(block),
                simple);
        protected static Parser program = R
            .Or(statement0, 
                RT(typeof(NullStatement)))
            .Sep(";", Token.EOL);

        public BasicParser()
        {
            reserved.Add(";");
            reserved.Add("}");
            reserved.Add(Token.EOL);
            
            operators.Add("=", 1, Operators.RIGHT);
            operators.Add("==", 2, Operators.LEFT);
            operators.Add("<", 2, Operators.LEFT);
            operators.Add(">", 2, Operators.LEFT);
            operators.Add("+", 3, Operators.LEFT);
            operators.Add("-", 3, Operators.LEFT);
            operators.Add("*", 4, Operators.LEFT);
            operators.Add("/", 4, Operators.LEFT);
            operators.Add("%", 4, Operators.LEFT);
        }

        public ASTree Parse(Lexer lexer)
        {
            return program.Parse(lexer);
        }
    }
}
