using UnityEngine;

[CreateAssetMenu(menuName = "SpellEffects/SpeedEffect")]
public class SpeedEffect : ScriptableObject, ISpellEffect
{
    public float speedChange; 

    private MoveBehaviour moveBehaviour;
    private FlyBehaviour flyBehaviour;

    public void Apply(Transform target, Vector3 hitPoint, float deltaTime)
    {

        if(moveBehaviour == null)
        {      
            if (target.gameObject.GetComponent<MoveBehaviour>() == null)
            return;
            
            moveBehaviour = target.gameObject.GetComponent<MoveBehaviour>();
        }

       
        moveBehaviour.walkSpeed  = moveBehaviour.walkSpeed * speedChange;
        moveBehaviour.runSpeed = moveBehaviour.runSpeed * speedChange;
        moveBehaviour.sprintSpeed = moveBehaviour.sprintSpeed * speedChange;

        if (flyBehaviour == null) {
            if (target.gameObject.GetComponent<FlyBehaviour>() == null)
                return;

            flyBehaviour = target.gameObject.GetComponent<FlyBehaviour>();
        }
            flyBehaviour.flySpeed = flyBehaviour.flySpeed * speedChange;

        
      
    }
}
