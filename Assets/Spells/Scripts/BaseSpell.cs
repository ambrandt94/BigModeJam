using UnityEngine;

[CreateAssetMenu(menuName = "Spells/BaseSpell")]
public abstract class BaseSpell : ScriptableObject, ISpell
{
    public string spellName;
    public float manaCost;
    public float cooldown;
    public GameObject effectPrefab;
    public bool effectPrefabRemainsChild;
    public bool hasDuration;
    public Sprite icon;
    public string description;
   
    public ScriptableObject[] spellEffects;

    public virtual void Cast(SpellCaster caster, Vector3 origin,Vector3 direction)
    {
        throw new System.NotImplementedException();
    }

    public virtual void CastingUpdate(SpellCaster caster)
    {
    }

}
