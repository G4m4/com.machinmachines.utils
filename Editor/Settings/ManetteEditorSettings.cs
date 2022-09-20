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

using UnityEditor;

using UnityEngine;

namespace MachinMachines
{
    namespace Utils
    {
        namespace Settings
        {
            public class ManetteEditorSettings : SettingsBase<ManetteEditorSettings>
            {
                [Header("General paths")]

                [IsFilepath(false, "", IsFilepathAttribute.FileType.Directory)]
                [Tooltip("'Local folder' (within the episode project assets hierarchy) where we store temporary data pulled by the description files")]
                public string LocalEpisodeFolder = "Assets/Local";

                [Header("Anim importer")]
                public bool RemoveFBXAfterImport = true;
                public bool OverwriteExistingFiles = false;
                public bool StoreBinarizedAssets = true;
                [IsFilepath(false, "", IsFilepathAttribute.FileType.Directory)]
                public string AnimationsAssetsFolder = "Assets/_Anim/Clips";
                [IsFilepath(false, "", IsFilepathAttribute.FileType.Directory)]
                public string FBXImportFolder = "Assets/_Anim/SourceFBX";
                public AnimationUtility.TangentMode TangentMode = AnimationUtility.TangentMode.Constant;

                [Space(32)]

                [Header("FBX importer")]
                [SerializeField]
                public bool EnableFBXPostProcessorChecks = false;
            }
        }
    }
}
