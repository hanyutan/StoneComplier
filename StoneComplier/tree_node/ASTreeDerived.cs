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
            {
                object rvalue = Right.Eval(env);
                return ComputeAssign(env, rvalue);
            }
            else
                return ComputeOp(env);
        }

        object ComputeAssign(Env env, object rvalue)
        {
            if(Left is PrimaryExpr)
            {
                // 类的成员变量/函数赋值，调用StoneObject.Write
                PrimaryExpr primary = (PrimaryExpr)Left;
                if(primary.HasPostfix(0) && primary.GetPostfix(0) is Dot)
                {
                    object target = primary.EvalNestPostfix(env, 1);  // 可能存在table.get().next.x = 3 这种嵌套形式，target最终计算等于table.get().next
                    if (target is StoneObject)
                        return SetField((StoneObject)target, (Dot)primary.GetPostfix(0), rvalue);
                }
            }
            else if (Left is IdName)
            {
                string name = ((IdName)Left).Value;
                env.Put(name, rvalue);
                return rvalue;   // 赋值表达式的返回值
            }
            throw new StoneException("BinaryOp: ComputeAssign failed");
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
                throw new StoneException($"BinaryOp.SetField: access memeber {member} failed at {GetLocation()}");
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
