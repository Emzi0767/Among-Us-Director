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
using System.Threading;
using System.Threading.Tasks;
using Emzi0767.AmongUsDirector.Services;
using Microsoft.Extensions.Hosting;

namespace Emzi0767.AmongUsDirector
{
    public sealed class AmongUsDirectorHostedService : IHostedService
    {
        private DiscordBotService DiscordBot { get; }
        private AmongUsGame Game { get; }
        private GameManagerService GameManager { get; }
        private DiscoveryServer Discovery { get; }
        private MothershipCommArray Comms { get; }
        private RedisClientService Redis { get; }

        public AmongUsDirectorHostedService(
            DiscordBotService discordBot,
            AmongUsGame game,
            GameManagerService gameManager,
            DiscoveryServer discovery,
            MothershipCommArray comms,
            RedisClientService redis)
        {
            this.DiscordBot = discordBot;
            this.Game = game;
            this.GameManager = gameManager;
            this.Discovery = discovery;
            this.Comms = comms;
            this.Redis = redis;

            this.Game.GameStarted += this.Game_GameStarted;
            this.Game.GameEnded += this.Game_GameEnded;
            this.Game.PlayerJoined += this.Game_PlayerJoined;
            this.Game.PlayerLeft += this.Game_PlayerLeft;
            this.Game.PlayerDied += this.Game_PlayerDied;
            this.Game.MeetingStarted += this.Game_MeetingStarted;
            this.Game.MeetingEnded += this.Game_MeetingEnded;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await this.DiscordBot.StartAsync();
            this.Comms.Start();
            this.Discovery.Start();
            await this.Redis.StartAsync();
            await this.GameManager.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            this.Redis.Stop();
            await this.Discovery.StopAsync();
            await this.Comms.StopAsync();
            this.Game.Stop();
            await this.DiscordBot.StopAsync();
        }

        private async Task Game_GameStarted(AmongUsGame sender, GameStartAsyncEventArgs e)
        {
            this.GameManager.ResetDeaths();
            if (this.GameManager.VoiceChannel != 0ul)
                await this.DiscordBot.MuteAllAsync(this.GameManager.VoiceChannel);

            var chn = this.GameManager.TextChannel;
            if (chn == 0ul)
                return;

            await this.DiscordBot.SendMessageAsync(chn, $"Game started, map: {e.Map}");
        }

        private async Task Game_GameEnded(AmongUsGame sender, GameEndAsyncEventArgs e)
        {
            if (this.GameManager.VoiceChannel != 0ul)
                await this.DiscordBot.UnmuteAllAsync(this.GameManager.VoiceChannel);

            var chn = this.GameManager.TextChannel;
            if (chn == 0ul)
                return;

            await this.DiscordBot.SendMessageAsync(chn, "Game ended");
        }

        private async Task Game_PlayerJoined(AmongUsGame sender, PlayerAsyncEventArgs e)
        {
            this.GameManager.AddPlayer(e.Player.Name);

            var chn = this.GameManager.TextChannel;
            if (chn == 0ul)
                return;

            await this.DiscordBot.SendMessageAsync(chn, $"New player: {e.Player.Name}");
        }

        private async Task Game_PlayerLeft(AmongUsGame sender, PlayerAsyncEventArgs e)
        {
            this.GameManager.RemovePlayer(e.Player.Name);

            var chn = this.GameManager.TextChannel;
            if (chn == 0ul)
                return;

            await this.DiscordBot.SendMessageAsync(chn, "Game ended");
        }

        private Task Game_PlayerDied(AmongUsGame sender, PlayerAsyncEventArgs e)
        {
            this.GameManager.MarkDead(e.Player.Name);
            return Task.CompletedTask;
        }

        private async Task Game_MeetingStarted(AmongUsGame sender, MeetingStartAsyncEventArgs e)
        {
            if (this.GameManager.VoiceChannel != 0ul)
            {
                var unmute = this.GameManager.GetUnmutables();
                var undeaf = this.GameManager.GetUndeafables();

                await this.DiscordBot.UndeafenAsync(this.GameManager.Guild, undeaf);
                await this.DiscordBot.UnmuteAsync(this.GameManager.Guild, unmute);
            }

            var chn = this.GameManager.TextChannel;
            if (chn == 0ul)
                return;

            await this.DiscordBot.SendMessageAsync(chn, "Meeting started");
        }

        private Task Game_MeetingEnded(AmongUsGame sender, MeetingEndAsyncEventArgs e)
        {
            if (this.GameManager.Guild == 0ul)
                return Task.CompletedTask;

            _ = Task.Delay(TimeSpan.FromSeconds(e.ExileDuration))
                .ContinueWith(this.Game_MeetingEnded_Continuation);

            return Task.CompletedTask;
        }

        private async Task Game_MeetingEnded_Continuation(Task _)
        {
            if (this.GameManager.VoiceChannel != 0ul)
            {
                var mute = this.GameManager.GetUnmutables();
                var unmute = this.GameManager.GetUndeafables();

                await this.DiscordBot.MuteAsync(this.GameManager.Guild, mute);
                await this.DiscordBot.UnmuteAsync(this.GameManager.Guild, unmute);
            }

            var chn = this.GameManager.TextChannel;
            if (chn == 0ul)
                return;

            await this.DiscordBot.SendMessageAsync(chn, "Meeting ended");
        }
    }
}
