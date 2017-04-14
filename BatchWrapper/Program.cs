using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BatchWrapper
{
    class Program
    {
        private static string s_resourcePrefix = "BatchWrapper.Scripts.";

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }

            if (args[0].ToLower() == "-list")
            {
                ListAll();
            }
            else if (args[0].ToLower() == "-run")
            {
                string scriptName = args[1];
                string scriptArgs = string.Empty;

                if (args.Length > 2)
                {
                    scriptArgs = string.Join(" ", args.Skip(2).ToArray());
                }

                RunScript(scriptName, scriptArgs);
            }
            else if (args[0].ToLower() == "-extract")
            {
                string scriptName = args[1];
                string destFolder = args[2];

                ExtractScript(scriptName, destFolder);
            }
        }

        static void PrintUsage()
        {
            Console.WriteLine("BatchWrapper.exe -list");
            Console.WriteLine("BatchWrapper.exe -extract SCRIPT_NAME");
            Console.WriteLine("BatchWrapper.exe -run SCRIPT_NAME ARGS");
        }

        static void ListAll()
        {
            string[] resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();

            foreach (string resourceName in resourceNames)
            {
                Console.WriteLine(resourceName.Substring(s_resourcePrefix.Length));
            }
        }

        static void ExtractScript(string name, string destination)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            string[] resourceNames = asm.GetManifestResourceNames();
            string path = Path.Combine(destination, name);
            string fullname = (s_resourcePrefix + name).ToLower();
            bool found = false;

            foreach (string resourceName in resourceNames)
            {
                if (resourceName.ToLower() == fullname)
                {
                    using (Stream resourceStream = asm.GetManifestResourceStream(resourceName))
                    using (Stream fileStream = File.OpenWrite(path))
                    {
                        resourceStream.CopyTo(fileStream);
                    }

                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Console.WriteLine($"Couldn't find script with name '{name}'");
            }
        }

        static void RunScript(string name, string args)
        {
            string path = Path.GetTempPath();

            ExtractScript(name, path);
            ProcessStartInfo psi = new ProcessStartInfo(Path.Combine(path, name), args);
            Process proc = Process.Start(psi);
            proc.WaitForExit();
        }
    }
}
