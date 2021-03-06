﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Rubberduck.Settings;
using Rubberduck.Common;
using Rubberduck.Interaction;
using NLog;
using Rubberduck.SettingsProvider;
using Rubberduck.UI.Command;
using Rubberduck.VBEditor.VbeRuntime.Settings;
using Rubberduck.Resources;
using Rubberduck.Resources.Settings;

namespace Rubberduck.UI.Settings
{
    public enum DelimiterOptions
    {
        Period = 46,
        Slash = 47
    }

    public sealed class GeneralSettingsViewModel : SettingsViewModelBase<Rubberduck.Settings.GeneralSettings>, ISettingsViewModel<Rubberduck.Settings.GeneralSettings>
    {
        private readonly IOperatingSystem _operatingSystem;
        private readonly IMessageBox _messageBox;
        private readonly IVbeSettings _vbeSettings;
        private readonly IFilePersistanceService<HotkeySettings> _hotkeyService;

        private bool _indenterPrompted;
        private readonly IReadOnlyList<Type> _experimentalFeatureTypes;

        public GeneralSettingsViewModel(
            Configuration config, 
            IOperatingSystem operatingSystem, 
            IMessageBox messageBox,
            IVbeSettings vbeSettings,
            IExperimentalTypesProvider experimentalTypesProvider,
            IFilePersistanceService<Rubberduck.Settings.GeneralSettings> service,
            IFilePersistanceService<HotkeySettings> hotkeyService) 
            : base(service)
        {
            _operatingSystem = operatingSystem;
            _messageBox = messageBox;
            _vbeSettings = vbeSettings;
            _experimentalFeatureTypes = experimentalTypesProvider.ExperimentalTypes;

            Languages = new ObservableCollection<DisplayLanguageSetting>(Locales.AvailableCultures
                .OrderBy(locale => locale.NativeName)
                .Select(locale => new DisplayLanguageSetting(locale.Name)));

            LogLevels = new ObservableCollection<MinimumLogLevel>(
                LogLevelHelper.LogLevels.Select(l => new MinimumLogLevel(l.Ordinal, l.Name)));
            TransferSettingsToView(config.UserSettings.GeneralSettings, config.UserSettings.HotkeySettings);

            ShowLogFolderCommand = new DelegateCommand(LogManager.GetCurrentClassLogger(), _ => ShowLogFolder());
            ExportButtonCommand = new DelegateCommand(LogManager.GetCurrentClassLogger(), _ => ExportSettings(GetCurrentGeneralSettings()));
            ImportButtonCommand = new DelegateCommand(LogManager.GetCurrentClassLogger(), _ => ImportSettings());

            _hotkeyService = hotkeyService;
        }

        public List<ExperimentalFeatures> ExperimentalFeatures { get; set; }

        public ObservableCollection<DisplayLanguageSetting> Languages { get; set; } 

