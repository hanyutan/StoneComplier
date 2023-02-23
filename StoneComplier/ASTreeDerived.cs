using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneComplier
{
    // 每个类都代表了一种语法树节点

    public class NumLiteral : ASTLeaf
    {
        // 整型字面量
        public NumLiteral(Token t) : base(t)
        {
            if (t.Type != TokenType.Number)
                throw new StoneException("Token is not number");
        }

        public int Value => token.GetNumber();

        public override object Eval(Env env)
        {
            return Value;
        }
    }

    public class StringLiteral : ASTLeaf
    {
        // 整型字面量
        public StringLiteral(Token t) : base(t)
        {
            if (t.Type != TokenType.String)
                throw new StoneException("Token is not string");
        }

        public string Value => token.GetText();

        public override object Eval(Env env)
        {
            return Value;
        }
    }


    public class IdName : ASTLeaf
    {
        // 变量名、类名、函数名
        public IdName(Token t) : base(t)
        {
            if (t.Type != TokenType.Identifier)
                throw new StoneException("Token is not identifier");
        }

        public string Value => token.GetText();

        public override object Eval(Env env)
        {
            return env.Get(Value);
        }
    }

    public class BinaryOp : ASTList
    {
        // 二元运算
        public BinaryOp(List<ASTree> list) : base(list)
        {
            if (list.Count != 3)
                throw new StoneException("BinaryOp need 3 elements");
        }

        public ASTree Left => Children[0];

        // 运算符也是一个leaf node，存在于token中
        public string Operator => Children[1].ToString();

        public ASTree Right => Children[2];

        public override object Eval(Env env)
        {
            if (Operator == "=")
                return ComputeAssign(env);
            else
                return ComputeOp(env);
        }

        object ComputeAssign(Env env)
        {
            object rvalue = Right.Eval(env);
            if (Left is IdName)
            {
                string name = ((IdName)Left).Value;
                env.Put(name, rvalue);
                return rvalue;   // 赋值表达式的返回值
            }
            else
                throw new StoneException("BinaryOp: ComputeAssign failed");
        }

        object ComputeOp(Env env)
        {
            object left = Left.Eval(env);
            object right = Right.Eval(env);
            string op = Operator;

            if (left is int && right is int)
            {
                return ComputeNumber((int)left, op, (int)right);
            }
            else if (op == "+")
            {
                return left.ToString() + right.ToString();
            }
            else if (op == "==")
            {
                if (left == null)
                    return right == null ? 1 : 0;
                else
                    return left == right ? 1 : 0;
            }
            else
                throw new StoneException("BinaryOp: ComputeOp failed");
        }

        object ComputeNumber(int left, string op, int right)
        {
            switch (op)
            {
                case "+": return left + right;
                case "-": return left - right;
                case "*": return left * right;
                case "/": return left / right;
                case "%": return left % right;
                case "==": return left == right ? 1 : 0;
                case ">": return left > right ? 1 : 0;
                case "<": return left < right ? 1 : 0;
                default:
                    throw new StoneException($"BinaryOp: ComputeNumber failed, operator = {op}");
            }
        }
    }

    public class PrimaryExpr : ASTList
    {
        // 语法模式 非终结符
        public PrimaryExpr(List<ASTree> list) : base(list)
        {

        }

        public ASTree Operand => Children[0];

        public static ASTree Create(List<ASTree> list)
        {
            if (list.Count == 1)
                return list[0];
            else
                return new PrimaryExpr(list);
        }

        public override object Eval(Env env)
        {
            return EvalNestArgs(env, 0);
        }

        object EvalNestArgs(Env env, int nest)
        {
            // 函数调用阶段，

            /* 如果是num/id/string，list.Count = 1, 就形成不了PrimaryExpr这个子节点
             * 所以但凡走到这个函数了，都是函数调用
             * 支持类似f(9)(3)形式，计算嵌套的每组函数实参args调用结果
             */
            if (HasPosfix(nest))
            {
                object target = EvalNestArgs(env, nest + 1);
                return GetPosfix(nest).Eval(env, target);   // 拿到Function，交给Arguments计算
            }
            else
            {
                // Operand是函数名，Eval返回环境中查找到的Function函数对象
                return Operand.Eval(env);
            }
        }

        public Postfix GetPosfix(int nest)
        {
            // 嵌套参数调用，从右向左，先把右侧的参数压栈存起来，最先处理的是左侧第一个调用
            return (Postfix)Children[Children.Count - 1 - nest];
        }

        public bool HasPosfix(int nest)
        {
            // 参数倒序，从右向左？？
            return Children.Count - 1 - nest > 0;
        }
    }

    public class NegativeExpr : ASTList
    {
        public NegativeExpr(List<ASTree> list) : base(list)
        {

        }

        public ASTree Operand => Children[0];

        public override string ToString()
        {
            return "-" + Operand.ToString();
        }

        public override object Eval(Env env)
        {
            object result = Operand.Eval(env);
            if (result is int)
                return -(int)result;
            else
                throw new StoneException("NegativeExpr: not int");
        }
    }

    public class BlockStatement : ASTList
    {
        public BlockStatement(List<ASTree> list) : base(list)
        {

        }

        public override object Eval(Env env)
        {
            object result = 0;
            // 返回最后一个语句的值
            foreach (ASTree child in Children)
            {
                if (child is not NullStatement)
                    result = child.Eval(env);
            }
            return result;
        }
    }

    public class IfStatement : ASTList
    {
        public IfStatement(List<ASTree> list) : base(list)
        {

        }

        public ASTree Condition => Children[0];

        public ASTree ThenBlock => Children[1];

        public ASTree ElseBlock => (Children.Count > 2) ? Children[2] : null;

        public override string ToString()
        {
            return "(if " + Condition.ToString() + " then " + ThenBlock.ToString() + " else " + ElseBlock.ToString() + ")";
        }

        public override object Eval(Env env)
        {
            object cond = Condition.Eval(env);
            if (cond is int && (int)cond != 0)
                return ThenBlock.Eval(env);
            else
            {
                if (ElseBlock == null)
                    return 0;
                else
                    return ElseBlock.Eval(env);
            }
        }
    }


    public class WhileStatement : ASTList
    {
        public WhileStatement(List<ASTree> list) : base(list)
        {

        }

        public ASTree Condition => Children[0];

        public ASTree Body => Children[1];

        public override string ToString()
        {
            return "(while" + Condition.ToString() + " do " + Body.ToString() + ")";
        }

        public override object Eval(Env env)
        {
            object result = 0;
            for (; ; )
            {
                object cond = Condition.Eval(env);
                if (cond is int && (int)cond != 0)
                    result = Body.Eval(env);    // 条件满足就继续算
                else
                    return result;   // 条件不满足就返回最后一个值
            }
        }
    }
    public class NullStatement : ASTList
    {
        public NullStatement(List<ASTree> list) : base(list)
        {

        }

        public override object Eval(Env env)
        {
            return null;
        }
    }

    public class DefStatement : ASTList
    {
        public DefStatement(List<ASTree> list) : base(list)
        {

        }

        public string FuncName => ((ASTLeaf)Children[0]).ToString();

        public ParameterList Parameters => (ParameterList)Children[1];

        public BlockStatement Body => (BlockStatement)Children[2];

        public override string ToString()
        {
            return "(def " + FuncName + " " + Parameters.ToString() + " " + Body.ToString() + ")";
        }

        public override object Eval(Env env)
        {
            // 函数定义阶段，创建函数对象，并添加到全局环境
            env.Put(FuncName, new Function(FuncName, Parameters, Body, env));
            return FuncName;
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
            string param_name = ((ASTLeaf)Children[index]).ToString();
            ((NestedEnv)env).AddNew(param_name, value);   // 参数一定是最开始没有，直接加进去
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
        // 搞了一个抽象定义，作为各种参数形式的未来扩展吧？
        // 子类Arguments表示实参序列
        // 以后可以搞个子类ArrayRef用于支持数组
    }

    public class Arguments : Postfix
    {
        public Arguments(List<ASTree> list) : base(list)
        {

        }

        public int Size => Children.Count;

        public override object Eval(Env caller_env, object value)
        {
            // value是从环境中拿到的Function对象 
            // caller_env是调用函数时所处的环境，目前暂时就是全局环境
            // 然后利用caller_env、实参列表children、函数对象value，来计算函数调用结果
            if (value is not Function)
                throw new StoneException("Wrong function");

            Function func = (Function)value;

            // 形参，检查数量应与实参一致
            ParameterList param_list = func.Parameters;
            if (Size != param_list.Size)
                throw new StoneException("Function arguments number not equal to definition");

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
    }

    public class Closure: ASTList
    {
        public Closure(List<ASTree> list) : base(list)
        {

        }
        public ParameterList Parameters => (ParameterList)Children[0];

        public BlockStatement Body => (BlockStatement)Children[1];

        public override string ToString()
        {
            return "(fun " + Parameters.ToString() + " " + Body.ToString() + ")";
        }

        public override object Eval(Env env)
        {
            // 直接返回闭包对象，env为闭包【定义】时所处的环境
            // 调用闭包时如果局部环境中找不到就来这里定义时候的环境查找
            return new Function(null, Parameters, Body, env);
        }

    }
}
