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
            return new Function(null, Parameters, Body, env);
        }
    }
}
