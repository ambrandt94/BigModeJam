using ChainLink.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BigModeJam
{
    public class  NPCManager : Singleton<NPCManager> {
        [SerializeField,ReadOnly]
        private GameObject[] characterTravelPoints;

        [SerializeField]
        private GenericAssetPool npcCharacterPool;

        public GameObject GetCharacterPrefab(string group = "")
        {
            return npcCharacterPool.GetRandom(group) as GameObject;
        }

        public void AllRandom()
        {
            foreach (CharacterPather pather in FindObjectsOfType<CharacterPather>()) {
                pather.SearchForRandomTravelPoint();
            }
        }

        public void FindCharacterTravelPoints()
        {
            characterTravelPoints = GameObject.FindGameObjectsWithTag("TravelPoint");
        }

        public Transform GetRandomTravelPoint()
        {
            GameObject obj = ChainUtils.GetRandom(characterTravelPoints);
            if(obj == null)
                return null;
            return obj.transform;
        }
    }
}