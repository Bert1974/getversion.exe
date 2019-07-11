using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace launch32
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length >= 1)
            {
                try
                {
                    var a = Assembly.Load(args[0]);
                    if (a.EntryPoint.ReturnType == typeof(int))
                    {
                        return (int)a.EntryPoint.Invoke(null, new object[] { args.Skip(1).ToArray() });
                    }
                    else
                    {
                        a.EntryPoint.Invoke(null, new object[] { args.Skip(1).ToArray() });
                        return 0;
                    }
                }
                catch (Exception e)
                {
                }
            }
            return 1;
        }
    }
}
