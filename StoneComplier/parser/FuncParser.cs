using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneComplier
{
    public class FuncParser: BasicParser
    {
        // 支持函数的解析器，继承自BasicParser，这里仅定义新增的部分
        
        /* 暂且规定stone语言的函数必定有返回值，没有return语句，所以就把最后一句的结果返回
         * def语句仅能用于最外层代码，暂不支持用户在代码块{}中定义函数
         */

        protected static Parser param_ = R.Identifier(reserved);  // 没有加type，可能是因为不是变量名所以不需要作为节点加入语法树
        protected static Parser params_ = RT(typeof(ParameterList)).Ast(param_).Repeat(R.Sep(",").Ast(param_));  // 防止跟关键字params冲突
        protected static Parser param_list = R.Sep("(").Maybe(params_).Sep(")");
        // 使用Maybe而不是Option，保证有一颗子树（仅有根节点，子节点数量为0，正好表示没有参数）
        protected static Parser def = RT(typeof(DefStatement)).Sep("def").Identifier(reserved).Ast(param_list).Ast(block);
        protected static Parser args = RT(typeof(Arguments)).Ast(expr0).Repeat(R.Sep(",").Ast(expr0));
        protected static Parser postfix = R.Sep("(").Maybe(args).Sep(")");

        public FuncParser(): base()
        {
            reserved.Add(")");   // 避免右括号成为标识符

            // 原有非终结符的修改由构造函数来完成
            // 注意必须在原有基础上添加，不能重新赋值，不然之前引用到它们的地方得不到更新
            primary.Repeat(postfix);
            simple.Option(args);
            program.InsertChoice(def);
        }
    }
}
