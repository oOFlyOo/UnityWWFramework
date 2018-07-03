
#define UseAnimatorUtility

using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using WWFramework.Extension;

namespace WWFramework.Optimization.Editor
{
    public static class ModelOptimizeHelper
    {
        public const string FashionPath = "Assets/GameResources/ArtResources/Characters/Fashion";
        public const string PetPath = "Assets/GameResources/ArtResources/Characters/Pet";
        public const string WeaponPath = "Assets/GameResources/ArtResources/Characters/Weapon";
        public const string SoulPath = "Assets/GameResources/ArtResources/Characters/Soul";
        public const string OtherPath = "Assets/GameResources/ArtResources/Characters/Other";

        private const string PetPrefix = "pet_";
        private const string RidePrefix = "ride_";
        // 美术直接挂上了特效？
        private const string ParticleViewPrefix = "Particle View";

        private const string ErrorApplyOptimizeOption = @"{0} 无法修改导入优化设置选项，应该是哪里强制设置了，请手动添加绑点或者联系技术！";

        private enum ModelType
        {
            None,
            Fashion,
            Pet,
            Ride,
            Weapon,
            Soul,
            Other,
        }

        /// <summary>
        /// 如果手动处理过，则最好先还原再来做优化
        /// 坑爹的Unity，自己都不用OptimizeTransformHierarchy，就来坑人
        /// 由于
        /// </summary>
        /// <param name="importer"></param>
        public static void OptimizeGameObject(ModelImporter importer)
        {
            var modelTpye = GetModelType(importer);
            var prefabPath = GetPrefabPath(importer);

            var prefab = AssetDatabase.LoadMainAssetAtPath(prefabPath) as GameObject;
            if (prefab == null)
            {
                Debug.LogErrorFormat(importer, "{0} 找不到Prefab {1}", importer.assetPath, prefabPath);
                return;
            }
            else
            {
                var prefabAnimator = prefab.GetComponent<Animator>();
                if (prefabAnimator == null)
                {
                    Debug.LogErrorFormat(prefab, "{0} 找不到 Animator", prefabPath);
                    return;
                }
                else if (!prefabAnimator.hasTransformHierarchy)
                {
                    Debug.LogErrorFormat(prefab, "{0} 优化过的请先还原", prefabPath);
                    return;
                }
            }

#if !UseAnimatorUtility
            if (importer.optimizeGameObjects)
            {
                importer.optimizeGameObjects = false;
                importer.SaveAndReimport();

                if (importer.optimizeGameObjects)
                {
                    Debug.LogErrorFormat(importer, ErrorApplyOptimizeOption, importer.assetPath);
                    return;
                }
            }
#endif

            var modelPrefab = AssetDatabase.LoadMainAssetAtPath(importer.assetPath) as GameObject;
            //            var prefabIns = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            var prefabIns = Object.Instantiate(prefab);
            //            var prefabInsOrigin = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            var prefabInsOrigin = Object.Instantiate(prefab);
            var modelIns = PrefabUtility.InstantiatePrefab(modelPrefab) as GameObject;
            prefabIns.transform.position = Vector3.zero;
            prefabInsOrigin.transform.position = Vector3.zero;
            modelIns.transform.position = Vector3.zero;

            // 将名字还原回去，才能很好的对比
            RevertPreProcess(modelTpye, modelIns, prefabIns);
            // 养成优化的好习惯
            var prefabInsTrans = prefabIns.transform;
            var modelInsTrans = modelIns.transform;
            var allModelPaths = new HashSet<string>();
            allModelPaths.UnionWith(importer.transformPaths);

#if UseAnimatorUtility
            // 这里需要改名后的节点，用于移动新增节点
            var prefabIns2 = Object.Instantiate(prefabIns);
            var prefabIns2Trans = prefabIns2.transform;
            prefabIns2Trans.position = Vector3.zero;
#endif

            // 不是模型原本的节点，后来加上去的
            // 这里注意，跟在根节点后面及子节点的，不做处理
            List<string> addPaths = null;
            Dictionary<Transform, Transform> new2OriginMap = null;
            HashSet<string> extraPaths = null;
            HashSet<Transform> moveObjs = null;
#if UseAnimatorUtility
            if (importer.optimizeGameObjects)
            {
                // 这样做会断了Connect，不过没有关系
                AnimatorUtility.DeoptimizeTransformHierarchy(modelIns);
            }
            GetAddPaths(modelInsTrans, prefabIns2Trans, out addPaths, out new2OriginMap, mapTrans:prefabInsTrans);
            GetExtraPaths(addPaths, prefabIns2Trans, out extraPaths, out moveObjs);
#else
            GetAddPaths(modelInsTrans, prefabInsTrans, out addPaths, out new2OriginMap, allModelPaths);
            GetExtraPaths(addPaths, prefabInsTrans, out extraPaths, out moveObjs);
#endif

            // 配置里面的也得导出
            extraPaths.UnionWith(GetConfigExtart(importer, modelTpye));
            var extraArray = extraPaths.ToArray();

            // 强制覆盖
            importer.extraExposedTransformPaths = extraArray;
            importer.optimizeGameObjects = true;
            importer.SaveAndReimport();
#if !UseAnimatorUtility
            if (!importer.optimizeGameObjects)
            {
                Debug.LogErrorFormat(importer, ErrorApplyOptimizeOption, importer.assetPath);
            }
            else
#endif
            {
#if !UseAnimatorUtility
                CopyChange(prefabInsTrans, new2OriginMap);
#else
                AnimatorUtility.OptimizeTransformHierarchy(prefabIns, extraArray);
                FixSkinnedMeshRenderer(modelIns, prefabIns);
#endif

                AddNewGameObjects(moveObjs, new2OriginMap);

#if !UseAnimatorUtility
                // 改名
                PreProcess(modelTpye, modelIns, prefabInsOrigin);
                PrefabUtility.ReplacePrefab(modelIns, prefab);
#else
                // 改名
                PreProcess(modelTpye, prefabIns, prefabInsOrigin);
                PrefabUtility.ReplacePrefab(prefabIns, prefab);
#endif
            }

            Object.DestroyImmediate(prefabIns);
#if UseAnimatorUtility
            Object.DestroyImmediate(prefabIns2);
#endif
            Object.DestroyImmediate(prefabInsOrigin);
            Object.DestroyImmediate(modelIns);
        }

