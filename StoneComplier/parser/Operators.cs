using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneComplier
{
    public class Precedence
    {
        public int priority;   // 优先级，数值越大优先级越高，1 2 3 4
        public bool left_asso; // 左结合or右结合

        public Precedence(int i, bool b)
        {
            priority = i;
            left_asso = b;
        }
    }

    public class Operators
    {
        public static bool LEFT = true;
        public static bool RIGHT = false;
        Dictionary<string, Precedence> operators = new Dictionary<string, Precedence>();

        public void Add(string name, int priority, bool left_asso)
        {
            operators.Add(name, new Precedence(priority, left_asso));
        }

        public Precedence Get(string name)
        {
            if (operators.ContainsKey(name))
                return operators[name];
            return null;
        }
    }
}
