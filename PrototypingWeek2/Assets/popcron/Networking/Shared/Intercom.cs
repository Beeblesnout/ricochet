using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Text;
using UnityEngine;

namespace Popcron.Networking.Shared
{
    [Serializable]
    public class Intercom
    {
        private static System.Random random = new System.Random(DateTime.Now.Millisecond);

        private MemoryMappedViewAccessor networkApplicationView;
        private MemoryMappedViewAccessor editorView;

        public List<Message> pool = new List<Message>();
        public List<long> pastMessages = new List<long>();
        public PollSource source;

        private Action<Message> onProcessData;

        public Action<Message> Callback
        {
            get
            {
                return onProcessData;
            }
            set
            {
                onProcessData = value;
            }
        }

        private PollSource OppositeSource
        {
            get
            {
                if (source == PollSource.Editor) return PollSource.NetworkApplication;
                if (source == PollSource.NetworkApplication) return PollSource.Editor;

                throw new Exception();
            }
        }

        //taken from https://stackoverflow.com/questions/6651554/random-number-in-long-range-is-this-the-way
        private static long UniqueID
        {
            get
            {
                long min = long.MinValue;
                long max = long.MaxValue;

                //Working with ulong so that modulo works correctly with values > long.MaxValue
                ulong uRange = (ulong)(max - min);

                //Prevent a modolo bias; see https://stackoverflow.com/a/10984975/238419
                //for more information.
                //In the worst case, the expected number of calls is 2 (though usually it's
                //much closer to 1) so this loop doesn't really hurt performance at all.
                ulong ulongRand;
                do
                {
                    byte[] buf = new byte[8];
                    random.NextBytes(buf);
                    ulongRand = (ulong)BitConverter.ToInt64(buf, 0);
                } while (ulongRand > ulong.MaxValue - ((ulong.MaxValue % uRange) + 1) % uRange);

                return (long)(ulongRand % uRange) + min;
            }
        }

        private MemoryMappedViewAccessor Input
        {
            get
            {
                if (source == PollSource.NetworkApplication)
                {
                    if (editorView == null)
                    {
                        try
                        {
                            MemoryMappedFile memoryFile = MemoryMappedFile.OpenExisting(OppositeSource.ToString() + Settings.CurrentlyUniqueID);
                            editorView = memoryFile.CreateViewAccessor();
                        }
                        catch
                        {

                        }
                    }

                    return editorView;
                }
                else if (source == PollSource.Editor)
                {
                    if (networkApplicationView == null)
                    {
                        try
                        {
                            MemoryMappedFile memoryFile = MemoryMappedFile.OpenExisting(OppositeSource.ToString() + Settings.CurrentlyUniqueID);
                            networkApplicationView = memoryFile.CreateViewAccessor();
                        }
                        catch
                        {

                        }
                    }

                    return networkApplicationView;
                }
                else
                {
                    return null;
                }
            }
        }

        private MemoryMappedViewAccessor Output
        {
            get
            {
                if (source == PollSource.NetworkApplication)
                {
                    if (networkApplicationView == null)
                    {
                        MemoryMappedFile memoryFile = MemoryMappedFile.OpenExisting(PollSource.NetworkApplication.ToString() + Settings.CurrentlyUniqueID);
                        networkApplicationView = memoryFile.CreateViewAccessor();
                    }

                    return networkApplicationView;
                }
                else if (source == PollSource.Editor)
                {
                    if (editorView == null)
                    {
                        MemoryMappedFile memoryFile = MemoryMappedFile.OpenExisting(PollSource.Editor.ToString() + Settings.CurrentlyUniqueID);
                        editorView = memoryFile.CreateViewAccessor();
                    }

                    return editorView;
                }
                else
                {
                    return null;
                }
            }
        }

        public Intercom(PollSource source)
        {
            this.source = source;
        }

        private void FinishedReading()
        {
            if (!Settings.IsFinishedReading(source))
            {
                //tell the other application that we finished reading all messages
                //Debug.Log("FinishedReading");
                Settings.SetFinishedReadingState(source, true);
            }
        }

        public void Send(Message message)
        {
            //store this message in a pool
            pool.Add(message);

            //Debug.Log("ADDED " + (PipeMessageType)message.Type + " to pool");
        }

        private void SendPooledMessages()
        {
            int position = 0;

            //add unique id
            Output.Write(position, UniqueID);
            position += 8;

            //add how many messages will be sent
            Output.Write(position, (ushort)pool.Count);
            position += 2;

            for (int p = 0; p < pool.Count; p++)
            {
                Message message = pool[p];
                SendSingleMessage(message, ref position);
            }

            pool.Clear();
        }

        private void SendSingleMessage(Message message, ref int position)
        {
            if (message == null) return;

            int startingPosition = position;

            //add the message type
            Output.Write(position, message.Type);
            position += 2;

            //add the message length
            ushort length = (ushort)message.Data.Length;
            Output.Write(position, length);
            position += 2;

            //add all of the content
            for (int i = 0; i < length; i++)
            {
                Output.Write(position, message.Data[i]);
                position++;
            }

            //Debug.Log("SENT " + (PipeMessageType)message.Type + " at " + startingPosition);
        }

        public void Poll()
        {
            MemoryMappedViewAccessor input = Input;
            if (input == null) return;

            //the other application finished polling all messages
            if (Settings.IsFinishedReading(OppositeSource))
            {
                //send out the next batch of messages
                if (pool.Count > 0)
                {
                    //Debug.Log("Other app finished reading all messages, sending " + pool.Count + " message(s).");
                    SendPooledMessages();
                }
                else
                {
                    //Debug.Log("No messages to send.");
                }
                Settings.SetFinishedReadingState(OppositeSource, false);
            }

            int position = 0;

            //read the next 8 bytes as an id
            long id = input.ReadInt64(position);
            position += 8;

            //this message was already processed
            if (pastMessages.Contains(id))
            {
                Settings.SetFinishedReadingState(source, true);
                return;
            }

            //next 2 bytes is how many messages there are
            ushort messages = input.ReadUInt16(position);
            position += 2;

            if (messages > 0)
            {
                List<Message> queue = new List<Message>();
                for (int m = 0; m < messages; m++)
                {
                    //read message type
                    ushort type = input.ReadUInt16(position);
                    position += 2;

                    //read message length
                    ushort length = input.ReadUInt16(position);
                    position += 2;

                    //read message data itself
                    byte[] data = new byte[length];
                    for (int i = 0; i < length; i++)
                    {
                        data[i] = input.ReadByte(position);
                        position++;
                    }

                    //process data
                    Message message = new Message(type, data);
                    queue.Add(message);
                }

                for (int i = 0; i < queue.Count; i++)
                {
                    try
                    {
                        onProcessData?.Invoke(queue[i]);
                    }
                    catch
                    {

                    }
                }

                Settings.SetFinishedReadingState(source, true);
                //Debug.Log("Received " + messages + " messages from other app.");
                pastMessages.Add(id);
            }
            else
            {
                Settings.SetFinishedReadingState(source, true);
                //Debug.Log("No message read.");
            }
        }
    }
}
