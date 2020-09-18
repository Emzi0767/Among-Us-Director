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

using Emzi0767.Utilities;

namespace Emzi0767.AmongUsDirector
{
    /// <summary>
    /// Wraps <see cref="PlayerJoinEventArgs"/>, <see cref="PlayerLeaveEventArgs"/>, or <see cref="PlayerDeathEventArgs"/>.
    /// </summary>
    public sealed class PlayerAsyncEventArgs : AsyncEventArgs
    {
        /// <summary>
        /// Gets the player who caused the event.
        /// </summary>
        public Player Player { get; }

        /// <summary>
        /// Creates a new instance of wrapper event args.
        /// </summary>
        /// <param name="e">Wrapped event's arguments.</param>
        internal PlayerAsyncEventArgs(PlayerJoinEventArgs e)
        {
            this.Player = e.Player;
        }

        /// <summary>
        /// Creates a new instance of wrapper event args.
        /// </summary>
        /// <param name="e">Wrapped event's arguments.</param>
        internal PlayerAsyncEventArgs(PlayerLeaveEventArgs e)
        {
            this.Player = e.Player;
        }

        /// <summary>
        /// Creates a new instance of wrapper event args.
        /// </summary>
        /// <param name="e">Wrapped event's arguments.</param>
        internal PlayerAsyncEventArgs(PlayerDeathEventArgs e)
        {
            this.Player = e.Player;
        }

        /// <summary>
        /// Creates a new instance of event args from data.
        /// </summary>
        /// <param name="id">ID of the player.</param>
        /// <param name="name">Name of the player.</param>
        /// <param name="dead">Whether the player is dead.</param>
        /// <param name="impostor">Whether the player is an impostor.</param>
        internal PlayerAsyncEventArgs(sbyte id, string name, bool dead, bool impostor)
        {
            this.Player = new Player(id, name, dead, impostor);
        }
    }
}
