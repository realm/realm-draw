﻿////////////////////////////////////////////////////////////////////////////
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

using Realms;

namespace DrawXShared
{
    // We store settings in a dedicated Realm. This Realm will contain one instance of this class.
    public class DrawXSettings : RealmObject
    {
        public string LastColorUsed { get; set; } = "Indigo";

        public string ServerIP { get; set; }  // without prefix ie no http://

        public string Username { get; set; }

        public string Password { get; set; }
    }
}
