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

namespace Emzi0767.AmongUsDirector
{
    /// <summary>
    /// Determines the type of event received from the probe.
    /// </summary>
    public enum ProbeEventType : int
    {
        /// <summary>
        /// Defines an unknown event type.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Defines the event type to be a game start event.
        /// </summary>
        GameStart = 1,

        /// <summary>
        /// Defines the event type to be a game end event.
        /// </summary>
        GameEnd = 2,

        /// <summary>
        /// Defines the event type to be a player join event.
        /// </summary>
        PlayerJoin = 3,

        /// <summary>
        /// Defines the event type to be a player leave event.
        /// </summary>
        PlayerLeave = 4,

        /// <summary>
        /// Defines the event type to be a player death event.
        /// </summary>
        PlayerDeath = 5,

        /// <summary>
        /// Defines the event type to be a meeting start event.
        /// </summary>
        MeetingStart = 6,

        /// <summary>
        /// Defines the event type to be a meeting end event.
        /// </summary>
        MeetingEnd = 7,

        /// <summary>
        /// Defines the event to be a probe termination event.
        /// </summary>
        ProbeTermination = -1
    }
}
