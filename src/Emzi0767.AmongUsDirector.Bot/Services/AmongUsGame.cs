﻿// This file is part of Among Us Director project.
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
using Emzi0767.Utilities;
using Microsoft.Extensions.Logging;

namespace Emzi0767.AmongUsDirector
{
    public sealed class AmongUsGame
    {
        private ILogger<AmongUsGame> Logger { get; }
        private MothershipCommArray Comms { get; }

        private readonly AsyncEvent<AmongUsGame, GameStartAsyncEventArgs> _gameStarted;
        private readonly AsyncEvent<AmongUsGame, GameEndAsyncEventArgs> _gameEnded;
        private readonly AsyncEvent<AmongUsGame, PlayerAsyncEventArgs> _playerJoined;
        private readonly AsyncEvent<AmongUsGame, PlayerAsyncEventArgs> _playerLeft;
        private readonly AsyncEvent<AmongUsGame, PlayerAsyncEventArgs> _playerDied;
        private readonly AsyncEvent<AmongUsGame, MeetingStartAsyncEventArgs> _meetingStarted;
        private readonly AsyncEvent<AmongUsGame, MeetingEndAsyncEventArgs> _meetingEnded;

        public AmongUsGame(ILoggerFactory loggerFactory, MothershipCommArray comms)
        {
            this.Logger = loggerFactory.CreateLogger<AmongUsGame>();
            this.Comms = comms;

            this._gameStarted = new AsyncEvent<AmongUsGame, GameStartAsyncEventArgs>("AMONGUS_GAME_STARTED", TimeSpan.Zero, this.AsyncEventExceptionHandler);
            this._gameEnded = new AsyncEvent<AmongUsGame, GameEndAsyncEventArgs>("AMONGUS_GAME_ENDED", TimeSpan.Zero, this.AsyncEventExceptionHandler);
            this._playerJoined = new AsyncEvent<AmongUsGame, PlayerAsyncEventArgs>("AMONGUS_PLAYER_JOINED", TimeSpan.Zero, this.AsyncEventExceptionHandler);
            this._playerLeft = new AsyncEvent<AmongUsGame, PlayerAsyncEventArgs>("AMONGUS_PLAYER_LEFT", TimeSpan.Zero, this.AsyncEventExceptionHandler);
            this._playerDied = new AsyncEvent<AmongUsGame, PlayerAsyncEventArgs>("AMONGUS_PLAYER_DIED", TimeSpan.Zero, this.AsyncEventExceptionHandler);
            this._meetingStarted = new AsyncEvent<AmongUsGame, MeetingStartAsyncEventArgs>("AMONGUS_MEETING_STARTED", TimeSpan.Zero, this.AsyncEventExceptionHandler);
            this._meetingEnded = new AsyncEvent<AmongUsGame, MeetingEndAsyncEventArgs>("AMONGUS_MEETING_ENDED", TimeSpan.Zero, this.AsyncEventExceptionHandler);

            this.Comms.ProbeEventReceived += this.Comms_ProbeEventReceived;
        }

        public void Stop()
        {
            this._gameStarted.UnregisterAll();
            this._gameEnded.UnregisterAll();
            this._playerJoined.UnregisterAll();
            this._playerLeft.UnregisterAll();
            this._playerDied.UnregisterAll();
            this._meetingStarted.UnregisterAll();
            this._meetingEnded.UnregisterAll();
        }

        public event AsyncEventHandler<AmongUsGame, GameStartAsyncEventArgs> GameStarted
        {
            add => this._gameStarted.Register(value);
            remove => this._gameStarted.Unregister(value);
        }

        public event AsyncEventHandler<AmongUsGame, GameEndAsyncEventArgs> GameEnded
        {
            add => this._gameEnded.Register(value);
            remove => this._gameEnded.Unregister(value);
        }

        public event AsyncEventHandler<AmongUsGame, PlayerAsyncEventArgs> PlayerJoined
        {
            add => this._playerJoined.Register(value);
            remove => this._playerJoined.Unregister(value);
        }

        public event AsyncEventHandler<AmongUsGame, PlayerAsyncEventArgs> PlayerLeft
        {
            add => this._playerLeft.Register(value);
            remove => this._playerLeft.Unregister(value);
        }

