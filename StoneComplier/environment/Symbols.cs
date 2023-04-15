using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneComplier
{
    public class Location
    {
        public int nest;
        public int index;

        public Location(int nest, int index)
        {
            this.nest = nest;
            this.index = index;
        }
    }

    public class Symbols
    {
        // 哈希表，用于记录变量名与保存位置的对应关系
        // 也有嵌套关系，用于表达变量所处的作用域
        protected Symbols outer;

        protected Dictionary<string, int> table = new Dictionary<string, int>();

        public Symbols(Symbols outer = null)
        {
            this.outer = outer;
        }

        public int Size()
        {
            return table.Count();
        }

        public void Append(Symbols s)
        {
            foreach(var pair in s.table)
            {
                table.Add(pair.Key, pair.Value);
            }
        }

        public int Find(string key)
        {
            if (!table.ContainsKey(key))
                return -1;
            else
                return table[key];
        }

        public virtual Location Get(string key, int nest = 0)
        {
            if(!table.ContainsKey(key))
            {
                if (outer == null)
                    return null;
                else
                    return outer.Get(key, nest + 1);
            }
            else
            {
                int index = table[key];
                return new Location(nest, index);
            }
        }

        public virtual int PutInner(string key)
        {
            if (!table.ContainsKey(key))
                return Add(key);
            else
                return table[key];
        }

        public virtual Location Put(string key)
        {
            Location loc = Get(key, 0);
            if (loc == null)
                return new Location(0, Add(key));
            else
                return loc;
        }

        public int Add(string key)
        {
            int i = table.Count();
            table.Add(key, i);
            return i;
        }

    }

    public class SymbolThis: Symbols
    {
        public static readonly string NAME = "this";
        
        public SymbolThis(Symbols outer): base(outer)
        {
            Add(NAME);
        }

        public override int PutInner(string key)
        {
            throw new StoneException("fatal");
        }

        public override Location Put(string key)
        {
            // 实际是存到了字段symbols里
            Location loc = outer.Put(key);
            if (loc.nest >= 0)
                loc.nest++;
            return loc;
        }
    }

    public class MemberSymbols: Symbols
    {
        public static readonly int METHOD = -1;  // 固定数值代替nest，可以通过此数值判断名称是否为通常的变量名
        public static readonly int FIELD= -2;

        protected int type;
        public MemberSymbols(Symbols outer, int type):base(outer)
        {
            this.type = type;
        }

        public override Location Get(string key, int nest = 0)
        {
            if (!table.ContainsKey(key))
            {
                if (outer == null)
                    return null;
                else
                    return outer.Get(key, nest);   // 区别：nest不再+1
            }
            else
            {
                int index = table[key];
                return new Location(type, index);  // type代替nest标识存储对象的类型
            }
        }

        public override Location Put(string key)
        {
            Location loc = Get(key, 0);
            if (loc == null)
                return new Location(type, Add(key));
            else
                return loc;
        }
    }
}
