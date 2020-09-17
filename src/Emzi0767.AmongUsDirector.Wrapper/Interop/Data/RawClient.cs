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
    internal struct RawClient
    {
        [FieldOffset(0x00)]
        public IntPtr Klass;

        [FieldOffset(0x3C)]
        public MatchMakerMode Mode;

        [FieldOffset(0x40)]
        public int GameId;

        [FieldOffset(0x44)]
        public int HostId;

        [FieldOffset(0x48)]
        public int ClientId;

        [FieldOffset(0x64)]
        public GameState GameState;

        [FieldOffset(0x74)]
        public GameMode GameMode;
    }

    internal enum GameState : int
    { 
        NotJoined = 0,
        Joined = 1,
        Started = 2,
        Ended = 3
    }

    internal enum MatchMakerMode : int
    {
        None = 0,
        Client = 1,
        HostClient = 2
    }

    internal enum GameMode : int
    {
        LocalGame = 0,
        OnlineGame = 1,
        FreePlay = 2
    }
}
