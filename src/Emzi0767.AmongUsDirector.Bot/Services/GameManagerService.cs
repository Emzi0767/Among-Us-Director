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
using System.Threading.Tasks;

namespace Emzi0767.AmongUsDirector
{
    public sealed class GameManagerService
    {
        public ulong VoiceChannel { get; private set; }
        public ulong TextChannel { get; private set; }
        public ulong Guild { get; private set; }
        private IDictionary<ulong, string> MemberPlayerMap { get; }
        private IDictionary<string, ulong> PlayerMemberMap { get; }
        private HashSet<string> DeadPlayers { get; }
        private HashSet<string> AllPlayers { get; }

        private UserMappingStorageService UserMappingStorage { get; }

        public GameManagerService(UserMappingStorageService userMappingStorage)
        {
            this.MemberPlayerMap = new Dictionary<ulong, string>();
            this.PlayerMemberMap = new Dictionary<string, ulong>();
            this.DeadPlayers = new HashSet<string>();
            this.AllPlayers = new HashSet<string>();

            this.UserMappingStorage = userMappingStorage;
        }

        public async Task StartAsync()
        {
            await foreach (var mapping in this.UserMappingStorage.RetireveAllAsync())
            {
                this.MemberPlayerMap[mapping.MemberId] = mapping.PlayerName;
                this.PlayerMemberMap[mapping.PlayerName] = mapping.MemberId;
            }

            this.VoiceChannel = await this.UserMappingStorage.GetVoiceChannelAsync();
            this.Guild = await this.UserMappingStorage.GetGuildAsync();
            this.TextChannel = await this.UserMappingStorage.GetOutputChannelAsync();
        }

        public string GetPlayerForMember(ulong memberId)
            => this.MemberPlayerMap.TryGetValue(memberId, out var playerName)
                ? playerName
                : null;

        public ulong GetMemberForPlayer(string playerName)
            => this.PlayerMemberMap.TryGetValue(playerName, out var memberId)
                ? memberId
                : 0UL;

        public async Task<bool> AssociateAsync(ulong memberId, string player)
        {
            if (memberId == 0)
            {
                // disassociate
                if (!this.PlayerMemberMap.TryGetValue(player, out var mbr))
                    return false;

                await this.UserMappingStorage.UnmapAsync(mbr);
                this.PlayerMemberMap.Remove(player);
                this.MemberPlayerMap.Remove(mbr);
                return true;
            }

            if (this.MemberPlayerMap.ContainsKey(memberId) || this.PlayerMemberMap.ContainsKey(player))
                return false;

            await this.UserMappingStorage.CommitMapAsync(memberId, player);
            this.MemberPlayerMap[memberId] = player;
            this.PlayerMemberMap[player] = memberId;
            return true;
        }

        public async Task<bool> UnassociateAsync(ulong memberId)
        {
            if (!this.MemberPlayerMap.TryGetValue(memberId, out var player))
                return false;

            await this.UserMappingStorage.UnmapAsync(memberId);
            this.PlayerMemberMap.Remove(player);
            this.MemberPlayerMap.Remove(memberId);
            return true;
        }

        public string GetAssociation(ulong memberId)
            => this.MemberPlayerMap.TryGetValue(memberId, out var player)
                ? player
                : null;

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
            this.AllPlayers.Remove(player);
            this.DeadPlayers.Remove(player);
        }

        public async Task SetVoiceChannelAsync(ulong channelId, ulong guildId)
        {
            this.VoiceChannel = channelId;
            this.Guild = guildId;

            await (channelId != 0ul
                ? this.UserMappingStorage.SetVoiceChannelAsync(channelId)
                : this.UserMappingStorage.UnsetVoiceChannelAsync());

            await (guildId != 0ul
                ? this.UserMappingStorage.SetGuildAsync(guildId)
                : this.UserMappingStorage.UnsetGuildAsync());
        }

        public async Task SetOutputChannelAsync(ulong channelId)
        {
            this.TextChannel = channelId;

            await (channelId != 0
                ? this.UserMappingStorage.SetOutputChannelAsync(channelId)
                : this.UserMappingStorage.UnsetOutputChannelAsync());
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
