using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneComplier
{
    public class ArrayLiteral : ASTList
    {
        // 数组字面量
        public ArrayLiteral(List<ASTree> list) : base(list)
        {
        }

        public int Size => Children.Count;

        public override object Eval(Env env)
        {
            // 创建一个c#数组，存储每个元素expr的计算结果
            object[] result = new object[Size];
            for(int i = 0; i < Size; ++i)
                result[i] = Children[i].Eval(env);
            
            return result;
        }

    }

    public class ArrayRef: Postfix
    {
        // 用于表示数组索引
        public ArrayRef(List<ASTree> list) : base(list)
        {
        }

        public ASTree Index => Children[0];

        public override string ToString()
        {
            return "[" + Index + "]";
        }

        public override object Eval(Env env, object value)
        {
            // 此函数由PrimaryExpr.EvalNestPostfix函数直接调用
            // value是从环境中拿到的c#数组对象
            if (value is object[])
            {
                object index = Index.Eval(env);   // 计算表达式得到实际的索引序号
                if(index is int)
                    return ((object[])value)[(int)index];
                
                throw new StoneException($"ArrayRef: index {index} is not int", this);
            }
            throw new StoneException($"ArrayRef: value is not array", this);
        }
    }
}
