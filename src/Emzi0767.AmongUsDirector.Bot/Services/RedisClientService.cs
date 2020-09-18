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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Emzi0767.AmongUsDirector
{
    public sealed class RedisClientService
    {
        private const string KeySeparator = "::";
        private const string KeyPrefix = "AmongUsBot";

        private ConnectionMultiplexer ConnectionMultiplexer { get; set; }
        private RedisConfiguration Configuration { get; }
        private ILogger<RedisClientService> Logger { get; }

        public RedisClientService(IOptions<RedisConfiguration> config, ILoggerFactory loggerFactory)
        {
            this.Configuration = config.Value;
            this.Logger = loggerFactory.CreateLogger<RedisClientService>();
        }

        public async Task StartAsync()
        {
            var cfg = new ConfigurationOptions
            {
                AllowAdmin = false,
                ClientName = "RosettaCTF",
                Ssl = this.Configuration.UseSsl,
                Password = this.Configuration.Password,
                SslProtocols = SslProtocols.Tls12
            };

            cfg.EndPoints.Add(this.Configuration.Host, this.Configuration.Port);

            this.ConnectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(cfg);
            this.Logger.LogDebug("Redis connected to {0}:{1}", this.Configuration.Host, this.Configuration.Port);
        }

        public void Stop()
        {
            this.ConnectionMultiplexer.Dispose();
        }

        public async Task<T> GetValueAsync<T>(params string[] keyIndices)
        {
            var db = this.ConnectionMultiplexer.GetDatabase(this.Configuration.Index);
            var key = FormatKey(keyIndices);
            var val = await db.StringGetAsync(key);

            this.Logger.LogDebug("Retrieving value '{0}'", key);
            return (T)Convert.ChangeType(val, typeof(T));
        }

        public async Task SetValueAsync<T>(T value, params string[] keyIndices)
        {
            var db = this.ConnectionMultiplexer.GetDatabase(this.Configuration.Index);
            var key = FormatKey(keyIndices);

            this.Logger.LogDebug("Setting value '{0}'", key);
            await db.StringSetAsync(key, Convert.ChangeType(value, TypeCode.String, CultureInfo.InvariantCulture) as string);
        }

        public async Task DeleteValueAsync(params string[] keyIndices)
        {
            var db = this.ConnectionMultiplexer.GetDatabase(this.Configuration.Index);
            var key = FormatKey(keyIndices);

            this.Logger.LogDebug("Deleting value '{0}'", key);
            await db.KeyDeleteAsync(key);
        }

        public async IAsyncEnumerable<string> GetKeysAsync(params string[] keyIndices)
        {
            var key = FormatKey(keyIndices);
            var srv = this.ConnectionMultiplexer.GetServer(this.ConnectionMultiplexer.GetEndPoints().First());

            await foreach (var rk in srv.KeysAsync(database: this.Configuration.Index, pattern: key))
                yield return rk.ToString().Substring(key.Length - 1);
        }

        private static string FormatKey(string[] key)
        {
            var blen = key.Sum(x => x.Length) + KeySeparator.Length * key.Length + KeyPrefix.Length;
            return string.Create(blen, key, FormatInner);

            static void FormatInner(Span<char> buff, string[] indices)
            {
                var sep = KeySeparator;
                var pfx = KeyPrefix;
                var slen = sep.Length;

                pfx.AsSpan().CopyTo(buff);
                buff = buff.Slice(pfx.Length);

                for (int i = 0; i < indices.Length; ++i)
                {
                    var idc = indices[i];

                    sep.AsSpan().CopyTo(buff);
                    idc.AsSpan().CopyTo(buff = buff.Slice(slen));
                    buff = buff.Slice(idc.Length);
                }
            }
        }
    }
}
