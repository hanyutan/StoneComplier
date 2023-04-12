using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneComplier
{
    public class StoneObject
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
            e.PutInner(member, value);
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

    public class OptStoneObject
    {
        protected OptClassInfo class_info;   // 引用的共享部分，记录字段名称和方法信息
        protected object[] fields;           // 记录实例化对象自己的字段值

        public OptStoneObject(OptClassInfo class_info, int size)
        {
            this.class_info = class_info;
            this.fields = new object[size];
        }

        public OptClassInfo GetClassInfo()
        {
            return class_info;
        }

        // 包含两组读写方法，一组通过名称、一组通过数组下标来引用
        // 语言处理器将根据目标对象是否为this对象来选择合适的方法
        public object Read(string name)
        {
            // 读取字段值或方法对象
            int index = class_info.GetFieldIndex(name);
            if (index >= 0)
                return fields[index];
            else
            {
                index = class_info.GetMethodIndex(name);
                if (index >= 0)
                    return GetMethod(index);
            }

            throw new StoneException($"Read {class_info.Name}.{name} failed");
        }

        public void Write(string name, object value)
        {
            //写入字段值
            int index = class_info.GetFieldIndex(name);
            if (index > 0)
                fields[index] = value;

            throw new StoneException($"Write {class_info.Name}.{name} failed");
        }

        public object Read(int index)
        {
            return fields[index];
        }

        public void Write(int index, object value)
        {
            fields[index] = value;
        }

        public object GetMethod(int index)
        {
            return class_info.GetMethod(this, index);
        }
    }
}