        public event AsyncEventHandler<AmongUsGame, PlayerAsyncEventArgs> PlayerDied
        {
            add => this._playerDied.Register(value);
            remove => this._playerDied.Unregister(value);
        }

        public event AsyncEventHandler<AmongUsGame, MeetingStartAsyncEventArgs> MeetingStarted
        {
            add => this._meetingStarted.Register(value);
            remove => this._meetingStarted.Unregister(value);
        }

        public event AsyncEventHandler<AmongUsGame, MeetingEndAsyncEventArgs> MeetingEnded
        {
            add => this._meetingEnded.Register(value);
            remove => this._meetingEnded.Unregister(value);
        }

        private async Task Process_GameStarted(ProbeGameStartEventPayload e)
        {
            var args = e.ToEventArgs();
            this.Logger.LogInformation("Game started: map {0}", e.Map);
            await this._gameStarted.InvokeAsync(this, args);
        }

        private async Task Process_GameEnded(ProbeGameEndEventPayload e)
        {
            var args = e.ToEventArgs();
            this.Logger.LogInformation("Game ended");
            await this._gameEnded.InvokeAsync(this, args);
        }

        private async Task Process_PlayerJoined(ProbePlayerEventPayload e)
        {
            var args = e.ToEventArgs();
            this.Logger.LogInformation("Player joined: {0}", e.Name);
            await this._playerJoined.InvokeAsync(this, args);
        }

        private async Task Process_PlayerLeft(ProbePlayerEventPayload e)
        {
            var args = e.ToEventArgs();
            this.Logger.LogInformation("Meeting left: {0}", e.Name);
            await this._playerLeft.InvokeAsync(this, args);
        }

        private async Task Process_PlayerDied(ProbePlayerEventPayload e)
        {
            var args = e.ToEventArgs();
            this.Logger.LogDebug("Player died: {0}", e.Name);
            await this._playerDied.InvokeAsync(this, args);
        }

        private async Task Process_MeetingStarted(ProbeMeetingStartEventPayload e)
        {
            var args = e.ToEventArgs();
            this.Logger.LogInformation("Meeting started");
            await this._meetingStarted.InvokeAsync(this, args);
        }

        private async Task Process_MeetingEnded(ProbeMeetingEndEventPayload e)
        {
            var args = e.ToEventArgs();
            this.Logger.LogInformation("Meeting ended: exile after {0:0.0}s", e.ExileDuration);
            await this._meetingEnded.InvokeAsync(this, args);
        }

        private void AsyncEventExceptionHandler<TArgs>(AsyncEvent<AmongUsGame, TArgs> asyncEvent, Exception exception, AsyncEventHandler<AmongUsGame, TArgs> handler, AmongUsGame sender, TArgs eventArgs)
            where TArgs : AsyncEventArgs
            => this.Logger.LogError(exception, "An exception occured while handling {0} game event.", asyncEvent.Name);

        private async Task Comms_ProbeEventReceived(MothershipCommArray sender, ProbeEventArgs e)
        {
            await (e.Event.EventType switch
            {
                ProbeEventType.GameStart => this.Process_GameStarted(e.Event.Payload as ProbeGameStartEventPayload),
                ProbeEventType.GameEnd => this.Process_GameEnded(e.Event.Payload as ProbeGameEndEventPayload),
                ProbeEventType.PlayerJoin => this.Process_PlayerJoined(e.Event.Payload as ProbePlayerEventPayload),
                ProbeEventType.PlayerLeave => this.Process_PlayerLeft(e.Event.Payload as ProbePlayerEventPayload),
                ProbeEventType.PlayerDeath => this.Process_PlayerDied(e.Event.Payload as ProbePlayerEventPayload),
                ProbeEventType.MeetingStart => this.Process_MeetingStarted(e.Event.Payload as ProbeMeetingStartEventPayload),
                ProbeEventType.MeetingEnd => this.Process_MeetingEnded(e.Event.Payload as ProbeMeetingEndEventPayload),

                _ => Task.CompletedTask
            });
        }
    }
}
