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
        private EmojiProvider EmojiProvider { get; }

        public GameCommandModule(GameManagerService gameManager, EmojiProvider emojiProvider)
        {
            this.GameManager = gameManager;
            this.EmojiProvider = emojiProvider;
        }

        [Command("claim")]
        [Description("Claims a player.")]
        public async Task ClaimAsync(CommandContext ctx,
            [Description("Player name to claim."), RemainingText] string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
            {
                await ctx.RespondAsync(this.EmojiProvider.Get(EmojiType.RaisedHand));
                return;
            }

            await this.GameManager.AssociateAsync(ctx.Member.Id, playerName);
            await ctx.RespondAsync(this.EmojiProvider.Get(EmojiType.OkHand));
        }

        [Command("unclaim")]
        [Description("Unclaims a player.")]
        public async Task UnclaimAsync(CommandContext ctx)
        {
            if (!await this.GameManager.UnassociateAsync(ctx.Member.Id))
            {
                await ctx.RespondAsync(this.EmojiProvider.Get(EmojiType.RaisedHand));
                return;
            }

            await ctx.RespondAsync(this.EmojiProvider.Get(EmojiType.OkHand));
        }

        [Command("associate"), Aliases("assoc")]
        [Description("Associates a discord member with a player.")]
        [RequireUserPermissions(Permissions.ManageRoles)]
        public async Task AssociateAsync(CommandContext ctx,
            [Description("Discord user to associate.")] DiscordMember member,
            [Description("Player to associate with."), RemainingText] string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
            {
                await ctx.RespondAsync(this.EmojiProvider.Get(EmojiType.RaisedHand));
                return;
            }

            await this.GameManager.AssociateAsync(member.Id, playerName);
            await ctx.RespondAsync(this.EmojiProvider.Get(EmojiType.OkHand));
        }

        [Command("unassociate"), Aliases("unassoc", "disassociate", "disassoc")]
        [Description("Disassociates a discord member with a player.")]
        [RequireUserPermissions(Permissions.ManageRoles)]
        public async Task UnassociateAsync(CommandContext ctx,
            [Description("Discord user to disassociate."), RemainingText] DiscordMember member)
        {
            if (!await this.GameManager.UnassociateAsync(member.Id))
            {
                await ctx.RespondAsync(this.EmojiProvider.Get(EmojiType.RaisedHand));
                return;
            }

            await ctx.RespondAsync(this.EmojiProvider.Get(EmojiType.OkHand));
        }

        [Command("whoami"), Aliases("name")]
        [Description("Displays your current player name.")]
        public async Task WhoAmIAsync(CommandContext ctx)
        {
            var player = this.GameManager.GetAssociation(ctx.Member.Id);
            await ctx.Channel.SendMessageAsync(player != null
                ? $"Your player name: {player}"
                : "You did not yet associate a player name with your Discord account. Check out `claim` command.", mentions: Array.Empty<IMention>());
        }

        [Command("channel"), Aliases("chn", "vc")]
        [Description("Designates a voice channel for the game.")]
        [RequireUserPermissions(Permissions.ManageChannels)]
        public async Task SetVoiceChannelAsync(CommandContext ctx,
            [Description("Voice channel to use for the game."), RemainingText] DiscordChannel channel)
        {
            if (channel.Type != ChannelType.Voice)
            {
                await ctx.RespondAsync(this.EmojiProvider.Get(EmojiType.RaisedHand));
                return;
            }

            await this.GameManager.SetVoiceChannelAsync(channel?.Id ?? 0ul, channel?.Guild.Id ?? 0ul);
            await ctx.RespondAsync(this.EmojiProvider.Get(EmojiType.OkHand));
        }

        [Command("output")]
        [Description("Designates a text channel to post minimal diagnostic information to.")]
        [RequireUserPermissions(Permissions.ManageChannels)]
        public async Task SetTextChannelAsync(CommandContext ctx,
            [Description("Text channel to send minimal diagnostic information to."), RemainingText] DiscordChannel channel = null)
        {
            if (channel == null)
                channel = ctx.Channel;

            if (channel.Type != ChannelType.Text)
            {
                await ctx.RespondAsync(this.EmojiProvider.Get(EmojiType.RaisedHand));
                return;
            }

            await this.GameManager.SetOutputChannelAsync(channel?.Id ?? 0);
            await ctx.RespondAsync(this.EmojiProvider.Get(EmojiType.OkHand));
        }

        [Command("diag")]
        [Description("Outputs diagnostic info.")]
        [RequireUserPermissions(Permissions.ManageChannels)]
        public async Task DiagnosticsAsync(CommandContext ctx)
        {
            await ctx.RespondAsync($"Voice: {this.GameManager.VoiceChannel}\nGuild: {this.GameManager.Guild}\nOutput: {this.GameManager.TextChannel}");
        }
    }
}
