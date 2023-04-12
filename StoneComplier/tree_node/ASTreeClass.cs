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
            // new一个类，执行构造函数(类定义)中的每个语句，将成员添加进env
            // todo 可能会有点问题，如果有成员变量与全局环境中的变量同名，这样写会直接修改全局变量
            // 可能的措施：为member成员独立搞个语法树节点，重新定义Eval，直接加进局部环境
            if(Config.OptimizeClassObject)
            {
                foreach (var child in Children)
                    if(child is not DefStatement)
                        child.Eval(env);    // 只计算字段值，存到OptStoneObject的fields里；字段名和方法名已经在ClassStatement->Eval、Body->Lookup方法中，存进field names和methodDef里了
            }
            else
            {
                foreach (var child in Children)
                    child.Eval(env);
            }
            
            return null;
        }

        public void Lookup(Symbols syms, Symbols method_names, Symbols field_names, List<DefStatement> methods)
        {
            foreach (var child in Children)
            {
                if (child is DefStatement)
                {
                    DefStatement def = (DefStatement)child;
                    int old_size = method_names.Size();
                    int i = method_names.PutInner(def.Name);
                    if (i >= old_size)
                        methods.Add(def);   // 新增函数
                    else
                        methods[i] = def;   // 覆盖同名函数
                    def.LookupAsMethod(field_names);
                }
                else
                {
                    child.Lookup(syms);     // 字段名会通过SymbolThis->outer存进field_names中，question 为什么不直接field_names.PutInner呢？？
                }
            }
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
            if(Config.OptimizeClassObject)
            {
                Symbols method_names = new MemberSymbols(env.GetSymbols(), MemberSymbols.METHOD);
                Symbols field_names = new MemberSymbols(method_names, MemberSymbols.FIELD);
                OptClassInfo info = new OptClassInfo(this, env, method_names, field_names);
                env.Put(Name, info);

                List<DefStatement> methods = new List<DefStatement>();
                var super_class = info.GetSuperClass();
                if (super_class != null)
                    super_class.CopyTo(field_names, method_names, methods);  // 继承父类的字段与方法

                Symbols new_syms = new SymbolThis(field_names);
                Body.Lookup(new_syms, method_names, field_names, methods);
                info.SetMethod(methods);
            }
            else
            {
                ClassInfo info = new ClassInfo(this, env);
                env.Put(Name, info);
            }
            
            return Name;
        }

        public override void Lookup(Symbols symbols)
        {
            // 由eval去执行与lookup方法功能相当的操作
            // 因为如果程序定义的类需要继承一个父类，那必然需要环境中已经记录了这一父类，
            // 否则语言处理器找不到父类的定义就无法确定需要继承的方法与字段，
            // 导致lookup方法就无法执行。
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

        // for inline cache
        protected OptClassInfo class_info = null;
        protected bool is_field;
        protected int index;

        public override object Eval(Env env, object value)
        {
            // 此函数由PrimaryExpr.EvalNestPostfix函数直接调用
            // value是句点.左侧的计算结果
            if(Config.OptimizeClassObject)
            {
                if(value is OptClassInfo)
                {
                    if(Name == "new")
                    {
                        OptClassInfo info = (OptClassInfo)value;
                        ArrayEnv new_env = new ArrayEnv(1, info.Environment);
                        OptStoneObject obj = new OptStoneObject(info, info.GetFieldsSize());
                        new_env.Put(0, 0, obj);  // obj的局部环境，只存一个this指向自己即可，用于在IdName根据index(-1/-2)查找时GetThis
                        InitObject(info, obj, new_env);
                        return obj;
                    }
                    else
                        throw new StoneException($"Dot: {Name} access failed, not support static member", this);
                }
                else if(value is OptStoneObject)
                {
                    try
                    {
                        OptStoneObject target = (OptStoneObject)value;
                        if (Config.OptimizeInlineCache)
                        {
                            if (target.GetClassInfo() != class_info)
                                UpdateCache(target);
                            if (is_field)
                                return target.Read(index);
                            else
                                return target.GetMethod(index);  // 为啥还要把字段和方法分开呢？Read里面不是处理过了吗？
                        }
                        else
                            return target.Read(Name);
                    }
                    catch
                    {
                        throw new StoneException($"Dot: member {Name} access failed, not defined", this);
                    }
                }
                else
                    throw new StoneException($"Dot: value type wrong {value.GetType().Name}", this);
            }
            else
            {
                if (value is ClassInfo)
                {
                    if (Name == "new")
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
                        throw new StoneException($"Dot: {Name} access failed, not support static member", this);
                }
                else if (value is StoneObject)
                {
                    try
                    {
                        // value是要访问的对象
                        // 读取类成员，实现方法调用与字段访问
                        return ((StoneObject)value).Read(Name);
                    }
                    catch
                    {
                        throw new StoneException($"Dot: member {Name} access failed, not defined", this);
                    }
                }
                else
                    throw new StoneException($"Dot: value type wrong {value.GetType().Name}", this);
            }
        }

        // 没有提供lookup实现，因为.引用字段或方法时，只有在程序执行时才能确定对象的类型，进而得到字段或方法的存储位置index

        protected void UpdateCache(OptStoneObject target)
        {
            class_info = target.GetClassInfo();
            int i = class_info.GetFieldIndex(Name);
            if(i >= 0)
            {
                is_field = true;
                index = i;
                return;
            }
            i = class_info.GetMethodIndex(Name);
            if(i >= 0)
            {
                is_field = false;
                index = i;
                return;
            }
            throw new StoneException($"Dot: member {Name} access failed, not defined", this);
        }

        void InitObject(ClassInfo info, Env env)
        {
            // 初始化父类
            var super_class = info.GetSuperClass();
            if (super_class != null)
                InitObject(super_class, env);

            // 初始化本类中的成员
            info.Body.Eval(env);
        }

        void InitObject(OptClassInfo info, OptStoneObject obj, Env env)
        {
            // question: 这个obj参数传进来有啥用哦？？
            var super_class = info.GetSuperClass();
            if (super_class != null)
                InitObject(super_class, obj, env);

            info.Body.Eval(env);
        }
    }
}
