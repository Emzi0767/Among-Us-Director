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

using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;

namespace Emzi0767.AmongUsDirector
{
    [Group("admin")]
    [Description("Provides bot administration commands.")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    [RequireOwner, RequireGuild]
    [Hidden]
    public sealed class AdminCommandModule : BaseCommandModule
    {
        [Command("sudo")]
        [Description("Impersonates a user and executes a command.")]
        [Hidden]
        public async Task SudoAsync(CommandContext ctx, 
            [Description("Member to impersonate.")] DiscordMember member, 
            [Description("Command to run"), RemainingText] string input)
        {
            var cmd = ctx.CommandsNext.FindCommand(input, out var args);
            if (cmd == null)
                throw new CommandNotFoundException(input);

            var fctx = ctx.CommandsNext.CreateFakeContext(member, ctx.Channel, input, ctx.Prefix, cmd, args);
            await ctx.CommandsNext.ExecuteCommandAsync(fctx);
        }
    }
}
