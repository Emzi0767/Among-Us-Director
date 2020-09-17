# Among Us Director
A Discord toolkit for managing Among Us games. Primarly aimed at Discord, specifically muting people outside of 
meetings, and muting during meetings.

## Credit
@Niewiarowski - for initial idea, and guidance to make this project happen

## Usage
### The library
First, attach to the game via `GameProcess.Attach()`. The received instance is `IDisposable`, and also contains 
various gameplay events. Hook as needed.

### The bot
Supply the token via `config.json` (see `config.json.example`). Run the bot.
