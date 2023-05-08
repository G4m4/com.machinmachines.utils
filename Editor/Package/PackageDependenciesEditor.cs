// Copyright 2023 MachinMachines
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

using UnityEditor;
using UnityEngine;

using MachinMachines.DGML;
using UnityEditor.IMGUI.Controls;
using MachinMachines.Utils;
using UnityEngine.UIElements;

namespace MachinMachines.Packages
{
    public class PackageDependenciesWindow : EditorWindow
    {
        private TextAsset _packagesDependenciesTextAsset = null;
        static TreeViewState treeViewState_ = null;
        static GenericHierarchicalTreeView<PackageDependenciesGraphItem> treeView_ = null;

        [MenuItem("MachinMachines/PackageDependencies")]
        static void Init()
        {
            GetWindow(typeof(PackageDependenciesWindow));
        }

        public void OnGUI()
        {
            _packagesDependenciesTextAsset = EditorGUILayout.ObjectField(
                _packagesDependenciesTextAsset,
                typeof(TextAsset),
                false,
                GUILayout.MaxWidth(256.0f)) as TextAsset;

            if (GUILayout.Button("Build hierarchical tree"))
            {
                string assetPath = AssetDatabase.GetAssetPath(_packagesDependenciesTextAsset);
                if (!string.IsNullOrEmpty(assetPath)) {
                    using (StreamReader stream = new(assetPath))
                    {
                        PackageDependencies data = PackageDependencies.Read<PackageDependencies>(stream.ReadToEnd());
                        string graphPath = Path.ChangeExtension(assetPath, DGMLSerialiser.Extension);
                        PackageDependenciesGraphItem.BuildChildrenTree(data,
                                                                       graph => DumpAsDGML(graph, graphPath));
                    }
                }
            }
            if (treeView_ != null)
            {
                var rect = EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));

                treeView_.OnGUI(rect);

                EditorGUILayout.EndVertical();
            }
        }

        static private void DumpAsDGML(PackageDependenciesGraphItem graph, string graphPath)
        {
            graph.DumpAsDGMLToPath(graphPath);
            treeViewState_ = new TreeViewState();
            treeView_ = new GenericHierarchicalTreeView<PackageDependenciesGraphItem>(graph, treeViewState_);
            treeView_.Reload();
        }
    }
}
