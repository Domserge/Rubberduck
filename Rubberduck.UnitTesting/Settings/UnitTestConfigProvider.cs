﻿using Rubberduck.SettingsProvider;

namespace Rubberduck.UnitTesting.Settings
{
    public class UnitTestConfigProvider : IConfigProvider<UnitTestSettings>
    {
        private readonly IPersistanceService<UnitTestSettings> _persister;
        private readonly UnitTestSettings _defaultSettings;

        public UnitTestConfigProvider(IPersistanceService<UnitTestSettings> persister)
        {
            _persister = persister;

            _defaultSettings = new UnitTestSettings
            {
                BindingMode = BindingMode.LateBinding,
                AssertMode = AssertMode.StrictAssert,
                ModuleInit = true,
                MethodInit = true,
                DefaultTestStubInNewModule = false
            };
        }

        public UnitTestSettings Create()
        {
            return _persister.Load(_defaultSettings) ?? _defaultSettings;
        }

        public UnitTestSettings CreateDefaults()
        {
            return _defaultSettings;
        }

        public void Save(UnitTestSettings settings)
        {
            _persister.Save(settings);
        }
    }
}
