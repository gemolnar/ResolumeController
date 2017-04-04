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
    //class OscState
    //{
    //    List<object> _values = new List<object>();
    //    public List<object> Values {  get { return _values; } }
    //    public int Count { get; set; }
    //}


    class MidiReader : IDisposable
    {
        private InputDevice _inDevice = null;

        public MidiReader(int midiDeviceId)
        {
            _inDevice = new InputDevice(midiDeviceId);
            _inDevice.ChannelMessageReceived += OnChannelMessageReceived;
            _inDevice.MessageReceived += OnMessageReceived;
            _inDevice.ShortMessageReceived += OnShortMessageReceived;
            _inDevice.StartRecording();
        }

        private void OnShortMessageReceived(object sender, ShortMessageEventArgs e)
        {
            var logMessage = $"{e.Message.Message} (ShortMessage of type {e.Message.MessageType}, status {e.Message.Status}).";
            //Console.WriteLine(logMessage);
        }

        private void OnMessageReceived(IMidiMessage message)
        {
        }

        private void OnChannelMessageReceived(object sender, ChannelMessageEventArgs e)
        {
            string logMessage;
            switch(e.Message.Command)
            {
                case ChannelCommand.NoteOn:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Note n = (Note)(e.Message.Data1 % 12);
                    logMessage = $"NoteOn >{n} (Velocity: {e.Message.Data2}, CH#{e.Message.MidiChannel}).";
                    break;
                case ChannelCommand.NoteOff:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    logMessage = $"NoteOff>{e.Message.Data1} (Velocity: {e.Message.Data2}, CH#{e.Message.MidiChannel}).";
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    logMessage = $"CH#{e.Message.MidiChannel} Command: {e.Message.Command}, Data1: {e.Message.Data1}, Data2: {e.Message.Data2}).";
                    break;
            }
            Console.WriteLine(logMessage);
        }

        public static IEnumerable<string> GetDevices()
        {
            for (int deviceIndex = 0; deviceIndex < InputDevice.DeviceCount; deviceIndex++)
            {
                var deviceInfo = InputDevice.GetDeviceCapabilities(deviceIndex);
                yield return $"[ID={deviceIndex}] \"{deviceInfo.name}\", V{deviceInfo.driverVersion / 256}.{deviceInfo.driverVersion % 256}";
            } 
        }

        public void Dispose()
        {
            _inDevice.StartRecording();
            _inDevice.Dispose();
        }
    }

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
