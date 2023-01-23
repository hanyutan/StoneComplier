using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StoneComplier
{
    public class NumToken: Token
    {
        // Token的子类

        private int value;

        public NumToken(int line_num, int value):base(line_num)
        {
            this.value = value;
            Type = TokenType.Number;
        }

        public override int GetNumber()
        {
            return value;
        }

        public override string GetText()
        {
            return value.ToString();
        }
    }
}
