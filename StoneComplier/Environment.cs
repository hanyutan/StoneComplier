using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneComplier
{
    public interface Env
    {
        // 避免跟System.Environment冲突
        // 环境对象指一种用于记录变量名称和值的对应关系的数据结构
        
        /* 环境中可以记录的名值对有哪些：
         * 整数值 int对象
         * 字符串 string对象
         * 函数 Function对象
         * 原生函数 NativeFunction对象
         * 类定义 ClassInfo对象
         * Stone语言的对象 StoneObject对象
         */

        public void Put(string name, object value);
        public object Get(string name);
        public Env Where(string name);
    }

    public class BasicEnv: Env
    {
        Dictionary<string, object> maps = new Dictionary<string, object>();
        public void Put(string name, object value)
        {
            if (maps.ContainsKey(name))
                maps[name] = value;
            else
                maps.Add(name, value);
        }

        public object Get(string name)
        {
            if(maps.ContainsKey(name))
                return maps[name];

            throw new StoneException($"Environment not contains {name}");
        }

        public Env Where(string name)
        {
            if (maps.ContainsKey(name))
                return this;
            else
                return null;
        }
    }

    /* 变量的作用域通常由嵌套结构实现，需要为每一种作用域准备一个单独的环境，并根据需要来嵌套环境
     * 查找变量时，程序首先查找与最内层作用域对应的环境，如果没找到再向外逐层查找
     * stone语言中目前不支持在函数内嵌套定义函数，也不考虑{}代码块的独立作用域，因此其始终只有2个作用域：全局、局部
     * 
     * 变量的生存周期可以通过环境对象的创建及清除时机来控制
     */

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

        public void PutInner(string name, object value)
        {
            // 直接向局部作用域添加，不考虑outer
            if (maps.ContainsKey(name))
                maps[name] = value;
            else
                maps.Add(name, value);
        }

        public void Put(string name, object value)
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

        public object Get(string name)
        {
            if (maps.ContainsKey(name))
                return maps[name];      // 查找局部作用域
            else if(outer != null)
                return outer.Get(name); // 查找全局作用域

            throw new StoneException($"Environment not contains {name}");
        }

        public Env Where(string name)
        {
            if (maps.ContainsKey(name))
                return this;
            else if (outer == null)
                return null;
            else
                return outer.Where(name);
        }
    }
}
