using System;
using System.Collections.Generic;
using System.IO;
#if PRE_V1_39_1
using System.Linq;
#endif
using System.Reflection;
using System.Threading.Tasks;
using IPA.Config.Data;
using IPA.Config.Stores.Converters;
using IPA.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace NoteTweaks.Configuration
{
    // https://github.com/kinsi55/CS_BeatSaber_Camera2/blob/6b6807d60da8e7c922bba0c74800f9095d93c247/Utils/JsonConverters.cs#L85
    // was going to just convert these to JArray objects but it looked ugly and i hated it
    public class Vector2Converter : JsonConverter<Vector2> {
        public override void WriteJson(JsonWriter writer, Vector2 vec, JsonSerializer serializer) {
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(vec.x);
            writer.WritePropertyName("y");
            writer.WriteValue(vec.y);
            writer.WriteEndObject();
        }

        public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer) {
            Vector2 vec = new Vector2();

            while (reader.TokenType != JsonToken.EndObject)
            {
                reader.Read();
                
                if(reader.TokenType == JsonToken.PropertyName) {
                    string property = reader.Value?.ToString();
                    float val = (float)reader.ReadAsDecimal().GetValueOrDefault();
                    switch (property)
                    {
                        case "x": vec.x = val; break;
                        case "y": vec.y = val; break;
                    }
                }
            }
            
            return vec;
        }
    }
    public class Vector3Converter : JsonConverter<Vector3> {
        public override void WriteJson(JsonWriter writer, Vector3 vec, JsonSerializer serializer) {
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(vec.x);
            writer.WritePropertyName("y");
            writer.WriteValue(vec.y);
            writer.WritePropertyName("z");
            writer.WriteValue(vec.z);
            writer.WriteEndObject();
        }

        public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer) {
            Vector3 vec = new Vector3();

            while (reader.TokenType != JsonToken.EndObject)
            {
                reader.Read();
                
                if(reader.TokenType == JsonToken.PropertyName) {
                    string property = reader.Value?.ToString();
                    float val = (float)reader.ReadAsDecimal().GetValueOrDefault();
                    switch (property)
                    {
                        case "x": vec.x = val; break;
                        case "y": vec.y = val; break;
                        case "z": vec.z = val; break;
                    }
                }
            }
            
            return vec;
        }
    }
    
    public class ColorConverter : JsonConverter<Color>
    {
        private static readonly HexColorConverter Converter = new HexColorConverter();
        
        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
        {
            JToken val = new JValue(Converter.ToValue(value, new object()).ToString().Replace("\"", ""));
            val.WriteTo(writer);
        }

        public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JToken val = JToken.Load(reader);
            return Converter.FromValue(Value.Text(val.Value<string>()), new object());
        }
    }
    
    internal class ConfigurationPresetManager
    {
        private static PluginConfig Config => PluginConfig.Instance;
        internal static Dictionary<string, PluginConfig> Presets { get; set; } = new Dictionary<string, PluginConfig>();
        internal static readonly string PresetPath = Path.Combine(UnityGame.UserDataPath, "NoteTweaks", "Presets");
        
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings();
        
        public ConfigurationPresetManager()
        {
            if (!Directory.Exists(PresetPath))
            {
                Directory.CreateDirectory(PresetPath);
            }
            
            SerializerSettings.Converters.Add(new Vector2Converter());
            SerializerSettings.Converters.Add(new Vector3Converter());
            SerializerSettings.Converters.Add(new ColorConverter());
            SerializerSettings.Formatting = Formatting.Indented;
            SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            SerializerSettings.MetadataPropertyHandling = MetadataPropertyHandling.Ignore;
        }

        public static string SavePreset(string presetName)
        {
            string path = Path.Combine(PresetPath, presetName + ".json");
            string[] dirs = path.Split(Path.DirectorySeparatorChar);
#if PRE_V1_39_1
            string safePath = String.Join(Path.DirectorySeparatorChar.ToString(), dirs.ToList().GetRange(dirs.Length - 4, 4).ToArray());
#else
            string safePath = String.Join(Path.DirectorySeparatorChar.ToString(), dirs.GetRange(dirs.Length - 4, 4).ToArray());
#endif

            try
            {
                File.WriteAllText(path, Config.GetSerializedJson(SerializerSettings));
                return $"<color=#FFFFFFCC>Saved preset to<color=#FFFFFFFF>\n{safePath}";
            }
            catch (Exception e)
            {
                Plugin.Log.Error(e);
                return $"<color=#FFCCCCCC>Error with saving {safePath}<color=#FFCCCCFF>\n{e.GetType().Name}";
            }
        }

        public static async Task LoadPreset(string presetName)
        {
            string path = Path.Combine(PresetPath, presetName + ".json");
            if (!File.Exists(path))
            {
                Plugin.Log.Info("Preset file doesn't exist");
                return;
            }
                
            PluginConfig preset = JsonConvert.DeserializeObject(File.ReadAllText(path), typeof(PluginConfig)) as PluginConfig;
            Plugin.Log.Info($"Deserialized preset {presetName}");

            foreach (PropertyInfo property in typeof(PluginConfig).GetProperties())
            {
                if (property.CanWrite && (property.MemberType & MemberTypes.Property) == MemberTypes.Property)
                {
                    bool keepGoing = false;
                    foreach (CustomAttributeData attributeData in property.CustomAttributes)
                    {
                        if (attributeData.AttributeType == typeof(JsonPropertyAttribute))
                        {
                            keepGoing = true;
                        }
                    }

                    if (!keepGoing)
                    {
                        continue;
                    }
                    
                    property.SetValue(Config, property.GetValue(preset));
                }
            }
            
            Plugin.ClampSettings();
            await UI.SettingsFlowCoordinator._settingsViewController.RefreshAll();
        }
    }
}