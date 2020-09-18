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
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Emzi0767.AmongUsDirector
{
    public sealed class DiscordBotService
    {
        private DiscordClient Discord { get; }
        private CommandsNextExtension CommandsNext { get; }
        private ILogger<DiscordBotService> Logger { get; }

        public DiscordBotService(
            ILoggerFactory loggerFactory,
            IOptions<BotConfiguration> config,
            IServiceProvider services)
        {
            var dcfg = new DiscordConfiguration
            {
                Token = config.Value.Token,
                TokenType = TokenType.Bot,

                LoggerFactory = loggerFactory,
                MinimumLogLevel = LogLevel.Information
            };
            this.Discord = new DiscordClient(dcfg);

            this.CommandsNext = this.Discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new[] { config.Value.Prefix },
                CaseSensitive = false,
                EnableMentionPrefix = true,
                Services = services
            });

            this.CommandsNext.RegisterCommands<AdminCommandModule>();
            this.CommandsNext.RegisterCommands<GameCommandModule>();

            this.CommandsNext.CommandErrored += this.CommandsNext_CommandErrored;

            this.Logger = loggerFactory.CreateLogger<DiscordBotService>();
            this.Logger.LogInformation("Discord client created");
        }

        public async Task StartAsync()
        {
            await this.Discord.ConnectAsync(new DiscordActivity("the impostor kill all crew", ActivityType.Watching));
        }

        public async Task StopAsync()
        {
            await this.Discord.DisconnectAsync();
            this.Discord.Dispose();
        }

        public async Task MuteAllAsync(ulong channelId)
        {
            var chn = await this.Discord.GetChannelAsync(channelId);
            var members = chn.Users;
            var tasks = members.Select(x => x.ModifyAsync(x => { x.Deafened = true; x.Muted = true; }));
            await Task.WhenAll(tasks);
        }

        public async Task MuteAsync(ulong guildId, IEnumerable<ulong> memberIds)
        {
            var gld = this.Discord.Guilds[guildId];
            var tasks = memberIds.Select(x => gld.GetMemberAsync(x));
            var mbrs = await Task.WhenAll(tasks);
            var tasks2 = mbrs.Select(x => x.ModifyAsync(x => { x.Deafened = true; x.Muted = true; }));
            await Task.WhenAll(tasks2);
        }

        public async Task UnmuteAllAsync(ulong channelId)
        {
            var chn = await this.Discord.GetChannelAsync(channelId);
            var members = chn.Users;
            var tasks = members.Select(x => x.ModifyAsync(x => { x.Deafened = false; x.Muted = false; }));
            await Task.WhenAll(tasks);
        }

        public async Task UnmuteAsync(ulong guildId, IEnumerable<ulong> memberIds)
        {
            var gld = this.Discord.Guilds[guildId];
            var tasks = memberIds.Select(x => gld.GetMemberAsync(x));
            var mbrs = await Task.WhenAll(tasks);
            var tasks2 = mbrs.Select(x => x.ModifyAsync(x => { x.Deafened = false; x.Muted = false; }));
            await Task.WhenAll(tasks2);
        }

        public async Task UndeafenAsync(ulong guildId, IEnumerable<ulong> memberIds)
        {
            var gld = this.Discord.Guilds[guildId];
            var tasks = memberIds.Select(x => gld.GetMemberAsync(x));
            var mbrs = await Task.WhenAll(tasks);
            var tasks2 = mbrs.Select(x => x.ModifyAsync(x => { x.Deafened = false; x.Muted = true; }));
            await Task.WhenAll(tasks2);
        }

        public async Task SendMessageAsync(ulong channelId, string message)
        {
            var chn = await this.Discord.GetChannelAsync(channelId);
            await chn.SendMessageAsync(message);
        }

        public bool IsInGuild(ulong guild)
            => this.Discord.Guilds.ContainsKey(guild);

        public DiscordEmoji GetEmote(string name)
            => DiscordEmoji.FromName(this.Discord, name);

        private Task CommandsNext_CommandErrored(CommandErrorEventArgs e)
        {
            this.Logger.LogError(e.Exception, "User '{0}' failed to execute '{1}' in '{2}' ({3}).",
                e.Context.User,
                e.Command?.QualifiedName ?? "<none>",
                e.Context.Channel.Name,
                e.Context.Channel.Id);
            return Task.CompletedTask;
        }
    }
}