        public static void RevertOptimizeGameObject(ModelImporter importer)
        {
            var modelTpye = GetModelType(importer);
            var prefabPath = GetPrefabPath(importer);

            var prefab = AssetDatabase.LoadMainAssetAtPath(prefabPath) as GameObject;
            if (prefab == null)
            {
                Debug.LogErrorFormat(importer, "{0} 找不到Prefab {1}", importer.assetPath, prefabPath);
                return;
            }
            else
            {
                var prefabAnimator = prefab.GetComponent<Animator>();
                if (prefabAnimator == null)
                {
                    Debug.LogErrorFormat(prefab, "{0} 找不到 Animator", prefabPath);
                    return;
                }
                else if (prefabAnimator.hasTransformHierarchy)
                {
                    Debug.LogErrorFormat(prefab, "{0} 并没有优化过", prefabPath);
                    return;
                }
            }

#if !UseAnimatorUtility
            if (!importer.optimizeGameObjects)
            {
                importer.optimizeGameObjects = true;
                importer.SaveAndReimport();

                if (!importer.optimizeGameObjects)
                {
                    Debug.LogErrorFormat(importer, ErrorApplyOptimizeOption, importer.assetPath);
                    return;
                }
            }
#endif

            var modelPrefab = AssetDatabase.LoadMainAssetAtPath(importer.assetPath) as GameObject;
            var prefabIns = Object.Instantiate(prefab);
            var prefabInsOrigin = Object.Instantiate(prefab);
            var modelIns = PrefabUtility.InstantiatePrefab(modelPrefab) as GameObject;
            prefabIns.transform.position = Vector3.zero;
            modelIns.transform.position = Vector3.zero;

            // Prefab的名字要是更改过，则恢复到改名前
            RevertPreProcess(modelTpye, modelIns, prefabIns);

#if !UseAnimatorUtility
            var modelInsTrans = modelIns.transform;
            var prefabInsTrans = prefabIns.transform;

            List<string> addPaths = null;
            Dictionary<Transform, Transform> new2OriginMap = null;
            GetAddPaths(modelInsTrans, prefabInsTrans, out addPaths, out new2OriginMap);

            HashSet<string> extraPaths = null;
            HashSet<Transform> moveObjs = null;
            GetExtraPaths(addPaths, prefabInsTrans, out extraPaths, out moveObjs);
#endif

            if (importer.optimizeGameObjects)
            {
                importer.optimizeGameObjects = false;
                importer.SaveAndReimport();

#if !UseAnimatorUtility
                if (importer.optimizeGameObjects)
                {
                    Debug.LogErrorFormat(importer, ErrorApplyOptimizeOption, importer.assetPath);
                }
                else
#endif
                {
#if !UseAnimatorUtility
                    CopyChange(prefabInsTrans, new2OriginMap);
                    AddNewGameObjects(moveObjs, new2OriginMap);
#else
                    // 解体，要是名字对应不上，则会新增节点
                    AnimatorUtility.DeoptimizeTransformHierarchy(prefabIns);
                    FixSkinnedMeshRenderer(modelIns, prefabIns);
#endif

#if !UseAnimatorUtility
                    // 这里再次检查改名
                    PreProcess(modelTpye, modelIns, prefabInsOrigin);
                    PrefabUtility.ReplacePrefab(modelIns, prefab);
#else
                    // 这里再次检查改名
                    PreProcess(modelTpye, prefabIns, prefabInsOrigin);
                    PrefabUtility.ReplacePrefab(prefabIns, prefab);
#endif
                }
            }

            Object.DestroyImmediate(prefabIns);
            Object.DestroyImmediate(prefabInsOrigin);
            Object.DestroyImmediate(modelIns);
        }

