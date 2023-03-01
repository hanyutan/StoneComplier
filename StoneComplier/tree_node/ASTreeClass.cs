using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneComplier
{
    public class ClassBody : ASTList
    {
        public ClassBody(List<ASTree> list) : base(list)
        {

        }

        public override object Eval(Env env)
        {
            // 执行类定义中的每个语句，将成员添加进env
            // 可能会有点问题，如果有成员变量与全局环境中的变量同名，这样写会直接修改全局变量
            foreach (var child in Children)
                child.Eval(env);
            return null;
        }
    }

    public class ClassStatement: ASTList
    {
        public ClassStatement(List<ASTree> list) : base(list)
        {

        }

        public string Name => ((ASTLeaf)Children[0]).ToString();

        public string SuperClass
        {
            get
            {
                if (Children.Count < 3)
                    return null;
                else
                    return ((ASTLeaf)Children[1]).ToString();
            }
        }

        public ClassBody Body => (ClassBody)Children[Children.Count - 1];

        public override string ToString()
        {
            string parent = SuperClass != null ? SuperClass : "*";
            return "(class " + Name + " : " + parent + " " + Body.ToString() + ")";
        }

        public override object Eval(Env env)
        {
            // 创建ClassInfo对象，添加到env
            ClassInfo info = new ClassInfo(this, env);
            env.Put(Name, info);
            return Name;
        }
    }

    public class Dot: Postfix
    {
        public Dot(List<ASTree> list) : base(list)
        {

        }

        public string Name => ((ASTLeaf)Children[0]).ToString();

        public override string ToString()
        {
            return "." + Name;
        }

        public override object Eval(Env env, object value)
        {
            // 此函数由PrimaryExpr.EvalNestPostfix函数直接调用
            // value是句点.左侧的计算结果
            if (value is ClassInfo)
            {
                if(Name == "new")
                {
                    // 创建实例
                    ClassInfo info = (ClassInfo)value;
                    NestedEnv nest_env = new NestedEnv(info.Environment);   // 类内局部环境
                    StoneObject obj = new StoneObject(nest_env);
                    nest_env.Put("this", obj);
                    InitObject(info, nest_env);
                    return obj;
                }
                else
                    throw new StoneException($"Dot: {Name} access failed, not support static member");

            }
            else if(value is StoneObject)
            {
                try
                {
                    // value是要访问的对象
                    // 读取类成员，实现方法调用与字段访问
                    return ((StoneObject)value).Read(Name);
                }
                catch
                {
                    throw new StoneException($"Dot: member {Name} access failed, not defined");
                }
            }
            else
                throw new StoneException($"Dot: value type wrong {value.GetType().Name} ");
        }

        void InitObject(ClassInfo info, Env env)
        {
            // 初始化父类
            if(info.SuperClass != null)
                InitObject(info.SuperClass, env);

            // 初始化本类中的成员
            info.Body.Eval(env);
        }
    }
}
