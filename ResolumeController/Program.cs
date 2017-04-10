using Rug.Osc;
using Sanford.Multimedia;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ResolumeController
{

    class Program
    {
        private static Resolume _resolume;
        private static MyMidiClass _abletonIn;


        static void Main(string[] args)
        {
            var options = new  ProgramOptions();
            args = new string[] { "-i", "7001", "-q", "dfd" };
            if (!CommandLine.Parser.Default.ParseArguments(args, options))
            {
                foreach (var error in options.LastParserState.Errors)
                {
                    Console.WriteLine(error.BadOption.LongName);
                }
            }
            if (options.Verbose)
            {
                Console.WriteLine("InputPort: {0}", options.ResolumeInputPort);
            }

            foreach (var deviceName in MyMidiClass.GetInputDevices())
            {
                Console.WriteLine(deviceName);
            }

            _abletonIn = new MyMidiClass(6);


            _resolume = new Resolume(7001, IPAddress.Loopback, 7000);
            _resolume.ValueChanged += OnResolumeValueChanged;

            _resolume.Start();

            _abletonIn.ChannelPressureChanged += AbletonChannelPressureChanged;
            _abletonIn.NoteChanged += AbletonNoteChanged;

            bool exit = false;
            do
            {
                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.D1:
                        _resolume.SendValue("/activelayer/name", "GRReerRR");
                        break;
                    case ConsoleKey.D2:
                        _resolume.SendValue("/activelayer/opacityandvolume", (Single)0.3333);
                        break;

                    case ConsoleKey.Escape:
                        exit = true;
                        break;
                }
            }
            while (!exit);

            Console.WriteLine("Stopping listeners...");
            _resolume.ValueChanged -= OnResolumeValueChanged;
            _resolume.Stop();
            Console.WriteLine("Stopped.");

            Environment.Exit(0);
        }

        private static void AbletonNoteChanged(int channel, Note note, int octave, int velocity)
        {

            if (channel == 1 && octave == 3 && velocity > 0) // Drum rack, default set
            {
                switch (note)
                {
                    case Note.C: // Kick
                        _resolume.SetMutator("/layer3/link1/values", new EnvelopeMutator((Single)velocity / 128, 350));
                        break;
                    case Note.CSharp: // Snare
                        _resolume.SetMutator("/layer3/link2/values", new EnvelopeMutator((Single)velocity / 128, 400));
                        break;
                    case Note.D: // HH
                        _resolume.SetMutator("/layer3/link3/values", new EnvelopeMutator((Single)velocity / 128, 100));
                        break;
                }

            }


            switch(channel)
            {
                default:
                    Console.ForegroundColor = velocity > 0 ? ConsoleColor.Green : ConsoleColor.DarkGreen;
                    var logMessage = $"Note > {note} (Velocity: {velocity}, CH#{channel}).";
                    Console.WriteLine(logMessage);
                    break;
            }
        }

        private static void AbletonChannelPressureChanged(int channel, int channelPressure)
        {
            if (channel == 0)
            {
                Single v = (Single)channelPressure / (Single)128;
                _resolume.SendValue("/activelayer/opacityandvolume", v);
            }
        }

        private static void OnResolumeValueChanged(string oscPath, ResolumeValue value)
        {
            switch (oscPath)
            {
                default:

                    break;
            }
            if (value.UpdateCount < 10  )
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"{oscPath} > {value}");
            }
        }

    }
}