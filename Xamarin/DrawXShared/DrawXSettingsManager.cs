////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using Realms;

namespace DrawXShared
{
    internal static class DrawXSettingsManager
    {
        private static Realm _localSettingsRealm;
        private static DrawXSettings _savedSettings;

        public static DrawXSettings Settings
        {
            get
            {
                if (_savedSettings == null)
                {
                    _savedSettings = _localSettingsRealm.All<DrawXSettings>().FirstOrDefault();
                }

                if (_savedSettings == null)
                {
                    Write(() =>
                    {
                        _savedSettings = _localSettingsRealm.Add(new DrawXSettings
                        {
                            LastColorUsed = "Indigo"
                        });
                    });
                }

                return _savedSettings;
            }
        }

        internal static void InitLocalSettings()
        {
            var settingsConf = new RealmConfiguration("DrawXsettings.realm");
            settingsConf.ObjectClasses = new[] { typeof(DrawXSettings) };
            settingsConf.SchemaVersion = 3;  // set explicitly and bump as we add setting properties
            _localSettingsRealm = Realm.GetInstance(settingsConf);
        }

        // bit of a hack which only works when the caller has objects already on the _realmLocalSettings Realm
        internal static void Write(Action writer)
        {
            _localSettingsRealm.Write(writer);
        }
    }
}
