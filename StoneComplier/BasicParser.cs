using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneComplier
{
    /* stone BNF 语法规则（不考虑优先级，用其他方法处理）
    primary: "(" expr ")" | NUMBER | IDENTIFIER | STRING    基本元素：括号括起的表达式、整型字面量、标识符、字符串字面量
    factor: "-" primary | primary                           注：感觉可以和primary合并到一起，前面加个["-"](...|...|...|...)
    expr: factor { OP factor }                              双目运算符连接的两侧
    block: "{" [statement] {(";" | EOL) [statement]} "}"    由大括号括起来的statement语句序列，语句之间用分块或换行符分割，支持空语句（注：为啥不用{[statement] (";" | EOL)}来表示？代码块中最后一句可以省略分号或换行符）
    simple: expr                                            简单语句
    statement: "if" expr block ["else" block] | "while" expr block | simple 可以是if语句、wile语句、或者简单表达式语句
    program: [statement] (";" | EOL)                        一行stone语言程序，可以表示空行（注：怎样区分一行program和statement？program既可以是处于代码块之外的一条语句，也可以是一行完整的程序）
     */

    public class BasicParser
    {
        static List<string> reserved = new List<string> { ";", "}", Token.EOL };   // 不会作为变量名使用
        static Operators operators = new Operators();

        // 首先定义了大量Parser类型字段，将BNF语法规则转换为c#语言程序
        // 这些Parser对象能够根据各种类型的非终结符模式来执行语法分析，返回一颗抽象语法树


        // 用于简化写法
        static Parser R => Parser.Rule();
        static Parser RT(Type type)
        {
            return Parser.Rule(type);
        }

        static Parser expr0 = R;      // 语法规则的定义是递归的，先占个位，不然后面为null
        static Parser statement0 = R;

        static Parser primary = RT(typeof(PrimaryExpr))
            .Or(R.Sep("(").Ast(expr0).Sep(")"),
                R.Number(typeof(NumLiteral)),
                R.Identifier(reserved, typeof(DefName)),
                R.String(typeof(StringLiteral)));
        static Parser factor = R
            .Or(RT(typeof(NegativeExpr)).Sep("-").Ast(primary),
                primary);
        static Parser expr = expr0.Expression(factor, operators, typeof(BinaryOp));
        static Parser block = RT(typeof(BlockStatement))
            .Sep("{").Option(statement0)
            .Repeat(R.Sep(";", Token.EOL).Option(statement0))
            .Sep("}");
        static Parser simple = RT(typeof(PrimaryExpr)).Ast(expr0);
        static Parser statement = statement0.Or(
            RT(typeof(IfStatement)).Sep("if").Ast(expr).Ast(block).Option(R.Sep("else").Ast(block)),
            RT(typeof(WhileStatement)).Sep("while").Ast(expr0).Ast(block),
            simple);
        static Parser program = R.Or(statement0, RT(typeof(NullStatement))).Sep(";", Token.EOL);

        public BasicParser()
        {
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