        private static List<string> GetConfigExtart(ModelImporter importer, ModelType type)
        {
            var extraList = new List<string>();

            return extraList;
        }

        /// <summary>
        /// 这里只针对简单的改名，要是复杂的，则很难对应上
        /// </summary>
        /// <param name="importType"></param>
        /// <param name="modelInstance"></param>
        /// <param name="prefabInstance"></param>
        private static void PreProcess(ModelType importType, GameObject modelInstance, GameObject prefabInstance)
        {
            //兼容美术的潜规则，针对 fbx文件 和 prefab文件节点名对应不上的情况
            if (importType == ModelType.Ride)
            {
                var prefabSkin = prefabInstance.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                // 潜规则，有一个改名了全都改名
                if (prefabSkin[0].name.StartsWith(RidePrefix))
                {
                    var skins = modelInstance.GetComponentsInChildren<SkinnedMeshRenderer>(true);

                    skins.ForEach(item =>
                    {
                        string itemName = item.gameObject.name;
                        if (!itemName.StartsWith(RidePrefix))
                        {
                            item.gameObject.name = itemName.Insert(0, RidePrefix);
                        }
                    });
                }
            }
        }

        /// <summary>
        /// 和 PreProcess 相反
        /// </summary>
        /// <param name="importType"></param>
        /// <param name="modelInstance"></param>
        /// <param name="prefabInstance"></param>
        private static void RevertPreProcess(ModelType importType, GameObject modelInstance, GameObject prefabInstance)
        {
            //兼容美术的潜规则，针对 fbx文件 和 prefab文件节点名对应不上的情况
            if (importType == ModelType.Ride)
            {
                var prefabSkin = prefabInstance.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                // 潜规则，有一个改名了全都改名
                if (prefabSkin[0].name.StartsWith(RidePrefix))
                {
                    var skins = modelInstance.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    var regex = new Regex(RidePrefix);
                    skins.ForEach(item =>
                    {
                        string itemName = item.name;
                        if (!itemName.StartsWith(RidePrefix))
                        {
                            // 根据Mesh来匹配最准确
                            var matchRender = prefabSkin.FirstOrDefault(renderer => renderer.sharedMesh == item.sharedMesh);
                            if (matchRender != null)
                            {
                                matchRender.name = regex.Replace(matchRender.name, "", 1, 0);
                            }
                        }
                    });
                }
            }
        }

