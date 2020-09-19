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

        public unsafe int FindPattern(byte?[] pattern, ReadOnlySpan<byte> buffer)
        {
            if (pattern == null || pattern.Length == 0)
                return 0;

            // split into subpatterns
            var subpatterns = new List<Subpattern>(4);
            var lastNull = false;
            var start = 0;
            for (var i = 0; i < pattern.Length; i++)
            {
                if (pattern[i] == null && !lastNull)
                {
                    var subpattern = new byte[i - start];
                    for (var j = 0; j < subpattern.Length; j++)
                        subpattern[j] = pattern[j + start].Value;

                    subpatterns.Add(new Subpattern { Gap = -1, Pattern = subpattern });

                    start = i;
                    lastNull = true;
                }
                else if (pattern[i] != null && lastNull)
                {
                    subpatterns.Add(new Subpattern { Gap = i - start });

                    start = i;
                    lastNull = false;
                }
            }

            var sub = new Subpattern
            {
                Gap = lastNull ? pattern.Length - start : -1,
                Pattern = lastNull ? null : new byte[pattern.Length - start]
            };
            subpatterns.Add(sub);
            for (var j = 0; j < sub.Pattern.Length; j++)
                sub.Pattern[j] = pattern[j + start].Value;

            if (subpatterns[0].Gap != -1) // not be doing dis anyways
                return 0;

            // scan for first subpattern
            var pos = 0;
            while (pos < buffer.Length - pattern.Length)
            {
                var buff = buffer.Slice(pos);

                var subi = 0;
                var subpattern = subpatterns[subi];

                var fpos = buff.IndexOf(subpattern.Pattern);
                if (fpos == -1)
                    break;

                // scan for next subpatterns
                var spos = pos + fpos;
                pos += fpos + subpattern.Pattern.Length;
                for (++subi; subi < subpatterns.Count; subi++)
                {
                    subpattern = subpatterns[subi];
                    if (subpattern.Gap > 0)
                    {
                        pos += subpattern.Gap;
                        continue;
                    }

                    buff = buffer.Slice(pos);
                    if (!buff.Slice(0, subpattern.Pattern.Length).SequenceEqual(subpattern.Pattern))
                        break;

                    pos += subpattern.Pattern.Length;
                }

                // did we find all?
                if (subi == subpatterns.Count)
                    return spos;
            }

            return 0;
        }

        public unsafe IEnumerable<NamedOffset> FindOffsets(NamedPattern[] patterns, IntPtrEx @base, int size)
        {
            var buff = new byte[size];
            fixed (byte* ptr = buff)
                if (!this.TryReadRawMemory(@base, ptr, size, out var read) || read != size)
                    return null;

            var baseval = @base.Pointer.ToInt32();
            var offsets = new NamedOffset[patterns.Length];
            for (var i = 0; i < patterns.Length; i++)
            {
                var pattern = patterns[i];
                var off = this.FindPattern(pattern.Pattern, buff);

                var ptr = 0;
                if (!this.TryReadRawMemory(@base + off + pattern.PointerStart, &ptr, sizeof(int), out var read) || read != sizeof(int))
                    throw new InvalidProcessException();

                offsets[i] = new NamedOffset(ptr - baseval, pattern.Name);
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
