/* stone:
 * 流程：词法分析（正则匹配拆分）、语法分析（一边获取单词一边构造抽象语法树）
 * 
 * 
 * 词法分析器 lexical analyzer / lexer / scanner
 * 单词 token

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
            string code_file_path = "../../../stone_src/while_loop.stone";
            using (FileStream fsRead = new FileStream(code_file_path, FileMode.Open, FileAccess.Read))
            {
                Lexer lexer = new Lexer(fsRead);
                for (Token token; (token = lexer.Read()) != Token.EOF;)
                    Console.WriteLine(token.GetText());
            }


            //string line = "test#aaatest#";
            //string pattern = @"test#";
            //foreach (Match match in Regex.Matches(line, pattern))
            //{
            //    Console.WriteLine(match.Value);
            //}

        }
    }
}
