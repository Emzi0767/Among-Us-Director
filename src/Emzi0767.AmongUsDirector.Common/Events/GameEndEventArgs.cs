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

namespace Emzi0767.AmongUsDirector
{
    /// <summary>
    /// Indicates the game has ended.
    /// </summary>
    public sealed class GameEndEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new instance of the event args.
        /// </summary>
        internal GameEndEventArgs()
        { }
    }
}
