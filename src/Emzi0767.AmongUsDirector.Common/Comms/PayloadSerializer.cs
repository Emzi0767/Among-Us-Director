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
using MessagePack.Resolvers;

namespace Emzi0767.AmongUsDirector
{
    /// <summary>
    /// Handles serializing and deserializing prboe payloads.
    /// </summary>
    public sealed class PayloadSerializer
    {
        private IFormatterResolver Resolver { get; }
        private MessagePackSerializerOptions Options { get; }

        /// <summary>
        /// Creates a new serializer.
        /// </summary>
        public PayloadSerializer()
        {
            this.Resolver = StandardResolverAllowPrivate.Instance;
            this.Options = MessagePackSerializerOptions.Standard
                .WithAllowAssemblyVersionMismatch(false)
                .WithCompression(MessagePackCompression.None)
                .WithOldSpec(false)
                .WithOmitAssemblyVersion(false)
                .WithResolver(this.Resolver)
                .WithSecurity(MessagePackSecurity.UntrustedData);
        }

        /// <summary>
        /// Serializes a payload.
        /// </summary>
        /// <typeparam name="T">Type of payload to serialize.</typeparam>
        /// <param name="payload">Payload to serialize.</param>
        /// <returns>Serialized payload.</returns>
        public byte[] Serialize<T>(T payload)
            => MessagePackSerializer.Serialize(payload, this.Options);

        /// <summary>
        /// Deserializes a payload.
        /// </summary>
        /// <typeparam name="T">Type of payload to deserialize.</typeparam>
        /// <param name="payload">Payload to deserialize.</param>
        /// <returns>Deserialized payload.</returns>
        public T Deserialize<T>(byte[] payload)
            => MessagePackSerializer.Deserialize<T>(payload, this.Options);
    }
}
