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

namespace MachinMachines
{
    namespace Utils
    {
        namespace Settings
        {
            public class ManetteSettings : SettingsBase<ManetteSettings>
            {
                [Header("General paths")]

                [Tooltip("Path where to look for BG models")]
                public string FolderBG = "02_BACKGROUND";
                [Tooltip("Path where to look for BE models")]
                public string FolderBE = "04_BGELEMENTS";
                [Tooltip("Path where to look for CH models")]
                public string FolderCH = "01_CHARACTER";
                [Tooltip("Path where to look for PR models")]
                public string FolderPR = "03_PROP";

                [Tooltip("Eventual folder where models get dumped by the creation pipeline")]
                public string AssetsExportFolder = "export";

                [Space(32)]

                [Header("SBX")]

                [IsFilepath(false, "", IsFilepathAttribute.FileType.RootDirectory)]
                [Tooltip("Root folder for models")]
                public string AssetsFolder = "X:/03_ASSETS";

                [IsFilepath(false, "", IsFilepathAttribute.FileType.RootDirectory)]
                [Tooltip("Root folder for shots description files")]
                public string ShotsFolder = "X:/04_SHOTS";

                [Space(32)]

                [Header("CBX")]

                [IsFilepath(false, "", IsFilepathAttribute.FileType.Directory)]
                [Tooltip("Local folder for episode specific description files and data")]
                public string EpisodeDataRootPath = "Assets/Episode";

                [Space(32)]

                [Header("MBX presets")]

                [IsFilepath(false, "", IsFilepathAttribute.FileType.Directory)]
                [Tooltip("Root folder for MBX presets, relative to the AssetsFolder")]
                public string MbxpFolder = "07_TEXTURES/MBXP";
            }
        }
    }
}
