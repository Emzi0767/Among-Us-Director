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
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Emzi0767.AmongUsDirector
{
    /// <summary>
    /// Contains operations pertaining to the game's process.
    /// </summary>
    public sealed class GameProcess : IDisposable
    {
        private const string ProcessName = "Among Us";
        private const string ModuleName = "GameAssembly.dll";

        private readonly Process _proc;
        private readonly IntPtrEx _module;
        private readonly int _moduleSize;

        private readonly CancellationTokenSource _cts;
        private readonly CancellationToken _ct;
        private Thread _eventDispatcherThread;

        private GameProcess(Process p, IntPtrEx gameModule, int moduleSize)
        {
            this._proc = p;
            this._module = gameModule;
            this._moduleSize = moduleSize;
            this._cts = new CancellationTokenSource();
            this._ct = this._cts.Token;

            this._proc.Exited += this._proc_Exited;
        }

        /// <summary>
        /// Starts the event dispatcher for this game process.
        /// </summary>
        public void Start()
        {
            this._eventDispatcherThread = new Thread(this.EventDispatcherLoop);
            this._eventDispatcherThread.SetApartmentState(ApartmentState.MTA);
            this._eventDispatcherThread.Start();
        }

        /// <summary>
        /// Disposes of this process.
        /// </summary>
        public void Dispose()
        {
            this._cts.Cancel();
            this._cts.Dispose();
            try
            {
                this._eventDispatcherThread.Join();
            }
            catch { }
        }

        /// <summary>
        /// Triggered whenever game starts.
        /// </summary>
        public event EventHandler<GameStartEventArgs> GameStarted;
        private void TriggerEvent(object sender, GameStartEventArgs ea)
        {
            if (this.GameStarted != null)
                this.GameStarted(sender, ea);
        }

        /// <summary>
        /// Triggered whenever game ends.
        /// </summary>
        public event EventHandler<GameEndEventArgs> GameEnded;
        private void TriggerEvent(object sender, GameEndEventArgs ea)
        {
            if (this.GameEnded != null)
                this.GameEnded(sender, ea);
        }

        /// <summary>
        /// Triggered whenever a player joins.
        /// </summary>
        public event EventHandler<PlayerJoinEventArgs> PlayerJoined;
        private void TriggerEvent(object sender, PlayerJoinEventArgs ea)
        {
            if (this.PlayerJoined != null)
                this.PlayerJoined(sender, ea);
        }

        /// <summary>
        /// Triggered whenenver a player leaves.
        /// </summary>
        public event EventHandler<PlayerLeaveEventArgs> PlayerLeft;
        private void TriggerEvent(object sender, PlayerLeaveEventArgs ea)
        {
            if (this.PlayerLeft != null)
                this.PlayerLeft(sender, ea);
        }

        /// <summary>
        /// Triggered whenever a player dies.
        /// </summary>
        public event EventHandler<PlayerDeathEventArgs> PlayerDied;
        private void TriggerEvent(object sender, PlayerDeathEventArgs ea)
        {
            if (this.PlayerDied != null)
                this.PlayerDied(sender, ea);
        }

        /// <summary>
        /// Triggered whenever the impostor status of a player changes.
        /// </summary>
        public event EventHandler<PlayerImpostorStatusChangeEventArgs> PlayerImpostorStatusChanged;
        private void TriggerEvent(object sender, PlayerImpostorStatusChangeEventArgs ea)
        {
            if (this.PlayerImpostorStatusChanged != null)
                this.PlayerImpostorStatusChanged(sender, ea);
        }

        /// <summary>
        /// Triggered whenever a meeting starts.
        /// </summary>
        public event EventHandler<MeetingStartEventArgs> MeetingStarted;
        private void TriggerEvent(object sender, MeetingStartEventArgs ea)
        {
            if (this.MeetingStarted != null)
                this.MeetingStarted(sender, ea);
        }

        /// <summary>
        /// Triggered whenever a meeting ends.
        /// </summary>
        public event EventHandler<MeetingEndEventArgs> MeetingEnded;
        private void TriggerEvent(object sender, MeetingEndEventArgs ea)
        {
            if (this.MeetingEnded != null)
                this.MeetingEnded(sender, ea);
        }

        private void EventDispatcherLoop()
        {
            var mem = new ProcessMemory(this._proc);
            var reader = new GameReader(mem, this._module, this._moduleSize);
            reader.GameStarted += (o, e) => this.TriggerEvent(o, e);
            reader.GameEnded += (o, e) => this.TriggerEvent(o, e);
            reader.PlayerJoined += (o, e) => this.TriggerEvent(o, e);
            reader.PlayerLeft += (o, e) => this.TriggerEvent(o, e);
            reader.PlayerDied += (o, e) => this.TriggerEvent(o, e);
            reader.PlayerImpostorStatusChanged += (o, e) => this.TriggerEvent(o, e);
            reader.MeetingStarted += (o, e) => this.TriggerEvent(o, e);
            reader.MeetingEnded += (o, e) => this.TriggerEvent(o, e);

            while (!this._ct.IsCancellationRequested)
            {
                reader.DoRead();
                Thread.Yield();
            }
        }

        private void _proc_Exited(object sender, EventArgs e)
        {
            this.Dispose();
        }

        /// <summary>
        /// Attaches to game process, and returns a method allowing reading it.
        /// </summary>
        /// <returns>Game process wrapper.</returns>
        public static unsafe GameProcess Attach()
        {
            // get the process
            var proc = Process.GetProcessesByName(ProcessName).First();

            // enumerate modules
            const int mcount = 256;
            var hmodules = stackalloc IntPtr[mcount];
            var sphmodules = new Span<IntPtr>(hmodules, mcount);
            if (!PInvoke.EnumerateProcessModules(proc.Handle, hmodules, sphmodules.Length * IntPtr.Size, out var writtenBytes))
                throw new InvalidProcessException();

            // compute module count
            var moduleCount = writtenBytes / IntPtr.Size;

            // alloc buffer for names
            const int nsize = 256;
            var hname = stackalloc char[nsize];

            // find the module
            IntPtr module = IntPtr.Zero;
            for (var i = 0; i < moduleCount; ++i)
            {
                var len = PInvoke.GetModuleBaseName(proc.Handle, sphmodules[i], hname, nsize);
                if (len == 0)
                    continue;

                var n = new string(hname, 0, (int)len);
                if (n == ModuleName)
                {
                    module = sphmodules[i];
                    break;
                }
            }

            if (module == IntPtr.Zero)
                throw new InvalidProcessException();

            var modInfo = default(ModuleInfo);
            if (!PInvoke.GetModuleInformation(proc.Handle, module, &modInfo, sizeof(ModuleInfo)))
                throw new InvalidProcessException();

            return new GameProcess(proc, module, modInfo.SizeOfImage);
        }
    }
}
