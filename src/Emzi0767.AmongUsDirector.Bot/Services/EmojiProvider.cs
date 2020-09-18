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

using DSharpPlus.Entities;

namespace Emzi0767.AmongUsDirector
{
    public sealed class EmojiProvider
    {
        private const ulong MsEmojiGuildId = 333537188451057664ul;

        private const string OkHandMs = ":msokhand:";
        private const string RaisedHandMs = ":msraisedhand:";

        private const string OkHandGeneric = ":ok_hand:";
        private const string RaisedHandGeneric = ":raised_hand:";

        private DiscordBotService DiscordBot { get; }

        public EmojiProvider(DiscordBotService discordBot)
        {
            this.DiscordBot = discordBot;
        }

        public DiscordEmoji Get(EmojiType type)
        {
            var hasMs = this.DiscordBot.IsInGuild(MsEmojiGuildId);
            var name = type switch
            {
                EmojiType.OkHand => hasMs ? OkHandMs : OkHandGeneric,
                EmojiType.RaisedHand => hasMs ? RaisedHandMs : RaisedHandGeneric,
                _ => null
            };

            if (name == null)
                return null;

            return this.DiscordBot.GetEmote(name);
        }
    }
}
