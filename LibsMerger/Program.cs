using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LibsMerger
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            // if (args.Length != 2)
            // {
            //     Console.WriteLine($"{args.Length} arguments detected, you can only use 2");
            //     return;
            // }
            //
            // string directory1 = args[0];
            // string directory2 = args[1];

            string directory1 = "D:\\Guillem\\SDKsUpdate\\11.1\\9.Resolved_Empty_Appsflyer";
            string directory2 = "D:\\Guillem\\SDKsUpdate\\11.1\\Test\\Plugins\\Android-Google";
            
            LibrariesMerger merger = new(directory1, directory2);
            merger.Run();
        }
    }
}