using Rug.Osc;
using Sanford.Multimedia;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ResolumeController
{




    class Program
    {
        private static Resolume _resolume;
        private static MidiReader _abletonIn;

        static void Main(string[] args)
        {
            foreach (var deviceName in MidiReader.GetDevices())
            {
                Console.WriteLine(deviceName);
            }

            _abletonIn = new MidiReader(8);

            _resolume = new Resolume(7001);
            _resolume.Start();

            // wait for a key press to exit
            Console.WriteLine("Press any key to exit");
            Console.ReadKey(true);

            _resolume.Stop();
        }

  
    }
}