        #region 辅助
        private static ModelType GetModelType(ModelImporter importer)
        {
            var path = importer.assetPath;
            var name = Path.GetFileNameWithoutExtension(path);
            if (path.StartsWith(FashionPath))
            {
                return ModelType.Fashion;
            }
            else if (path.StartsWith(PetPath))
            {
                if (name.StartsWith(PetPrefix))
                {
                    return ModelType.Pet;
                }
                else if (name.StartsWith(RidePrefix))
                {
                    return ModelType.Ride;
                }
                return ModelType.None;
            }
            else if (path.StartsWith(WeaponPath))
            {
                return ModelType.Weapon;
            }
            else if (path.StartsWith(SoulPath))
            {
                return ModelType.Soul;
            }
            else if (path.StartsWith(OtherPath))
            {
                return ModelType.Other;
            }

            return ModelType.None;
        }

        /// <summary>
        /// 暂时只考虑对同名的进行处理
        /// 后续可以考虑符合的都进行更改
        /// </summary>
        /// <param name="importer"></param>
        /// <returns></returns>
        private static string GetPrefabPath(ModelImporter importer)
        {
            var path = importer.assetPath.Replace("/Meshes/", "/Prefabs/");
            path = path.Replace(Path.GetExtension(path), ".prefab");

            return path;
        }

        private static string GetPathExcludeRoot(this Transform trans, Transform root)
        {
            if (trans == null || root == null || trans == root)
            {
                return string.Empty;
            }

            if (trans.parent != root)
            {
                return string.Format("{0}/{1}", trans.parent.GetPathExcludeRoot(root), trans.name);
            }

            return trans.name;
        }

        private static void GetAddPaths(Transform originTrans, Transform newTrans, out List<string> addPaths,
            out Dictionary<Transform, Transform> new2OriginMap, HashSet<string> originPaths = null, Transform mapTrans = null)
        {
            addPaths = new List<string>();
            new2OriginMap = new Dictionary<Transform, Transform>();

            // 如果自己计算的话，得保证优化状态一致
            if (originPaths == null)
            {
                originPaths = new HashSet<string>();
                foreach (var trans in originTrans.GetComponentsInChildren<Transform>(true))
                {
                    originPaths.Add(trans.GetPathExcludeRoot(originTrans));
                }
            }

            // 兼容 AnimatorUtility
            originTrans = mapTrans ?? originTrans;

            var transforms = newTrans.GetComponentsInChildren<Transform>(true);
            // 谁把同名的弄出来，干死他
            foreach (var trans in transforms)
            {
                if (trans == newTrans)
                {
                    new2OriginMap.Add(newTrans, originTrans);
                    continue;
                }

                var path = trans.GetPathExcludeRoot(newTrans);
                if (!originPaths.Contains(path))
                {
                    addPaths.Add(path);
                }
                else
                {
                    // 保留引用，后面还原到Prefab状态需要用上；使用搜索得保证优化状态一致
                    new2OriginMap.Add(trans, originTrans.Find(path));
                }
            }
        }

        private static void GetExtraPaths(List<string> addPaths, Transform fromObj, out HashSet<string> extraPaths, out HashSet<Transform> moveObjs)
        {
            // 被包括的在前面
            addPaths.Sort((path1, path2) => path1.CompareTo(path2));
            extraPaths = new HashSet<string>();
            moveObjs = new HashSet<Transform>();
            // 首个字符没用
            var comparePath = "/";
            foreach (var path in addPaths)
            {
                // 依赖在根节点
                if (!path.Contains("/"))
                {
                    // 根节点不需要添加导出
                    moveObjs.Add(fromObj.Find(path));
                    comparePath = path;
                }
                else
                {
                    if (!path.StartsWith(comparePath))
                    {
                        // 有可能多个增加的节点共用一个父节点
                        extraPaths.Add(Path.GetDirectoryName(path));

                        moveObjs.Add(fromObj.Find(path));
                        comparePath = path;
                    }
                }
            }
        }

