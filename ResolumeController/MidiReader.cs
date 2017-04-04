using Sanford.Multimedia;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResolumeController
{
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
            switch (e.Message.Command)
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

}
