using System;

namespace HangFireExample
{
    public class MyBackgroundJob
    {
        public static void Execute()
        {
            Console.WriteLine("Background job executed!");
        }
    }
}