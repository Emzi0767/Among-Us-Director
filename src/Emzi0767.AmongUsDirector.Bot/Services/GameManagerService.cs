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

using System.Collections.Generic;
using System.Linq;

namespace Emzi0767.AmongUsDirector
{
    public sealed class GameManagerService
    {
        public ulong VoiceChannel { get; set; }
        public ulong TextChannel { get; set; }
        public ulong Guild { get; set; }
        private IDictionary<ulong, string> MemberPlayerMap { get; }
        private IDictionary<string, ulong> PlayerMemberMap { get; }
        private HashSet<string> DeadPlayers { get; }
        private HashSet<string> AllPlayers { get; }

        public GameManagerService()
        {
            this.MemberPlayerMap = new Dictionary<ulong, string>();
            this.PlayerMemberMap = new Dictionary<string, ulong>();
            this.DeadPlayers = new HashSet<string>();
            this.AllPlayers = new HashSet<string>();
        }

        public string GetPlayerForMember(ulong memberId)
            => this.MemberPlayerMap.TryGetValue(memberId, out var playerName)
                ? playerName
                : null;

        public ulong GetMemberForPlayer(string playerName)
            => this.PlayerMemberMap.TryGetValue(playerName, out var memberId)
                ? memberId
                : 0UL;

        public bool Associate(ulong memberId, string player)
        {
            if (memberId == 0)
            {
                // disassociate
                if (!this.PlayerMemberMap.TryGetValue(player, out var mbr))
                    return false;

                this.PlayerMemberMap.Remove(player);
                this.MemberPlayerMap.Remove(mbr);
                return true;
            }

            if (this.MemberPlayerMap.ContainsKey(memberId) || this.PlayerMemberMap.ContainsKey(player))
                return false;

            this.MemberPlayerMap[memberId] = player;
            this.PlayerMemberMap[player] = memberId;
            return true;
        }

        public bool MarkDead(string player)
        {
            if (!this.PlayerMemberMap.ContainsKey(player))
                return false;

            this.DeadPlayers.Add(player);
            return true;
        }

        public void AddPlayer(string player)
        {
            this.AllPlayers.Add(player);
        }

        public void RemovePlayer(string player)
        {
            this.Associate(0ul, player);
            this.AllPlayers.Remove(player);
            this.DeadPlayers.Remove(player);
        }

        public IEnumerable<ulong> GetUnmutables()
            => this.AllPlayers
                .Except(this.DeadPlayers)
                .Select(x => new { mapped = this.PlayerMemberMap.TryGetValue(x, out var member), member })
                .Where(x => x.mapped)
                .Select(x => x.member);

        public IEnumerable<ulong> GetUndeafables()
            => this.DeadPlayers
                .Select(x => new { mapped = this.PlayerMemberMap.TryGetValue(x, out var member), member })
                .Where(x => x.mapped)
                .Select(x => x.member);
    }
}
