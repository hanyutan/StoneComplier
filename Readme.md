# Stone Complier
《两周自制脚本语言》千叶滋 著，C# 实现版本

 ## 流程
 * 1.词法分析（正则匹配拆分）
 * 2.语法分析（使用BNF来表示语法，查找模式匹配，一边获取单词一边构造抽象语法树，根据语法规则将stone语言改写为C#代码）
 * 3.解释器（从抽象语法树的根节点遍历到叶节点，计算各个节点的内容）
 * 4.添加函数
 *   - 扩充BNF语法规则
 *   - 明确变量的时间范围——生存周期，和空间范围——作用域
 *   - 闭包是一种特殊的匿名函数，可以实现把函数赋值给变量，或者作为参数传递给其他函数
 * 5.基于类的面向对象
 *   - 仅支持单一继承
 *   - 由于不含静态类型，无法使用接口的概念
 *   - 不支持定义带参构造函数
 *   - 无法显示定义构造函数，对象一旦创建，就会从上往下依次执行大括号中的类定义语句，这就作为stone语言的"构造函数"
 *   - {}之间可以出现def语句或者赋值表达式，如果赋值对象不是已有的全局变量，则将其视为类新添加的字段
 *   - 支持继承，用extends标识
 *   - 不支持方法重载，同一个类中无法定义参数个数或类型不同的同名方法
 *   - stone语言的字段与方法之间没有明确的区分，方法是一种以Function对象为值的字段


## 概念
 * 词法分析器 lexical analyzer / lexer / scanner
 * 语法分析器 parser
 * 单词 token
 * BNF（一种表示语法的范式）举例：四则运算表达式的语法规则
 *     factor:     NUMBER | "(" expression ")"      factor由单独的整型字面量构成，或者由括号包起的expression构成（注：这里体现了递归循环）
 *     term:       factor { ( "*" | "/" ) factor }  term可以由单独的factor构成，也可以后续再接乘除另一个或多个factor项（注：大括号表示某模式至少出现0次，中括号表示某模式出现0次或1次，括号将里面的内容当作一个完整的模式）
 *     expression: term { ( "+" | "-" ) term }      expression可以由单独的term构成，也可以后续再接加减另一个或多个term项（注：规则里还内含了加减乘除的优先级）

## 大纲
### 1. 基础
- √ 语法功能，整数四则运算，字符串处理，支持变量，if while基本控制语句，动态数据类型，支持注释
- √ 词法分析器，正则库
- √ 抽象语法树，BNF
- √ 语法解释器，解析器组合子库
- √ 设计基本的解释器，GluonJ

### 2. 增强
- √ 增加static方法调用支持
- 增加类与对象的语法，闭包实现
- 增加数组功能
- √ 增强解释器功能，能执行函数、支持闭包语法

### 3. 性能优化
- 优化访问变量性能，搜索id而不是变量名
- 优化调用对象和字段的性能，搜索id而不是变量名，增加内联缓存
- 虚拟机 中间代码
- 支持静态数据类型，增加类型检查

### 4. 高级
- 手工设计词法分析器，正则匹配
- √ 语法分析基本算法，LL语法
- √ 解析器组合子库的源码分析
- GluonJ注意事项
- 抽象语法树，节点对象的类会包含各种类型的方法，可以用其他设计模式实现而不用GluonJ