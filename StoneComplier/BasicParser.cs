using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneComplier
{
    /* stone BNF 基础语法规则（不考虑优先级，用其他方法处理）
    primary: "(" expr ")" | NUMBER | IDENTIFIER | STRING    基本元素：括号括起的表达式、整型字面量、标识符、字符串字面量
    factor: "-" primary | primary                           注：感觉可以和primary合并到一起，前面加个["-"](...|...|...|...)
    expr: factor { OP factor }                              双目运算符连接的两侧
    block: "{" [statement] {(";" | EOL) [statement]} "}"    由大括号括起来的statement语句序列，语句之间用分块或换行符分割，支持空语句（注：为啥不用{[statement] (";" | EOL)}来表示？代码块中最后一句可以省略分号或换行符）
    simple: expr                                            简单语句
    statement: "if" expr block ["else" block] | "while" expr block | simple 可以是if语句、wile语句、或者简单表达式语句
    program: [statement] (";" | EOL)                        一行stone语言程序，可以表示空行（注：怎样区分一行program和statement？program既可以是处于代码块之外的一条语句，也可以是一行完整的程序）
     */

    /* 函数相关的语法规则
     * param: IDENTIFIER                        定义时的形参，倒是不需要指定类型，直接写个变量名就好
     * params: param { "," param }              参数之间以逗号分隔
     * param_list: "(" [params] ")"             定义时用括号括起来，但可以没有参数
     * def: "def" IDENTIFIER param_list block   中间IDENTIFIER是函数名
     * args: expr { "," expr }                  调用时的实参
     * postfix: "(" [args] ")"                  调用时用括号括起的实参列表
     * 以下与原有不同：
     * primary: ( "(" expr ")" | NUMBER | IDENTIFIER | STRING ) { postfix }     为啥放到末尾若干个？数字又不能接传参？？？
     * simple: expr [ args ]          当语句中只含有一个函数调用时，可以不加括号传参
     * program: [def | statement] (";" | EOL)
     */

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
                R.Identifier(reserved, typeof(DefName)),
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
