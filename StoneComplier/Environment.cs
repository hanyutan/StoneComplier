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
        public void Put(string name, object value);
        public object Get(string name);
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
            {
                return maps[name];
            }
            throw new StoneException($"Environment maps not contains {name}");
        }
    }
}
