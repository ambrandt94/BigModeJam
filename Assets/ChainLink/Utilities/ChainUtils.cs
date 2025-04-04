using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

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
            if (instance == null)
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

    public class RoutineRunner : Singleton<RoutineRunner>
    {
        private Dictionary<string, Coroutine> routineTable;

        public void StartRoutine(IEnumerator routine)
        {
            StartCoroutine(routine);
        }

        public void InstantiateAfterDelay(GameObject prefab, float delay, Vector3 pos, Quaternion rot, Transform parent, Action<GameObject> afterSpawn)
        {
            Debug.Log("Queue Instantiate...");
            Action action = () => {
                if (parent == null) {
                    Debug.Log("SPAWN");
                    GameObject obj = Instantiate(prefab, pos, rot);
                    afterSpawn?.Invoke(obj);
                } else {
                    Debug.Log("SPAWN");
                    GameObject obj = Instantiate(prefab, pos, rot, parent);
                    afterSpawn?.Invoke(obj);
                }
            };
            DoAfterDelay(action, delay);
        }

        public void InstantiateAfterDelay(GameObject prefab, float delay, Transform parent, bool parentToParent, Action<GameObject> afterSpawn)
        {
            Debug.Log("Queue Instantiate...");
            Action action = () => {
                if (parentToParent) {
                    GameObject obj = Instantiate(prefab, parent);
                    afterSpawn?.Invoke(obj);
                } else {
                    GameObject obj = Instantiate(prefab, parent.position, parent.rotation);
                    afterSpawn?.Invoke(obj);
                }
            };
            DoAfterDelay(action, delay);
        }


        public void DoAfterDelay(Action action, float delay)
        {
            StartCoroutine(DoAfterDelayRoutine(action, delay));
        }

        private IEnumerator DoAfterDelayRoutine(Action action, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            action?.Invoke();
        }
    }
}