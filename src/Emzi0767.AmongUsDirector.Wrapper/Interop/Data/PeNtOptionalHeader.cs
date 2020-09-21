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
    internal unsafe struct PeNtOptionalHeader
    {
        public const int IMAGE_NUMBEROF_DIRECTORY_ENTRIES = 16;

        [FieldOffset(0x00)]
        public PeMagic Magic;

        [FieldOffset(0x02)]
        public byte MajorLinkerVersion;

        [FieldOffset(0x03)]
        public byte MinorLinkerVersion;

        [FieldOffset(0x04)]
        public int SizeOfCode;

        [FieldOffset(0x08)]
        public int SizeOfInitializedData;

        [FieldOffset(0x0C)]
        public int SizeOfUninitializedData;

        [FieldOffset(0x10)]
        public int AddressOfEntryPoint;

        [FieldOffset(0x14)]
        public int BaseOfCode;

        [FieldOffset(0x18)]
        public int BaseOfData;

        [FieldOffset(0x1C)]
        public int ImageBase;

        [FieldOffset(0x20)]
        public int SectionAlignment;

        [FieldOffset(0x24)]
        public int FileAlignment;

        [FieldOffset(0x28)]
        public ushort MajorOperatingSystemVersion;

        [FieldOffset(0x2A)]
        public ushort MinorOperatingSystemVersion;

        [FieldOffset(0x2C)]
        public ushort MajorImageVersion;

        [FieldOffset(0x2E)]
        public ushort MinorImageVersion;

        [FieldOffset(0x30)]
        public ushort MajorSubsystemVersion;

        [FieldOffset(0x32)]
        public ushort MinorSubsystemVersion;

        [FieldOffset(0x34)]
        public int Win32VersionValue;

        [FieldOffset(0x38)]
        public int SizeOfImage;

        [FieldOffset(0x3C)]
        public int SizeOfHeaders;

        [FieldOffset(0x40)]
        public int CheckSum;

        [FieldOffset(0x44)]
        public PeSubsystem Subsystem;

        [FieldOffset(0x46)]
        public PeDllCharacteristics DllCharacteristics;

        [FieldOffset(0x48)]
        public int SizeOfStackReserve;

        [FieldOffset(0x4C)]
        public int SizeOfStackCommit;

        [FieldOffset(0x50)]
        public int SizeOfHeapReserve;

        [FieldOffset(0x54)]
        public int SizeOfHeapCommit;

        [FieldOffset(0x58)]
        public int LoaderFlags;

        [FieldOffset(0x5C)]
        public int NumberOfRvaAndSizes;

        [FieldOffset(0x60)]
        public fixed byte DataDirectory[IMAGE_NUMBEROF_DIRECTORY_ENTRIES * 8];
    } // size: 0xE0 = 0x60 + 0x80

    internal enum PeMagic : ushort
    {
        NT_OPTIONAL_HDR32_MAGIC = 0x010B,
        NT_OPTIONAL_HDR64_MAGIC = 0x020B,
        ROM_OPTIONAL_HDR_MAGIC = 0x0107
    }

    internal enum PeSubsystem : ushort
    {
        UNKNOWN = 0,
        NATIVE = 1,
        WINDOWS_GUI = 2,
        WINDOWS_CUI = 3,
        OS2_CUI = 5,
        POSIX_CUI = 7,
        WINDOWS_CE_GUI = 9,
        EFI_APPLICATION = 10,
        EFI_BOOT_SERVICE_DRIVER = 11,
        EFI_RUNTIME_DRIVER = 12,
        EFI_ROM = 13,
        XBOX = 14,
        WINDOWS_BOOT_APPLICATION = 16
    }

    internal enum PeDllCharacteristics : ushort
    {
        RESERVED = 0x0001 | 0x0002 | 0x0004 | 0x0008 | 0x1000 | 0x4000,
        DYNAMIC_BASE = 0x0040,
        FORCE_INTEGRITY = 0x0080,
        NX_COMPAT = 0x0100,
        NO_ISOLATION = 0x0200,
        NO_SEH = 0x0400,
        NO_BIND = 0x0800,
        WDM_DRIVER = 0x2000,
        TERMINAL_SERVER_AWARE = 0x8000
    }
}
