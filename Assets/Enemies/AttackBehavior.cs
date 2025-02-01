using DestroyIt;
using UnityEngine;

public class AttackBehavior : MonoBehaviour
{
    [SerializeField] private SpellCaster spellCaster;
    [SerializeField] private float attackRange = 15f;
    [SerializeField] private float attackCooldown = 2f;

    private float lastAttackTime;
    private Transform player;
    private Destructible currentDestructibleTarget;

    private void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
    }

    public bool CanAttack(Transform target)
    {
        if (spellCaster == null || target == null) return false;
        if (!spellCaster.HasSpells()) return false;
        if (Vector3.Distance(transform.position, target.position) > attackRange) return false;
        if (Time.time < lastAttackTime + attackCooldown) return false;

        return true;
    }

    public void Attack(Transform target)
    {
        if (!CanAttack(target)) return;

        Vector3 attackDirection = (target.position - transform.position).normalized;
        int ranSpellIndex = Random.Range(0, spellCaster.spells.Length);
        spellCaster.Cast(ranSpellIndex, spellCaster.spellOrigin.position, attackDirection);
        lastAttackTime = Time.time;

        if (target.gameObject.GetComponent<Destructible>() != null) 
        {
            Destructible destructible = target.GetComponent<Destructible>();
            destructible?.ApplyDamage(Random.Range(10f, 25f));
        }
    }

    public void SetDestructibleTarget(Destructible target)
    {
        currentDestructibleTarget = target;
    }
}
