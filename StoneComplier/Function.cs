using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneComplier
{
    public class Function
    {
        string func_name;
        public ParameterList Parameters;
        public BlockStatement Body;
        Env env;   // 定义时的全局作用域

        public Function(string func_name, ParameterList parameters, BlockStatement body, Env env)
        {
            this.func_name = func_name;
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
            return "<fun: " + func_name + ">";
        }
    }
}
