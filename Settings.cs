using System.Configuration;

namespace ConfigMerger.Properties
{
    internal sealed partial class Settings : ApplicationSettingsBase
    {
        private static Settings defaultInstance = ((Settings)(ApplicationSettingsBase.Synchronized(new Settings())));

        public static Settings Default
        {
            get
            {
                return defaultInstance;
            }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("")]
        public string LastSourceDirectory
        {
            get
            {
                return ((string)(this["LastSourceDirectory"]));
            }
            set
            {
                this["LastSourceDirectory"] = value;
            }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("")]
        public string LastTargetDirectory
        {
            get
            {
                return ((string)(this["LastTargetDirectory"]));
            }
            set
            {
                this["LastTargetDirectory"] = value;
            }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("true")]
        public bool CreateBackups
        {
            get
            {
                return ((bool)(this["CreateBackups"]));
            }
            set
            {
                this["CreateBackups"] = value;
            }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("true")]
        public bool ShowConfirmationDialogs
        {
            get
            {
                return ((bool)(this["ShowConfirmationDialogs"]));
            }
            set
            {
                this["ShowConfirmationDialogs"] = value;
            }
        }
    }
}