﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DataMartESImporter {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
    internal sealed partial class DataMartAppSettings : global::System.Configuration.ApplicationSettingsBase {
        
        private static DataMartAppSettings defaultInstance = ((DataMartAppSettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new DataMartAppSettings())));
        
        public static DataMartAppSettings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool stop {
            get {
                return ((bool)(this["stop"]));
            }
            set {
                this["stop"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("5")]
        public int SleepTimeInMinutes {
            get {
                return ((int)(this["SleepTimeInMinutes"]));
            }
            set {
                this["SleepTimeInMinutes"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("\\\\dropShareServerName\\")]
        public string dropshareServerName {
            get {
                return ((string)(this["dropshareServerName"]));
            }
            set {
                this["dropshareServerName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("\\\\dropShareServerName\\FolderWhereJSONFilesAreLocated\\")]
        public string dropshareFolderName {
            get {
                return ((string)(this["dropshareFolderName"]));
            }
            set {
                this["dropshareFolderName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://ElasticSearchServerUrl:9200")]
        public string elasticSearchURL {
            get {
                return ((string)(this["elasticSearchURL"]));
            }
            set {
                this["elasticSearchURL"] = value;
            }
        }
    }
}
