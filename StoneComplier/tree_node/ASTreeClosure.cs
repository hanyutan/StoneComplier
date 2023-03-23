using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneComplier
{
    public class Closure : ASTList
    {
        public Closure(List<ASTree> list) : base(list)
        {

        }
        public ParameterList Parameters => (ParameterList)Children[0];

        public BlockStatement Body => (BlockStatement)Children[1];

        public override string ToString()
        {
            return "(fun " + Parameters.ToString() + " " + Body.ToString() + ")";
        }

        public override object Eval(Env env)
        {
            // 直接返回闭包对象，env为闭包【定义】时所处的环境
            // 调用闭包时如果局部环境中找不到就来这里定义时候的环境查找
            if(Config.OptimizeVariableRW)
                return new OptFunction(Parameters, Body, env, size);
            else
                return new Function(null, Parameters, Body, env);
        }

        int size;      // 闭包里参数与局部变量的数量

        public override void Lookup(Symbols symbols)
        {
            size = Lookup(symbols, Parameters, Body);
        }

        public static int Lookup(Symbols symbols, ParameterList parameters, BlockStatement body)
        {
            Symbols new_symbols = new Symbols(symbols);  // 新建一个用于嵌套存储局部变量
            parameters.Lookup(new_symbols);              // 存储每个参数在局部环境中的index
            body.Lookup(new_symbols);                    // 为函数体中出现的变量名都存上nest和index信息
            return new_symbols.Size();
        }
    }
}
