using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace getversion
{
    class Program
    {
        static void Help()
        {
            Console.WriteLine("project-page: https://github.com/Bert1974/getversion.exe");
            Console.WriteLine("usage: getversion.exe ({-replace searchfor replacewith}, ..) (-version_ext \"\") (-assembly {version-assembly-file}) {inutfile} ({ outputfile})");
        }

        static int Main(string[] args)
        {
            if (args.Contains("?"))
            {
                Help();
                return 0;
            }
            string versionext = "", verfile = null;

            Dictionary<string, string> replace = new Dictionary<string, string>();

            int pos = 0;

            // match options
            while (pos < args.Length && args[pos].StartsWith("-"))
            {
                switch (args[pos].Substring(1))
                {
                    case "version_ext":
                    case "assembly":
                        if (pos + 1 < args.Length)
                        {
                            switch (args[pos++].Substring(1))
                            {
                                case "version_ext": versionext = args[pos++]; break;
                                case "assembly": verfile = args[pos++]; break;
                            }
                        }
                        else
                        {
                            Console.Error.WriteLine($"not enough parameters for {args[pos]}");
                            return 1;
                        }
                        break;
                    case "replace":
                        if (pos + 2 < args.Length)
                        {
                            replace[args[pos + 1]] = args[pos + 2];
                            pos += 3;
                        }
                        else
                        {
                            Console.Error.WriteLine("not enough parameters for replace");
                            return 1;
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
            // get input and out file
            if (pos < args.Length)
            {
                string infile = Path.Combine(Environment.CurrentDirectory, args[pos++]);
                string content;
                if (!File.Exists(infile))
                {
                    Console.Error.WriteLine($"input-file not found '{infile }'");
                    return 1;
                }
                try
                {
                    content = File.ReadAllText(infile);//.Replace(matchstr, $"{version}{versionext}");
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"Error '{e.Message}' reading '{infile}'");
                    return 1;
                }
                // string content is file to convert
                if (verfile!=null)
                {
                    verfile = Path.Combine(Environment.CurrentDirectory, verfile);
                    if (!File.Exists(verfile))
                    {
                        Console.Error.WriteLine($"version-assembly not found '{verfile}'");
                        return 1;
                    }
                    try
                    {
                        var a = Assembly.LoadFile(verfile);

                        content = ApplyAttribute(a, content, typeof(AssemblyCopyrightAttribute), "$copyright$", "Copyright");
                        content = ApplyAttribute(a, content, typeof(AssemblyDescriptionAttribute), "$description$", "Description");
                        content = ApplyAttribute(a, content, typeof(AssemblyTitleAttribute), "$title$", "Title");
                        content = ApplyAttribute(a, content, typeof(AssemblyTitleAttribute), "$id$", "Title");
                        content = content.Replace("$version$", $"{a.GetName().Version}{versionext}");
                    }
                    catch
                    {
                        Console.Error.WriteLine($"error loading version-assembly '{verfile}'");
                        return 1;
                    }
                }
                // replace keyvalues from -replace option
                foreach (string searchfor in replace.Keys)
                {
                    content = content.Replace(searchfor, replace[searchfor]);
                }

                string outfile;
                if (pos < args.Length)
                {
                    // last parameter should be outfile
                    outfile = Path.Combine(Environment.CurrentDirectory, args[pos++]);

                    if (pos != args.Length)
                    {
                        Console.Error.WriteLine($"too many arguments");
                        return 1;
                    }
                }
                else
                {
                    // only input file, change extension (toggle _ at start of it)
                    var ext = Path.GetExtension(infile);

                    if (ext.StartsWith("_"))
                    {
                        outfile = Path.Combine(Path.GetDirectoryName(infile), $"{Path.GetFileNameWithoutExtension(infile)}.{ext.Substring(1)}");
                    }
                    else
                    {
                        outfile = Path.Combine(Path.GetDirectoryName(infile), $"{Path.GetFileNameWithoutExtension(infile)}._{ext}");
                    }
                }
                try
                {
                    File.WriteAllText(outfile, content);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"Error '{e.Message}' writing '{outfile}'");
                    return 1;
                }
            }
            if (pos == 0)
            {
                // no arguments
                Help();
                return 1;
            }
            return 0;
        }

        private static string ApplyAttribute(Assembly assembly, string content, Type attributetype, string searchfor, string propertyname)
        {
            object t = assembly.GetCustomAttributes(attributetype, true).FirstOrDefault();

            if (t != null)
            {
                string text = (string)t.GetType().GetProperty(propertyname).GetValue(t, new object[0]);
                return content.Replace(searchfor, text);
            }
            return content;
        }
    }
}
