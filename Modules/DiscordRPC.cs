using DiscordRPC;
using DiscordRPC.Logging;
using System;

namespace TMP.NET.Modules
{
    public class DiscordRPC
    {
        private DiscordRpcClient _client;
        DCID _dcID = new DCID();
        public void Initialize()
        {
            _client = new DiscordRpcClient(_dcID._appID);
            _client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };

            _client.OnReady += (sender, e) =>
            {
                Console.WriteLine("Received Ready from user {0}", e.User.Username);
            };

            _client.OnPresenceUpdate += (sender, e) =>
            {
                Console.WriteLine("Received update {0}", e.Presence);
            };

            _client.OnError += (sender, e) =>
            {
                Console.WriteLine($"Error: {e.Message}");
            };

            _client.Initialize();

            //updatePresence(null, null, "tmp_logo", "Track My Playtime");
        }

        public void Deinitialize()
        {
            try
            {
                if(_client != null)
                {
                    _client.ClearPresence();
                    _client.Deinitialize();
                    _client.Dispose();
                }
            }
            catch (Exception ex)
            {
                MainWindow.log.Error("Exception thrown when dispose DiscordRpcCLient", ex);
                Console.WriteLine(ex.Message);
            }
        }

        public void UpdatePresence(string details, string state)
        {
            if (_client.IsDisposed)
                return;


            _client.SetPresence(new RichPresence()
            {
                Details = details,
                State = state,
                Timestamps = new Timestamps()
                {
                    Start = DateTime.UtcNow
                }
            });
        }

        public void UpdatePresence(string details, string state, string large_image_key, string large_image_text)
        {
            if (_client.IsDisposed)
                return;

            _client.SetPresence(new RichPresence()
            {
                Details = details,
                State = state,
                Assets = new Assets()
                {
                    LargeImageKey = large_image_key,
                    LargeImageText = large_image_text
                },
                Timestamps = new Timestamps()
                {
                    Start = DateTime.UtcNow
                }
            });
        }

        public void UpdatePresence(string details, string state, string large_image_key, string large_image_text, string small_image_key, string small_image_text)
        {
            if (_client.IsDisposed)
                return;

            _client.SetPresence(new RichPresence()
            {
                Details = details,
                State = state,
                Assets = new Assets()
                {
                    LargeImageKey = large_image_key,
                    LargeImageText = large_image_text,
                    SmallImageKey = small_image_key,
                    SmallImageText = small_image_text
                },
                Timestamps = new Timestamps()
                {
                    Start = DateTime.UtcNow
                }
            });
        }

        public void ClearCurrentPresence()
        {
            _client.ClearPresence();
        }
    }
}
