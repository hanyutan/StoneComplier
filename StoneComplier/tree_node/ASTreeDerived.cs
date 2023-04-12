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
                throw new StoneException("Token is not number", this);
        }

        public int Value => token.GetNumber();

        public override object Eval(Env env)
        {
            return Value;
        }
    }

    public class StringLiteral : ASTLeaf
    {
        // 字符串字面量
        public StringLiteral(Token t) : base(t)
        {
            if (t.Type != TokenType.String)
                throw new StoneException("Token is not string", this);
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
                throw new StoneException("Token is not identifier", this);
        }

        public string Value => token.GetText();

        public override object Eval(Env env)
        {
            if (Config.OptimizeClassObject)
            {
                if (index == UNKNOWN)
                    return env.Get(Value);
                else if (nest == MemberSymbols.FIELD)
                    return GetThis(env).Read(index);
                else if (nest == MemberSymbols.METHOD)
                    return GetThis(env).GetMethod(index);
                else
                    return env.Get(nest, index);
            }
            else if (Config.OptimizeVariableRW)
            {
                if (index == UNKNOWN)
                    return env.Get(Value);   // 新增代码时未作记录的全局变量，还按老方法用变量名查找
                else
                    return env.Get(nest, index);
            }
            else
                return env.Get(Value);
        }

        static readonly int UNKNOWN = -1;
        int nest;     // 记录变量存放的环境嵌套层数
        int index = UNKNOWN;    // 记录变量对应的数组索引

        public override void Lookup(Symbols symbols)
        {
            // 作为右值
            // 通过Symbols查找变量的保存位置，将结果记录到相应的字段中
            Location loc = symbols.Get(Value);
            if (loc == null)
                throw new StoneException($"Undefined name: {Value}");
            else
            {
                nest = loc.nest;
                index = loc.index;
            }
        }

        public void LookupForAssign(Symbols symbols)
        {
            // 作为左值
            Location loc = symbols.Put(Value);   // 可能是局部环境新增的变量，也可能是引用全局变量
            nest = loc.nest;
            index = loc.index;
        }

        public void EvalForAssign(Env env, object value)
        {
            if (Config.OptimizeClassObject)
            {
                if (index == UNKNOWN)
                    env.Put(Value, value);
                else if (nest == MemberSymbols.FIELD)
                    GetThis(env).Write(index, value);
                else if (nest == MemberSymbols.METHOD)
                    throw new StoneException($"Cannot update a method: {Value}");
                else
                    env.Put(nest, index, value);
            }
            else if (Config.OptimizeVariableRW)
            {
                if (index == UNKNOWN)
                    env.Put(Value, value);    // 可能是新增代码的全局变量，还未记录index；或者是类对象里的成员
                else
                    env.Put(nest, index, value);
            }
            else
                throw new StoneException("Wrong call");

        }

        protected OptStoneObject GetThis(Env env)
        {
            return (OptStoneObject)(env.Get(0, 0));
        }
    }

    public class BinaryOp : ASTList
    {
        // 二元运算
        public BinaryOp(List<ASTree> list) : base(list)
        {
            if (list.Count != 3)
                throw new StoneException("BinaryOp need 3 elements", this);
        }

        public ASTree Left => Children[0];

        // 运算符也是一个leaf node，存在于token中
        public string Operator => Children[1].ToString();

        public ASTree Right => Children[2];

        // for inline cache
        protected OptClassInfo class_info = null;
        protected int index;

        public override object Eval(Env env)
        {
            if (Operator == "=")
            {
                object rvalue = Right.Eval(env);
                return ComputeAssign(env, rvalue);
            }
            else
                return ComputeOp(env);
        }

        public override void Lookup(Symbols symbols)
        {
            if (Operator == "=")
            {
                if(Left is IdName)
                {
                    ((IdName)Left).LookupForAssign(symbols);
                    Right.Lookup(symbols);
                    return;
                }
            }

            Left.Lookup(symbols);
            Right.Lookup(symbols);
        }

        object ComputeAssign(Env env, object rvalue)
        {
            if(Left is PrimaryExpr)
            {
                PrimaryExpr primary = (PrimaryExpr)Left;
                if(primary.HasPostfix(0))
                {
                    object target = primary.EvalNestPostfix(env, 1);  // 可能存在table.get().next.x = 3 这种嵌套形式，target最终计算等于table.get().next
                    if (primary.GetPostfix(0) is Dot && target is StoneObject)
                    {
                        // 类的成员变量/函数赋值，调用StoneObject.Write
                        if(Config.OptimizeClassObject)
                            return SetField((OptStoneObject)target, (Dot)primary.GetPostfix(0), rvalue);
                        else
                            return SetField((StoneObject)target, (Dot)primary.GetPostfix(0), rvalue);
                    }
                    else if(primary.GetPostfix(0) is ArrayRef && target is object[])
                    {
                        // 数组元素赋值
                        object index = ((ArrayRef)primary.GetPostfix(0)).Index.Eval(env);
                        if (index is int)
                        {
                            ((object[])target)[(int)index] = rvalue;
                            return rvalue;
                        }
                    }
                }
                throw new StoneException("BinaryOp: ComputeAssign failed", this);
            }
            else if (Left is IdName)
            {
                if (Config.OptimizeVariableRW)
                    ((IdName)Left).EvalForAssign(env, rvalue);
                else
                {
                    string name = ((IdName)Left).Value;
                    env.Put(name, rvalue);
                }
                return rvalue;   // 赋值表达式的返回值
            }
            throw new StoneException("BinaryOp: ComputeAssign failed", this);
        }

        object SetField(StoneObject obj, Dot expr, object rvalue)
        {
            string member = expr.Name;
            try
            {
                obj.Write(member, rvalue);
                return rvalue;
            }
            catch
            {
                throw new StoneException($"BinaryOp.SetField: access memeber {member} failed", this);
            }
        }

        object SetField(OptStoneObject obj, Dot expr, object rvalue)
        {
            string member = expr.Name;
            try
            {
                if(Config.OptimizeInlineCache)
                {
                    if(obj.GetClassInfo() != class_info)
                    {
                        class_info = obj.GetClassInfo();
                        int i = class_info.GetFieldIndex(member);
                        if(i < 0)
                            throw new StoneException($"BinaryOp.SetField: access memeber {member} failed", this);
                        index = i;
                    }
                    obj.Write(index, rvalue);
                    return rvalue;
                }
                else
                {
                    obj.Write(member, rvalue);
                    return rvalue;
                }
            }
            catch
            {
                throw new StoneException($"BinaryOp.SetField: access memeber {member} failed", this);
            }
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
                throw new StoneException("BinaryOp: ComputeOp failed", this);
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
                    throw new StoneException($"BinaryOp: ComputeNumber failed, operator = {op}", this);
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
            return EvalNestPostfix(env, 0);
        }

        public object EvalNestPostfix(Env env, int nest)
        {
            // 函数调用阶段 or 索引类成员

            /* 如果是num/id/string，list.Count = 1, 就形成不了PrimaryExpr这个子节点
             * 所以但凡走到这个函数了，都是函数调用阶段 or 索引类成员
             * 支持类似f(9)(3)形式，计算嵌套的每组函数实参args调用结果
             */
            if (HasPostfix(nest))
            {
                object target = EvalNestPostfix(env, nest + 1);
                // 拿到Function，交给Arguments计算
                // 拿到ClassInfo，交给Dot计算
                // 拿到StoneObject，交给Dot计算
                return GetPostfix(nest).Eval(env, target);
            }
            else
            {
                // Operand是函数名，Eval返回环境中查找到的Function函数对象
                // Operand是类名，Eval返回环境中查找到的ClassInfo函数对象
                // Operand是实例名，Eval返回环境中查找到的StoneObject函数对象
                return Operand.Eval(env);
            }
        }

        public Postfix GetPostfix(int nest)
        {
            // 嵌套参数调用，从右向左，先把右侧的参数压栈存起来，最先处理的是左侧第一个调用
            return (Postfix)Children[Children.Count - 1 - nest];
        }

        public bool HasPostfix(int nest)
        {
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
                throw new StoneException("NegativeExpr: not int", this);
        }
    }

    public class BlockStatement : ASTList
    {
        public BlockStatement(List<ASTree> list) : base(list)
        {

        }

        public override object Eval(Env env)
        {
            object result = null;
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
}
