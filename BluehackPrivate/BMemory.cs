using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace SubService
{
    class BMemory
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, int lpBaseAddress, byte[] lpBuffer, int nSize, int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, int lpBaseAddress, byte[] buffer, int nSize, int lpNumberOfBytesRead);

        Process GameProcess;

        #region Process
        internal bool Attach()
        {
            Process[] ProcessList = Process.GetProcessesByName("csgo");

            if (ProcessList.Count<Process>() > 0)
            {
                GameProcess = ProcessList[0];
                return true;
            }
            return false;
        }

        internal bool CloseProcess(string ProcessName)
        {
            Process[] ProcessList = Process.GetProcessesByName(ProcessName);

            if (ProcessList.Count<Process>() > 0)
            {
                ProcessList[0].Kill();
                return true;
            }
            return false;
        }
        #endregion

        #region Read/Write
        internal T ReadMemory<T>(int BaseAddress) where T : struct
        {
            int ByteSize = Marshal.SizeOf(typeof(T));
            byte[] buffer = new byte[ByteSize];
            ReadProcessMemory(GameProcess.Handle, BaseAddress, buffer, buffer.Length, 0);
            return ByteArrayToStructure<T>(buffer);
        }

        internal byte[] ReadBytes(int BaseAddress, int nSize)
        {
            byte[] buffer = new byte[nSize];
            ReadProcessMemory(GameProcess.Handle, BaseAddress, buffer, nSize, 0);
            return buffer;
        }

        internal float[] ReadMatrix<T>(int BaseAddress, int MatrixSize) where T : struct
        {
            int ByteSize = Marshal.SizeOf(typeof(T));
            byte[] buffer = new byte[ByteSize * MatrixSize];
            ReadProcessMemory(GameProcess.Handle, BaseAddress, buffer, buffer.Length, 0);
            return ConvertToFloatArray(buffer);
        }

        internal void WriteMemory<T>(int BaseAddress, object Value) where T : struct
        {
            byte[] buffer = StructureToByteArray(Value);
            WriteProcessMemory(GameProcess.Handle, BaseAddress, buffer, buffer.Length, 0);
        }
        #endregion

        #region Byte
        public static float[] ConvertToFloatArray(byte[] bytes)
        {
            if (bytes.Length % 4 != 0)
            {
                throw new ArgumentException();
            }

            float[] floats = new float[bytes.Length / 4];
            for (int i = 0; i < floats.Length; i++)
            {
                floats[i] = BitConverter.ToSingle(bytes, i * 4);
            }
            return floats;
        }

        private static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }
        }

        private static byte[] StructureToByteArray(object obj)
        {
            int length = Marshal.SizeOf(obj);
            byte[] array = new byte[length];
            IntPtr pointer = Marshal.AllocHGlobal(length);
            Marshal.StructureToPtr(obj, pointer, true);
            Marshal.Copy(pointer, array, 0, length);
            Marshal.FreeHGlobal(pointer);
            return array;
        }
        #endregion

        internal int GetModule(string ModuleName)
        {
            foreach (ProcessModule Module in GameProcess.Modules)
            {
                if (Module.ModuleName == ModuleName)
                {
                    return (int)Module.BaseAddress;
                }
            }

            return -1;
        }
    }
}
