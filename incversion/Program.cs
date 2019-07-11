using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace incversion
{
    class Program
    {
        static void Help()
        {
            Console.WriteLine("project-page: https://github.com/Bert1974/getversion.exe");
            Console.WriteLine("usage: incversion.exe ({-inc major/minor/revision/build}) {AssemblyInfo.cs}");
        }
        static int Main(string[] args)
        {
            int pos = 0;

            if (args.Contains("?"))
            {
                Help();
                return 0;
            }

            string vertype = "build";

            // match options
            while (pos < args.Length && args[pos].StartsWith("-"))
            {
                switch (args[pos].Substring(1))
                {
                    case "inc":
                        {
                            if (++pos < args.Length)
                            {
                                switch (args[pos])
                                {
                                    case "major":
                                    case "minor":
                                    case "revision":
                                    case "build":
                                        vertype = args[pos];
                                        break;

                                    default:
                                        Console.Error.WriteLine($"invalid argument {args[pos]}");
                                        return 1;
                                }
                                pos++;
                            }
                            else
                            {
                                Help();
                                return 1;
                            }
                        }
                        break;

                    case "help":
                        Help();
                        return 0;

                    default:
                        Console.Error.WriteLine($"invalid argument {args[pos]}");
                        return 1;
                }
            }
            if (pos == args.Length-1)
            {
                try
                {
                    var fn = Path.Combine(Environment.CurrentDirectory, args[pos]);

                    if (File.Exists(fn))
                    {
                        string[] lines = File.ReadAllLines(fn, Encoding.UTF8);

                        string oldver = null, newver = null;

                        for (int line = 0; line < lines.Length; line++)
                        {
                            if (lines[line].StartsWith("[assembly: AssemblyVersion(\""))
                            {
                                int ind = lines[line].IndexOf('"', 28);

                                if (ind != -1)
                                {
                                    oldver = lines[line].Substring(28, ind - 28);
                                    var vv = oldver.Split('.');
                                    var nn = vv.Select(_t => (_t == "*" ? -1 : int.Parse(_t))).ToArray();

                                    var tt = new string[] { "major", "minor", "revision", "build" };
                                    var t = Array.IndexOf(tt, vertype);

                                    if (t < vv.Length)
                                    {
                                        for (int njt = 0; njt <= t; njt++)
                                        {
                                            if (nn[njt] == -1)
                                            {
                                                throw new Exception("invalid version found");
                                            }
                                        }
                                        nn[t]++;
                                        for (int njt = t + 1; njt < vv.Length; njt++)
                                        {
                                            if (nn[njt] == -1)
                                            {
                                                break;
                                            }
                                            nn[njt] = 0;
                                        }
                                        newver = string.Join(".", nn.Select(_n => _n == -1 ? "*" : _n.ToString()));

                                        Console.WriteLine($"{fn}: {oldver}->{newver}");

                                        lines[line] = $"{lines[line].Substring(0, 28)}{newver}{lines[line].Substring(ind)}";
                                    }
                                    else
                                    {
                                        throw new Exception("Error parsing");
                                    }
                                }
                                else
                                {
                                    throw new Exception("Error parsing");
                                }
                            }
                        }
                        if (oldver != null)
                        {
                            for (int line = 0; line < lines.Length; line++)
                            {
                                if (lines[line].StartsWith("[assembly: AssemblyFileVersion(\""))
                                {
                                    int ind = lines[line].IndexOf('"', 32);

                                    if (ind != -1)
                                    {
                                        lines[line] = $"{lines[line].Substring(0, 32)}{newver}{lines[line].Substring(ind)}";
                                    }
                                    else
                                    {
                                        throw new Exception("Error parsing");
                                    }
                                }
                            }
                        }
                        else
                        {
                            throw new Exception("Error parsing, no AssemblyVersion found");
                        }
                        try
                        {
                            File.WriteAllLines(fn, lines, Encoding.UTF8);
                        }
                        catch (Exception e)
                        {
                            Console.Error.WriteLine($"Error '{e.Message}' saving {fn}");
                            return 1;
                        }
                    }
                    else
                    {
                        throw new FileNotFoundException(fn);
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.Message);
                    return 1;
                }
                return 0;
            }
            Help();
            return 0;
        }
    }
}
