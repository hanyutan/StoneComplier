using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneComplier
{
    public class Function
    {
        public ParameterList Parameters;
        public BlockStatement Body;   // 保存了函数的抽象语法树
        protected Env env;   // 定义时的全局作用域
        protected string name;

        public Function(string name, ParameterList parameters, BlockStatement body, Env env)
        {
            this.name = name;
            this.Parameters = parameters;
            this.Body = body;
            this.env = env;
        }

        public virtual Env MakeEnv()
        {
            return new NestedEnv(env);
        }

        public override string ToString()
        {
            return "<fun: " + name + " >";
        }
    }

    public class OptFunction: Function
    {
        // 与Function的区别是，通过ArrayEnv对象来实现函数的执行环境
        protected int size;
        public OptFunction(ParameterList parameters, BlockStatement body, Env env, int memory_size):base(null, parameters, body, env)
        {
            size = memory_size;
        }

        public override Env MakeEnv()
        {
            return new ArrayEnv(size, env);
        }
    }
}
