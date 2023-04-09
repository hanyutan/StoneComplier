using System;
using System.IO;

namespace StoneComplier
{
    public class Config
    {
        public static readonly bool OptimizeVariableRW = true;
        public static readonly bool OptimizeClassObject = true;
    }

    public class TestRunner
    {
        public static void Main(string[] args)
        {
            //test_lexer();                   // 词法分析
            //test_expr_parser();             // 四则运算，语法分析
            //test_op_precedence_parser();    // 符号优先法，语法分析
            //test_basic_parser();            // 语法分析
            //test_basic_interpreter();       // 解释器计算
            //test_def_function();            // 函数定义与调用
            //test_nest_function();           // 函数嵌套，动静态作用域演示
            //test_closure();                 // 测试闭包
            //test_native_function();         // 测试原生函数
            //test_def_class();               // 测试基于类的面向对象
            //test_array();                   // 测试数组
            test_optimization();   // 测试优化变量读写性能 // 测试优化对象操作性能
        }

        public static void test_optimization()
        {
            var env = new ResizableArrayEnv();
            Natives.ToNativeEnv(env);
            run("def_function", new FuncParser(), env);
        }

        public static void test_array()
        {
            var env = new NestedEnv();
            Natives.ToNativeEnv(env);
            run("array", new FuncParser(), env);
        }

        public static void test_def_class()
        {
            var env = new NestedEnv();
            Natives.ToNativeEnv(env);
            run("def_class", new FuncParser(), env);
        }

        public static void test_native_function()
        {
            var env = new NestedEnv();
            Natives.ToNativeEnv(env);
            run("native_function", new FuncParser(), env);
        }

        public static void test_closure()
        {
            run("closure", new FuncParser(), new NestedEnv());
        }

        public static void test_nest_function()
        {
            run("nest_function", new FuncParser(), new NestedEnv());
        }

        public static void test_def_function()
        {
            var env = new NestedEnv();
            Natives.ToNativeEnv(env);   // 为了与优化后进行速度对比
            run("def_function", new FuncParser(), env);
        }

        public static void test_basic_interpreter()
        {
            run("while_loop", new BasicParser(), new BasicEnv());
        }

        public static void run(string code_file_name, BasicParser parser, Env env)
        {
            string code_file_path = $"../../../stone_src/{code_file_name}.stone";
            using (FileStream fsRead = new FileStream(code_file_path, FileMode.Open, FileAccess.Read))
            {
                Lexer lexer = new Lexer(fsRead);

                // 解释运行代码
                Console.WriteLine("[eval output]");
                while (lexer.Peek(0) != Token.EOF)
                {
                    ASTree ast = parser.Parse(lexer);
                    if (ast is not NullStatement)
                    {
                        if(Config.OptimizeVariableRW)
                            ast.Lookup(env.GetSymbols());
                        object result = ast.Eval(env);
                        if(result!=null)
                            Console.WriteLine(result.ToString());
                    }
                }
            }
        }

        public static void test_basic_parser()
        {
            string code_file_path = "../../../stone_src/while_loop.stone";
            using (FileStream fsRead = new FileStream(code_file_path, FileMode.Open, FileAccess.Read))
            {
                Lexer lexer = new Lexer(fsRead);

                // 测试语法树构造
                Console.WriteLine("[parser output]");
                BasicParser parser = new BasicParser();
                while (lexer.Peek(0) != Token.EOF)
                {
                    ASTree ast = parser.Parse(lexer);
                    Console.WriteLine(ast.ToString());
                }
            }
        }

        public static void test_op_precedence_parser()
        {
            string code_file_path = "../../../stone_src/arithmetic.stone";
            using (FileStream fsRead = new FileStream(code_file_path, FileMode.Open, FileAccess.Read))
            {
                Lexer lexer = new Lexer(fsRead);

                // 测试语法树构造
                Console.WriteLine("[parser output]");
                OpPrecedenceParser parser = new OpPrecedenceParser(lexer);
                ASTree ast = parser.Expression();
                Console.WriteLine(ast.ToString());
            }
        }


        public static void test_expr_parser()
        {
            string code_file_path = "../../../stone_src/arithmetic.stone";
            using (FileStream fsRead = new FileStream(code_file_path, FileMode.Open, FileAccess.Read))
            {
                Lexer lexer = new Lexer(fsRead);

                // 测试语法树构造
                Console.WriteLine("[parser output]");
                ExprParser parser = new ExprParser(lexer);
                ASTree ast = parser.Expression();
                Console.WriteLine(ast.ToString());
            }
        }

        public static void test_lexer()
        {
            string code_file_path = "../../../stone_src/while_loop.stone";
            using (FileStream fsRead = new FileStream(code_file_path, FileMode.Open, FileAccess.Read))
            {
                Lexer lexer = new Lexer(fsRead);

                // 测试单词拆解
                Console.WriteLine("[lexer output]");
                for (Token token; (token = lexer.Read()) != Token.EOF;)
                    Console.WriteLine(token.GetText());
            }
        }

    }
}
