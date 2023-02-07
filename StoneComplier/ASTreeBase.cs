using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StoneComplier
{
    public enum ASTNodeType
    {
        Root,
        Branch,
        Leaf,

    }

    public class ASTree
    {
        // 抽象语法树，用于表示程序结构的树形结构
        // 这个类其实代表节点node，最终一棵树记录的就是一个根节点

        protected ASTNodeType type = ASTNodeType.Root;
        public ASTNodeType Type
        {
            get
            {
                return type;
            }
        }
        
        public virtual string GetLocation()
        {
            // 返回一个字符串，描述抽象语法树节点在程序内所处位置
            return "";
        }
        
        public override string ToString()
        {
            return "";
        }
    }

    public class ASTBranch: ASTree
    {
        // 语法树的中间节点
        public List<ASTree> Children = new List<ASTree>();

        public ASTBranch(List<ASTree> list)
        {
            type = ASTNodeType.Branch;
            Children = list;
        }

        public override string GetLocation()
        {
            // 返回一个字符串，描述抽象语法树节点在程序内所处位置
            foreach(var child in Children)
            {
                string loc = child.GetLocation();
                if (loc != "")
                    return loc;
            }
            return "";
        }

        public override string ToString()
        {
            string result = "(";
            string sep = "";
            foreach(var child in Children)
            {
                result += sep;
                result += child.ToString();
                sep = ", ";
            }
            result += ")";
            return result;
        }
    }

    public class ASTLeaf: ASTree
    {
        // 语法树的叶子节点
        protected Token token;   // 这里规定叶子节点必须与对应的token相关联

        public ASTLeaf(Token t)
        {
            type = ASTNodeType.Leaf;
            token = t;
        }

        public override string GetLocation()
        {
            // 返回一个字符串，描述抽象语法树节点在程序内所处位置
            return $"at line {token.LineNumber}";
        }

        public override string ToString()
        {
            return token.GetText();
        }

        public TokenType GetTokenType()
        {
            return token.Type;
        }
    }

}
