using UnityEngine;
using UnityEngine.Events;

public interface ISpellEffect
{
    void Apply(Transform target, Vector3 hitPoint, float deltaTime);
}
