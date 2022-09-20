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

using System.IO;
using System.Reflection;

using UnityEditor;

using UnityEngine;

namespace StudioManette
{
    namespace Utils
    {
        namespace Settings
        {
            // Abstract base class for a class of JSON settings that can be used both in-editor or ingame
            // Stored in Streaming Assets so the JSON can be edited manually later on
            //
            // This way, child classes only need to hold the actual settings data and nothing else
            //
            // IMPORTANT: paths in those settings have to be well-formed if they are being used "as is"
            // For instance this would work on Linux, not on Windows:
            // MbxpFolder = "X:/03_ASSETS/07_TEXTURES/MBXP";
            // But this would work anywhere as it is using platform-specific API to fix the path:
            // Path.GetFullPath(MbxpFolder)
            // So beware! There is a tringlerie de l'enfer(tm) to make sure that no full absolute paths are stored in settings
            abstract public class SettingsBase<T> : ScriptableObject where T : ScriptableObject
            {
                // All settings are stored in the same folder, with the file named after their class
                // In editor we read the local folder, at runtime it's in StreamingAssets
                public static readonly string kRuntimeSettingsPath = Path.Combine(Application.streamingAssetsPath,
                                                                                 "ManetteSettings",
                                                                                 $"{typeof(T).Name}.json");
#if UNITY_EDITOR
                public static readonly string kEditorSettingsPath = Path.Combine(Paths.GetPackageRelativePath("com.studiomanette.utils"),
                                                                                 "Assets",
                                                                                 "ManetteSettings",
                                                                                 $"{typeof(T).Name}.json");
                public static readonly string kSettingsPath = kEditorSettingsPath;
#else
            public static readonly string kSettingsPath = kRuntimeSettingsPath;
#endif  // UNITY_EDITOR

                private static T _instance = null;

                // Static getter for retrieving the instance
                // If no instance is loaded already it does so by reading the JSON file
                // If no file can be found it writes it
                public static T GetOrCreateSettings()
                {
                    if (_instance == null)
                    {
                        _instance = ScriptableObject.CreateInstance<T>();
                        string directory = Path.GetDirectoryName(kSettingsPath);
                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }
                        if (File.Exists(kSettingsPath))
                        {
                            JsonUtility.FromJsonOverwrite(File.ReadAllText(kSettingsPath), _instance);
                        }
                        // We always write the file in case the base ScriptableObject gets updated from the code
                        // TODO @gama: decide whether the file or the code is right once and for all!
                        File.WriteAllText(kSettingsPath, JsonUtility.ToJson(_instance, true));
#if UNITY_EDITOR
                        // This is only useful so the file appears immediately in the projet browser
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
#endif  // UNITY_EDITOR
                        CreateFolders(_instance);
                    }
                    return _instance;
                }

                // Tiny helper for settings marked with the "IsFilepath" attribute:
                // it automagically creates all referenced folders
                private static void CreateFolders(T settings)
                {
                    foreach (FieldInfo info in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance))
                    {
                        IsFilepathAttribute filePathAttribute = System.Attribute.GetCustomAttribute(info, typeof(IsFilepathAttribute)) as IsFilepathAttribute;
                        if (filePathAttribute != null)
                        {
                            string propertyValue = info.GetValue(settings) as string;
                            if (!string.IsNullOrEmpty(propertyValue))
                            {
                                // Make sure the path is relative to prevent the issue talked about in the class header comment
                                if (filePathAttribute.Filetype != IsFilepathAttribute.FileType.RootDirectory
                                    && Path.IsPathFullyQualified(propertyValue))
                                {
                                    Debug.LogError($"Path {propertyValue} for field {info.Name} in settings file {Path.GetFileName(kSettingsPath)} is absolute, this is forbidden to prevent cross-platform issues!");
                                }
                                // Make sure reference folders exist
                                if (!Directory.Exists(propertyValue))
                                {
                                    // Make sure the drive exists before trying to create it
                                    string pathRoot = Path.GetPathRoot(propertyValue);
                                    if (string.IsNullOrEmpty(pathRoot))
                                    {
                                        if (propertyValue.StartsWith("Assets", System.StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            // If we are trying to create some "assets/***" subfolder within a runtime build, just stop
                                            if (!Application.isEditor)
                                            {
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            // We are trying to create some unrooted folder, just stop
                                            continue;
                                        }
                                    }
                                    if (string.IsNullOrEmpty(pathRoot) || Directory.Exists(pathRoot))
                                    {
                                        // This might fail for various reasons (typically access rights),
                                        // so better make sure we catch the potential exception
                                        try
                                        {
                                            Directory.CreateDirectory(propertyValue);
                                        }
                                        catch
                                        {
                                            Debug.LogWarning($"No access rights to create folder {propertyValue}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
