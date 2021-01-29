using System;
using System.Collections.Concurrent;

namespace DigitalOceanBot.Services
{
    public class StorageService
    {
        private ConcurrentDictionary<string, object> _dictionary = new();

        public void AddOrUpdate<T>(string key, T val)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException($"Parametr {nameof(key)} can't be null");
            }

            if (val is null)
            {
                throw new ArgumentNullException($"Parametr {nameof(val)} can't be null");
            }

            try
            {
                _dictionary.AddOrUpdate(key, val, (k, v) => val);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public T Get<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException($"Parametr {nameof(key)} can't be null");
            }

            var result = _dictionary.TryGetValue(key, out var val);
            return result ? (T) val : default(T);
        }

        public void Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException($"Parametr {nameof(key)} can't be null");
            }
            
            var result = _dictionary.TryRemove(key, out var val);
        }
    }

    public static class StorageKeys
    {
        public const string BotCurrentState = "current_state";
        public const string MyDroplets = "my_droplets";
        public const string SelectedDroplet = "selected_droplet";
        public const string MyFirewalls = "my_firewalls";
        public const string SelectedFirewall = "selected_firewall";
    }
}