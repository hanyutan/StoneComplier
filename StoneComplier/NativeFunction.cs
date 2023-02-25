using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace StoneComplier
{
    public class NativeFunction
    {
        // 用于表示C#原生的static函数，可以在stone语言中调用
        string func_name;
        MethodInfo method;
        int params_num;
        public int ParamsNum => params_num;

        public NativeFunction(string func_name, MethodInfo method)
        {
            this.func_name = func_name;
            this.method = method;
            this.params_num = method.GetParameters().Length;
        }

        public object Invoke(object[] args)
        {
            try
            {
                return method.Invoke(null, args);
            }
            catch
            {
                throw new StoneException($"NativeFunction {func_name} invoke failed");
            }
        }

        public override string ToString()
        {
            return "<native: " + func_name + " >";
        }
    }
}
