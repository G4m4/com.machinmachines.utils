using UnityEngine;

namespace StudioManette
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
