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
    /// Represents a probe event.
    /// </summary>
    [MessagePackObject]
    public sealed class ProbeEvent
    {
        /// <summary>
        /// Gets the type of event that occured.
        /// </summary>
        [Key("eventType")]
        public ProbeEventType EventType { get; private set; }

        /// <summary>
        /// Gets the payload for the event.
        /// </summary>
        [Key("payload")]
        public ProbeEventPayload Payload { get; private set; }

        /// <summary>
        /// Creates a new event.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="payload">Payload for the event.</param>
        [SerializationConstructor]
        public ProbeEvent(ProbeEventType eventType, ProbeEventPayload payload)
        {
            this.EventType = eventType;
            this.Payload = payload;
        }
    }
}
