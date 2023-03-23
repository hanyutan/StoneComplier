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

        public Location Get(string key, int nest = 0)
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

        public int PutInner(string key)
        {
            if (!table.ContainsKey(key))
                return Add(key);
            else
                return table[key];
        }

        public Location Put(string key)
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
}
