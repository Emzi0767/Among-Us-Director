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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Emzi0767.AmongUsDirector.Services
{
    public sealed class DiscoveryServer
    {
        private const string ProbeAcknowledge = "AMONG US PROBE";
        private const string ProbeMessage = "AMONG US BOT";

        private int Port { get; }
        private UdpClient Udp { get; }
        private CancellationTokenSource CancellationTokenSource { get; }
        private CancellationToken CancellationToken => this.CancellationTokenSource.Token;
        private Task ReceiverTask { get; set; }

        public DiscoveryServer(IOptions<BotConfiguration> config)
        {
            this.Port = config.Value.DiscoveryPort;

            this.Udp = new UdpClient(AddressFamily.InterNetwork);
            this.Udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            this.Udp.ExclusiveAddressUse = false;

            this.CancellationTokenSource = new CancellationTokenSource();
        }

        public void Start()
        {
            this.Udp.Client.Bind(new IPEndPoint(IPAddress.Any, this.Port));
            this.ReceiverTask = Task.Run(this.ReceiverLoop, this.CancellationToken);
        }

        public async Task StopAsync()
        {
            this.CancellationTokenSource.Cancel();
            this.CancellationTokenSource.Dispose();

            this.Udp.Close();
            await this.ReceiverTask;
        }

        private async Task ReceiverLoop()
        {
            var delay = Task.Delay(-1, this.CancellationToken);

            var validate = this.PrepareValidationBuffer();
            var buff = this.PrepareBuffer();
            
            while (true)
            {
                var trecv = this.Udp.ReceiveAsync();
                var t = await Task.WhenAny(delay, trecv);
                if (t == trecv && !trecv.IsFaulted)
                    _ = this.Handle(trecv.Result, buff, validate);
                else
                    break;
            }
        }

        private async Task Handle(UdpReceiveResult recv, byte[] buff, byte[] validate)
        {
            await Task.Yield();

            if (recv.Buffer != null && this.ValidateBuffer(recv.Buffer, validate))
            {
                var ep = recv.RemoteEndPoint;
                ep = new IPEndPoint(ep.Address, this.Port - 1);
                await this.Udp.SendAsync(buff, buff.Length, ep);
            }
        }

        private byte[] PrepareBuffer()
        {
            var len = Encoding.ASCII.GetByteCount(ProbeMessage);
            var buff = new byte[len];
            Encoding.ASCII.GetBytes(ProbeMessage, buff);
            return buff;
        }

        private byte[] PrepareValidationBuffer()
        {
            var len = Encoding.ASCII.GetByteCount(ProbeAcknowledge);
            var buff = new byte[len];
            Encoding.ASCII.GetBytes(ProbeAcknowledge, buff);
            return buff;
        }

        private bool ValidateBuffer(byte[] buff, byte[] validate)
            => buff.AsSpan().SequenceEqual(validate);
    }
}
