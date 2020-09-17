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
using Emzi0767.Utilities;
using Microsoft.Extensions.Logging;

namespace Emzi0767.AmongUsDirector.Services
{
    public sealed class AmongUsGame
    {
        private GameProcess Process { get; }
        private AsyncExecutor Async { get; }
        private ILogger<AmongUsGame> Logger { get; }

        private readonly AsyncEvent<AmongUsGame, GameStartAsyncEventArgs> _gameStarted;
        private readonly AsyncEvent<AmongUsGame, GameEndAsyncEventArgs> _gameEnded;
        private readonly AsyncEvent<AmongUsGame, PlayerAsyncEventArgs> _playerJoined;
        private readonly AsyncEvent<AmongUsGame, PlayerAsyncEventArgs> _playerLeft;
        private readonly AsyncEvent<AmongUsGame, PlayerAsyncEventArgs> _playerDied;
        private readonly AsyncEvent<AmongUsGame, MeetingStartAsyncEventArgs> _meetingStarted;
        private readonly AsyncEvent<AmongUsGame, MeetingEndAsyncEventArgs> _meetingEnded;

        public AmongUsGame(AsyncExecutor async, ILoggerFactory loggerFactory)
        {
            this.Async = async;
            this.Logger = loggerFactory.CreateLogger<AmongUsGame>();

            this._gameStarted = new AsyncEvent<AmongUsGame, GameStartAsyncEventArgs>("AMONGUS_GAME_STARTED", TimeSpan.Zero, this.AsyncEventExceptionHandler);
            this._gameEnded = new AsyncEvent<AmongUsGame, GameEndAsyncEventArgs>("AMONGUS_GAME_ENDED", TimeSpan.Zero, this.AsyncEventExceptionHandler);
            this._playerJoined = new AsyncEvent<AmongUsGame, PlayerAsyncEventArgs>("AMONGUS_PLAYER_JOINED", TimeSpan.Zero, this.AsyncEventExceptionHandler);
            this._playerLeft = new AsyncEvent<AmongUsGame, PlayerAsyncEventArgs>("AMONGUS_PLAYER_LEFT", TimeSpan.Zero, this.AsyncEventExceptionHandler);
            this._playerDied = new AsyncEvent<AmongUsGame, PlayerAsyncEventArgs>("AMONGUS_PLAYER_DIED", TimeSpan.Zero, this.AsyncEventExceptionHandler);
            this._meetingStarted = new AsyncEvent<AmongUsGame, MeetingStartAsyncEventArgs>("AMONGUS_MEETING_STARTED", TimeSpan.Zero, this.AsyncEventExceptionHandler);
            this._meetingEnded = new AsyncEvent<AmongUsGame, MeetingEndAsyncEventArgs>("AMONGUS_MEETING_ENDED", TimeSpan.Zero, this.AsyncEventExceptionHandler);

            this.Process = GameProcess.Attach();

            this.Process.GameStarted += this.Process_GameStarted;
            this.Process.GameEnded += this.Process_GameEnded;

            this.Process.PlayerJoined += this.Process_PlayerJoined;
            this.Process.PlayerLeft += this.Process_PlayerLeft;
            this.Process.PlayerDied += this.Process_PlayerDied;

            this.Process.MeetingStarted += this.Process_MeetingStarted;
            this.Process.MeetingEnded += this.Process_MeetingEnded;
        }

        public void Start()
            => this.Process.Start();

        public void Stop()
        {
            this._gameStarted.UnregisterAll();
            this._gameEnded.UnregisterAll();
            this._playerJoined.UnregisterAll();
            this._playerLeft.UnregisterAll();
            this._playerDied.UnregisterAll();
            this._meetingStarted.UnregisterAll();
            this._meetingEnded.UnregisterAll();

            this.Process.Dispose();
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

        private void Process_GameStarted(object sender, GameStartEventArgs e)
        { 
            this.Logger.LogInformation("Game started: map {0}", e.Map);
            this.Async.Execute(this._gameStarted.InvokeAsync(this, new GameStartAsyncEventArgs(e)));
        }

        private void Process_GameEnded(object sender, GameEndEventArgs e)
        {
            this.Logger.LogInformation("Game ended");
            this.Async.Execute(this._gameEnded.InvokeAsync(this, new GameEndAsyncEventArgs(e)));
        }

        private void Process_PlayerJoined(object sender, PlayerJoinEventArgs e)
        {
            this.Logger.LogInformation("Player joined: {0}", e.Player.Name);
            this.Async.Execute(this._playerJoined.InvokeAsync(this, new PlayerAsyncEventArgs(e)));
        }

        private void Process_PlayerLeft(object sender, PlayerLeaveEventArgs e)
        {
            this.Logger.LogInformation("Meeting left: {0}", e.Player.Name);
            this.Async.Execute(this._playerLeft.InvokeAsync(this, new PlayerAsyncEventArgs(e)));
        }

        private void Process_PlayerDied(object sender, PlayerDeathEventArgs e)
        {
            this.Logger.LogDebug("Player died: {0}", e.Player.Name);
            this.Async.Execute(this._playerDied.InvokeAsync(this, new PlayerAsyncEventArgs(e)));
        }

        private void Process_MeetingStarted(object sender, MeetingStartEventArgs e)
        {
            this.Logger.LogInformation("Meeting started");
            this.Async.Execute(this._meetingStarted.InvokeAsync(this, new MeetingStartAsyncEventArgs(e)));
        }

        private void Process_MeetingEnded(object sender, MeetingEndEventArgs e)
        {
            this.Logger.LogInformation("Meeting ended: exile after {0:0.0}s", e.ExileDuration);
            this.Async.Execute(this._meetingEnded.InvokeAsync(this, new MeetingEndAsyncEventArgs(e)));
        }

        private void AsyncEventExceptionHandler<TArgs>(AsyncEvent<AmongUsGame, TArgs> asyncEvent, Exception exception, AsyncEventHandler<AmongUsGame, TArgs> handler, AmongUsGame sender, TArgs eventArgs)
            where TArgs : AsyncEventArgs
            => this.Logger.LogError(exception, "An exception occured while handling {0} game event.", asyncEvent.Name);
    }
}
