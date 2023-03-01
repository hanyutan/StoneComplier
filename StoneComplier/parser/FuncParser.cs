// 暂时用宏的方式，自由决定是否添加array、class等功能
#define SUPPORT_CLOSURE
#define SUPPORT_CLASS
#define SUPPORT_ARRAY

namespace StoneComplier
{
    public partial class FuncParser: BasicParser
    {
        // 支持函数的解析器，继承自BasicParser，这里仅定义新增的部分

        protected static Parser param_ = R.Identifier(reserved);     // 没有加type，可能是因为不是变量名所以不需要作为节点加入语法树
        protected static Parser params_ = RT(typeof(ParameterList)).Ast(param_).Repeat(R.Sep(",").Ast(param_));  // 防止跟关键字params冲突
        protected static Parser param_list = R.Sep("(").Maybe(params_).Sep(")");
        // 使用Maybe而不是Option，保证有一颗子树（仅有根节点，子节点数量为0，正好表示没有参数）
        protected static Parser def = RT(typeof(DefStatement)).Sep("def").Identifier(reserved).Ast(param_list).Ast(block);
        protected static Parser args = RT(typeof(Arguments)).Ast(expr0).Repeat(R.Sep(",").Ast(expr0));
        protected static Parser postfix = R.Sep("(").Maybe(args).Sep(")");

#if SUPPORT_CLASS
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
#endif

#if SUPPORT_ARRAY
        protected Parser elements = RT(typeof(ArrayLiteral))
            .Ast(expr)
            .Repeat(R.Sep(",").Ast(expr));
#endif

        public FuncParser(): base()
        {
            reserved.Add(")");   // 避免右括号成为标识符

            // 原有非终结符的修改由构造函数来完成
            // 注意必须在原有基础上添加，不能重新赋值，不然之前引用到它们的地方得不到更新
            primary.Repeat(postfix);
            simple.Option(args);
            program.InsertChoice(def);

#if SUPPORT_CLOSURE
            primary.InsertChoice(RT(typeof(Closure)).Sep("fun").Ast(param_list).Ast(block));
#endif

#if SUPPORT_CLASS
            postfix.InsertChoice(RT(typeof(Dot)).Sep(".").Identifier(reserved));
            program.InsertChoice(def_class);
#endif

#if SUPPORT_ARRAY
            reserved.Add("]");
            primary.InsertChoice(R.Sep("[").Maybe(elements).Sep("]"));  // maybe 可以是空数组
            postfix.InsertChoice(RT(typeof(ArrayRef))
                .Sep("[")
                .Ast(expr)
                .Sep("]"));
#endif
        }
    }
}
