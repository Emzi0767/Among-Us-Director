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
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Emzi0767.AmongUsDirector
{
    public sealed class ProbeCommArray
    {
        private const string HeaderUserAgent = "Among-Us-Probe";
        private const string MothershipResource = "/probe-endpoint";

        private ILogger<ProbeCommArray> Logger { get; }
        private ClientWebSocket AntennaArray { get; }
        private CancellationTokenSource CancellationTokenSource { get; }
        private CancellationToken CancellationToken => this.CancellationTokenSource.Token;
        private PayloadSerializer Serializer { get; }
        private SemaphoreSlim SendSemaphore { get; }
        private Task ReceiverTask { get; set; }

        public ProbeCommArray(PayloadSerializer serializer, ILoggerFactory loggerFactory)
        {
            this.Logger = loggerFactory.CreateLogger<ProbeCommArray>();

            this.AntennaArray = new ClientWebSocket();
            this.AntennaArray.Options.KeepAliveInterval = TimeSpan.FromSeconds(30);
            this.AntennaArray.Options.SetRequestHeader("User-Agent", HeaderUserAgent);

            this.CancellationTokenSource = new CancellationTokenSource();

            this.Serializer = serializer;
            this.SendSemaphore = new SemaphoreSlim(1, 1);
        }

        public async Task StartAsync(IPEndPoint ep)
        {
            var uri = new Uri($"ws://{ep.Address}:{ep.Port}{MothershipResource}");
            await this.AntennaArray.ConnectAsync(uri, this.CancellationToken);
            this.ReceiverTask = Task.Run(this.ReceiverLoop, this.CancellationToken);
            this.Logger.LogInformation("Connection established.");
        }

        public async Task StopAsync()
        {
            if (this.AntennaArray.State == WebSocketState.Open)
                await this.SendCloseAsync(WebSocketCloseStatus.NormalClosure, "Probe disconnected.");

            this.CancellationTokenSource.Cancel();
            this.CancellationTokenSource.Dispose();

            if (this.ReceiverTask != null)
                await this.ReceiverTask;
        }

        public async Task SendEventAsync(ProbeEvent payload)
        {
            if (this.AntennaArray.State != WebSocketState.Open)
                return;

            var bytes = this.Serializer.Serialize(payload);
            await this.SendSemaphore.WaitAsync(1);

            try
            {
                await this.AntennaArray.SendAsync(bytes, WebSocketMessageType.Binary, true, this.CancellationToken);
            }
            finally
            {
                this.SendSemaphore.Release(1);
            }
        }

        private async Task SendCloseAsync(WebSocketCloseStatus status, string message)
        {
            await this.SendSemaphore.WaitAsync(1);

            try
            {
                await this.AntennaArray.CloseAsync(status, message, CancellationToken.None);
            }
            finally
            {
                this.SendSemaphore.Release(1);
            }
        }

        private async Task ReceiverLoop()
        {
            var buff = new ArraySegment<byte>(new byte[4096]);

            var result = default(WebSocketReceiveResult);
            try
            {
                while (!this.CancellationToken.IsCancellationRequested
                    && this.AntennaArray.State == WebSocketState.Open)
                {
                    do
                    {
                        result = await this.AntennaArray.ReceiveAsync(buff, this.CancellationToken);
                        if (result.MessageType == WebSocketMessageType.Close)
                            break;
                    }
                    while (!result.EndOfMessage);
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogCritical(ex, "Exception occured while communicating game events.");
            }

            if (result != null && this.AntennaArray.State == WebSocketState.CloseReceived)
                await this.SendCloseAsync(result.CloseStatus.Value, result.CloseStatusDescription);
        }
    }
}
