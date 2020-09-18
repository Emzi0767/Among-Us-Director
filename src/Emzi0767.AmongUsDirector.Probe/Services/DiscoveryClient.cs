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
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Emzi0767.AmongUsDirector.Services
{
    public sealed class DiscoveryClient
    {
        private const string ProbeMessage = "AMONG US PROBE";
        private const string ProbeAcknowledge = "AMONG US BOT";

        private int Port { get; }

        public DiscoveryClient(IOptions<ProbeConfiguration> config)
        {
            this.Port = config.Value.DiscoveryPort;
        }

        public async Task<IPEndPoint> PerformDiscoveryAsync()
        {
            var buff = this.PrepareBuffer();
            var validate = this.PrepareValidationBuffer();

            // IPv6 no worky so far
            //using var udp6 = new UdpClient(AddressFamily.InterNetworkV6);
            //udp6.Client.Bind(new IPEndPoint(IPAddress.IPv6Any, this.Port));
            //udp6.Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.AddMembership, new IPv6MulticastOption(IPAddress.Parse("ff02::1")));

            using var udp4 = new UdpClient(AddressFamily.InterNetwork);
            udp4.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udp4.ExclusiveAddressUse = false;
            udp4.Client.Bind(new IPEndPoint(IPAddress.Any, this.Port - 1));

            //await udp6.SendAsync(buff, buff.Length);
            await udp4.SendAsync(buff, buff.Length, new IPEndPoint(IPAddress.Broadcast, this.Port));

            //var tcs6 = new TaskCompletionSource<IPEndPoint>();
            var tcs4 = new TaskCompletionSource<IPEndPoint>();
            var delay = Task.Delay(TimeSpan.FromSeconds(2.5));

            //await Task.WhenAll(
            //    this.ReceiveUntil(udp6, tcs6, validate, delay),
            //    this.ReceiveUntil(udp4, tcs4, validate, delay));
            await this.ReceiveUntil(udp4, tcs4, validate, delay);

            //var t6 = tcs6.Task;
            //if (!t6.IsCanceled && t6.Result != null)
            //    return t6.Result;

            var t4 = tcs4.Task;
            if (!t4.IsCanceled && t4.Result != null)
                return t4.Result;

            return null;
        }

        private async Task ReceiveUntil(UdpClient udp, TaskCompletionSource<IPEndPoint> tcs, byte[] validate, Task delay)
        {
            while (true)
            {
                var trecv = udp.ReceiveAsync();
                var t = await Task.WhenAny(trecv, delay);
                if (t == trecv && !trecv.IsFaulted)
                {
                    _ = this.Handle(trecv.Result, tcs, validate);
                }
                else
                {
                    if (!tcs.Task.IsCompleted)
                        tcs.SetCanceled();
                    break;
                }
            }
        }

        private async Task Handle(UdpReceiveResult recv, TaskCompletionSource<IPEndPoint> tcs, byte[] validate)
        {
            await Task.Yield();

            if (recv.Buffer != null && this.ValidateBuffer(recv.Buffer, validate))
                tcs.SetResult(recv.RemoteEndPoint);
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
