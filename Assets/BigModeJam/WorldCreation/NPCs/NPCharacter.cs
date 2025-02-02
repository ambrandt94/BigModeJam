using BigModeJam;
using UnityEngine;

public class NPCharacter : MonoBehaviour
{
    private CharacterPather pather;

    private void Start()
    {
        pather = GetComponent<CharacterPather>();
        GameObject obj = Instantiate(NPCManager.Instance.GetCharacterPrefab("modern"),transform);
        obj.transform.localPosition= Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        pather.Initialize();
    }
}
