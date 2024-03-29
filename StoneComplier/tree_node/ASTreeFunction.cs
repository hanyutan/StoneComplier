﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneComplier
{

    public class DefStatement : ASTList
    {
        public DefStatement(List<ASTree> list) : base(list)
        {

        }

        public string Name => ((ASTLeaf)Children[0]).ToString();

        public ParameterList Parameters => (ParameterList)Children[1];

        public BlockStatement Body => (BlockStatement)Children[2];

        public override string ToString()
        {
            return "(def " + Name + " " + Parameters.ToString() + " " + Body.ToString() + ")";
        }

        public override object Eval(Env env)
        {
            // 函数定义阶段，创建函数对象，并添加到全局环境
            if (Config.OptimizeVariableRW && index != UNKNOWN)
                env.Put(0, index, new OptFunction(Parameters, Body, env, size));   // lookup阶段只是记录符号表，确定index位置，eval阶段才是真正创建函数对象并存入环境
            else
                env.Put(Name, new Function(Name, Parameters, Body, env));
            return Name;
        }

        static readonly int UNKNOWN = -1;
        int index = UNKNOWN;     // 函数在全局环境中的索引
        int size;      // 函数里参数与局部变量的数量

        public override void Lookup(Symbols symbols)
        {
            index = symbols.PutInner(Name);                     // 将函数名存入最内层的环境（可以覆盖外层同名函数）
            size = Closure.Lookup(symbols, Parameters, Body);   // 函数里参数与局部变量的数量
        }

        public void LookupAsMethod(Symbols symbols)
        {
            Symbols new_syms = new Symbols(symbols);
            new_syms.PutInner(SymbolThis.NAME);
            Parameters.Lookup(new_syms);
            Body.Lookup(new_syms);
            size = new_syms.Size();
        }

        public int Locals()
        {
            return size;
        }
    }

    public class ParameterList : ASTList
    {
        public ParameterList(List<ASTree> list) : base(list)
        {

        }

        public int Size => Children.Count;

        public void Eval(Env env, int index, object value)
        {
            // 寻找第几个形参名，将实参值添加进局部环境
            if (Config.OptimizeVariableRW && offsets != null)
                env.Put(0, offsets[index], value);
            else
            {
                string param_name = ((ASTLeaf)Children[index]).ToString();
                env.PutInner(param_name, value);   // 参数一定是最开始没有，直接加进去
            }
        }

        int[] offsets;     // 存储每个参数在局部环境中的index
        public override void Lookup(Symbols symbols)
        {
            // 通过Symbols查找变量的保存位置，将结果记录到相应的字段中
            offsets = new int[Size];
            for(int i = 0; i < Size; ++i)
            {
                string param_name = ((ASTLeaf)Children[i]).ToString();
                offsets[i] = symbols.PutInner(param_name);  // 参数一定是最开始没有，直接加进去
            }
        }
    }

    public class Postfix : ASTList
    {
        public Postfix(List<ASTree> list) : base(list)
        {

        }
        public virtual object Eval(Env env, object value)
        {
            return null;
        }
        // 搞了一个抽象定义，作为各种参数形式的未来扩展
        // 子类Arguments表示实参序列
        // 子类Dot表示类的成员调用
        // 子类ArrayRef用于支持数组索引
    }

    public class Arguments : Postfix
    {
        public Arguments(List<ASTree> list) : base(list)
        {

        }

        public int Size => Children.Count;

        public object ProcessNativeFunction(Env caller_env, object value)
        {
            NativeFunction func = (NativeFunction)value;
            if (Size != func.ParamsNum)
                throw new StoneException("Function arguments number not equal to definition", this);

            object[] args = new object[func.ParamsNum];
            for (int i = 0; i < Size; ++i)
                args[i] = Children[i].Eval(caller_env);

            return func.Invoke(args);

        }
        public object ProcessNormalFunction(Env caller_env, object value)
        {
            Function func = (Function)value;

            // 形参，检查数量应与实参一致
            ParameterList param_list = func.Parameters;
            if (Size != param_list.Size)
                throw new StoneException("Function arguments number not equal to definition", this);

            Env nest_env = func.MakeEnv();                // 静态作用域：nest_env.outer是def函数时所处的环境，目前暂时就是全局环境
            //((NestedEnv)nest_env).SetOuter(caller_env);   // 动态作用域

            // 实参，挨个计算并以形参名添加进局部环境
            for (int i = 0; i < Size; ++i)
            {
                // 实参值要在全局环境中计算
                object arg_value = Children[i].Eval(caller_env);
                // i 指代第几个参数，找到形参名加入到局部环境
                param_list.Eval(nest_env, i, arg_value);

            }

            // 最后在局部环境中计算函数体
            // 注意：nest_env正好在Body计算结束后被销毁，即局部变量的生命周期也终止
            return func.Body.Eval(nest_env);
        }

        public override object Eval(Env caller_env, object value)
        {
            // 此函数由PrimaryExpr.EvalNestPostfix函数直接调用
            // value是从环境中拿到的Function对象 
            // caller_env是调用函数时所处的环境，目前暂时就是全局环境
            // 然后利用caller_env、实参列表children、函数对象value，来计算函数调用结果
            if (value is NativeFunction)
                return ProcessNativeFunction(caller_env, value);
            else if (value is Function)
                return ProcessNormalFunction(caller_env, value);
            else
                throw new StoneException("Wrong function", this);
        }
    }
}
