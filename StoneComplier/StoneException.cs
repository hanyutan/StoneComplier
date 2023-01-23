using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StoneComplier.AST;

namespace StoneComplier
{
    public class StoneException : Exception
    {
        public StoneException(string text) : base(text)
        {

        }

        public StoneException(string text, ASTree tree) : base(text + " " /*+ tree.location()*/)
        {

        }

    }
}
