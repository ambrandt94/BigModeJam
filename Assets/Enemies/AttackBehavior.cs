using UnityEngine;

public class AttackBehavior : MonoBehaviour
{
    [SerializeField] private SpellCaster spellCaster; // Reference the SpellCaster component
    [SerializeField] private float attackRange = 15f;
    [SerializeField] private float attackCooldown = 2f;

    private float lastAttackTime;
    private Transform player;

    private void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
    }

    public bool CanAttack()
    {
        if (spellCaster == null || player == null) return false;
        if (!spellCaster.HasSpells()) return false;
        if (Vector3.Distance(transform.position, player.position) > attackRange) return false;
        if (Time.time < lastAttackTime + attackCooldown) return false;

        return true;
    }

    public void Attack()
    {
        if (!CanAttack()) return;

        Vector3 attackDirection = (player.position - transform.position).normalized;
        int ranSpellIndex = Random.Range(0, spellCaster.spells.Length);
        spellCaster.Cast(ranSpellIndex, spellCaster.spellOrigin.position, attackDirection); // Use a random attack
        lastAttackTime = Time.time;
    }
}
