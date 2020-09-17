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
    /// Advanced <see cref="IntPtr"/> with arithmetic functions, cause nint no exist in 3.x yet.
    /// </summary>
    public readonly struct IntPtrEx : IEquatable<IntPtrEx>, IEquatable<IntPtr>
    {
        /// <summary>
        /// Gets the underlying pointer.
        /// </summary>
        public IntPtr Pointer { get; }

        /// <summary>
        /// Creates a new pointer wrapper from given pointer.
        /// </summary>
        /// <param name="ptr">Pointer to wrap.</param>
        public IntPtrEx(IntPtr ptr)
        {
            this.Pointer = ptr;
        }

        /// <summary>
        /// Checks whether this pointer is equal to another pointer.
        /// </summary>
        /// <param name="other">Pointer to compare against.</param>
        /// <returns>Whether the 2 pointers are equal.</returns>
        public bool Equals(IntPtrEx other)
            => this == other;

        /// <summary>
        /// Checks whether this pointer is equal to another pointer.
        /// </summary>
        /// <param name="other">Pointer to compare against.</param>
        /// <returns>Whether the 2 pointers are equal.</returns>
        public bool Equals(IntPtr other)
            => this.Pointer == other;

        /// <summary>
        /// Checks whether this pointer is equal to another object.
        /// </summary>
        /// <param name="obj">Object to compare against.</param>
        /// <returns>Whether this object is equal to the other object.</returns>
        public override bool Equals(object obj)
        {
            if (obj is IntPtrEx ptrex)
                return this.Equals(ptrex);

            if (obj is IntPtr ptr)
                return this.Equals(ptr);

            return false;
        }

        /// <summary>
        /// Gets a hash code identifying this pointer.
        /// </summary>
        /// <returns>Hash code of this pointer.</returns>
        public override int GetHashCode()
            => this.Pointer.GetHashCode();

        /// <summary>
        /// Converts this pointer to string.
        /// </summary>
        /// <returns>String representation of this pointer.</returns>
        public override string ToString()
            => this.Pointer.ToString();

        public static implicit operator IntPtrEx(IntPtr ptr)
            => new IntPtrEx(ptr);

        public static explicit operator IntPtr(IntPtrEx ptrex)
            => ptrex.Pointer;

        public static bool operator ==(IntPtrEx left, IntPtrEx right)
            => left.Pointer == right.Pointer;

        public static bool operator !=(IntPtrEx left, IntPtrEx right)
            => left.Pointer != right.Pointer;

        public static unsafe IntPtrEx operator +(IntPtrEx left, int right)
            => new IntPtr((byte*)left.Pointer.ToPointer() + right);

        public static unsafe IntPtrEx operator +(IntPtrEx left, long right)
            => new IntPtr((byte*)left.Pointer.ToPointer() + right);

        public static unsafe IntPtrEx operator -(IntPtrEx left, int right)
            => left + -right;

        public static unsafe IntPtrEx operator -(IntPtrEx left, long right)
            => left + -right;
    }
}
