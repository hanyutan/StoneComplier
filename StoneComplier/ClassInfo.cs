using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneComplier
{
    class ClassInfo
    {
        protected ClassStatement definition;  // 保存了类的抽象语法树
        protected Env env;     // 定义时的环境
        public Env Environment => env;

        public string Name => definition.Name;
        public ClassInfo SuperClass;
        public ClassBody Body => definition.Body;

        public ClassInfo(ClassStatement definition, Env env)
        {
            this.definition = definition;
            this.env = env;
            if (definition.SuperClass == null)
                SuperClass = null;
            else
            {
                object obj = env.Get(definition.SuperClass);
                if (obj is ClassInfo)
                    SuperClass = (ClassInfo)obj;
                else
                    throw new StoneException("ClassInfo: unknown super class", definition);
            }
        }

        public override string ToString()
        {
            return "<class: " + Name + " >";
        }
    }
}
