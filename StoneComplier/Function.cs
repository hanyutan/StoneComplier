using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneComplier
{
    public class Function
    {
        string name;
        public ParameterList Parameters;
        public BlockStatement Body;   // 保存了函数的抽象语法树
        Env env;   // 定义时的全局作用域

        public Function(string name, ParameterList parameters, BlockStatement body, Env env)
        {
            this.name = name;
            this.Parameters = parameters;
            this.Body = body;
            this.env = env;
        }

        public Env MakeEnv()
        {
            return new NestedEnv(env);
        }

        public override string ToString()
        {
            return "<fun: " + name + " >";
        }
    }
}
