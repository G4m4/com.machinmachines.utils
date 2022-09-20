// Copyright 2022 MachinMachines
//
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;

namespace StudioManette
{
    namespace Utils
    {
        namespace Settings
        {
            public class ProdSettings : SettingsBase<ProdSettings>
            {
                [Header("Production")]

                [Tooltip("Project short initials")]
                public string ProductionShortName = "SMJ";

                [Tooltip("Sequence prefix for folder/file names")]
                public string ProductionSequencePrefixName = "s";

                [Tooltip("Shot prefix for folder/file names")]
                public string ProductionShotPrefixName = "p";

                [Tooltip("Animation framerate")]
                public float Framerate = 25.0f;
            }
        }
    }
}
