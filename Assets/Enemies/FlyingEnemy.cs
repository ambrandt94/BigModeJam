using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using DestroyIt;

public class FlyingEnemy : MonoBehaviour
{
    public SmoothPatrolBehavior patrolBehavior;
    public ChaseBehavior chaseBehavior;
    public AttackBehavior attackBehavior;
    public DetectionSystem detectionSystem;
    public TakeoffBehavior takeoffBehavior;  

    public enum State { Patrolling, Chasing, Attacking, Destroying, TakingOff }
    public State currentState = State.Patrolling;

    private Transform playerTarget;
    private Destructible destructibleTarget;

    public event System.Action<FlyingEnemy> OnDestroyed;

    private void OnDestroy()
    {
        OnDestroyed?.Invoke(this);
    }

    private void Start()
    {
        if (!takeoffBehavior.CheckTakeoff(this))
        {
            SwitchState(State.TakingOff);
        }
        else
        {
            SwitchState(State.Patrolling);
        }
    }

    private void Update()
    {
        switch (currentState)
        {
            case State.Patrolling:
                if (FindPlayerTarget()) SwitchState(State.Chasing);
                else if (FindDestructibleTarget()) SwitchState(State.Destroying);
                break;

            case State.Chasing:
                chaseBehavior.Chase();
                if (attackBehavior.CanAttack(playerTarget)) SwitchState(State.Attacking);
                else if (!detectionSystem.CanSeePlayer()) SwitchState(State.Patrolling);
                break;

            case State.Attacking:
                attackBehavior.Attack(playerTarget);
                if (!attackBehavior.CanAttack(playerTarget)) SwitchState(State.Chasing);
                break;

            case State.Destroying:
                MoveToTarget(destructibleTarget.transform.position);
                if (destructibleTarget == null || destructibleTarget.IsDestroyed)
                {
                    SwitchState(State.Patrolling);
                }
                else if (attackBehavior.CanAttack(destructibleTarget.transform))
                {
                    attackBehavior.Attack(destructibleTarget.transform);
                }
                break;

            case State.TakingOff:
                takeoffBehavior.PerformTakeoff();
                if (takeoffBehavior.HasTakenOff) SwitchState(State.Patrolling);
                break;
        }
    }

    private void SwitchState(State newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        if (newState == State.Patrolling)
        {
            patrolBehavior.StartPatrolling();
        }
        else
        {
            patrolBehavior.StopPatrolling();
        }
    }

    private bool FindPlayerTarget()
    {
        if (detectionSystem.CanSeePlayer())
        {
            playerTarget = GameObject.FindGameObjectWithTag("Player")?.transform;
            return playerTarget != null;
        }
        return false;
    }

    private bool FindDestructibleTarget()
    {
        if (Destructible.AllTrackedDestructibles.Count == 0) return false;

        List<Destructible> validTargets = Destructible.AllTrackedDestructibles
            .Where(d => !d.IsDestroyed && d.gameObject.activeInHierarchy)
            .ToList();

        if (validTargets.Count > 0)
        {
            destructibleTarget = validTargets.OrderBy(d => Vector3.Distance(transform.position, d.transform.position)).First();
            return true;
        }

        return false;
    }

    private void MoveToTarget(Vector3 targetPosition)
    {
        float distance = Vector3.Distance(targetPosition, transform.position);
        if(distance <= chaseBehavior.minDistanceToPlayer)
        {
            return;
        }

        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, chaseBehavior.chaseSpeed * Time.deltaTime);


        if (direction != Vector3.zero) // Prevent errors when reaching target
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, chaseBehavior.rotationSpeed * Time.deltaTime);
        }
    }
}
