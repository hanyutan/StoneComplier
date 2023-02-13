using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneComplier
{
    public class OpPrecedenceParser
    {
        /* 算符优先分析法 operator precedence parsing,
         * 是LR(1)的弱化版本，自底向上分析语法分析，只能对数学运算之类的简单语句执行语法分析
         * 不必为每一种优先级类别的运算符去创建一条语法规则
         * 新增运算符时只需要添加至operators字段即可
         * 举例：Number "+" Number "*" Number
         * 需要知道中间那个Number究竟是+的右操作数，还是*的左操作数——由DoShift来判断
         */
        public class Precedence
        {
            public int priority;   // 优先级，数值越大优先级越高，1 2 3 4
            public bool left_asso; // 左结合or右结合

            public Precedence(int i, bool b)
            {
                priority = i;
                left_asso = b;
            }
        }

        private Lexer lexer;
        protected Dictionary<string, Precedence> operators;
     
        public OpPrecedenceParser(Lexer p)
        {
            lexer = p;
            operators = new Dictionary<string, Precedence>();
            operators.Add("<", new Precedence(1, true));
            operators.Add(">", new Precedence(1, true));
            operators.Add("+", new Precedence(2, true));
            operators.Add("-", new Precedence(2, true));
            operators.Add("*", new Precedence(3, true));
            operators.Add("/", new Precedence(3, true));
            operators.Add("^", new Precedence(4, false));
        }

        public ASTree Expression()
        {
            // 在读取但此后比较其前后运算符的优先级
            ASTree right = Factor();
            Precedence next;
            while((next = NextOperator()) != null)
            {
                right = DoShift(right, next.priority);
            }
            return right;
        }

        ASTree DoShift(ASTree left, int prior)
        {
            // 往右看一个factor，看看怎么结合
            ASTLeaf op = new ASTLeaf(lexer.Read());
            ASTree right = Factor();
            Precedence next;
            while ((next = NextOperator()) != null && RightIsExpr(prior, next))
            {
                right = DoShift(right, next.priority);
            }
            return new BinaryOp(new List<ASTree> { left, op, right });
        }

        Precedence NextOperator()
        {
            Token token = lexer.Peek(0);
            string token_text = token.GetText();
            if (token.Type == TokenType.Identifier && operators.ContainsKey(token_text))
                return operators[token_text];
            else
                return null;
        }

        bool RightIsExpr(int prior, Precedence next)
        {
            // 比较左右操作符的优先级
            // 返回true代表中间数是右侧操作符的左操作数，即跟右侧结合
            if (next.left_asso)
                return prior < next.priority;
            else
                return prior <= next.priority;
        }

        ASTree Factor()
        {
            // 代表factor非终结符
            if (IsToken("("))
            {
                DropToken();
                ASTree expression = Expression();
                VerifyToken(")");
                return expression;
            }
            else
            {
                Token token = lexer.Read();
                if (token.Type == TokenType.Number)
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
