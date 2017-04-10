using Sanford.Multimedia;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResolumeController
{

    public static class MidiState
    {
        public static readonly int[,,] Current = new int[16, 12, 11];

        public static int GetValue(this int[,,] noteStatuses, int channel, Note note, int octacve)
        {
            return noteStatuses[channel, (int)note, octacve];
        }

        public static void SetValue(this int[,,] noteStatuses, int channel, Note note, int octacve, int value)
        {
            noteStatuses[channel, (int)note, octacve] = value;
        }

    }

    public delegate void ChannelPressureChangedEventHandler(int channel, int channelPressure);
    public delegate void NoteChangedEventHandler(int channel, Note note, int octave, int velocity);

    class MyMidiClass : IDisposable
    {

        private InputDevice _inDevice = null;
        private OutputDevice _outDevice = null;

        public event ChannelPressureChangedEventHandler ChannelPressureChanged;
        public event NoteChangedEventHandler NoteChanged;

        public MyMidiClass(int midiDeviceId)
        {
            _inDevice = new InputDevice(midiDeviceId);
            _inDevice.ChannelMessageReceived += OnChannelMessageReceived;
            _inDevice.ShortMessageReceived += OnShortMessageReceived;
            _inDevice.StartRecording();
        }

        private void OnShortMessageReceived(object sender, ShortMessageEventArgs e)
        {

            var logMessage = $"{e.Message.Message} (ShortMessage of type {e.Message.MessageType}, status {e.Message.Status}).";
            //Console.WriteLine(logMessage);
        }

        private void OnChannelMessageReceived(object sender, ChannelMessageEventArgs e)
        {
            string logMessage = null;
            Note n = (Note)(e.Message.Data1 % 12);
            int octave = e.Message.Data1 / 12;
            switch (e.Message.Command)
            {
                case ChannelCommand.NoteOn:
                    MidiState.Current.SetValue(e.Message.MidiChannel, n, octave, e.Message.Data2);
                    NoteChanged?.Invoke(e.Message.MidiChannel, n, octave, e.Message.Data2);
                    break;
                case ChannelCommand.NoteOff:
                    MidiState.Current.SetValue(e.Message.MidiChannel, n, octave, 0);
                    NoteChanged?.Invoke(e.Message.MidiChannel, n, octave, e.Message.Data2);
                    break;
                case ChannelCommand.ChannelPressure:
                    Console.ForegroundColor = ConsoleColor.DarkBlue;
                    logMessage = $"Aftertouch> {e.Message.Data1}, CH#{e.Message.MidiChannel}).";
                    ChannelPressureChanged?.Invoke(e.Message.MidiChannel, e.Message.Data1);
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    logMessage = $"CH#{e.Message.MidiChannel} Command: {e.Message.Command}, Data1: {e.Message.Data1}, Data2: {e.Message.Data2}).";
                    break;
            }
            if (logMessage != null)
                Console.WriteLine(logMessage);
        }

        public static IEnumerable<string> GetInputDevices()
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
            _outDevice.Dispose();
        }
    }

}
