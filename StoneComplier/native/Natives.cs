using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace StoneComplier
{
    public class Natives
    {
        public static void ToNativeEnv(Env env)
        {
            // Natives里有很多static方法，其内部调用了c#原生方法，然后添加env时get Natives method即可
            // 不一定要反射直接拿到c#函数
            AppendNativeFunction(env, "print");
            AppendNativeFunction(env, "read");
            AppendNativeFunction(env, "length");
            AppendNativeFunction(env, "to_int");
            AppendNativeFunction(env, "time_start");
            AppendNativeFunction(env, "time_end");
        }

        public static void AppendNativeFunction(Env env, string func_name)
        {
            MethodInfo method = null;
            try
            {
                method = typeof(Natives).GetMethod(func_name);
            }
            catch
            {
                throw new StoneException($"Natives: can not find the native function {func_name}");
            }

            env.Put(func_name, new NativeFunction(func_name, method));
        }

        public static void print(object obj)
        {
            Console.WriteLine(obj.ToString());
        }

        public static string read()
        {
            // todo 输入字符串
            return null;
        }

        public static int length(string s)
        {
            return s.Length;
        }

        public static int to_int(object value)
        {
            if (value is string)
                return int.Parse((string)value);
            else if (value is int)
                return (int)value;
            else
                throw new StoneException($"to_int failed: value = {value}");

        }

        public static DateTime start_time;

        public static void time_start()
        {
            start_time = DateTime.Now;
        }

        public static void time_end()
        {
            var cost_time = (DateTime.Now - start_time).TotalMilliseconds;
            Console.WriteLine($"cost time = {cost_time / 1000}s");
        }
    }
}
