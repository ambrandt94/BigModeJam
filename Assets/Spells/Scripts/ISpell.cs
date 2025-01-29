
using UnityEngine;

public interface ISpell
{
    void Cast(SpellCaster caster, Vector3 origin, Vector3 direction);
}
