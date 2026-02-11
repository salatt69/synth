using RoR2;
using ProjectSynth.Hologram;
using RoR2.Projectile;
using UnityEngine;

public class DivaLifetime : MonoBehaviour
{
    private ProjectileSimple simple;
    private ProjectileStickOnImpactByNormal stick;
    private bool extended;

    public float flyingLifetime;
    public float stuckLifetime;

    private void Awake()
    {
        simple = GetComponent<ProjectileSimple>();
        stick = GetComponent<ProjectileStickOnImpactByNormal>();
    }

    private void OnEnable()
    {
        extended = false;
        simple.lifetime = flyingLifetime;
        simple.stopwatch = 0f;
        Chat.AddMessage("flying time in use.");
    }

    private void FixedUpdate()
    {
        if (!extended && stick && stick.stuck)
        {
            Chat.AddMessage("stuck time in use.");
            extended = true;
            simple.lifetime = stuckLifetime;
            simple.stopwatch = 0f;
        }
    }
}
