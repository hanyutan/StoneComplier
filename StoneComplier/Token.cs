using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StoneComplier
{
    public enum TokenType
    {
        None,
        Identifier,  // 标识符：变量名、函数名、类名、+-运算符、() , ; ""标点符号、保留字
        Number,      // 整型字面量
        String,      // 字符串字面量
    }

    public class Token
    {
        // 存单词的字符串、类型、所处位置的行号
        public static readonly Token EOF = new Token(-1) { };   // end of file

        public const string EOL = "\\n";   // end of line

        private int line_number;
        public int LineNumber
        {
            get { return line_number; }
            set { line_number = value; }
        }

        private TokenType type = TokenType.None;
        public TokenType Type
        {
            get { return type; }
            set { type = value; }
        }
     
        protected Token(int line)
        {
            line_number = line;
        }

        public virtual int GetNumber()
        {
            throw new StoneException("not number token");
        }

        public virtual string GetText()
        {
            return "";
        }
    }

}
