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
            return "<object: " + GetHashCode() + " >"; ;
        }

        public object Read(string member)
        {
            Env e = GetEnv(member);
            return e.Get(member);
        }

        public void Write(string member, object value)
        {
            Env e = GetEnv(member);
            ((NestedEnv)e).PutInner(member, value);
            // 索引类成员过来的，肯定要在局部环境中处理，不用考虑outer
        }

        Env GetEnv(string member)
        {
            // 需要判断member所处的env，防止把outer env中的变量当作类内字段去引用
            Env e = env.Where(member);
            if (e != null && e == env)
                return e;
            else
                throw new StoneException($"StoneObject {GetHashCode()} member {member} access failed");
        }
    }
}
