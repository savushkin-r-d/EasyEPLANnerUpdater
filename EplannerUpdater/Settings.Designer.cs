//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Updater {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.8.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("savushkin-r-d")]
        public string GitOwner {
            get {
                return ((string)(this["GitOwner"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("EasyEPLANner")]
        public string GitRepo {
            get {
                return ((string)(this["GitRepo"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool ShowPullRequests {
            get {
                return ((bool)(this["ShowPullRequests"]));
            }
            set {
                this["ShowPullRequests"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public int RunMode {
            get {
                return ((int)(this["RunMode"]));
            }
            set {
                this["RunMode"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string ReleaseTag {
            get {
                return ((string)(this["ReleaseTag"]));
            }
            set {
                this["ReleaseTag"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string EplanAppPath {
            get {
                return ((string)(this["EplanAppPath"]));
            }
            set {
                this["EplanAppPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool UsePullRequestVersion {
            get {
                return ((bool)(this["UsePullRequestVersion"]));
            }
            set {
                this["UsePullRequestVersion"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("-1")]
        public int PullRequestNumber {
            get {
                return ((int)(this["PullRequestNumber"]));
            }
            set {
                this["PullRequestNumber"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string PAT {
            get {
                return ((string)(this["PAT"]));
            }
            set {
                this["PAT"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2021.7")]
        public string InitialReleaseAfter {
            get {
                return ((string)(this["InitialReleaseAfter"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.DateTime UpdaterReleaseLastCheck {
            get {
                return ((global::System.DateTime)(this["UpdaterReleaseLastCheck"]));
            }
            set {
                this["UpdaterReleaseLastCheck"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string UpdaterReleaseTag {
            get {
                return ((string)(this["UpdaterReleaseTag"]));
            }
            set {
                this["UpdaterReleaseTag"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("EasyEPLANnerUpdater")]
        public string UpdaterGitRepo {
            get {
                return ((string)(this["UpdaterGitRepo"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string BTODescriptionUsedVersion {
            get {
                return ((string)(this["BTODescriptionUsedVersion"]));
            }
            set {
                this["BTODescriptionUsedVersion"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("ptusa-Lua-dairy-system")]
        public string BTODescriptionRepo {
            get {
                return ((string)(this["BTODescriptionRepo"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("EasyEplannerObjectsDescription/sys_base_objects_description.lua")]
        public string BTODescriptionPath {
            get {
                return ((string)(this["BTODescriptionPath"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Lua\\BaseObjectsDescriptionFiles\\sys_base_objects_description.lua")]
        public string BTODescriptionFilePath {
            get {
                return ((string)(this["BTODescriptionFilePath"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string LastLoginedUser {
            get {
                return ((string)(this["LastLoginedUser"]));
            }
            set {
                this["LastLoginedUser"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://github.com/savushkin-r-d/EasyEPLANnerUpdater/blob/main/README.md#")]
        public string ReadMeUrl {
            get {
                return ((string)(this["ReadMeUrl"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://github.com/settings/tokens/new")]
        public string CreateNewTokenUrl {
            get {
                return ((string)(this["CreateNewTokenUrl"]));
            }
        }
    }
}
