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

namespace Emzi0767.AmongUsDirector
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var proc = GameProcess.Attach();
            Console.WriteLine("Attached to process");

            proc.GameStarted += (o, e) => Console.WriteLine("EV: GAME_START -> map: {0}", e.Map);
            proc.GameEnded += (o, e) => Console.WriteLine("EV: GAME_END");

            proc.PlayerJoined += (o, e) => Console.WriteLine("EV: PLAYER_JOIN -> {0}", e.Player.Name);
            proc.PlayerLeft += (o, e) => Console.WriteLine("EV: PLAYER_LEAVE -> {0}", e.Player.Name);
            proc.PlayerDied += (o, e) => Console.WriteLine("EV: PLAYER_DEATH -> {0}", e.Player.Name);
            proc.PlayerImpostorStatusChanged += (o, e) => Console.WriteLine("EV: PLAYER_IMPOSTOR_STATUS_CHANGE -> {0}, {1}", e.Player.Name, e.Player.IsImpostor);

            proc.MeetingStarted += (o, e) => Console.WriteLine("EV: MEETING_START");
            proc.MeetingEnded += (o, e) => Console.WriteLine("EV: MEETING_END -> exile: {0:0.0}s", e.ExileDuration);

            proc.Start();
            Console.WriteLine("Loop running");

            Console.WriteLine("Press any to quit");
            Console.ReadKey(true);

            proc.Dispose();
            Console.WriteLine("Detached");
        }
    }
}
