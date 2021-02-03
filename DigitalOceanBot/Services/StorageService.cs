using System;
using System.Collections.Concurrent;

namespace DigitalOceanBot.Services
{
    public sealed class StorageService
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
        public const string BotCurrentState = "botCurrentState";
        public const string Droplets = "droplets";
        public const string DropletId = "dropletId";
        public const string NewDroplet = "newDroplet";
        public const string Firewalls = "firewalls";
        public const string FirewallId = "firewallId";
        public const string NewFirewall = "newFirewall";
        public const string Projects = "projects";
        public const string ProjectId = "projectId";
        public const string NewProject = "newProject";
        public const string Images = "images";
        public const string ImageId = "imageId";
        public const string Regions = "regions";
    }
}