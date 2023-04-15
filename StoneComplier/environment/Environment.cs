using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneComplier
{
    public class Env
    {
        // 避免跟System.Environment冲突
        // 环境对象指一种用于记录变量名称和值的对应关系的数据结构

        public virtual void Put(string name, object value)
        {
            throw new StoneException($"No implementation: access by {name}");
        }

        public virtual void PutInner(string name, object value)
        {
            throw new StoneException($"No implementation: access by {name}");
        }

        public virtual object Get(string name)
        {
            throw new StoneException($"No implementation: access by {name}");
        }

        public virtual Env Where(string name)
        {
            throw new StoneException($"No implementation: access by {name}");
        }

        public virtual Symbols GetSymbols()
        {
            throw new StoneException("No symbols");
        }

        public virtual void Put(int nest, int index, object value)
        {
            throw new StoneException($"No implementation: access by {index}");
        }

        public virtual object Get(int nest, int index)
        {
            throw new StoneException($"No implementation: access by {index}");
        }
    }

    public class BasicEnv: Env
    {
        Dictionary<string, object> maps = new Dictionary<string, object>();
        public override void Put(string name, object value)
        {
            if (maps.ContainsKey(name))
                maps[name] = value;
            else
                maps.Add(name, value);
        }

        public override object Get(string name)
        {
            if(maps.ContainsKey(name))
                return maps[name];

            throw new StoneException($"Environment not contains {name}");
        }

        public override Env Where(string name)
        {
            if (maps.ContainsKey(name))
                return this;
            else
                return null;
        }
    }

    public class NestedEnv : Env
    {
        Dictionary<string, object> maps = new Dictionary<string, object>();
        Env outer = null;     // 因为由内到外查找，所以不是外部引用内部环境

        public NestedEnv(Env env = null)
        {
            SetOuter(env);
        }

        public void SetOuter(Env env = null)
        {
            outer = env;
        }

        public override void PutInner(string name, object value)
        {
            // 直接向局部作用域添加，不考虑outer
            if (maps.ContainsKey(name))
                maps[name] = value;
            else
                maps.Add(name, value);
        }

        public override void Put(string name, object value)
        {
            Env e = Where(name);
            if (e != null && e == outer)
            {
                // 外部作用域有，更新外部值
                e.Put(name, value);
            }
            else
            {
                // 内部作用域有，更新内部值
                // 内部外部作用域都没有，新增内部值
                PutInner(name, value);
            }
        }

        public override object Get(string name)
        {
            if (maps.ContainsKey(name))
                return maps[name];      // 查找局部作用域
            else if(outer != null)
                return outer.Get(name); // 查找全局作用域

            throw new StoneException($"Environment not contains {name}");
        }

        public override Env Where(string name)
        {
            if (maps.ContainsKey(name))
                return this;
            else if (outer == null)
                return null;
            else
                return outer.Where(name);
        }
    }

    public class ArrayEnv : Env
    {
        // 记录函数的参数和局部变量的环境
        // 不用dictionary改用array，加快变量读写速度
        protected object[] values;
        protected Env outer;

        public ArrayEnv(int size, Env env)
        {
            values = new object[size];
            outer = env;
        }

        public void SetOuter(Env env = null)
        {
            outer = env;
        }

        public override object Get(int nest, int index)
        {
            //nest代表在从内往外第几层env里，index代表变量存在数组中的第几个位置
            if (nest == 0)
                return values[index];
            else if (outer == null)
                return null;
            else
                return ((ArrayEnv)outer).Get(nest - 1, index);
        }

        public override void Put(int nest, int index, object value)
        {
            if(nest == 0)
                values[index] = value;
            else if (outer == null)
                throw new StoneException("no outer environment");
            else
                ((ArrayEnv)outer).Put(nest - 1, index, value);
        }

        // 不再需要PutInner，因为可以直接由Put的参数nest指定存储层级位置
    }

    public class ResizableArrayEnv : ArrayEnv
    {
        // 用于记录全局变量  // 支持按name查找，也支持按index查找（继承了ArrayEnv）
        // 可以保存任意数量的变量，在新增程序语句时可以修改环境中的变量数量
        protected Symbols names;    // 为啥不直接在这里用一个Dictionary表示？

        public ResizableArrayEnv(): base(10, null)
        {
            names = new Symbols();
        }

        public override Symbols GetSymbols()
        {
            return names;
        }

        public override void Put(string name, object value)
        {
            Env e = Where(name);
            if (e == null)
                e = this;
            e.PutInner(name, value);
        }

        public override void PutInner(string name, object value)
        {
            Assign(names.PutInner(name), value);
        }

        public override object Get(string name)
        {
            int i = names.Find(name);
            if(i < 0)
            {
                if (outer == null)
                    return null;
                else
                    return outer.Get(name);
            }
            else
            {
                return values[i];
            }
        }

        public override Env Where(string name)
        {
            if (names.Find(name) >= 0)
                return this;
            else if (outer == null)
                return null;
            else
                return ((ResizableArrayEnv)outer).Where(name);
        }

        public override void Put(int nest, int index, object value)
        {
            if (nest == 0)
                Assign(index, value);
            else
                base.Put(nest, index, value);
        }

        void Assign(int index, object value)
        {
            if(index >= values.Length)
            {
                // 数组扩容
                int new_len = values.Length * 2;
                if (index >= new_len)
                    new_len = index + 1;
                Array.Resize<object>(ref values, new_len);
            }
            values[index] = value;
        }
    }
}
