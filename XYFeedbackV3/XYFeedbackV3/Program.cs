using System;
using System.Threading;

namespace XYFeedbackV3
{
    class Program
    {
        static NIDaq daq;
        static CsvWriter writer;

        static void Main(string[] args)
        {
            String c = "";
            String[] header = { "TraLoc", "LatLoc" };

            daq = new NIDaq();

            writer = new CsvWriter(",", "Location.csv");
            writer.WriteHeader(header);

            Console.WriteLine("Press enter key to stop");

            do
            {

                writer.AddDoubleArray(daq.ReadUSBDa());


                if (Console.KeyAvailable)
                {
                    c = Console.ReadKey().Key.ToString();
                }

                Thread.Sleep(500);

            } while (c != ConsoleKey.Enter.ToString());

        }
    }
}