        private static void AddNewGameObjects(HashSet<Transform> moveObjs,
            Dictionary<Transform, Transform> new2OriginMap)
        {
            foreach (var obj in moveObjs)
            {
                AddGameObject(obj, new2OriginMap[obj.parent]);
            }
        }

        private static void AddGameObject(Transform fromObj, Transform toParent)
        {
            var go = Object.Instantiate(fromObj.gameObject, toParent);
            go.name = fromObj.name;
            //            fromObj.SetParent(toParent);
        }

        private static void CopyChange(Transform fromTrans, Dictionary<Transform, Transform> new2OriginMap)
        {
            // 对已有节点进行赋值
            foreach (var keyValue in new2OriginMap)
            {
                CopyTransform(keyValue.Key, keyValue.Value);
            }
            // Animator 赋值
            var fromAnimator = fromTrans.GetComponent<Animator>();
            CopyAnimator(fromAnimator, new2OriginMap[fromAnimator.transform].GetComponent<Animator>());
            // SkinMesh
            foreach (var fromRender in fromTrans.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                var modelRender = new2OriginMap[fromRender.transform].GetComponent<SkinnedMeshRenderer>();
                CopySkinnedMeshRenderer(fromRender, modelRender);
            }
        }

        private static void CopyTransform(Transform fromObj, Transform toObj)
        {
            toObj.gameObject.tag = fromObj.gameObject.tag;
            toObj.gameObject.layer = fromObj.gameObject.layer;
        }

        private static void CopyAnimator(Animator fromObj, Animator toObj)
        {
            toObj.runtimeAnimatorController = fromObj.runtimeAnimatorController;
            toObj.applyRootMotion = fromObj.applyRootMotion;
            toObj.updateMode = fromObj.updateMode;
            toObj.cullingMode = fromObj.cullingMode;
        }

        private static void CopySkinnedMeshRenderer(SkinnedMeshRenderer fromObj, SkinnedMeshRenderer toObj)
        {
            toObj.shadowCastingMode = fromObj.shadowCastingMode;
            toObj.receiveShadows = fromObj.receiveShadows;
            toObj.motionVectors = fromObj.motionVectors;
            toObj.sharedMaterials = fromObj.sharedMaterials;
            toObj.lightProbeUsage = fromObj.lightProbeUsage;
            toObj.reflectionProbeUsage = fromObj.reflectionProbeUsage;
            toObj.probeAnchor = fromObj.probeAnchor;
            toObj.quality = fromObj.quality;
            toObj.updateWhenOffscreen = fromObj.updateWhenOffscreen;
            toObj.skinnedMotionVectors = fromObj.skinnedMotionVectors;
            //            toObj.rootBone = fromObj.rootBone;
        }
        #endregion

        #region  AnimatorUtility Bug修复
        private static void FixSkinnedMeshRenderer(SkinnedMeshRenderer fromObj, SkinnedMeshRenderer toObj, bool dependModel = false)
        {
            var bounds = fromObj.localBounds;
            if (!dependModel)
            {
                var min = bounds.min;
                var max = bounds.max;
                var fromScale = fromObj.transform.localScale;
                var toScale = toObj.transform.localScale;
                toScale.Scale(new Vector3(1 / fromScale.x, 1 / fromScale.y, 1 / fromScale.z));
                min.Scale(toScale);
                max.Scale(toScale);
                bounds.SetMinMax(min, max);
            }

            toObj.localBounds = bounds;
        }

        /// <summary>
        /// fromObj 本身必须是正常的
        /// </summary>
        /// <param name="fromObj"></param>
        /// <param name="toObj"></param>
        private static void FixSkinnedMeshRenderer(GameObject fromObj, GameObject toObj, bool dependModel = false)
        {
            var fromRenders = fromObj.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var toRenders = toObj.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var toRenderDict = new Dictionary<string, SkinnedMeshRenderer>();
            toRenders.ForEach(renderer => toRenderDict.Add(renderer.sharedMesh.name, renderer));

            fromRenders.ForEach(renderer => FixSkinnedMeshRenderer(renderer, toRenderDict[renderer.sharedMesh.name], dependModel));
        }
        #endregion
    }
}