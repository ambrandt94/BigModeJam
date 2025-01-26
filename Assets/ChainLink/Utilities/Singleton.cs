using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChainLink.Core
{
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {

        public static T Instance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<T>();
                    if (instance == null) {
                        instance = new GameObject(typeof(T).ToString()).AddComponent<T>();
                    }
                }
                return instance;
            }
            private set => instance = value;
        }
        private static T instance;

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(this.gameObject);
        }
    }
}