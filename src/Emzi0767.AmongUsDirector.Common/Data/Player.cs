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

namespace Emzi0767.AmongUsDirector
{
    /// <summary>
    /// Represents a player.
    /// </summary>
    public sealed class Player
    {
        /// <summary>
        /// Gets the player's ID.
        /// </summary>
        public sbyte Id { get; }

        /// <summary>
        /// Gets the player's name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets whether the player is dead.
        /// </summary>
        public bool IsDead { get; internal set; }

        /// <summary>
        /// Gets whether the player is an impostor.
        /// </summary>
        public bool IsImpostor { get; internal set; }

        public Player(sbyte id, string name, bool dead, bool impostor)
        {
            this.Id = id;
            this.Name = name;
            this.IsDead = dead;
            this.IsImpostor = impostor;
        }

        /// <summary>
        /// Compares this player to another object.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>Whether the the object is a player and is equal to this player.</returns>
        public override bool Equals(object obj)
            => obj is Player other
            && other.Id == this.Id
            && other.Name == this.Name;

        /// <summary>
        /// Gets the hash code of this object.
        /// </summary>
        /// <returns>Hash code of this object.</returns>
        public override int GetHashCode()
            => HashCode.Combine(this.Id, this.Name);

        /// <summary>
        /// Converts this player to a string representation.
        /// </summary>
        /// <returns>String representation of this player.</returns>
        public override string ToString()
            => $"ID: {this.Id}; Name: {this.Name}; Impostor: {this.IsImpostor}; Dead: {this.IsDead}";

        public static bool operator ==(Player left, Player right)
            => left.Id == right.Id && left.Name == right.Name;

        public static bool operator !=(Player left, Player right)
            => left.Id != right.Id || left.Name != right.Name;
    }
}
