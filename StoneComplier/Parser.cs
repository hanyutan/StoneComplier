using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneComplier
{
    public class Parser
    {
        // 解析器组合子类型的语法分析库
        // - 外部可以用此库自己构建语法规则，而不是在库里把规则写死
        // - 将很多解决简单问题的方法组合到一起，形成解决复杂问题的方法
        // BNF语法规则转换为c#语言程序
        // 主要使用LL语法分析方法，部分结合了算符优先法

        protected abstract class Element
        {
            // element为语法规则所包含的一个元素
            public abstract void Parse(Lexer lexer, List<ASTree> ast);  // 解析该元素，并将结果添加到抽象语法树中

            public abstract bool Match(Lexer lexer);  // 判断是否匹配该元素，主要用于分支、循环时的选择判定
        }

        protected class Tree: Element
        {
            Parser parser;

            public Tree(Parser p)
            {
                parser = p;
            }

            public override void Parse(Lexer lexer, List<ASTree> ast)
            {
                ast.Add(parser.Parse(lexer));
            }

            public override bool Match(Lexer lexer)
            {
                return parser.Match(lexer);
            }
        }


        protected class OrTree : Element
        {
            List<Parser> parsers = new List<Parser>();

            public OrTree(params Parser[] ps)
            {
                foreach (var p in ps)
                    parsers.Add(p);
            }

            public override void Parse(Lexer lexer, List<ASTree> ast)
            {
                Parser p = Choose(lexer);
                if (p == null)
                    throw new StoneException("Parser.OrTree: parse failed, choose returns null");
                else
                    ast.Add(p.Parse(lexer));
            }

            public override bool Match(Lexer lexer)
            {
                return Choose(lexer) != null;
            }

            Parser Choose(Lexer lexer)
            {
                // 从不同的分支中选择一条
                foreach(var p in parsers)
                {
                    if (p.Match(lexer))
                        return p;
                }
                return null;
            }

            public void Insert(Parser p)
            {
                parsers.Insert(0, p);
            }
        }

        protected class Repeator : Element
        {
            Parser parser;
            bool only_once;

            public Repeator(Parser p, bool once)
            {
                parser = p;
                only_once = once;
            }

            public override void Parse(Lexer lexer, List<ASTree> ast)
            {
                while(parser.Match(lexer))
                {
                    ASTree t = parser.Parse(lexer);
                    if (t is not ASTList || ((ASTList)t).Children.Count > 0)  // Leaf和有child的List，add进语法树；还可能解析出来空行，那就不用add
                        ast.Add(t);
                    if (only_once)
                        break;
                }
            }

            public override bool Match(Lexer lexer)
            {
                return parser.Match(lexer);
            }
        }

        protected abstract class AToken: Element
        {
            Factory factory;   // 用于创建抽象语法树最终的叶子节点
            public AToken(Type type)
            {
                if (type == null) type = typeof(ASTLeaf);
                CheckSubclassType(type, typeof(ASTLeaf));
                factory = Factory.Get(type, typeof(Token));
            }

            public abstract bool Test(Token token);

            public override void Parse(Lexer lexer, List<ASTree> ast)
            {
                Token token = lexer.Read();
                if (Test(token))
                {
                    ASTree leaf = factory.Make(token);
                    ast.Add(leaf);
                }
                else
                    throw new StoneException("Parser.AToken: parse failed, test not passed");
            }

            public override bool Match(Lexer lexer)
            {
                return Test(lexer.Peek(0));
            }
        }

        protected class IdToken: AToken
        {
            List<string> reserved = new List<string>();
            public IdToken(List<string> res, Type type) : base(type)
            {
                if (res != null)
                    reserved = res;
            }

            public override bool Test(Token token)
            {
                return token.Type == TokenType.Identifier && !reserved.Contains(token.GetText());
            }
        }

        protected class NumToken : AToken
        {
            public NumToken(Type type) : base(type)
            {
            }

            public override bool Test(Token token)
            {
                return token.Type == TokenType.Number;
            }
        }

        protected class StrToken : AToken
        {
            public StrToken(Type type) : base(type)
            {
            }

            public override bool Test(Token token)
            {
                return token.Type == TokenType.String;
            }
        }

        protected class Leaf: Element
        {
            List<string> tokens = new List<string>();
            
            public Leaf(params string[] pat)
            {
                foreach (var t in pat)
                    tokens.Add(t);
            }

            public override void Parse(Lexer lexer, List<ASTree> ast)
            {
                Token token = lexer.Read();
                if(token.Type == TokenType.Identifier)
                {
                    foreach(var t in tokens)
                    {
                        if(t == token.GetText())
                        {
                            Find(ast, token);
                            return;
                        }
                    }
                }
            }

            public override bool Match(Lexer lexer)
            {
                Token token = lexer.Peek(0);
                if (token.Type == TokenType.Identifier)
                {
                    foreach (var t in tokens)
                    {
                        if (t == token.GetText())
                            return true;
                    }
                }
                return false;
            }

            protected virtual void Find(List<ASTree> ast, Token token)
            {
                ast.Add(new ASTLeaf(token));
            }
        }

        protected class Skip: Leaf
        {
            public Skip(params string[] pat) : base(pat)
            {

            }

            protected override void Find(List<ASTree> ast, Token token)
            {

            }
        }

        protected class Expr: Element
        {
            // 唯一一个比较复杂的element
            Factory factory;
            Operators operators;
            Parser factor;
            public Expr(Type type, Parser parser, Operators ops)
            {
                CheckSubclassType(type, typeof(ASTree));
                factory = Factory.GetForASTList(type);
                operators = ops;
                factor = parser;
            }

            public override void Parse(Lexer lexer, List<ASTree> ast)
            {
                ASTree right = factor.Parse(lexer);
                Precedence next;
                while ((next = NextOperator(lexer)) != null)
                    right = DoShift(lexer, right, next.priority);

                ast.Add(right);
            }

            ASTree DoShift(Lexer lexer, ASTree left, int prior)
            {
                // 往右看一个factor，看看怎么结合
                List<ASTree> list = new List<ASTree>();
                list.Add(left);

                ASTLeaf op = new ASTLeaf(lexer.Read());
                list.Add(op);

                ASTree right = factor.Parse(lexer);
                Precedence next;
                while ((next = NextOperator(lexer)) != null && RightIsExpr(prior, next))
                    right = DoShift(lexer, right, next.priority);

                list.Add(right);
                return factory.Make(list);
            }

            Precedence NextOperator(Lexer lexer)
            {
                Token token = lexer.Peek(0);
                string token_text = token.GetText();
                if (token.Type == TokenType.Identifier)
                    return operators.Get(token_text);
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

            public override bool Match(Lexer lexer)
            {
                return factor.Match(lexer);
            }
        }

        protected class Factory
        {
            static string name = "Create";

            protected Func<object, ASTree> make_func = null;
            protected Factory(Func<object, ASTree> func)
            {
                make_func = func;
            }

            public ASTree Make(object arg)
            {
                return make_func(arg);
            }

            public static Factory GetForASTList(Type type)
            {
                Factory factory = Get(type, typeof(List<ASTree>));
                if(factory == null)
                {
                    // type为空时，直接返回语法树
                    Func<object, ASTree> func = (object arg) => {
                        List<ASTree> ast = (List<ASTree>)arg;
                        if (ast.Count == 1)
                            return ast[0];
                        else
                            return new ASTList(ast);
                    };
                    factory = new Factory(func);
                }
                return factory;
            }

            public static Factory Get(Type type, Type arg_type)
            {
                // 反射获得创建方法，统一创建抽象语法树最终的叶子节点or分支节点
                if (type == null)
                    return null;
                CheckSubclassType(type, typeof(ASTree));

                var method = type.GetMethod(name);
                if(method != null)
                {
                    Func<object, ASTree> func = (object arg) => { 
                        return (ASTree)method.Invoke(null, new object[] { arg });
                    };
                    return new Factory(func);
                }

                var constructor = type.GetConstructor(new Type[] { arg_type });
                if(constructor != null)
                {
                    Func<object, ASTree> func = (object arg) => {
                        return (ASTree)constructor.Invoke(new object[] { arg });
                    };
                    return new Factory(func);
                }
                
                throw new StoneException("No Such Constructor");
            }
        }

        List<Element> elements = new List<Element>();
        Factory factory;

        public static Parser Rule(Type type = null)
        {
            // Rule方法是用于创建Parser对象的factory方法
            // 由它创建的Parser对象的模式为空，需要依顺序添加终结符或非终结符
            // type为根节点的特定类型
            return new Parser(type);
        }

        public Parser(Type type)
        {
            CheckSubclassType(type, typeof(ASTree));
            Reset(type);
        }

        public Parser(Parser p)
        {
            elements = p.elements;
            factory = p.factory;
        }

        public ASTree Parse(Lexer lexer)
        {
            List<ASTree> results = new List<ASTree>();
            foreach(var element in elements)
                element.Parse(lexer, results);

            return factory.Make(results);
        }

        public bool Match(Lexer lexer)
        {
            if (elements.Count == 0)
                return true;
            else
            {
                Element element = elements[0];
                return element.Match(lexer);
            }
        }

        public Parser Reset(Type type = null)
        {
            // 清空语法规则
            elements = new List<Element>();
            factory = Factory.GetForASTList(type);
            return this;
        }

        public Parser Number(Type type = null)
        {
            // Number: 向规则中添加终结符（整型字面量）
            elements.Add(new NumToken(type));
            return this;
        }

        public Parser Identifier(List<string> reserved, Type type = null)
        {
            // Identifier: 向规则中添加终结符（除reserved以外的标识符）
            elements.Add(new IdToken(reserved, type));
            return this;
        }

        public Parser String(Type type = null)
        {
            // String: 向规则中添加终结符（字符串字面量）
            elements.Add(new StrToken(type));
            return this;
        }

        public Parser Token(params string[] pat)
        {
            // Token: 向规则中添加终结符（与pat匹配的标识符）
            // 比如在已经定义的二元运算符以外，又不可以省略的符号
            elements.Add(new Leaf(pat));
            return this;
        }

        public Parser Sep(params string[] pat)
        {
            // Sep: 向规则中添加终结符（与pat匹配的标识符）（未包含于抽象语法树的分隔字符）
            elements.Add(new Skip(pat));
            return this;
        }

        public Parser Ast(Parser parser)
        {
            // Ast: 向规则中添加非终结符，参数是非终结符所对应的Parser对象
            elements.Add(new Tree(parser));
            return this;
        }

        public Parser Or(params Parser[] ps)
        {
            // Or：表示BNF中的 |(或)，向规则中添加若干个or关系的非终结符
            elements.Add(new OrTree(ps));
            return this;
        }

        public Parser Maybe(Parser parser)
        {
            // Maybe: 向规则中添加可省略的非终结符
            // 即使省略，也会作为一颗仅有根节点的抽象语法树处理
            // question 没太懂？？？
            Parser p2 = new Parser(parser);
            p2.Reset();
            elements.Add(new OrTree(parser, p2));
            return this;
        }

        public Parser Option(Parser parser)
        {
            // Option: 表示BNF中的 [](匹配0或1次)，向规则中添加可省略的非终结符
            elements.Add(new Repeator(parser, true));
            return this;
        }

        public Parser Repeat(Parser parser)
        {
            // Repeat：表示BNF中的 {}(匹配0或多次)构成的循环
            elements.Add(new Repeator(parser, false));
            return this;
        }

        public Parser Expression(Parser parser, Operators ops, Type type = null)
        {
            // Expression: 向规则中添加双目运算表达式
            elements.Add(new Expr(type, parser, ops));
            return this;
        }

        public Parser InsertChoice(Parser parser)
        {
            // 为语法规则起始处的or添加新的分支选项
            // 其实就是为了更新规则临时做的弥补
            Element element = elements[0];
            if (element is OrTree)
                ((OrTree)element).Insert(parser);
            else
            {
                Parser otherwise = new Parser(this);
                Reset(null);
                Or(parser, otherwise);
            }
            return this;
        }

        public static void CheckSubclassType(Type type, Type base_type)
        {
            if (type != null && base_type != null)
            {
                if (type != base_type && !type.IsSubclassOf(base_type))
                    throw new StoneException($"Type {type.Name} is not subclass of {base_type.Name}");
            }
        }
    }
}
