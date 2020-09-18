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

using System.Threading;
using System.Threading.Tasks;
using Emzi0767.AmongUsDirector.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Emzi0767.AmongUsDirector
{
    public sealed class ProbeHostedService : IHostedService
    {
        private ILogger<ProbeHostedService> Logger { get; }
        private DiscoveryClient Discovery { get; }
        private AmongUsGame Game { get; }
        private ProbeCommArray Comms { get; }

        public ProbeHostedService(
            ILoggerFactory loggerFactory,
            DiscoveryClient discovery, 
            AmongUsGame game, 
            ProbeCommArray comms)
        {
            this.Logger = loggerFactory.CreateLogger<ProbeHostedService>();
            this.Discovery = discovery;
            this.Game = game;
            this.Comms = comms;

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
            var ep = await this.Discovery.PerformDiscoveryAsync();
            if (ep == null)
            {
                this.Logger.LogCritical("Probe could not communicate with mothership. Will continue to function.");
                return;
            }

            this.Logger.LogInformation("Discovered mothership: {0}", ep);
            await this.Comms.StartAsync(ep);
            this.Game.Start();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            this.Game.Stop();
            await this.Comms.SendEventAsync(new ProbeEvent(ProbeEventType.ProbeTermination, null));
            await this.Comms.StopAsync();
        }

        private async Task Game_GameStarted(AmongUsGame sender, GameStartAsyncEventArgs e)
            => await this.Comms.SendEventAsync(new ProbeEvent(ProbeEventType.GameStart, ProbeGameStartEventPayload.FromEventArgs(e)));

        private async Task Game_GameEnded(AmongUsGame sender, GameEndAsyncEventArgs e)
            => await this.Comms.SendEventAsync(new ProbeEvent(ProbeEventType.GameEnd, ProbeGameEndEventPayload.FromEventArgs(e)));

        private async Task Game_PlayerJoined(AmongUsGame sender, PlayerAsyncEventArgs e)
            => await this.Comms.SendEventAsync(new ProbeEvent(ProbeEventType.PlayerJoin, ProbePlayerEventPayload.FromEventArgs(e)));

        private async Task Game_PlayerLeft(AmongUsGame sender, PlayerAsyncEventArgs e)
            => await this.Comms.SendEventAsync(new ProbeEvent(ProbeEventType.PlayerLeave, ProbePlayerEventPayload.FromEventArgs(e)));

        private async Task Game_PlayerDied(AmongUsGame sender, PlayerAsyncEventArgs e)
            => await this.Comms.SendEventAsync(new ProbeEvent(ProbeEventType.PlayerDeath, ProbePlayerEventPayload.FromEventArgs(e)));

        private async Task Game_MeetingStarted(AmongUsGame sender, MeetingStartAsyncEventArgs e)
            => await this.Comms.SendEventAsync(new ProbeEvent(ProbeEventType.MeetingStart, ProbeMeetingStartEventPayload.FromEventArgs(e)));

        private async Task Game_MeetingEnded(AmongUsGame sender, MeetingEndAsyncEventArgs e)
            => await this.Comms.SendEventAsync(new ProbeEvent(ProbeEventType.MeetingEnd, ProbeMeetingEndEventPayload.FromEventArgs(e)));
    }
}
