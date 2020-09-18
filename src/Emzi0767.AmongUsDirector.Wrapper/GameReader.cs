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
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Emzi0767.AmongUsDirector
{
    internal sealed class GameReader
    {
        private readonly ProcessMemory _mem;
        private readonly IntPtrEx _module;

        private GameStateInfo _state;

        public GameReader(ProcessMemory pmem, IntPtrEx module)
        {
            this._mem = pmem;
            this._module = module;
            this._state = new GameStateInfo();
        }

        public void DoRead()
        {
            var (timer, map) = this.ReadMapAndExileTimer();

            if (!this.ReadStartedStatus(out var client))
            {
                if (this._state.HasStarted)
                {
                    this._state.HasStarted = false;
                    this._state.IsInMeeting = false;
                    this._state.Players.Clear();

                    if (this.GameEnded != null)
                        this.GameEnded(null, new GameEndEventArgs());
                }
            }
            else
            {
                if (!this._state.HasStarted)
                {
                    this._state.HasStarted = true;
                    this._state.IsInMeeting = false;

                    Thread.Sleep(500); // What the actual fuck kind of an object takes more than 300ms to initialize
                    (_, map) = this.ReadMapAndExileTimer();

                    if (this.GameStarted != null)
                        this.GameStarted(null, new GameStartEventArgs(map));
                }
            }

            if (client.GameState == GameState.NotJoined)
                return;

            var players = this.ReadPlayers();
            foreach (var player in players)
            {
                if (!this._state.Players.Contains(player))
                {
                    this._state.Players.Add(player);

                    if (this.PlayerJoined != null)
                        this.PlayerJoined(null, new PlayerJoinEventArgs(player));
                }
                else
                {
                    var refplayer = this._state.Players.FirstOrDefault(x => x == player);

                    if (player.IsDead != refplayer.IsDead)
                    {
                        refplayer.IsDead = player.IsDead;

                        if (this.PlayerDied != null)
                            this.PlayerDied(null, new PlayerDeathEventArgs(refplayer));
                    }

                    if (player.IsImpostor != refplayer.IsImpostor)
                    {
                        refplayer.IsImpostor = player.IsImpostor;

                        if (this.PlayerImpostorStatusChanged != null)
                            this.PlayerImpostorStatusChanged(null, new PlayerImpostorStatusChangeEventArgs(refplayer));
                    }
                }
            }

            if (players.Count != 0) // logically, this can never be true - if you're in a game, there's at least one player in
            {
                var flagged = new List<Player>(this._state.Players.Count);
                foreach (var player in this._state.Players)
                {
                    if (!players.Contains(player))
                    {
                        flagged.Add(player);

                        if (this.PlayerLeft != null)
                            this.PlayerLeft(null, new PlayerLeaveEventArgs(player));
                    }
                }

                foreach (var fplayer in flagged)
                    this._state.Players.Remove(fplayer);
            }

            var inMeeting = this.ReadMeetingStatus();
            if (this._state.IsInMeeting != inMeeting)
            {
                this._state.IsInMeeting = inMeeting;

                if (inMeeting && this.MeetingStarted != null)
                    this.MeetingStarted(null, new MeetingStartEventArgs());
                else if (!inMeeting && this.MeetingEnded != null)
                    this.MeetingEnded(null, new MeetingEndEventArgs(timer + 3.5F));
            }
        }

        public event EventHandler<GameStartEventArgs> GameStarted;
        public event EventHandler<GameEndEventArgs> GameEnded;

        public event EventHandler<PlayerJoinEventArgs> PlayerJoined;
        public event EventHandler<PlayerLeaveEventArgs> PlayerLeft;
        public event EventHandler<PlayerDeathEventArgs> PlayerDied;
        public event EventHandler<PlayerImpostorStatusChangeEventArgs> PlayerImpostorStatusChanged;

        public event EventHandler<MeetingStartEventArgs> MeetingStarted;
        public event EventHandler<MeetingEndEventArgs> MeetingEnded;

        private bool ReadStartedStatus(out RawClient client)
        {
            var clientPtr = this._mem.ReadPointerChain(this._module, Offsets.ClientBase, Offsets.Il2CppStaticsOffset, Offsets.None);
            client = this._mem.Read<RawClient>(clientPtr);
            this.ValidateKlass(client.Klass, Offsets.ClientName);

            return client.GameMode == GameMode.FreePlay && client.GameState == GameState.Joined
                || client.GameMode != GameMode.FreePlay && client.GameState == GameState.Started;
        }

        private HashSet<Player> ReadPlayers()
        {
            var gameDataPtr = this._mem.ReadPointerChain(this._module, Offsets.GameDataBase, Offsets.Il2CppStaticsOffset, Offsets.None);
            var gameData = this._mem.Read<RawGameData>(gameDataPtr);
            this.ValidateKlass(gameData.Klass, Offsets.GameDataName);

            var playerList = this._mem.Read<RawList>(gameData.AllPlayers);
            var playerCount = playerList.Size;
            var players = new HashSet<Player>(playerCount);
            var arrayBase = playerList.Fields;
            for (var i = 0; i < playerCount; i++)
            {
                try
                {
                    var playerPtr = this._mem.ReadPointer(arrayBase + Offsets.ArrayFirst + i * IntPtr.Size);
                    var player = this._mem.Read<RawPlayerInfo>(playerPtr);
                    var playerNameStruct = this._mem.Read<RawString>(player.Name);
                    var playerName = this._mem.ReadString(player.Name + Offsets.StringFirst, playerNameStruct.Length);
                    if (string.IsNullOrWhiteSpace(playerName))
                        continue;

                    players.Add(new Player(player.Id, playerName, player.Dead, player.Impostor));
                }
                catch { /* Occasionally a player leaves between cycles, that can lead to memory read errors. */ }
            }

            return players;
        }

        private bool ReadMeetingStatus()
        {
            var meetingHudPtr = this._mem.ReadPointer(this._module + Offsets.MeetingHudBase);
            if (meetingHudPtr == IntPtr.Zero)
                return false;

            meetingHudPtr = this._mem.ReadPointerChain(meetingHudPtr, Offsets.Il2CppStaticsOffset, Offsets.None);
            if (meetingHudPtr == IntPtr.Zero)
                return false;

            var meetingHud = this._mem.Read<RawMeetingHud>(meetingHudPtr);
            this.ValidateKlass(meetingHud.Klass, Offsets.MeetingHudName);

            return meetingHud.MeetingState != MeetingState.Proceeding;
        }

        private (float, GameMap) ReadMapAndExileTimer()
        {
            var timer = float.NaN;
            var map = GameMap.Unknown;

            var shipStatusPtr = this._mem.ReadPointer(this._module + Offsets.ShipStatusBase);
            if (shipStatusPtr.Pointer == IntPtr.Zero)
                return (timer, map);

            shipStatusPtr = this._mem.ReadPointerChain(shipStatusPtr, Offsets.Il2CppStaticsOffset, Offsets.None);

            var shipStatus = this._mem.Read<RawShipStatus>(shipStatusPtr);
            this.ValidateKlass(shipStatus.Klass, Offsets.ShipStatusName);

            map = (GameMap)shipStatus.MapType;
            if (shipStatus.ExileController == IntPtr.Zero)
                return (timer, map);

            var exileController = this._mem.Read<RawExileController>(shipStatus.ExileController);
            this.ValidateKlass(exileController.Klass, Offsets.ExileControllerName, Offsets.MiraExileControllerName, Offsets.PollusExileControllerName);

            timer = exileController.Duration;
            return (timer, map);
        }

        private void ValidateKlass(IntPtrEx klassPtr, params string[] refNames)
        {
            // For whatever bloody reason, the pointers are off by one sometimes.
            // Always case with LSB
            var ptrv = klassPtr.Pointer.ToInt32();
            if ((ptrv & 0x1) == 0x1)
                ptrv &= ~0x1;

            var klass = this._mem.Read<RawClassInfo>(new IntPtr(ptrv));
            var name = this._mem.ReadAnsiString(klass.Name);
            if (!refNames.Any(x => name == x))
                throw new InvalidProcessException();
        }
    }
}
