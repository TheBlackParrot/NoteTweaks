using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Heck.SettingsSetter;
using JetBrains.Annotations;
using UnityEngine;

// using UITweaks and Counters+ as reference
// https://github.com/Exomanz/UITweaks/blob/master/UITweaks/Utilities/SettableSettings/UITweaksSettableSettings.cs
namespace NoteTweaks.Configuration
{
    // directly from Counters+, didn't want to re-invent the wheel if it works
    // https://github.com/NuggoDEV/CountersPlus/blob/master/Counters%2B/ConfigModels/SettableSettings/CountersPlusWrapperSetting.cs
    public class NoteTweaksWrapperSetting : ISettableSetting
    {
        private readonly PropertyInfo _settingsProperty;
        private readonly object _settingsInstance;

        private object _originalValue;

        public NoteTweaksWrapperSetting(string groupName, string fieldName,
            PropertyInfo settingsProperty, object settingsInstance)
        {
            GroupName = groupName;
            FieldName = fieldName;

            _settingsProperty = settingsProperty;
            _settingsInstance = settingsInstance;
        }

        public string GroupName { get; }

        public string FieldName { get; }

        public object TrueValue => _settingsProperty.GetValue(_settingsInstance);

        public void SetTemporary(object tempValue)
        {
            if (tempValue != null)
            {
                _originalValue = _settingsProperty.GetValue(_settingsInstance);

                if (_settingsProperty.PropertyType.IsEnum)
                {
                    tempValue = Enum.Parse(_settingsProperty.PropertyType, tempValue.ToString());
                }
                else if (_settingsProperty.PropertyType == typeof(Color))
                {
                    ColorUtility.TryParseHtmlString(tempValue.ToString(), out Color tempColorValue);
                    tempValue = tempColorValue;
                }
                else if (_settingsProperty.PropertyType == typeof(int))
                {
                    tempValue = Convert.ToInt32(tempValue);
                }
                else if (_settingsProperty.PropertyType == typeof(float) || _settingsProperty.PropertyType == typeof(double))
                {
                    tempValue = Convert.ToSingle(tempValue);
                }

                _settingsProperty.SetValue(_settingsInstance, tempValue);
            }
            else if (_originalValue != null)
            {
                _settingsProperty.SetValue(_settingsInstance, _originalValue);
                _originalValue = null;
            }
        }
    }
    
    [UsedImplicitly]
    internal class NoteTweaksSettableSettings : IDisposable
    {
        private static bool HasRunBefore { get; set; }
        private const string GroupIdentifier = "_noteTweaks";
        private const BindingFlags BindingFlags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance;
        private static readonly List<ISettableSetting> SettableSettings = new List<ISettableSetting>();
        private static readonly string[] BlockedSettings = { "Instance", "DisableIfNoodle" };
        
        public NoteTweaksSettableSettings()
        {
            if (HasRunBefore)
            {
                return;
            }
            HasRunBefore = true;

            Type settableSettingType = typeof(NoteTweaksWrapperSetting);
            PropertyInfo[] properties = typeof(PluginConfig).GetProperties(BindingFlags)
                .Where(x => x.CanWrite && x.CanRead && (x.PropertyType != typeof(Vector2) || x.PropertyType != typeof(Vector3))).ToArray();
            
            foreach (PropertyInfo property in properties)
            {
                string propName = property.Name;
                if (BlockedSettings.Contains(propName))
                {
                    continue;
                }

                //public NoteTweaksWrapperSetting(this, string groupName, string fieldName, PropertyInfo settingsProperty, object settingsInstance)
                ISettableSetting setting = Activator.CreateInstance(settableSettingType, "NoteTweaks", propName, property, PluginConfig.Instance)
                    as ISettableSetting;

                SettableSettings.Add(setting);

                string heckFieldName = $"_{propName[0].ToString().ToLower() + propName.Substring(1)}";
                if (setting != null)
                {
                    SettingSetterSettableSettingsManager.RegisterSettableSetting(GroupIdentifier, heckFieldName, setting);
                    Plugin.Log.Info($"NoteTweaks settable setting: {heckFieldName} as {property.PropertyType.Name}");
                }
            }
        }

        public void Dispose()
        {
            SettableSettings.ForEach(x => x.SetTemporary(null));
        }
    }
}