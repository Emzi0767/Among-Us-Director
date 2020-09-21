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

using System.Runtime.InteropServices;

namespace Emzi0767.AmongUsDirector
{
    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct PeImageSectionHeader
    {
        public const int IMAGE_SIZEOF_SHORT_NAME = 8;

        [FieldOffset(0x00)]
        public fixed byte Name[IMAGE_SIZEOF_SHORT_NAME];

        [FieldOffset(IMAGE_SIZEOF_SHORT_NAME)]
        public int VirtualSize;

        [FieldOffset(IMAGE_SIZEOF_SHORT_NAME + 0x04)]
        public int VirtualAddress;

        [FieldOffset(IMAGE_SIZEOF_SHORT_NAME + 0x08)]
        public int SizeOfRawData;

        [FieldOffset(IMAGE_SIZEOF_SHORT_NAME + 0x0C)]
        public int PointerToRawData;

        [FieldOffset(IMAGE_SIZEOF_SHORT_NAME + 0x10)]
        public int PointerToRelocations;

        [FieldOffset(IMAGE_SIZEOF_SHORT_NAME + 0x14)]
        public int PointerToLinenumbers;

        [FieldOffset(IMAGE_SIZEOF_SHORT_NAME + 0x18)]
        public ushort NumberOfRelocations;

        [FieldOffset(IMAGE_SIZEOF_SHORT_NAME + 0x1A)]
        public ushort NumberOfLinenumbers;

        [FieldOffset(IMAGE_SIZEOF_SHORT_NAME + 0x1C)]
        public int Characteristics;
    } // size: 0x28
}
