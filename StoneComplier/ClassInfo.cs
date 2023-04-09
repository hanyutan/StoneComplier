using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneComplier
{
    public class ClassInfo
    {
        protected ClassStatement definition;  // 保存了类的抽象语法树
        protected Env env;     // 定义时的环境
        public Env Environment => env;

        public string Name => definition.Name;

        public ClassBody Body => definition.Body;

        protected ClassInfo super_class;
        public virtual ClassInfo GetSuperClass()
        {
            return super_class;
        }

        public ClassInfo(ClassStatement definition, Env env)
        {
            this.definition = definition;
            this.env = env;
            if (definition.SuperClass == null)
                super_class = null;
            else
            {
                object obj = env.Get(definition.SuperClass);
                if (obj is ClassInfo)
                    super_class = (ClassInfo)obj;
                else
                    throw new StoneException("ClassInfo: unknown super class", definition);
            }
        }

        public override string ToString()
        {
            return "<class: " + Name + " >";
        }
    }

    public class OptClassInfo: ClassInfo
    {
        protected Symbols methods;
        protected Symbols fields;
        protected DefStatement[] method_defs;

        public OptClassInfo(ClassStatement definition, Env env, Symbols methods, Symbols fields): base(definition, env)
        {
            this.methods = methods;
            this.fields = fields;
            this.method_defs = null;
        }

        public int GetFieldsSize()
        {
            return fields.Size();
        }

        public override OptClassInfo GetSuperClass()
        {
            return (OptClassInfo)super_class;
        }

        public void CopyTo(Symbols f, Symbols m, List<DefStatement> mdef_list)
        {
            f.Append(fields);
            m.Append(methods);
            foreach (var def in method_defs)
                mdef_list.Add(def);
        }

        public int GetFieldIndex(string name)
        {
            return fields.Find(name);
        }

        public int GetMethodIndex(string name)
        {
            return methods.Find(name);
        }

        public object GetMethod(OptStoneObject self, int index)
        {
            DefStatement def = method_defs[index];
            return new OptMethod(def.Parameters, def.Body, Environment, def.Locals(), self);
        }

        public void SetMethod(List<DefStatement> mdef_list)
        {
            method_defs = mdef_list.ToArray();
        }


    }

    public class OptMethod: OptFunction
    {
        OptStoneObject self;
        public OptMethod(ParameterList parameters, BlockStatement body, Env env, int memory_size, OptStoneObject self): base(parameters, body, env, memory_size)
        {
            this.self = self;
        }

        public override Env MakeEnv()
        {
            ArrayEnv e = new ArrayEnv(size, env);
            e.Put(0, 0, self);
            return e;
        }
    }
}
