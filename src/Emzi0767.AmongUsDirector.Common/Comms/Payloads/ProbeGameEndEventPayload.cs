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

using MessagePack;

namespace Emzi0767.AmongUsDirector
{
    /// <summary>
    /// Contains arguments for game start event.
    /// </summary>
    [MessagePackObject]
    public sealed class ProbeGameEndEventPayload : ProbeEventPayload
    {
        /// <summary>
        /// Creates event args from this payload.
        /// </summary>
        /// <returns>Constructed event args.</returns>
        public GameEndAsyncEventArgs ToEventArgs()
            => new GameEndAsyncEventArgs();

        /// <summary>
        /// Creates a payload from event args.
        /// </summary>
        /// <param name="e">Event args to construct from.</param>
        /// <returns>Constructed event payload.</returns>
        public static ProbeGameEndEventPayload FromEventArgs(GameEndAsyncEventArgs e)
            => new ProbeGameEndEventPayload();
    }
}
