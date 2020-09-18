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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Emzi0767.Types;
using Emzi0767.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Emzi0767.AmongUsDirector
{
    public sealed class MothershipCommArray
    {
        private const string HeaderUserAgent = "Among-Us-Probe";
        private const string MothershipResource = "/probe-endpoint";

        private ILogger<MothershipCommArray> Logger { get; }
        private HttpListener AntennaArray { get; }
        private CancellationTokenSource CancellationTokenSource { get; }
        private CancellationToken CancellationToken => this.CancellationTokenSource.Token;
        private PayloadSerializer Serializer { get; }
        private SemaphoreSlim SendSemaphore { get; }
        private Task HandlerTask { get; set; }
        private Task SocketTask { get; set; }

        private readonly AsyncEvent<MothershipCommArray, ProbeEventArgs> _eventReceived;

        public MothershipCommArray(PayloadSerializer serializer, IOptions<BotConfiguration> config, ILoggerFactory loggerFactory)
        {
            this.Logger = loggerFactory.CreateLogger<MothershipCommArray>();

            this.AntennaArray = new HttpListener();
            this.AntennaArray.Prefixes.Add($"http://*:{config.Value.DiscoveryPort}/");

            this._eventReceived = new AsyncEvent<MothershipCommArray, ProbeEventArgs>("PROBE_EVENT", TimeSpan.Zero, this.EventExceptionHandler);

            this.CancellationTokenSource = new CancellationTokenSource();

            this.Serializer = serializer;
            this.SendSemaphore = new SemaphoreSlim(1, 1);
        }

        public void Start()
        {
            this.AntennaArray.Start();
            this.HandlerTask = Task.Run(this.ReceiverLoop);
        }

        public async Task StopAsync()
        {
            this._eventReceived.UnregisterAll();

            this.CancellationTokenSource.Cancel();
            this.CancellationTokenSource.Dispose();
            this.AntennaArray.Stop();

            if (this.HandlerTask != null)
                await this.HandlerTask;

            if (this.SocketTask != null)
                await this.SocketTask;
        }

        public event AsyncEventHandler<MothershipCommArray, ProbeEventArgs> ProbeEventReceived
        {
            add => this._eventReceived.Register(value);
            remove => this._eventReceived.Unregister(value);
        }

        private void EventExceptionHandler(AsyncEvent<MothershipCommArray, ProbeEventArgs> asyncEvent, Exception exception, AsyncEventHandler<MothershipCommArray, ProbeEventArgs> handler, MothershipCommArray sender, ProbeEventArgs eventArgs)
            => this.Logger.LogError(exception, "An exception occured while handling {0} comms event.", asyncEvent.Name);

        private async Task ReceiverLoop()
        {
            var http = this.AntennaArray;

            while (!this.CancellationToken.IsCancellationRequested)
            {
                var ctx = default(HttpListenerContext);

                try { ctx = await http.GetContextAsync(); }
                catch { break; }

                if (!ctx.Request.Headers.AllKeys.Contains("User-Agent", StringComparer.OrdinalIgnoreCase) ||
                    ctx.Request.Headers.Get("User-Agent") != HeaderUserAgent ||
                    ctx.Request.Url.AbsolutePath != MothershipResource ||
                    !ctx.Request.IsWebSocketRequest ||
                    !await this.SendSemaphore.WaitAsync(0))
                {
                    await this.Respond403(ctx.Response);
                    ctx.Response.Close();
                }

                var wsctx = await ctx.AcceptWebSocketAsync(null);
                this.SocketTask = Task.Run(async () => await this.SocketLoop(wsctx.WebSocket), this.CancellationToken);
            }
        }

        private async Task SocketLoop(WebSocket ws)
        {
            var buff = new ArraySegment<byte>(new byte[4096]);

            var result = default(WebSocketReceiveResult);
            try
            {
                while (!this.CancellationToken.IsCancellationRequested 
                    && ws.State == WebSocketState.Open)
                {
                    using var mem = new MemoryBuffer(
                        segmentSize: 4096,
                        initialSegmentCount: 1,
                        clearOnDispose: false);

                    do
                    {
                        result = await ws.ReceiveAsync(buff, this.CancellationToken);
                        if (result.MessageType == WebSocketMessageType.Close)
                            break;

                        mem.Write(buff);
                    }
                    while (!result.EndOfMessage);

                    if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        var ev = this.Serializer.Deserialize<ProbeEvent>(mem.ToArray());
                        await this._eventReceived.InvokeAsync(this, new ProbeEventArgs(ev));
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogCritical(ex, "Exception occured while receiving game events.");
            }

            if (result != null && ws.State == WebSocketState.CloseReceived)
                await ws.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);

            this.SendSemaphore.Release(1);
            this.Logger.LogInformation("Probe disconnected");
        }

        private async Task Respond403(HttpListenerResponse response)
        {
            response.StatusCode = 403;
            response.StatusDescription = "Bad request";
            response.ContentType = "text/plain";
            using (var sw = new StreamWriter(response.OutputStream, Encoding.ASCII))
                await sw.WriteLineAsync("You are not authorized to call this endpoint.");
        }
    }
}
