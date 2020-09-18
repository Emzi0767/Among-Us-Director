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
using System.Globalization;
using System.Threading.Tasks;

namespace Emzi0767.AmongUsDirector
{
    public sealed class UserMappingStorageService
    {
        private const string PlayerMapPrefix = "playerMap";
        private const string VoiceChannelPrefix = "vc";
        private const string GuildPrefix = "guild";
        private const string OutputChannelPrefix = "output";

        private RedisClientService Redis { get; }

        public UserMappingStorageService(RedisClientService redis)
        {
            this.Redis = redis;
        }

        public async IAsyncEnumerable<MemberPlayerMapping> RetireveAllAsync()
        {
            await foreach (var k in this.Redis.GetKeysAsync(PlayerMapPrefix, "*"))
            {
                var player = await this.Redis.GetValueAsync<string>(PlayerMapPrefix, k);
                var memberId = ulong.Parse(k, NumberStyles.Number, CultureInfo.InvariantCulture);

                yield return new MemberPlayerMapping(memberId, player);
            }
        }

        public async Task CommitMapAsync(ulong memberId, string playerName)
            => await this.Redis.SetValueAsync(playerName, PlayerMapPrefix, memberId.ToString(CultureInfo.InvariantCulture));

        public async Task UnmapAsync(ulong memberId)
            => await this.Redis.DeleteValueAsync(PlayerMapPrefix, memberId.ToString(CultureInfo.InvariantCulture));

        public async Task<ulong> GetVoiceChannelAsync()
            => await this.Redis.GetValueAsync<ulong>(VoiceChannelPrefix);

        public async Task SetVoiceChannelAsync(ulong channelId)
            => await this.Redis.SetValueAsync(channelId, VoiceChannelPrefix);

        public async Task UnsetVoiceChannelAsync()
            => await this.Redis.DeleteValueAsync(VoiceChannelPrefix);

        public async Task<ulong> GetGuildAsync()
            => await this.Redis.GetValueAsync<ulong>(GuildPrefix);

        public async Task SetGuildAsync(ulong guildId)
            => await this.Redis.SetValueAsync(guildId, GuildPrefix);

        public async Task UnsetGuildAsync()
            => await this.Redis.DeleteValueAsync(GuildPrefix);

        public async Task<ulong> GetOutputChannelAsync()
            => await this.Redis.GetValueAsync<ulong>(OutputChannelPrefix);

        public async Task SetOutputChannelAsync(ulong channelId)
            => await this.Redis.SetValueAsync(channelId, OutputChannelPrefix);

        public async Task UnsetOutputChannelAsync()
            => await this.Redis.DeleteValueAsync(OutputChannelPrefix);
    }
}
