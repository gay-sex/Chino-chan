using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chino_chan.Modules
{
    public class Updater
    {
        CSharpCodeProvider Provider;
        CompilerParameters Parameters;

        public Updater()
        {
            Provider = new CSharpCodeProvider();
            Parameters = new CompilerParameters()
            {
                OutputAssembly = "",
                GenerateExecutable = true
            };
        }

        private bool Compile()
        {
            var FileNames = Directory.EnumerateFiles("GitUpdate", "*.cs").ToArray();
            var Result = Provider.CompileAssemblyFromFile(Parameters, FileNames);

            if (Result.Errors.Count > 0)
            {
                foreach (CompilerError Error in Result.Errors)
                {
                    Global.Logger.Log(ConsoleColor.Red, LogType.Updater, "Compile Error",
                        "Line number " + Error.Line +
                        ", Error Number: " + Error.ErrorNumber +
                        ", '" + Error.ErrorText + ";" + Environment.NewLine);
                }
                return false;
            }
            return true;
        }
    }
}
