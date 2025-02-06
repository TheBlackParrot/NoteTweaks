using System;
using System.Collections.Generic;
using System.Reflection;
using Heck.SettingsSetter;
using JetBrains.Annotations;

// using UITweaks as reference
// https://github.com/Exomanz/UITweaks/blob/master/UITweaks/Utilities/SettableSettings/UITweaksSettableSettings.cs
namespace NoteTweaks.Configuration
{
    [UsedImplicitly]
    internal class NoteTweaksSettableSettings : IDisposable
    {
        private static bool HasRunBefore { get; set; } = false;
        private const string GroupIdentifier = "_NoteTweaks";
        private static readonly List<ISettableSetting> SettableSettings = new List<ISettableSetting>();
        
        /*public NoteTweaksSettableSettings()
        {
            if (HasRunBefore)
            {
                return;
            }
            HasRunBefore = true;

            foreach (PropertyInfo property in typeof(PluginConfig).GetProperties())
            {
                Type propType = property.PropertyType;
                string normalizedTypeName = propType.BaseType?.Name ?? propType.Name;

                ISettableSetting setting = Activator.CreateInstance(propType, $"NoteTweaks - {normalizedTypeName}", property.Name, enabledProperty, property)
                    as ISettableSetting;

                SettableSettings.Add(setting);
                
                string heckFieldName = $"_{GroupIdentifier}_{property.Name}";
                if (setting != null)
                {
                    SettingSetterSettableSettingsManager.RegisterSettableSetting(GroupIdentifier, heckFieldName, setting);
                }
            }
        }*/

        public void Dispose()
        {
            SettableSettings.ForEach(x => x.SetTemporary(null));
        }
    }
}