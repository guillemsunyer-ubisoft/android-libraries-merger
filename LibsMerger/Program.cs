using System;

namespace LibsMerger
{
    internal static class Program
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

            string directory1 = "D:\\Guillem\\SDKsUpdate\\feature_11.1.3_FirebaseUpdate\\1.Liftoff";
            string directory2 = "D:\\Guillem\\SDKsUpdate\\feature_11.1.3_FirebaseUpdate\\2.Game\\Plugins\\Android-Google";
            
            LibrariesMerger merger = new(directory1, directory2);
            merger.Run();
        }
    }
}