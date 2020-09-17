// This file is part of Among Us Director project.
// 
// Copyright 2020 Emzi0767
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Diagnostics;
using System.Text;

namespace Emzi0767.AmongUsDirector
{
    /// <summary>
    /// Wraps the memory of a foreign process.
    /// </summary>
    public sealed class ProcessMemory
    {
        private readonly Process _proc;

        internal ProcessMemory(Process proc)
        {
            this._proc = proc;
        }

        /// <summary>
        /// Reads data of a blittable data structure.
        /// </summary>
        /// <typeparam name="T">Type of data to read.</typeparam>
        /// <param name="addr">Address to read from.</param>
        /// <returns>Read data.</returns>
        public unsafe T Read<T>(IntPtrEx addr)
            where T : unmanaged
        {
            var val = default(T);
            var size = sizeof(T);
            if (!this.TryReadRawMemory(addr, &val, size, out var read) || read != size)
                throw new MemoryReadException();

            return val;
        }

        /// <summary>
        /// Reads a pointer from the process' memory.
        /// </summary>
        /// <param name="addr">Address to read from.</param>
        /// <returns>Read pointer.</returns>
        public unsafe IntPtrEx ReadPointer(IntPtrEx addr)
        {
            var ptrval = 0;
            if (!this.TryReadRawMemory(addr, &ptrval, sizeof(int), out var read) || read != sizeof(int))
                throw new MemoryReadException();

            return new IntPtr(ptrval);
        }

        /// <summary>
        /// Reads and follows a pointer chain.
        /// </summary>
        /// <param name="addr">Address to start at.</param>
        /// <param name="offsets">Offsets to follow.</param>
        /// <returns>Final pointer.</returns>
        public unsafe IntPtrEx ReadPointerChain(IntPtrEx addr, params int[] offsets)
        {
            foreach (var offset in offsets)
                addr = this.ReadPointer(addr + offset);

            return addr;
        }

        /// <summary>
        /// Reads a string from the process' memory.
        /// </summary>
        /// <param name="addr">Address to read from.</param>
        /// <param name="maxLength">Maximum string length to read.</param>
        /// <returns>Read string.</returns>
        public unsafe string ReadString(IntPtrEx addr, int maxLength = 256)
        {
            var buff = stackalloc char[maxLength];
            if (!this.TryReadRawMemory(addr, buff, maxLength * sizeof(char), out var read))
                throw new MemoryReadException();

            var spbuff = new Span<char>(buff, read / sizeof(char));
            var len = spbuff.IndexOf('\0');
            if (len < 0)
                return new string(spbuff);

            return new string(spbuff.Slice(0, len));
        }

        /// <summary>
        /// Reads an ANSI string from the process' memory.
        /// </summary>
        /// <param name="addr">Address to read from.</param>
        /// <param name="maxLength">Maximum string length to read.</param>
        /// <returns>Read string.</returns>
        public unsafe string ReadAnsiString(IntPtrEx addr, int maxLength = 256)
        {
            var buff = stackalloc byte[maxLength];
            if (!this.TryReadRawMemory(addr, buff, maxLength, out var read))
                throw new MemoryReadException();

            var spbuff = new Span<byte>(buff, read);
            var len = spbuff.IndexOf((byte)0x00);
            if (len >= 0)
                spbuff = spbuff.Slice(0, len);

            return Encoding.ASCII.GetString(spbuff);
        }

        private unsafe bool TryReadRawMemory(IntPtrEx addr, void* buff, int size, out int read)
        {
            read = -1;
            if (!PInvoke.ReadProcessMemory(this._proc.Handle, addr.Pointer, buff, new IntPtr(size), out var pread))
                return false;

            read = pread.ToInt32();
            return true;
        }
    }
}
