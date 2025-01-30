using UnityEngine;

[CreateAssetMenu(menuName = "Spells/BeamShape")]
public class BeamShape : BaseSpell
{
    public float range = 50f; // Max beam range

    public override void Cast(SpellCaster caster, Vector3 origin, Vector3 direction)
    {
        // Get the main camera
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("BeamShape: No Main Camera found!");
            return;
        }

        // Fire a ray from the camera to the center of the screen
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            foreach (var spellEffect in spellEffects)
            {
                if (spellEffect is ISpellEffect effect)
                {
                    effect.Apply(hit.transform, hit.point, Time.deltaTime);
                }
            }

            // Spawn the beam visual effect
            if (effectPrefab)
            {
                var beam = Instantiate(effectPrefab, caster.spellOrigin.position, Quaternion.identity);
                beam.transform.LookAt(hit.point);
                Destroy(beam, 0.1f);
            }
        }
        else
        {
            Debug.Log("BeamShape: No hit detected.");
        }
    }
}
