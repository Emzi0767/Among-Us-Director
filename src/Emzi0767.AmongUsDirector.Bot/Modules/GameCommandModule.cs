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

using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace Emzi0767.AmongUsDirector
{
    [ModuleLifespan(ModuleLifespan.Transient)]
    [RequireGuild]
    public sealed class GameCommandModule : BaseCommandModule
    {
        private GameManagerService GameManager { get; }

        public GameCommandModule(GameManagerService gameManager)
        {
            this.GameManager = gameManager;
        }

        [Command("claim")]
        [Description("Claims a player.")]
        public async Task ClaimAsync(CommandContext ctx,
            [Description("Player name to claim."), RemainingText] string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
            {
                await ctx.RespondAsync(DiscordEmoji.FromName(ctx.Client, ":msraisedhand:"));
                return;
            }

            this.GameManager.Associate(ctx.Member.Id, playerName);
            await ctx.RespondAsync(DiscordEmoji.FromName(ctx.Client, ":msokhand:"));
        }

        [Command("unclaim")]
        [Description("Unclaims a player.")]
        public async Task UnclaimAsync(CommandContext ctx,
            [Description("Player name to unclaim."), RemainingText] string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
            {
                await ctx.RespondAsync(DiscordEmoji.FromName(ctx.Client, ":msraisedhand:"));
                return;
            }

            this.GameManager.Associate(0, playerName);
            await ctx.RespondAsync(DiscordEmoji.FromName(ctx.Client, ":msokhand:"));
        }

        [Command("associate"), Aliases("assoc")]
        [Description("Associates a discord member with a player.")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task AssociateAsync(CommandContext ctx,
            [Description("Discord user to associate.")] DiscordMember member,
            [Description("Player to associate with."), RemainingText] string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
            {
                await ctx.RespondAsync(DiscordEmoji.FromName(ctx.Client, ":msraisedhand:"));
                return;
            }

            this.GameManager.Associate(member.Id, playerName);
            await ctx.RespondAsync(DiscordEmoji.FromName(ctx.Client, ":msokhand:"));
        }

        [Command("unassociate"), Aliases("unassoc", "disassociate", "disassoc")]
        [Description("Disassociates a discord member with a player.")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task UnassociateAsync(CommandContext ctx,
            [Description("Discord user to disassociate.")] DiscordMember member,
            [Description("Player to disassociate with."), RemainingText] string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
            {
                await ctx.RespondAsync(DiscordEmoji.FromName(ctx.Client, ":msraisedhand:"));
                return;
            }

            this.GameManager.Associate(0, playerName);
            await ctx.RespondAsync(DiscordEmoji.FromName(ctx.Client, ":msokhand:"));
        }

        [Command("channel"), Aliases("chn", "vc")]
        [Description("Designates a voice channel for the game.")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task SetVoiceChannelAsync(CommandContext ctx,
            [Description("Voice channel to use for the game."), RemainingText] DiscordChannel channel)
        {
            if (channel == null || channel.Type != ChannelType.Voice)
            {
                await ctx.RespondAsync(DiscordEmoji.FromName(ctx.Client, ":msraisedhand:"));
                return;
            }

            this.GameManager.VoiceChannel = channel.Id;
            this.GameManager.Guild = channel.Guild.Id;
            await ctx.RespondAsync(DiscordEmoji.FromName(ctx.Client, ":msokhand:"));
        }

        [Command("output")]
        [Description("Designates a text channel to post minimal diagnostic information to.")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task SetTextChannelAsync(CommandContext ctx,
            [Description("Text channel to send minimal diagnostic information to."), RemainingText] DiscordChannel channel = null)
        {
            if (channel == null)
                channel = ctx.Channel;

            if (channel.Type != ChannelType.Text)
            {
                await ctx.RespondAsync(DiscordEmoji.FromName(ctx.Client, ":msraisedhand:"));
                return;
            }

            this.GameManager.TextChannel = channel.Id;
            await ctx.RespondAsync(DiscordEmoji.FromName(ctx.Client, ":msokhand:"));
        }
    }
}
