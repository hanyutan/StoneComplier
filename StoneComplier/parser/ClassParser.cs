using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneComplier
{
    class ClassParser: ClosureParser
    {
        // 支持类的解析器
        protected static Parser member = R.Or(def, simple);
        protected static Parser class_body = RT(typeof(ClassBody))
            .Sep("{")
            .Option(member)
            .Repeat(R.Sep(";", Token.EOL).Option(member))
            .Sep("}");
        protected static Parser def_class = RT(typeof(ClassStatement))
            .Sep("class")
            .Identifier(reserved)
            .Option(R.Sep("extends").Identifier(reserved))
            .Ast(class_body);

        public ClassParser(): base()
        {
            postfix.InsertChoice(RT(typeof(Dot)).Sep(".").Identifier(reserved));
            program.InsertChoice(def_class);
        }
    }
}
