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

using Emzi0767.Utilities;

namespace Emzi0767.AmongUsDirector
{
    /// <summary>
    /// Wraps <see cref="GameEndEventArgs"/>.
    /// </summary>
    public sealed class GameEndAsyncEventArgs : AsyncEventArgs
    {
        /// <summary>
        /// Creates a new instance of wrapper event args.
        /// </summary>
        /// <param name="e">Wrapped event's arguments.</param>
        internal GameEndAsyncEventArgs(GameEndEventArgs e)
        { }

        /// <summary>
        /// Creates a new instance of event args from data.
        /// </summary>
        internal GameEndAsyncEventArgs()
        { }
    }
}
