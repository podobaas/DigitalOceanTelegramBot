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
                throw new ArgumentNullException($"Param {nameof(key)} can't be null");
            }

            if (val is null)
            {
                throw new ArgumentNullException($"Param {nameof(val)} can't be null");
            }
            
            _dictionary.AddOrUpdate(key, val, (_, _) => val);
        }

        public T Get<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException($"Param {nameof(key)} can't be null");
            }

            var result = _dictionary.TryGetValue(key, out var val);
            return result ? (T) val : default;
        }

        public void Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException($"Param {nameof(key)} can't be null");
            }
            
            var result = _dictionary.TryRemove(key, out _);

            if (!result)
            {
                throw new Exception($"Failed to remove value from dictionary. Key={key}");
            }
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
        public const string RegionId = "regionId";
        public const string Sizes = "sizes";
        public const string SizeId = "sizeId";
    }
}