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

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace MachinMachines.Utils
{
    /// <summary>
    /// Compare two game objects using only their scene path
    /// Notice that it can skip a fixed number of parents in the path
    /// So with rootItemsSkipCount = 1 we get:
    /// "bg_hseJulia01_variant/bg_hseJulia01_ctr_root01/bg_hseJulia01_ctr_pipe01_mstr/bg_hseJulia01_mdl_pipe01"
    /// ==
    /// "bg_hseJulia01_other/bg_hseJulia01_ctr_root01/bg_hseJulia01_ctr_pipe01_mstr/bg_hseJulia01_mdl_pipe01"
    /// </summary>
    public class GameObjectPathComparer : IEqualityComparer<GameObject>
    {
        int _rootItemsSkipCount;

        public GameObjectPathComparer(int rootItemsSkipCount)
        {
            _rootItemsSkipCount = rootItemsSkipCount;
        }

        public bool Equals(GameObject lhs, GameObject rhs)
        {
            // Check whether the objects are the same GO instance
            if (lhs.Equals(rhs))
            {
                return true;
            }
            string lhsRelativePath = string.Join('/',
                GameObjectHierarchy.GetScenePath(lhs).Split('/')
                                                     .Skip(_rootItemsSkipCount));
            string rhsRelativePath = string.Join('/',
                GameObjectHierarchy.GetScenePath(rhs).Split('/')
                                                     .Skip(_rootItemsSkipCount));
            return lhsRelativePath == rhsRelativePath;
        }

        public int GetHashCode(GameObject obj)
        {
            string relativePath = string.Join('/',
                GameObjectHierarchy.GetScenePath(obj).Split('/')
                                                     .Skip(_rootItemsSkipCount));
            return relativePath.GetHashCode();
        }
    }

    public static class GameObjectHierarchy
    {
        /// <summary>
        /// Browse all game objects children of the given root game object (not included in the results)
        /// </summary>
        public static IEnumerable<GameObject> BrowseChildHierarchy(GameObject root, bool includeRoot = false)
        {
            if (includeRoot)
            {
                yield return root;
            }
            foreach (Transform childTransform in root.transform)
            {
                foreach (GameObject child in BrowseChildHierarchy_r(childTransform.gameObject))
                {
                    yield return child;
                }
            }
        }

        /// <summary>
        /// Helper of the above BrowseChildHierarchy() returning the root game object
        /// </summary>
        public static IEnumerable<GameObject> BrowseChildHierarchy_r(GameObject root)
        {
            yield return root;
            foreach (Transform childTransform in root.transform)
            {
                foreach (GameObject child in BrowseChildHierarchy_r(childTransform.gameObject))
                {
                    yield return child;
                }
            }
        }

        /// <summary>
        /// Get to the root of the given leaf game object (included in the results)
        /// </summary>
        public static IEnumerable<GameObject> BrowseParentHierarchy(GameObject leaf, GameObject topObject = null)
        {
            // This time the leaf is not skipped
            yield return leaf;
            Transform parentTransform = leaf.transform.parent;
            while ((parentTransform != null) && (topObject == null || parentTransform != topObject.transform))
            {
                GameObject current = parentTransform.gameObject;
                yield return current;
                parentTransform = current.transform.parent;
            }
        }

        /// <summary>
        /// Retrieve an object full path in the scene hierarchy
        /// </summary>
        public static string GetScenePath(GameObject gameObject)
        {
            return string.Join('/', BrowseParentHierarchy(gameObject)
                                    .Reverse()
                                    .Select(item => item.name)
                              );
        }

        /// <summary>
        /// Retrieve a transform from its name in a game object children
        /// Similar to Transform.Find() but case sensitivity is controlled
        /// + it is recursive into the entire children hierarchy!
        /// </summary>
        public static Transform Find_r(GameObject gameObject, string n)
        {
            GameObject found = BrowseChildHierarchy(gameObject)
                                .Where(item => item.name.Equals(n,
                                                                StringComparison.OrdinalIgnoreCase))
                                .FirstOrDefault();
            if (found != null)
            {
                return found.transform;
            }
            return null;
        }
    }
}
