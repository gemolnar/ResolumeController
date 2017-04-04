using Rug.Osc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ResolumeController
{
    public delegate void ChangedEventHandler(string oscPath, object value);
    


    public class Resolume
    {
        private readonly OscReceiver _resolumeOscReceiver;
        private readonly Thread _listenerThread;
        private readonly ResolumeValueCollection _values = new ResolumeValueCollection();

        public ChangedEventHandler ValueChanged;

        public Resolume(int listenOnPort)
        {
            
            _resolumeOscReceiver = new OscReceiver(listenOnPort);
            _listenerThread = new Thread(new ThreadStart(ListenLoop));
        }

        public void Start()
        {
            _resolumeOscReceiver.Connect();
            _listenerThread.Start();
        }

        public void Stop()
        {
            _resolumeOscReceiver.Close();
            _listenerThread.Join();
        }

        public ResolumeValue GetValue(string oscPath)
        {
            ResolumeValue v;
            if (_values.TryGetValue(oscPath, out v))
            {
                return v;
            }
            else
            {
                return null;
            }
        }

        public void SetValue(string oscPath, ResolumeValue value)
        {
            _values[oscPath] = value;
            ValueChanged?.Invoke(oscPath, value);
        }


        private static ResolumeValue GenerateNewResolumeValue(object argument)
        {
            ResolumeValue generatedResolumeValue;
            if (argument is string)
            {
                generatedResolumeValue = new ResolumeValue<String>((string)argument);
            }
            else if (argument is int)
            {
                generatedResolumeValue = new ResolumeValue<Int32>((int)argument);
            }
            else if (argument is Single)
            {
                generatedResolumeValue = new ResolumeValue<Single>((float)argument);
            }
            else
            {
                throw new ApplicationException($"Type not supported: {argument.GetType().FullName}");
            }
            return generatedResolumeValue;
        }

        private void UpdateExistingResolumeValue(object argument, ResolumeValue oldValue)
        {
            if (argument is string)
            {
                (oldValue as ResolumeValue<string>).Value = (string)argument;
            }
            else if (argument is int)
            {
                (oldValue as ResolumeValue<int>).Value = (Int32)argument;
            }
            else if (argument is Single)
            {
                (oldValue as ResolumeValue<Single>).Value = (Single)argument;
            }
            else
            {
                throw new ApplicationException($"Type not supported: {argument.GetType().FullName}");
            }
        }

        public static object ExtractArgumentFromMessage (OscMessage message)
        {
            var arguments = message.ToArray();
            if (arguments.Length != 1)
                throw new InvalidOperationException($"Only 1 argument per message is supported, {message.Address} contained {arguments.Length}.");
            return arguments[0];
        }

        private void ListenLoop()
        {
            try
            {
                while (_resolumeOscReceiver.State != OscSocketState.Closed)
                {
                    // if we are in a state to recieve
                    if (_resolumeOscReceiver.State == OscSocketState.Connected)
                    {
                        // get the next message, this will block until one arrives or the socket is closed
                        OscBundle packet = _resolumeOscReceiver.Receive() as OscBundle;
                        foreach (OscMessage message in packet)
                        {
                            object argument = ExtractArgumentFromMessage(message);
                            ResolumeValue resolumeValue;
                            if (!_values.TryGetValue(message.Address, out resolumeValue))
                            {
                                resolumeValue = GenerateNewResolumeValue(argument);
                                _values[message.Address] = resolumeValue;
                            }
                            else
                            {
                                UpdateExistingResolumeValue(argument, resolumeValue);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // if the socket was connected when this happens
                // then tell the user
                if (_resolumeOscReceiver.State == OscSocketState.Connected)
                {
                    Console.WriteLine("Exception in listen loop");
                    Console.WriteLine(ex.Message);
                }
            }
        }


    }
}
