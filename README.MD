# Among Us Director
A toolkit for managing Among Us games. Primarly aimed at Discord, specifically muting people outside of meetings, and 
muting during meetings.

## Credit
[@Niewiarowski](https://github.com/Niewiarowski) - for initial idea, and guidance to make this project happen

## Usage
### The library
First, attach to the game via `GameProcess.Attach()`. The received instance is `IDisposable`, and also contains 
various gameplay events. Hook as needed.

### The bot
The bot consists of 2 parts, which communicate over local network. Both components need the ability to bind UDP 
sockets, using the port defined in `config.json`. The port the client application binds to is 1 lower than the one 
defined in config, to avoid problems on single-host scenarios. It is advisable to pick a non-privileged port (default 
one should do just fine). The bot will also bind to a TCP socket on the same port, to facilitate WebSocket 
communication between the bot and the probe.

#### The bot
To begin, you need to create an application over at [Discord Developers website](https://discord.com/developers/applications). 
Make sure it has a bot associated with it, then copy its token (from the Bot tab) to the config. Next up, you will 
need a redis server, accessible by the bot, for persistence purposes. Dial its settings into config.json. You can also 
change the command prefix, however the bot will always be accessible through a mention prefix. You can now start the 
bot.

The bot needs to have permissions to mute and deafen members server-wide.

The bot will not function, unless you configure a voice channel for it. To do so, use the command `i!vc <channel>`, 
for example `i!vc General` or `i!vc 123456789123456789`. The command accepts a channel name, mention string, or ID. 
The invoker of this command needs to have manage channels permission.

You can also optionally specify a diagnostic text channel, by using `i!output` command. The command takes a channel as 
an argument, and if one is not supplied, will use the channel the command was invoked in. The invoker of this command 
needs to have manage channels permission.

Users need to claim their in-game player names. This is done via `i!claim name` (e.g. `i!claim Emzi`). A moderator 
(that is, a person with manage roles permission) can manually associate server members with player names, by using 
`i!assoc <member> <player>` command, for example `i!assoc @Emzi0767 Emzi`. The member can be supplied in form of a 
mention, user id, username, username#discrim, or nickname.

Player names can be unclaimed via `i!unclaim` command. The moderator version is `i!unassoc <member>`.

The assume examples assume `i!` is configured as the prefix. If the prefix was changed, modify invocations accordingly.

#### The probe
For the bot to take any action, it needs to receive events from the game. For this purpose, a probe application has 
been designed. Make sure to enter the same service discovery port into probe's `config.json` as you did for bot's 
`config.json`. Do note, the probe has to be ran *after* you start the game, and ideally, should be stopped before you 
quit the game. The probe will perform service discovery, and connect to the first bot instance on the local network it 
receives a valid discovery response from.

## Disclaimer
The probe actively inspects the game's memory. As you can imagine, this might be frowned upon by certain kinds of 
software, most notably antivirus applications, or anti-cheats. I do not take any responsibility for whatever might 
happen to you or your environment as a result of using this software. You were warned.

## Other considerations
As mentioned above, the probe reads the game's memory. Although both applications typically run as regular users, 
sometimes, administrative privileges might be required to achieve this.

Certain AVs might flag the probe as malicious software due to `ReadProcessMemory`, `EnumProcessModules`, 
`GetModuleBaseNameW`, and/or `GetModuleInformation` calls, the first of which is used extensively throughout the code. 
Should that occur, you might have to add the application to the exclusion list.

If you have doubts about the binary, you can use a .NET decompiler such as dnSpy or JetBrains dotPeek to inspect it, 
or better yet, you can compile the source code yourself. You will need .NET Core 3.1 SDK to do so.

Finally, under Windows, SmartScreen might flag the application as untrustworthy. Again, if you have doubts, read the 
above paragraph.

You need to configure your firewall such that it allows incoming and outgoing UDP traffic for both applications over 
local network. The bot also needs to be able to accept incoming TCP connections over the local network.

## Support me
Lots of effort went into making this software.

If you feel like I'm doing a good job, or just want to throw money at me, you can do so through any of the following:
- [Patreon](https://www.patreon.com/emzi0767)
- [PayPal](https://paypal.me/Emzi0767/5USD)

## Other questions
If you have other questions or would like to talk in general, feel free to visit my Discord server.

[![Emzi's Central Dispatch](https://discordapp.com/api/guilds/207879549394878464/embed.png?style=banner1)](https://discord.gg/rGKrJDR)
