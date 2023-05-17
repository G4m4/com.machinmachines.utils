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

using System.Collections.Generic;
using System.IO;
using System.Linq;

using MachinMachines.Utils;

using UnityEngine;
using UnityEngine.Profiling;

namespace MachinMachines.Common.Repository
{
    /// <summary>
    /// Singleton holding a reference to all files repositories
    /// There can be multiple ones per type (for instance multiple files for texture repositories)
    /// but also multiple types (for instance texture and materials repositories)
    /// 
    /// Notice that it behaves differently between editor/built game:
    /// - in editor/PIE we look for all assets of the correct type
    /// - in built games we expect FilesRepository to auto-register during their Awake() phase,
    /// which means that they have to be referenced by some owning scene object
    /// </summary>
    public class FilesRepositoryManager
    {
        public const string FileExtension = "filesrepository";

        /// <summary>
        /// Where to look for when resolving a path
        /// </summary>
        public enum PathResolution
        {
            Regular,  // Default
            EditorOnly,
            ExternalOnly
        }

        /// <summary>
        /// Thread-safe accessor to the unique instance
        /// Thread-safety is required during the (multithreaded) import pass
        /// No double-checked locking etc. as we are not caring too much
        /// about performance of this accessor so far
        /// </summary>
        public static FilesRepositoryManager Instance
        {
            get
            {
                Profiler.BeginSample("FilesRepositoryManager - instance getter");
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new FilesRepositoryManager();
                    }
                }
                Profiler.EndSample();
                return _instance;
            }
        }

        /// <summary>
        /// Mostly for debug
        /// </summary>
        public FilesRepository[] Repositories { get { return _repositories.ToArray(); } }

        /// <summary>
        /// Thread-safety required! @see Instance
        /// </summary>
        private static readonly object _lock = new object();
        private static FilesRepositoryManager _instance = new FilesRepositoryManager();
        // Ugly reentrancy flag to check if we are currently retrieving all file repository asset files
        private static bool _isBrowsingAssets = false;
        private HashSet<FilesRepository> _repositories = new HashSet<FilesRepository>();

        public IEnumerable<FilesRepository.Repository> GetRepositoriesForUsage(RepositoryUsage usage)
        {
            // Return all repositories for one given usage
            return _repositories.Where(item => item != null && item.Usage == usage)
                                // All flattened into one big list
                                .SelectMany(item => item._repositories)
                                // Ordered by priority
                                .OrderBy(item => item.Priority);
        }

        /// <summary>
        /// Should only be used for Repository imports
        /// </summary>
        public void AddRepository(FilesRepository item)
        {
            _repositories.Add(item);
        }

        /// <summary>
        /// List all possible paths
        /// </summary>
        public IEnumerable<string> GetAllPaths(RepositoryUsage usage)
        {
            return GetRepositoriesForUsage(usage).Select(item => item.Path);
        }

        /// <summary>
        /// Return true if the given path is contained within all repositories
        /// </summary>
        public bool ContainsPath(RepositoryUsage usage, string path)
        {
            foreach (FilesRepository.Repository repo in GetRepositoriesForUsage(usage))
            {
                if (repo.Path.StartsWith(path, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Main entry point: given a partial file path,
        /// retrieve the first full path that matches an existing file
        /// 
        /// External code using it (e.g. texture pool) tends to have a surrounding tringlerie,
        /// so make sure you are not bypassing something by calling it directly!
        /// </summary>
        /// <param name="usage"></param>
        /// <param name="relativePath"></param>
        /// <param name="pathResolution">where to look for when resolving:
        /// "editor" folders such as those starting with "Assets/" or "Packages/" or external ones?</param>
        /// <returns></returns>
        public string GetFullPathFrom(RepositoryUsage usage,
                                      string relativePath,
                                      PathResolution pathResolution = PathResolution.Regular)
        {
            bool isDirectoryPath = string.IsNullOrEmpty(Path.GetExtension(relativePath));
            foreach (FilesRepository.Repository repo in GetRepositoriesForUsage(usage))
            {
                bool pathIsInEditor = PathIsInEditor(repo.Path);
                switch (pathResolution)
                {
                    default:
                    case PathResolution.Regular:
                        {
                            break;
                        }
                    case PathResolution.EditorOnly:
                        {
                            if (!pathIsInEditor)
                            {
                                continue;
                            }
                            break;
                        }
                    case PathResolution.ExternalOnly:
                        {
                            if (pathIsInEditor)
                            {
                                continue;
                            }
                            break;
                        }
                }
                // Not really "absolute" as it can be relative to the project root
                string absolutePath = Path.Combine(repo.Path, relativePath);
                bool exist = false;
                if (isDirectoryPath)
                {
                    exist = Directory.Exists(absolutePath);
                }
                else
                {
                    exist = File.Exists(absolutePath);
                }
                if (exist)
                {
                    return absolutePath;
                }
            }

            return null;
        }

        /// <summary>
        /// Secondary entry point: given an absolute file path,
        /// retrieve the path relative to the first entry found that matches
        /// </summary>
        /// <returns>If no repository entry can be found, it returns null</returns>
        public string GetRelativePathFrom(RepositoryUsage usage, string path)
        {
            foreach (FilesRepository.Repository repo in GetRepositoriesForUsage(usage))
            {
                if (Paths.PathStartsWith(path, repo.Path))
                {
                    return Paths.GetRelativePath(repo.Path + "/", path);
                }
                // Support for local files: also check absolute path for local files
                if (Paths.PathStartsWith(repo.Path, "Assets"))
                {
                    if (Paths.PathStartsWith(path, Application.dataPath))
                    {
                        string fullAssetsPath = Path.Combine(Application.dataPath, "..", repo.Path + "/");
                        return Paths.GetRelativePath(fullAssetsPath, path);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// True for paths local to the editor
        /// </summary>
        static public bool PathIsInEditor(string path)
        {
            return Paths.PathStartsWith(path, "Assets") || Paths.PathStartsWith(path, "Packages");
        }

        /// <summary>
        /// SLOW function: retrieve and cache all repository description assets
        /// </summary>
        public void FindAllRepositories()
        {
            if (_isBrowsingAssets)
            {
                return;
            }
            Profiler.BeginSample("FilesRepositoryManager - FindAllRepositories");
#if UNITY_EDITOR
            _isBrowsingAssets = true;
            foreach (string GUID in UnityEditor.AssetDatabase.FindAssets("t:StudioManette.Bob.Repository.FilesRepository"))
            {
                string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(GUID);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    FilesRepository filesRepository = UnityEditor.AssetDatabase.LoadAssetAtPath<FilesRepository>(assetPath);
                    if (filesRepository != null)
                    {
                        _repositories.Add(filesRepository);
                    }
                }
            }
#else
                    // Nothing here, repositories have to be referenced by a scene object and will auto-register during their Awake()
#endif  // UNITY_EDITOR

            Profiler.EndSample();
        }

        private FilesRepositoryManager()
        {
            FindAllRepositories();
        }
    }
}
