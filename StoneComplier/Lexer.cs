using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace StoneComplier
{
    // 词法分析器
    public class Lexer
    {

        // 整型字面量：[0-9]+
        // 标识符: [A-Z_a-z][A-Z_a-z0-9]* 至少需要一个字母数字或下划线，且首字母不能是数字
        // 二元运算符：|==|<=|>=|&&|\|\|
        // 一元运算符和标点：\p{P}
        // 字符串字面量: "(\\"|\\\\|\\n|[^"])*"    整体的模式为"(pattern)*"，即双引号括起字符串，内部根据某个pattern匹配至少0次
        // ——模式pattern与\"、\\、\n、或者除双引号之外（防止字符串提前终止）的任意一个字符相匹配
        // ——由于正则表达式中要通过\来表达转义字符，所以想要匹配\"，就需要\\"
    
        public static readonly string regex_pattern ="\\s*((//.*)|([0-9]+)|(\"(\\\\\"|\\\\\\\\|\\\\n|[^\"])*\")|([A-Z_a-z][A_Z_a-z0-9]*|==|<=|>=|&&|\\|\\||\\p{IsBasicLatin}))?";
        // 汇总：\s*((//.*)|(pat1)|(pat2)|pat3)?
        // 空白字符、(注释|整型字面量|字符串字面量) | 标识符
        // 对一行代码不断用regexPat去匹配，就能不断得到下一个单词

        List<Token> queue = new List<Token>();  // 保存读取的单词，peek取值并返回，read弹出并返回
        bool has_more;   // 是否还有待处理的源代码
        StreamReader reader;  // 逐行读取源代码
        int line_num = 0;

        public Lexer(FileStream fs)
        {
            has_more = true;
            reader = new StreamReader(fs, Encoding.Default);
        }

        public Token Read()
        {
            // 从源代码头部开始逐一获取单词，调用函数时将返回一个新的单词，内存不必保留
            if (FillQueue(0))
            {
                Token token = queue[0];
                queue.RemoveAt(0);
                return token;
            }
            else
            {
                return Token.EOF;   // question 为啥不是EOL
            }
        }

        public Token Peek(int i)
        {
            // 预读，peek(i)将返回read方法即将返回的单词 之后的第i个单词，内存需要一直保留
            // 可以避免在中途法线构造有误时，回溯撤销抽象语法树的构造
            if(FillQueue(i))
            {
                return queue[i];
            }
            else
            {
                return Token.EOF;  // question 为啥不是EOL
            }
        }

        bool FillQueue(int i)
        {
            // 如果目标index超过现有队列长度，则继续读取来填满token队列
            while(i >= queue.Count())
            {
                if (has_more)
                    ReadLine();
                else
                    return false;
            }
            return true;
        }

        void ReadLine()
        {
            // 读取一行源代码进行词法分析，并填充进token队列
            ++line_num;
            string line;
            try
            {
                line = reader.ReadLine();
            }
            catch(IOException e)
            {
                throw new StoneException("read line failed...");
            }

            if (line == null)
            {
                has_more = false;
                return;
            }

            foreach (Match match in Regex.Matches(line, regex_pattern))
            {
                AddToken(line_num, match);
            }
            queue.Add(new IdToken(line_num, Token.EOL));
        }

        void AddToken(int line_num, Match match)
        {
            string value = match.Groups[1].Value;
            if (value != "")      // 尚未结束
            {
                if(match.Groups[2].Value == "")  // 不是注释
                {
                    Token token;
                    if(match.Groups[3].Value != "")              // 整型字面量
                        token = new NumToken(line_num, int.Parse(value));
                    else if(match.Groups[4].Value != "")         // 字符串字面量
                        token = new StrToken(line_num, ToStringLiterial(value));
                    else                                         // 保留字、变量名 类名 函数名、运算符、标点
                        token = new IdToken(line_num, value);

                    queue.Add(token);
                }
            }
        }

        string ToStringLiterial(string str)
        {
            var res = new StringBuilder();
            int len = str.Length - 1;
            for(int i=1; i < len; ++i)  // 去掉头尾match的双引号
            {
                char c = str[i];
                if(c=='\\' && i+1<len)
                {
                    char c2 = str[i + 1];
                    if (c2 == '"' || c2 == '\\')
                        c = str[++i];
                    else if(c2 == 'n')
                    {
                        ++i;
                        c = '\n';
                    }
                }
                res.Append(c);
            }
            return res.ToString();
        }
    }
}
