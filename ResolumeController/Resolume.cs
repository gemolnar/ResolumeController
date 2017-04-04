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
                            var arguments = message.ToArray();
                            if (arguments.Length != 1)
                                throw new InvalidOperationException("More than one argument is not supported.");
                            ResolumeValue val;
                            if (!_values.TryGetValue(message.Address, out val))
                            {
                                switch (arguments[0].GetType().Name)
                                {
                                    case "Single":
                                        val = new ResolumeValue<Single>((Single)arguments[0]);
                                        break;
                                    case "String":
                                        val = new ResolumeValue<String>((String)arguments[0]);
                                        break;
                                    case "Int32":
                                        val = new ResolumeValue<Int32>((Int32)arguments[0]);
                                        break;
                                    default:
                                        throw new ApplicationException($"Type not supported: {arguments[0].GetType().FullName}");
                                }
                                //_values.AddOrUpdate()
                                _values[message.Address] = val;
                            }
                            else
                            {
                                switch (arguments[0].GetType().Name)
                                {
                                    case "Single":
                                        (val as ResolumeValue<Single>).Value = (Single)arguments[0];
                                        break;
                                    case "String":
                                        (val as ResolumeValue<String>).Value = (String)arguments[0];
                                        break;
                                    case "Int32":
                                        (val as ResolumeValue<Int32>).Value = (Int32)arguments[0];
                                        break;
                                    default:
                                        throw new ApplicationException($"Type not supported: {arguments[0].GetType().FullName}");
                                }
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
