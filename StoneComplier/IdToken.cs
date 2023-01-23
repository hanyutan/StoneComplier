using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StoneComplier
{
    public class IdToken: Token
    {
        // Token的子类
        private string value;

        public IdToken(int line_num, string value) : base(line_num)
        {
            this.value = value;
            Type = TokenType.Identifier;
        }

        public override string GetText()
        {
            return value;
        }
    }
}
