using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneComplier
{
    public class ClosureParser: FuncParser
    {
        // 支持闭包的语法分析器
        public ClosureParser()
        {
            primary.InsertChoice(RT(typeof(Closure)).Sep("fun").Ast(param_list).Ast(block));
        }
    }
}