        private DisplayLanguageSetting _selectedLanguage;
        public DisplayLanguageSetting SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (!Equals(_selectedLanguage, value))
                {
                    _selectedLanguage = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<HotkeySetting> _hotkeys;
        public ObservableCollection<HotkeySetting> Hotkeys
        {
            get => _hotkeys;
            set
            {
                if (_hotkeys != value)
                {
                    _hotkeys = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool ShouldDisplayHotkeyModificationLabel
        {
            get
            {
                return _hotkeys.Any(s => !s.IsValid);
            }
        }

        private bool _autoSaveEnabled;
        public bool AutoSaveEnabled
        {
            get => _autoSaveEnabled;
            set
            {
                if (_autoSaveEnabled != value)
                {
                    _autoSaveEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _showSplashAtStartup;
        public bool ShowSplashAtStartup
        {
            get => _showSplashAtStartup;
            set
            {
                if (_showSplashAtStartup != value)
                {
                    _showSplashAtStartup = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _checkVersionAtStartup;
        public bool CheckVersionAtStartup
        {
            get => _checkVersionAtStartup;
            set
            {
                if (_checkVersionAtStartup != value)
                {
                    _checkVersionAtStartup = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _compileBeforeParse;
        public bool CompileBeforeParse
        {
            get => _compileBeforeParse;
            set
            {
                if (_compileBeforeParse == value)
                {
                    return;
                }

                if (value && _vbeSettings.CompileOnDemand)
                {
                    if(!SynchronizeVbeSettings())
                    {
                        return;
                    }
                }

                _compileBeforeParse = value;
                OnPropertyChanged();
            }
        }

        private bool _setDpiUnaware;
        public bool SetDpiUnaware
        {
            get => _setDpiUnaware;
            set
            {
                if (_setDpiUnaware != value)
                {
                    _setDpiUnaware = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool SetDpiUnawareEnabled
        {
            get
            {
                var osVersion = _operatingSystem.GetOSVersion();
                return osVersion != null && osVersion >= WindowsVersion.Windows81;
            }
        }

        private bool SynchronizeVbeSettings()
        {
            if (!_messageBox.ConfirmYesNo(RubberduckUI.GeneralSettings_CompileBeforeParse_WarnCompileOnDemandEnabled,
                RubberduckUI.GeneralSettings_CompileBeforeParse_WarnCompileOnDemandEnabled_Caption, true))
            {
                return false;
            }

            _vbeSettings.CompileOnDemand = false;
            _vbeSettings.BackGroundCompile = false;
            return true;
        }

        private int _autoSavePeriod;
        public int AutoSavePeriod
        {
            get => _autoSavePeriod;
            set
            {
                if (_autoSavePeriod != value)
                {
                    _autoSavePeriod = value;
                    OnPropertyChanged();
                }
            }
        }

        private DelimiterOptions _delimiter;
        public DelimiterOptions Delimiter
        {
            get => _delimiter;
            set
            {
                if (_delimiter != value)
                {
                    _delimiter = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<MinimumLogLevel> LogLevels { get; set; }
        private MinimumLogLevel _selectedLogLevel;
        private bool _userEditedLogLevel;

        public MinimumLogLevel SelectedLogLevel
        {
            get => _selectedLogLevel;
            set
            {
                if (!Equals(_selectedLogLevel, value))
                {
                    _userEditedLogLevel = true;
                    _selectedLogLevel = value;
                    OnPropertyChanged();
                }
            }
        }

        public CommandBase ShowLogFolderCommand { get; }

        private void ShowLogFolder()
        {
            _operatingSystem.ShowFolder(ApplicationConstants.LOG_FOLDER_PATH);
        }

        public void UpdateConfig(Configuration config)
        {
            config.UserSettings.GeneralSettings = GetCurrentGeneralSettings();
            config.UserSettings.HotkeySettings.Settings = Hotkeys.ToArray();
        }

        public void SetToDefaults(Configuration config)
        {
            TransferSettingsToView(config.UserSettings.GeneralSettings, config.UserSettings.HotkeySettings);
        }

        private Rubberduck.Settings.GeneralSettings GetCurrentGeneralSettings()
        {
            return new Rubberduck.Settings.GeneralSettings
            {
                Language = SelectedLanguage,
                CanShowSplash = ShowSplashAtStartup,
                CanCheckVersion = CheckVersionAtStartup,
                CompileBeforeParse = CompileBeforeParse,
                SetDpiUnaware =  SetDpiUnaware,
                IsSmartIndenterPrompted = _indenterPrompted,
                IsAutoSaveEnabled = AutoSaveEnabled,
                AutoSavePeriod = AutoSavePeriod,
                UserEditedLogLevel = _userEditedLogLevel,
                MinimumLogLevel = _selectedLogLevel.Ordinal,
                EnableExperimentalFeatures = ExperimentalFeatures
            };
        }

        protected override void TransferSettingsToView(Rubberduck.Settings.GeneralSettings toLoad)
        {
            TransferSettingsToView(toLoad, null);
        }

        private void TransferSettingsToView(IGeneralSettings general, IHotkeySettings hottkey)
        {
            SelectedLanguage = Languages.FirstOrDefault(culture => culture.Code == general.Language.Code);

            Hotkeys = hottkey == null
                ? new ObservableCollection<HotkeySetting>()
                : new ObservableCollection<HotkeySetting>(hottkey.Settings);
            ShowSplashAtStartup = general.CanShowSplash;
            CheckVersionAtStartup = general.CanCheckVersion;
            CompileBeforeParse = general.CompileBeforeParse;
            SetDpiUnaware = general.SetDpiUnaware;
            _indenterPrompted = general.IsSmartIndenterPrompted;
            AutoSaveEnabled = general.IsAutoSaveEnabled;
            AutoSavePeriod = general.AutoSavePeriod;
            _userEditedLogLevel = general.UserEditedLogLevel;
            _selectedLogLevel = LogLevels.First(l => l.Ordinal == general.MinimumLogLevel);

            ExperimentalFeatures = _experimentalFeatureTypes
                .SelectMany(s => s.CustomAttributes.Where(a => a.ConstructorArguments.Any()).Select(a => (string)a.ConstructorArguments.First().Value))
                .Distinct()
                .Select(s => new ExperimentalFeatures { IsEnabled = general.EnableExperimentalFeatures.SingleOrDefault(d => d.Key == s)?.IsEnabled ?? false, Key = s })
                .ToList();
        }

        protected override string DialogLoadTitle => SettingsUI.DialogCaption_LoadGeneralSettings;
        protected override string DialogSaveTitle => SettingsUI.DialogCaption_SaveGeneralSettings;

        protected override void ImportSettings()
        {
            using (var dialog = new OpenFileDialog
            {
                Filter = SettingsUI.DialogMask_XmlFilesOnly,
                Title = SettingsUI.DialogCaption_LoadGeneralSettings
            })
            {
                dialog.ShowDialog();
                if (string.IsNullOrEmpty(dialog.FileName)) return;
                var general = Service.Load(new Rubberduck.Settings.GeneralSettings(), dialog.FileName);
                var hotkey = _hotkeyService.Load(new HotkeySettings(), dialog.FileName);
                //Always assume Smart Indenter registry import has been prompted if importing.
                general.IsSmartIndenterPrompted = true;
                TransferSettingsToView(general, hotkey);
            }
        }

        protected override void ExportSettings(Rubberduck.Settings.GeneralSettings settings)
        {
            using (var dialog = new SaveFileDialog
            {
                Filter = SettingsUI.DialogMask_XmlFilesOnly,
                Title = SettingsUI.DialogCaption_SaveGeneralSettings
            })
            {
                dialog.ShowDialog();
                if (string.IsNullOrEmpty(dialog.FileName)) return;
                Service.Save(settings, dialog.FileName);
                _hotkeyService.Save(new HotkeySettings { Settings = Hotkeys.ToArray() }, dialog.FileName);
            }
        }
    }
}