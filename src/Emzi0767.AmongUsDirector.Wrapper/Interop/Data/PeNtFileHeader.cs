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
using System.Runtime.InteropServices;

namespace Emzi0767.AmongUsDirector
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct PeNtFileHeader
    {
        [FieldOffset(0x00)]
        public PeMachine Machine;

        [FieldOffset(0x02)]
        public ushort NumberOfSections;

        [FieldOffset(0x04)]
        public int TimeDateStamp;

        [FieldOffset(0x08)]
        public int PointerToSymbolTable;

        [FieldOffset(0x0C)]
        public int NumberOfSymbols;

        [FieldOffset(0x10)]
        public ushort SizeOfOptionalHeader;

        [FieldOffset(0x12)]
        public PeCharacteristics Characteristics;
    } // size: 0x14

    internal enum PeMachine : ushort
    {
        I386  = 0x014C,
        IA64  = 0x0200,
        AMD64 = 0x8664
    }

    [Flags]
    internal enum PeCharacteristics : ushort
    {
        /// <summary>
        /// Relocation information stripped.
        /// </summary>
        RELOCS_STRIPPED = 0x0001,

        /// <summary>
        /// Image is executable, no external references required.
        /// </summary>
        EXECUTABLE_IMAGE = 0x0002,

        /// <summary>
        /// Source line numbers were removed.
        /// </summary>
        LINE_NUMS_STRIPPED = 0x0004,

        /// <summary>
        /// Symbol table entries were removed.
        /// </summary>
        LOCAL_SYMS_STRIPPED = 0x0008,

        /// <summary>
        /// Aggressively trim working set.
        /// </summary>
        AGGRESIVE_WS_TRIM = 0x0010,

        /// <summary>
        /// Executable is aware of >2GB address space.
        /// </summary>
        LARGE_ADDRESS_AWARE = 0x0020,

        /// <summary>
        /// Bytes of the word are reserved.
        /// </summary>
        BYTES_REVERSED_LO = 0x0080,

        /// <summary>
        /// Computer supports 32-bit words.
        /// </summary>
        _32BIT_MACHINE = 0x0100,

        /// <summary>
        /// Debug information was separated into another file.
        /// </summary>
        DEBUG_STRIPPED = 0x0200,

        /// <summary>
        /// If on removable media, copy to swap before executing.
        /// </summary>
        REMOVABLE_RUN_FROM_SWAP = 0x0400,

        /// <summary>
        /// If on network storage, copy to swap before executing.
        /// </summary>
        NET_RUN_FROM_SWAP = 0x0800,

        /// <summary>
        /// Image is a system file.
        /// </summary>
        SYSTEM = 0x1000,

        /// <summary>
        /// Image is a DLL library and cannot be ran directly.
        /// </summary>
        DLL = 0x2000,

        /// <summary>
        /// Image can be ran on uniprocessor system.
        /// </summary>
        UP_SYSTEM_ONLY = 0x4000,

        /// <summary>
        /// Bytes of the word are reserved.
        /// </summary>
        BYTES_REVERSED_HI = 0x8000
    }
}
