using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    private Animator Animator {
        get {
            if (_animator == null)
                _animator = GetComponentInChildren<Animator>();
            return _animator;
        }
    }

    public bool IsWalking {
        get => walking; set {
            if (walking != value) {
                walking = value;
                SetWalkingState();
            }
        }
    }

    [SerializeField]
    private string walkingParam = "Walking";
    [SerializeField, OnValueChanged(nameof(SetWalkingState))]
    private bool walking = false;

    [SerializeField]
    private string horizontalParam = "Horizontal";
    [SerializeField, Range(-1, 1), OnValueChanged(nameof(SetMoveDirection))]
    private float horizontal = 0;
    [SerializeField]
    private string forwardParam = "Forward";
    [SerializeField, Range(-1, 1), OnValueChanged(nameof(SetMoveDirection))]
    private float forward = 0;

    [SerializeField]
    private List<AnimBool> Bools;
    [SerializeField]
    private List<AnimTrigger> Triggers;

    private Animator _animator;

    public void SetBool(string id, bool value)
    {
        if (Bools == null)
            return;
        foreach (var anim in Bools) {
            if (anim.Id == id)
                anim.Active = value;
        }
    }

    public void SetXYInput(float h, float f)
    {
        horizontal = h;
        forward = f;
        SetMoveDirection();
        walking = horizontal != 0 || forward != 0;
        SetWalkingState();
    }

    private void SetMoveDirection()
    {
        if (!Application.isPlaying)
            return;
        Animator.SetFloat(horizontalParam, horizontal);
        Animator.SetFloat(forwardParam, forward);
    }

    private void SetWalkingState()
    {
        if (!Application.isPlaying)
            return;
        Animator.SetBool(walkingParam, walking);
    }

    private void CheckTriggers()
    {
        if (Triggers == null)
            return;
        foreach (var trigger in Triggers) {
            if (trigger.Active) {
                Animator.SetTrigger(trigger.Id);
                trigger.Active = false;
            }
        }
    }

    private void CheckBools()
    {
        if (Triggers == null)
            return;
        foreach (var b in Bools) {
            Animator.SetBool(b.Id, b.Active);
        }
    }

    private void Update()
    {
        CheckTriggers();
        CheckBools();
    }
}

[System.Serializable]
public class AnimTrigger
{
    public string Id;
    public bool Active;
}

[System.Serializable]
public class AnimBool
{
    public string Id;
    public bool Active;
}
