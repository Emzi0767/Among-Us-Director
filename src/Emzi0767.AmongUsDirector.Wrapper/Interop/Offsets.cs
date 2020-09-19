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

namespace Emzi0767.AmongUsDirector
{
    internal static class Offsets
    {
        public const int None = 0x00;
        public const int Il2CppStaticsOffset = 0x5C;
        public const int ArrayLength = 0x0C;
        public const int ArrayFirst = 0x10;
        public const int StringFirst = 0x0C;

        public const string ClientName = "AmongUsClient";
        public static byte?[] ClientPattern { get; } = new byte?[] { 0xC7, 0x46, 0x08, 0xFF, 0xFF, 0xFF, 0xFF, 0xA1, null, null, null, null, 0x8B, 0x40, 0x5C, 0x8B, 0x30 };
        public const int ClientPatternPtrLocation = 8;
        //public const int ClientBase = 0xDA5ACC;
        public static int ClientBase { get; set; } = 0;

        public const string MeetingHudName = "MeetingHud";
        public static byte?[] MeetingHudPattern { get; } = new byte?[] { 0xC7, 0x43, 0x08, 0xFF, 0xFF, 0xFF, 0xFF, 0xA1, null, null, null, null, 0x8B, 0x40, 0x5C, 0x8B, 0x30 };
        public const int MeetingHudPatternPtrLocation = 8;
        //public const int MeetingHudBase = 0xDA58D0;
        public static int MeetingHudBase { get; set; } = 0;

        public const string GameDataName = "GameData";
        public static byte?[] GameDataPattern { get; } = new byte?[] { 0xC7, 0x47, 0x08, 0xFF, 0xFF, 0xFF, 0xFF, 0xA1, null, null, null, null, 0x53, 0x8B, 0x40, 0x5C };
        public const int GameDataPatternPtrLocation = 8;
        //public const int GameDataBase = 0xDA5A60;
        public static int GameDataBase { get; set; } = 0;

        public const string ShipStatusName = "ShipStatus";
        public static byte?[] ShipStatusPattern { get; } = new byte?[] { 0x89, 0x43, 0x08, 0xA1, null, null, null, null, 0x46, 0x83, 0xC7, 0x04 };
        public const int ShipStatusPatternPtrLocation = 4;
        //public const int ShipStatusBase = 0xDA5A50;
        public static int ShipStatusBase { get; set; } = 0;

        public const string ExileControllerName = "ExileController";
        public const string MiraExileControllerName = "MiraExileController";
        public const string PollusExileControllerName = "PbExileController";
    }
}
