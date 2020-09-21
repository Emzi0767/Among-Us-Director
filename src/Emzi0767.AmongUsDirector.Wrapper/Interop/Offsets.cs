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
        public static byte[] PeDataSectionName { get; } = new byte[] { 0x2E, 0x64, 0x61, 0x74, 0x61, 0x00, 0x00, 0x00 }; // .data\0\0\0

        public const int None = 0x00;
        public const int Il2CppStaticsOffset = 0x5C;
        public const int ArrayLength = 0x0C;
        public const int ArrayFirst = 0x10;
        public const int StringFirst = 0x0C;

        public const string ClientName = "AmongUsClient";
        public static int ClientBase { get; set; } = 0;

        public const string MeetingHudName = "MeetingHud";
        public static int MeetingHudBase { get; set; } = 0;

        public const string GameDataName = "GameData";
        public static int GameDataBase { get; set; } = 0;

        public const string ShipStatusName = "ShipStatus";
        public static int ShipStatusBase { get; set; } = 0;

        public const string ExileControllerName = "ExileController";
        public const string MiraExileControllerName = "MiraExileController";
        public const string PollusExileControllerName = "PbExileController";
    }
}
