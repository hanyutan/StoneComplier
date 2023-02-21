using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneComplier
{
    // partial分部类，分开维护
    public partial class ASTree
    {
        public virtual object Eval(Env env)
        {
            // 会递归调用子节点的eval方法
            throw new StoneException($"Eval failed: {ToString()}");
        }
    }

    public partial class ASTList : ASTree
    {
        public override object Eval(Env env)
        {
            throw new StoneException($"Eval failed: {ToString()}");
        }
    }

    public partial class ASTLeaf : ASTree
    {
        public override object Eval(Env env)
        {
            throw new StoneException($"Eval failed: {ToString()}");
        }
    }

    public partial class NumLiteral : ASTLeaf
    {
        public override object Eval(Env env)
        {
            return Value;
        }
    }

    public partial class StringLiteral : ASTLeaf
    {
        public override object Eval(Env env)
        {
            return Value;
        }
    }

    public partial class DefName : ASTLeaf
    {
        public override object Eval(Env env)
        {
            return env.Get(Value);
        }
    }

    public partial class BinaryOp : ASTList
    {
        public override object Eval(Env env)
        {
            if(Operator == "=")
                return ComputeAssign(env);
            else
                return ComputeOp(env);
        }

        object ComputeAssign(Env env)
        {
            object rvalue = Right.Eval(env);
            if (Left is DefName)
            {
                string name = ((DefName)Left).Value;
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
            switch(op)
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

    public partial class PrimaryExpr : ASTList
    {
        public override object Eval(Env env)
        {
            return null;
        }
    }

    public partial class NegativeExpr : ASTList
    {
        public override object Eval(Env env)
        {
            ASTree operand = Operand();
            object result = operand.Eval(env);
            if (result is int)
                return -(int)result;
            else
                throw new StoneException("NegativeExpr: not int");
        }
    }

    public partial class BlockStatement : ASTList
    {
        public override object Eval(Env env)
        {
            object result = 0;
            // 返回最后一个语句的值
            foreach(ASTree child in Children)
            {
                if(child is not NullStatement)
                    result = child.Eval(env);
            }
            return result;
        }
    }

    public partial class IfStatement : ASTList
    {
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

    public partial class WhileStatement : ASTList
    {
        public override object Eval(Env env)
        {
            object result = 0;
            for(; ; )
            {
                object cond = Condition.Eval(env);
                if (cond is int && (int)cond != 0)
                    result = Body.Eval(env);    // 条件满足就继续算
                else
                    return result;   // 条件不满足就返回最后一个值
            }
        }
    }

    public partial class NullStatement : ASTList
    {
        public override object Eval(Env env)
        {
            return null;
        }
    }
}
