# Stone Complier
《两周自制脚本语言》千叶滋 著，C# 实现版本

## 流程
* 1.词法分析
    - 正则匹配拆分
* 2.语法分析
    - 使用BNF来表示语法
    - 查找模式匹配，一边获取单词一边构造抽象语法树，根据语法规则将stone语言改写为C#代码
* 3.解释器
    - 从抽象语法树的根节点遍历到叶节点，计算各个节点的内容
* 4.添加函数
    - 扩充BNF语法规则
    - 暂且规定stone语言的函数必定有返回值，没有return语句，所以就把最后一句的结果返回
    - def语句仅能用于最外层代码，暂不支持函数的嵌套定义：不支持用户在代码块block中定义函数
    - 明确变量的时间范围——生存周期，和空间范围——作用域。  
    变量的作用域：通常由嵌套结构实现，需要为每一种作用域准备一个单独的环境，并根据需要来嵌套环境。  
    查找变量时，程序首先查找与最内层作用域对应的环境，如果没找到再向外逐层查找。  
    stone语言中目前不支持在函数内嵌套定义函数，也不考虑{}代码块的独立作用域，因此其始终只有2个作用域：全局、局部。  
    变量的生存周期可以通过环境对象的创建及清除时机来控制
    - 闭包是一种特殊的匿名函数，可以实现把函数赋值给变量，或者作为参数传递给其他函数

* 5.基于类的面向对象
    - 仅支持单一继承
    - 由于不含静态类型，无法使用接口的概念
    - 不支持定义带参构造函数
    - 无法显示定义构造函数，对象一旦创建，就会从上往下依次执行大括号中的类定义语句，这就作为stone语言的"构造函数"
    - {}之间可以出现def语句或者赋值表达式，如果赋值对象不是已有的全局变量，则将其视为类新添加的字段
    - 支持继承，用extends标识
    - 类似支持方法覆盖，因为子类方法被添加到环境中在父类方法之后，但这样做会导致无法调用被覆盖的父类方法。
    - 不支持方法重载，同一个类中无法定义参数个数或类型不同的同名方法
    - stone语言的字段与方法之间没有明确的区分，方法是一种以Function对象为值的字段
    - 采用闭包的形式来表现StoneObject对象的内部结构，即利用环境能保存字段值的特性来表示对象。将对象视作一种环境，就很容易实现对该对象自身(this)的方法调用与变量访问，指代自身的this可省略。所谓“采用闭包的形式”，可这样理解：将成员函数引用的成员变量视为“自由变量”，实例化时构造函数会将变量与方法记录进env（相当于定义闭包），等实际调用实例的方法时，就会去定义时的环境来读写变量。
    - 对于类的成员函数，会有3层嵌套环境：最内层的用于记录参数和局部变量；外一层用于记录StoneObject对象的字段与方法；最外层用于记录定义类时的全局变量。

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
- √ 增加类与对象的语法，闭包实现
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

## Stone语言的BNF语法规则
### 基础语法规则
不考虑优先级，用其他方法处理  
* `primary: "(" expr ")" | NUMBER | IDENTIFIER | STRING`  
基本元素：括号括起的表达式、整型字面量、标识符、字符串字面量  
* `factor: "-" primary | primary`  
todo 感觉可以和primary中的NUMBER合并到一起，... | ["-"] NUMBER | ...
* `expr: factor { OP factor }`  
双目运算符连接的两侧
* `block: "{" [statement] {(";" | EOL) [statement]} "}"`  
由大括号括起来的statement语句序列，语句之间用分块或换行符分割，支持空语句  
todo 为啥不用{[statement] (";" | EOL)}来表示？代码块中最后一句可以省略分号或换行符
* `simple: expr`  
简单语句
* `statement: "if" expr block ["else" block] | "while" expr block | simple`  
可以是if语句、wile语句、或者简单表达式语句
* `program: [statement] (";" | EOL)`  
一行stone语言程序，可以表示空行  
todo 怎样区分一行program和statement？program既可以是处于代码块之外的一条语句，也可以是一行完整的程序

### 函数相关的语法规则
* `param: IDENTIFIER`  
定义时的形参，倒是不需要指定类型，直接写个变量名就好
* `params: param { "," param }`
参数之间以逗号分隔
* `param_list: "(" [params] ")"`  
定义时用括号括起来，但可以没有参数
* `def: "def" IDENTIFIER param_list block`  
中间IDENTIFIER是函数名
* `args: expr { "," expr }`  
调用时的实参
* `postfix: "(" [args] ")"`  
调用时用括号括起的实参列表，叫做postfix是因为以后还要扩充，去代表不同的类似后缀一样的情况  

以下与原有不同：
* `primary: ( "(" expr ")" | NUMBER | IDENTIFIER | STRING ) { postfix }`  
todo 应该只有IDENTIFIER后面有可能加{ postfix }吧？为啥要统一放在全体的末尾
* `simple: expr [ args ]`  
当语句中只含有一个函数调用时，可以不加括号传参
* `program: [def | statement] (";" | EOL)`

### 闭包的语法规则
* `primary: "fun" param_list block | 原先的primary定义`

### 类的语法规则
* `member: def | simple`
* `class_body: "{" [member] { (";" | EOL) [member]} "}"`
* `def_class: "class" IDENTIFIER [ "extends" IDENTIFIER ] class_body`
* `postifx: "." IDENTIFIER | "(" [args] ")"`  
不仅能表示实参序列，还支持基于句点.来调用类的字段与方法
* `program: [def_class | def | statement] (";" | EOL)`

## 数据类型

环境中可以记录的名值对有哪些：
* 整数值 int对象
* 字符串 string对象
* 函数 Function对象
* 原生函数 NativeFunction对象
* 类定义 ClassInfo对象
* Stone语言的对象 StoneObject对象
