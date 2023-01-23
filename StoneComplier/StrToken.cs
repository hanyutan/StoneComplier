using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StoneComplier
{
    public class StrToken: Token
    {
        // Token的子类
        private string value;
        public StrToken(int line_num, string value) : base(line_num)
        {
            this.value = value;
            Type = TokenType.String;
        }

        public override string GetText()
        {
            return value;
        }
    }
}
