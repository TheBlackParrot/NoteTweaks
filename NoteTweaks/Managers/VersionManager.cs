using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using IPA.Utilities.Async;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NoteTweaks.UI;

namespace NoteTweaks.Managers
{
    public class ManifestData
    {
        public class LinksData
        {
            [JsonProperty("project-home")]
            public string ProjectHome { get; private set; }
        }
            
        [JsonProperty("gameVersion")]
        public string GameVersion { get; private set; }
            
        [JsonProperty("author")]
        public string Author { get; private set; }

        [JsonProperty("links")]
        public LinksData Links { get; private set; }
    }
    
    internal class VersionManager
    {
        private static readonly HttpClient HttpClient = new HttpClient
        {
            BaseAddress = new Uri("https://theblackparrot.me")
        };

        private async Task<Version> GetRemoteVersionData()
        {
            Plugin.Log.Info($"Getting version data from {HttpClient.BaseAddress} ...");
            
            HttpResponseMessage response = await HttpClient.GetAsync("NoteTweaks/version.json");
            if (!response.IsSuccessStatusCode)
            {
                Plugin.Log.Warn("Failed to get version data");
                return null;
            }
            
            Plugin.Log.Info($"Got version data from {response.RequestMessage.RequestUri}");

            string rawVersionData = await response.Content.ReadAsStringAsync();
            
            JObject json = JObject.Parse(rawVersionData);
            string rawVersionString = json[GameVersion.ToString(3)]?.ToString();
            
            if (rawVersionString != null)
            {
                return new Version(rawVersionString);
            }
            
            return null;
        }
        
        public readonly Version ModVersion = Assembly.GetExecutingAssembly().GetName().Version;
        public readonly ManifestData Manifest;
        public readonly Version GameVersion;
        public static Version LatestVersion;

        public VersionManager()
        {
            byte[] manifestData = SiraUtil.Extras.Utilities.GetResource(Assembly.GetExecutingAssembly(), "NoteTweaks.manifest.json");
            Manifest = JsonConvert.DeserializeObject<ManifestData>(System.Text.Encoding.UTF8.GetString(manifestData));
            
            GameVersion = new Version(Manifest.GameVersion);

            UnityMainThreadTaskScheduler.Factory.StartNew(async () =>
            {
                LatestVersion = await GetRemoteVersionData();
                Plugin.Log.Info("Latest version is " + LatestVersion.ToString(3));
                
                if (LatestVersion > ModVersion)
                {
                    MenuButtonManager.ColorizeButtonOnUpdateAvailable();
                }
            });
        }
    }
}