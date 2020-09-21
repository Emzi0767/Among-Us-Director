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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Emzi0767.AmongUsDirector
{
    /// <summary>
    /// Wraps the memory of a foreign process.
    /// </summary>
    internal sealed class ProcessMemory
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

        /// <summary>
        /// Finds classes in IL2CPP metadata.
        /// </summary>
        /// <param name="base">Base of the module to look in.</param>
        /// <param name="size">Size of the module to look in.</param>
        /// <param name="classNames">Names of the classes to find.</param>
        /// <returns>A collection of name-offset pairs.</returns>
        public unsafe IEnumerable<NamedOffset> FindClasses(IntPtrEx @base, int size, params string[] classNames)
        {
            var dos = default(PeDosHeader);
            if (!this.TryReadRawMemory(@base, &dos, sizeof(PeDosHeader), out var read) || read != sizeof(PeDosHeader))
                return null;

            var ntOffset = @base + dos.e_lfanew;
            var nt = default(PeNtHeader);
            if (!this.TryReadRawMemory(ntOffset, &nt, sizeof(PeNtHeader), out read) || read != sizeof(PeNtHeader))
                return null;

            var sectionOffset = ntOffset + sizeof(PeNtHeader);
            var section = default(PeImageSectionHeader);
            for (var i = 0; i < nt.FileHeader.NumberOfSections; i++)
            {
                if (!this.TryReadRawMemory(sectionOffset + i * sizeof(PeImageSectionHeader), &section, sizeof(PeImageSectionHeader), out read) || read != sizeof(PeImageSectionHeader))
                    continue;

                if (new Span<byte>(section.Name, PeImageSectionHeader.IMAGE_SIZEOF_SHORT_NAME).SequenceEqual(Offsets.PeDataSectionName))
                    break;
            }

            var names = new HashSet<string>(classNames);
            var offsets = new List<NamedOffset>(classNames.Length);

            var buff = new byte[section.VirtualSize];
            fixed (byte* ptr = buff)
                if (!this.TryReadRawMemory(@base + section.VirtualAddress, ptr, buff.Length, out read) || read != buff.Length)
                    return null;

            var ptrs = MemoryMarshal.Cast<byte, int>(buff.AsSpan());
            var bval = @base.Pointer.ToInt32();
            var classDesc = default(RawClassInfo);
            var xptr = new IntPtrEx(IntPtr.Zero);
            for (var i = ptrs.Length - 1; i >= 0 && names.Count > 0; --i)
            {
                xptr = new IntPtr(ptrs[i]);
                if (!this.TryReadRawMemory(xptr, &classDesc, sizeof(RawClassInfo), out read) || read != sizeof(RawClassInfo))
                    continue;

                if (classDesc.Klass != xptr)
                    continue;

                var name = this.ReadAnsiString(classDesc.Name, 64);
                if (names.Contains(name))
                {
                    offsets.Add(new NamedOffset(section.VirtualAddress + i * IntPtr.Size, name));
                    names.Remove(name);
                }
            }

            return offsets;
        }

        private unsafe bool TryReadRawMemory(IntPtrEx addr, void* buff, int size, out int read)
        {
            read = -1;
            if (!PInvoke.ReadProcessMemory(this._proc.Handle, addr.Pointer, buff, new IntPtr(size), out var pread))
                return false;

            read = pread.ToInt32();
            return true;
        }

        public struct Subpattern
        {
            public int Gap { get; set; }
            public byte[] Pattern { get; set; }
        }
    }
}
