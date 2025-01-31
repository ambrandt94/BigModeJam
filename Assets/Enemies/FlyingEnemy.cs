using System;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class FlyingEnemy : MonoBehaviour
{
    public SmoothPatrolBehavior patrolBehavior;
    public ChaseBehavior chaseBehavior;
    public AttackBehavior attackBehavior;
    public DetectionSystem detectionSystem;
    public TakeoffBehavior takeoffBehavior;

    public enum State { Patrolling, Chasing, Attacking, TakingOff }
    public State currentState = State.Patrolling;

    public event Action<FlyingEnemy> OnDestroyed;

    private void OnDestroy()
    {
        OnDestroyed?.Invoke(this); // Send this enemy object in the event
    }

    private void Start()
    {
        if (!takeoffBehavior.CheckTakeoff(this))
        {
            SwitchState(State.TakingOff);
        }
        else
        {
            SwitchState(State.Patrolling); // ✅ This ensures patrol starts properly
        }
    }

    private void Update()
    {
        switch (currentState)
        {
            case State.Patrolling:
                if (detectionSystem.CanSeePlayer())
                {
                    SwitchState(State.Chasing);
                }
                break;

            case State.Chasing:
                chaseBehavior.Chase();
                if (attackBehavior.CanAttack())
                {
                    SwitchState(State.Attacking);
                }
                else if (!detectionSystem.CanSeePlayer())
                {
                    SwitchState(State.Patrolling);
                }
                break;

            case State.Attacking:
                attackBehavior.Attack();
                if (!attackBehavior.CanAttack())
                {
                    SwitchState(State.Chasing);
                }
                break;

            case State.TakingOff:
                takeoffBehavior.PerformTakeoff();
                if (takeoffBehavior.HasTakenOff)
                {
                    SwitchState(State.Patrolling);
                }
                break;
        }
    }

    private void SwitchState(State newState)
    {
        if (currentState == newState) return; // ✅ Prevent unnecessary state re-entries

        currentState = newState;

        // ✅ Ensure `Patrol()` is only called when actually entering Patrolling state
        if (newState == State.Patrolling)
        {
            patrolBehavior.StartPatrolling();
        }
        else if (newState == State.Chasing)
        {
            patrolBehavior.StopPatrolling(); // ✅ Stop patrol movement before chasing
        }
    }
}
