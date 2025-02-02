using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace ChainLink.Core
{
    public static class ChainUtils
    {
        public static T GetRandom<T>(T[] array)
        {
            if (array == null)
                return default;
            if (array.Length == 0)
                return default;
            return array[Random.Range(0, array.Length)];
        }
        public static T GetRandom<T>(List<T> array)
        {
            if (array == null)
                return default;
            if (array.Count == 0)
                return default;
            return array[Random.Range(0, array.Count)];
        }

        public static T GetOrAddComponent<T>(this GameObject target) where T : UnityEngine.Component
        {
            T instance = target.GetComponent<T>();
            if(instance == null)
                instance = target.AddComponent<T>();
            return instance;
        }
        public static T GetOrAddComponent<T>(this Transform target) where T : UnityEngine.Component
        {
            T instance = target.GetComponent<T>();
            if (instance == null)
                instance = target.AddComponent<T>();
            return instance;
        }

        public static void MarkDirty(GameObject obj)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(obj);
            if (PrefabUtility.IsPartOfPrefabAsset(obj)) {
                PrefabUtility.SavePrefabAsset(obj);
            }
#endif
        }
    }
}