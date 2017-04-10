using Rug.Osc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ResolumeController
{
    public delegate void ChangedEventHandler(string oscPath, ResolumeValue value);
    


    public class Resolume
    {
        private readonly OscReceiver _resolumeOscReceiver;
        private readonly OscSender _resolumeOscSender;
        private readonly Thread _listenerThread;
        private readonly Thread _mutatorThread;
        private readonly ResolumeValueCollection _values = new ResolumeValueCollection();
        private readonly ResolumeMutatorCollection _mutators = new ResolumeMutatorCollection();
        private bool _isStopping = false;

        public event ChangedEventHandler ValueChanged;
        public IEnumerable<string> KnownAddresses => _values.Keys; 

        public Resolume(int listenOnPort, IPAddress sendToAddress, int sendToPort)
        {            
            _listenerThread = new Thread(new ThreadStart(ListenLoop));
            _mutatorThread = new Thread(new ThreadStart(MutatorLoop));
            _mutatorThread.Priority = ThreadPriority.AboveNormal;
            _resolumeOscReceiver = new OscReceiver(listenOnPort);
            _resolumeOscSender = new OscSender(sendToAddress, 0, sendToPort);
        }

        private void MutatorLoop()
        {
            while(!_isStopping)
            {
                List<Tuple<string, object>> messages = new List<Tuple<string, object>>();
                foreach (var mutatorAddress in _mutators.Keys)
                {
                    var m = _mutators[mutatorAddress];
                    object v;
                    if (m.TryGetValue(out v))
                    {
                        messages.Add(Tuple.Create(mutatorAddress, v));
                    }
                }
                SendValues(messages.ToArray());
                Thread.Sleep(10);
            }
        }

        public void Start()
        {
            _listenerThread.Start();
            _resolumeOscReceiver.Connect();
            _resolumeOscSender.Connect();
            _mutatorThread.Start();
        }

        public void Stop()
        {
            _isStopping = true;
            _resolumeOscSender.Dispose();
            _resolumeOscReceiver.Dispose();
            if (!_listenerThread.Join(1500))
            {
                _listenerThread.Abort();
            }
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

        public void SendValues(params Tuple<string, object>[] messages)
        {
            var b = new OscBundle(DateTime.Now, messages.Select(m => new OscMessage(m.Item1, m.Item2)).ToArray());
            _resolumeOscSender.Send(b);
        }


        public void SendValue(string oscAddress, object value)
        {
            var b = new OscBundle(DateTime.Now, new OscMessage(oscAddress, value));
            _resolumeOscSender.Send(b);
        }


        internal void SetMutator<T>(string oscAddress, ResolumeMutator<T> mutator)
        {
            _mutators[oscAddress] = mutator;    
        }


        private static ResolumeValue GenerateNewResolumeValue(object argument)
        {
            ResolumeValue generatedResolumeValue;
            if (argument is String)
            {
                generatedResolumeValue = new ResolumeValue<String>((string)argument);
            }
            else if (argument is Int32)
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

        private static void UpdateExistingResolumeValue(object argument, ResolumeValue oldValue)
        {
            if (argument is String)
            {
                (oldValue as ResolumeValue<String>).Value = (string)argument;
            }
            else if (argument is Int32)
            {
                (oldValue as ResolumeValue<Int32>).Value = (int)argument;
            }
            else if (argument is Single)
            {
                (oldValue as ResolumeValue<Single>).Value = (float)argument;
            }
            else
            {
                throw new ApplicationException($"Type not supported: {argument.GetType().FullName}");
            }
        }


        private static object ExtractArgumentFromMessage (OscMessage message)
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
                            ValueChanged?.Invoke(message.Address, resolumeValue);
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
