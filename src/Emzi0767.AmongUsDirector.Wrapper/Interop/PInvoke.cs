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
    internal static class PInvoke
    {
        [DllImport("psapi.dll", EntryPoint = "EnumProcessModules", SetLastError = true)]
        public static unsafe extern bool EnumerateProcessModules(IntPtr hProcess, IntPtr* lphModule, int cb, out int lpcbNeeded);

        [DllImport("psapi.dll", EntryPoint = "GetModuleFileNameExW", CharSet = CharSet.Unicode, SetLastError = true)]
        public static unsafe extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, char* lpBaseName, uint nSize);

        [DllImport("psapi.dll", EntryPoint = "GetModuleBaseNameW", CharSet = CharSet.Unicode, SetLastError = true)]
        public static unsafe extern uint GetModuleBaseName(IntPtr hProcess, IntPtr hModule, char* lpBaseName, uint nSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static unsafe extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, void* lpBuffer, IntPtr dwSize, out IntPtr lpNumberOfBytesRead);
    }
}
