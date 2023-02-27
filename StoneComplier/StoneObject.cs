using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneComplier
{
    class StoneObject
    {
        protected Env env;
        
        public StoneObject(Env env)
        {
            this.env = env;
        }

        public override string ToString()
        {
            return "<object: " + this.GetHashCode() + " >"; ;
        }

        public object Read(string member)
        {
            return env.Get(member);
        }

        public void Write(string member, object value)
        {
            ((NestedEnv)env).AddNew(member, value);
            // 索引类成员过来的，肯定要加到局部环境中，不用考虑outer
            // （但应该不会出现这种情况吧？Dot.Eval会对没定义的成员名进行拦截）
        }
    }
}
