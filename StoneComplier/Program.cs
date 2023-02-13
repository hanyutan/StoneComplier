/* stone:
 * 流程：词法分析（正则匹配拆分）、语法分析（使用BNF来表示语法，查找模式匹配，一边获取单词一边构造抽象语法树，根据语法规则将stone语言改写为C#代码）
 * 
 * 
 * 词法分析器 lexical analyzer / lexer / scanner
 * 语法分析器 parser
 * 单词 token
 * BNF（一种表示语法的范式）举例：四则运算表达式的语法规则
 *     factor:     NUMBER | "(" expression ")"      factor由单独的整型字面量构成，或者由括号包起的expression构成（注：这里体现了递归循环）
 *     term:       factor { ( "*" | "/" ) factor }  term可以由单独的factor构成，也可以后续再接乘除另一个或多个factor项（注：大括号表示某模式至少出现0次，中括号表示某模式出现0次或1次，括号将里面的内容当作一个完整的模式）
 *     expression: term { ( "+" | "-" ) term }      expression可以由单独的term构成，也可以后续再接加减另一个或多个term项（注：规则里还内含了加减乘除的优先级）

 * 
（基础）
- 语法功能，整数四则运算，字符串处理，支持变量，if while基本控制语句，动态数据类型，支持注释
- 词法分析器，正则库
- 抽象语法树，BNF
- 语法解释器，解析器组合子库
- 设计基本的解释器，GluonJ

（增强）
- 增加static方法调用支持
- 增加类与对象的语法，闭包实现
- 增加数组功能
- 增强解释器功能，能执行函数、支持闭包语法

（性能优化）
- 优化访问变量性能，搜索id而不是变量名
- 优化调用对象和字段的性能，搜索id而不是变量名，增加内联缓存
- 虚拟机 中间代码
- 支持静态数据类型，增加类型检查

（高级）
- 手工设计词法分析器，正则匹配
- 语法分析基本算法，LL语法
- 解析器组合子库的源码分析
- GluonJ注意事项
- 抽象语法树，节点对象的类会包含各种类型的方法，可以用其他设计模式实现而不用GluonJ
*/

using System;
using System.IO;
using System.Text.RegularExpressions;

namespace StoneComplier
{
    public class LexerRunner
    {
        public static void Main(string[] args)
        {
            //test_lexer();
            test_expr_parser();
            test_op_precedence_parser();
            //test_baisc_parser();

            //string line = "test#aaatest#";
            //string pattern = @"test#";
            //foreach (Match match in Regex.Matches(line, pattern))
            //{
            //    Console.WriteLine(match.Value);
            //}

        }

        public static void test_baisc_parser()
        {
            //string code_file_path = "../../../stone_src/while_loop.stone";
            //using (FileStream fsRead = new FileStream(code_file_path, FileMode.Open, FileAccess.Read))
            //{
            //    Lexer lexer = new Lexer(fsRead);

            //    // 测试单词拆解
            //    for (Token token; (token = lexer.Read()) != Token.EOF;)
            //        Console.WriteLine(token.GetText());

            //    // 测试语法树构造
            //    BasicParser parser = new BasicParser();
            //    while (lexer.Peek(0) != Token.EOF)
            //    {
            //        ASTree ast = parser.Parse(lexer);
            //        Console.WriteLine(ast.ToString());
            //    }
            //}
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
