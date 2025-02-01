using System.Collections.Generic;
using UnityEngine;

namespace ChainLink.Core
{
    public abstract class AssetPool<T> : ScriptableObject
    {
        public string PoolName;
        public List<AssetPoolGroup<T>> Groups;

        public List<T> GetAllAssets()
        {
            if (Groups == null)
                return new List<T>();
            List<T> assets = new List<T>();
            foreach (var group in Groups) {
                foreach (var item in group.Assets) {
                    if (!assets.Contains(item))
                        assets.Add(item);
                }
            }
            return assets;
        }

        public T GetRandom(string group = "")
        {
            if (!string.IsNullOrEmpty(group)) {
                AssetPoolGroup<T> selectedGroup = GetGroup(group);
                if (selectedGroup != null) {
                    return ChainUtils.GetRandom<T>(selectedGroup.Assets);
                }
            }
            return ChainUtils.GetRandom<T>(GetAllAssets());
        }

        public AssetPoolGroup<T> GetGroup(string name)
        {
            if (Groups != null) {
                foreach (AssetPoolGroup<T> group in Groups) {
                    if (group.GroupName == name)
                        return group;
                }
            }
            return null;
        }

        [System.Serializable]
        public class AssetPoolGroup<T>
        {
            public List<T> Assets {
                get {
                    if (assets == null)
                        assets = new List<T>();
                    return assets;
                }
            }
            public string GroupName;

            [SerializeField]
            private List<T> assets;
        }
    }
}