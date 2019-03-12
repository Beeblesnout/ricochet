using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using UnityEngine;

namespace Popcron.Networking.Shared
{
    public class Settings
    {
        public const int Capacity = 1024 * 1024 * 1024;
        public const long NetworkAcknowlegement = 0101_0101_0101_0101;

        public static int CurrentlyUniqueID
        {
            get
            {
                string key = Process.GetCurrentProcess().Id + "_CurrentlyUniqueID";
                return PlayerPrefs.GetInt(key);
            }
            set
            {
                string key = Process.GetCurrentProcess().Id + "_CurrentlyUniqueID";
                PlayerPrefs.SetInt(key, value);
            }
        }


        public static ulong Header
        {
            get
            {
                ulong h = 0;
                for (int i = 0; i < 64; i++)
                {
                    h |= (uint)(i % 2) << i;
                }
                return h;
            }
        }

        public static ulong Footer
        {
            get
            {
                ulong f = 0;
                for (int i = 0; i < 64; i++)
                {
                    f |= (uint)((i + 1) % 2) << i;
                }
                return f;
            }
        }

        public static bool IsFinishedReading(PollSource source)
        {
            try
            {
                MemoryMappedFile sharedFile = MemoryMappedFile.OpenExisting("pnc_shared_" + CurrentlyUniqueID);
                MemoryMappedViewAccessor sharedAccessor = sharedFile.CreateViewAccessor();

                int index = source == PollSource.NetworkApplication ? 2 : 3;
                bool finished = sharedAccessor.ReadByte(index) == 1;
                return finished;
            }
            catch
            {
                //happens if shared memory file doesnt exist yet
                return false;
            }
        }

        public static void SetFinishedReadingState(PollSource source, bool state)
        {
            MemoryMappedFile sharedFile = MemoryMappedFile.OpenExisting("pnc_shared_" + CurrentlyUniqueID);
            MemoryMappedViewAccessor sharedAccessor = sharedFile.CreateViewAccessor();

            int index = source == PollSource.NetworkApplication ? 2 : 3;
            sharedAccessor.Write(index, state ? (byte)1 : (byte)0);
        }

        public static bool EditorStreamClear
        {
            get
            {
                MemoryMappedFile sharedFile = MemoryMappedFile.OpenExisting("pnc_shared_" + CurrentlyUniqueID);
                MemoryMappedViewAccessor sharedAccessor = sharedFile.CreateViewAccessor();

                //first byte is clear flag
                bool clear = sharedAccessor.ReadByte(0) == 1;
                return clear;
            }
            set
            {
                MemoryMappedFile sharedFile = MemoryMappedFile.OpenExisting("pnc_shared_" + CurrentlyUniqueID);
                MemoryMappedViewAccessor sharedAccessor = sharedFile.CreateViewAccessor();

                //first byte is clear flag
                sharedAccessor.Write(0, value ? (byte)1 : (byte)0);
            }
        }

        public static bool NetworkApplicationStreamClear
        {
            get
            {
                MemoryMappedFile sharedFile = MemoryMappedFile.OpenExisting("pnc_shared_" + CurrentlyUniqueID);
                MemoryMappedViewAccessor sharedAccessor = sharedFile.CreateViewAccessor();

                //second byte is clear flag
                bool clear = sharedAccessor.ReadByte(1) == 1;
                return clear;
            }
            set
            {
                MemoryMappedFile sharedFile = MemoryMappedFile.OpenExisting("pnc_shared_" + CurrentlyUniqueID);
                MemoryMappedViewAccessor sharedAccessor = sharedFile.CreateViewAccessor();

                //second byte is clear flag
                sharedAccessor.Write(1, value ? (byte)1 : (byte)0);
            }
        }
    }
}