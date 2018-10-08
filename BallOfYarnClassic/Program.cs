// https://github.com/MaxxVelocity/BallOfYarnClassic.git

namespace BallOfYarnClassic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    class Program
    {
        private static bool shutdownInitiated = false;

        private static ConsoleKeyInfo lastKeyPress;

        static void Main(string[] args)
        {
            var backgroundThread = new Thread(new ThreadStart(BackgroundProcessing));


            backgroundThread.Start();

            lastKeyPress = Console.ReadKey();

            shutdownInitiated = true;           
        }

        public static void BackgroundProcessing()
        {
            Console.WriteLine("Press any key to exit.");

            while (!shutdownInitiated)
            {
                Thread.Sleep(5000);

                Console.WriteLine("Still running...");
            }

            Console.WriteLine("Shutting down background process...");

            if (lastKeyPress.KeyChar == 'n')
            {
                throw new Exception("You dropped an n bomb!");
            }
        }
    }
}
