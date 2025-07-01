using System.Configuration;

namespace ConfigMerger.Properties
{
    internal sealed partial class Settings : ApplicationSettingsBase
    {
        private static Settings defaultInstance = ((Settings)(ApplicationSettingsBase.Synchronized(new Settings())));

        public static Settings Default => defaultInstance;

        [UserScopedSetting()]
        [DefaultSettingValue("")]
        public string LastSourceDirectory
        {
            get => ((string)(this["LastSourceDirectory"]));
            set => this["LastSourceDirectory"] = value;
        }

        [UserScopedSetting()]
        [DefaultSettingValue("")]
        public string LastTargetDirectory
        {
            get => ((string)(this["LastTargetDirectory"]));
            set => this["LastTargetDirectory"] = value;
        }

        [UserScopedSetting()]
        [DefaultSettingValue("true")]
        public bool CreateBackups
        {
            get => ((bool)(this["CreateBackups"]));
            set => this["CreateBackups"] = value;
        }

        [UserScopedSetting()]
        [DefaultSettingValue("false")]
        public bool DarkTheme
        {
            get => ((bool)(this["DarkTheme"]));
            set => this["DarkTheme"] = value;
        }

        [UserScopedSetting()]
        [DefaultSettingValue("true")]
        public bool ShowConfirmationDialogs
        {
            get => ((bool)(this["ShowConfirmationDialogs"]));
            set => this["ShowConfirmationDialogs"] = value;
        }
    }
}