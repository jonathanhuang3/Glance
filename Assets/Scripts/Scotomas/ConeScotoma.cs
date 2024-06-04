using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConeScotoma : Scotoma
{
    private ParticleSystem ps;
    private ParticleSystem.ShapeModule shape;
    private ParticleSystem.MainModule main;

    private List<int> occlusionSteps = new List<int>();

    protected override void OnEnable()
    {
        base.OnEnable();
        ps = GetComponent<ParticleSystem>();

        if (ps != null)
        {
            // For optotype, can get away with 3000 - 7000 particles with radius of 0.5 and 0 dispersion
            // For Dots, will need to increase to 10000 particles with radius of 1.1 and 0 dispersion
            main = ps.main;
            shape = ps.shape;
            ParticleSystem.EmissionModule emissionModule = ps.emission;
            ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = ps.velocityOverLifetime;

            main.startLifetime = 10000f;
            main.startSpeed = 0f;
            main.maxParticles = 3000;
            // Emission
            emissionModule.rateOverTime = 100000f;
            // Shape
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;
            shape.randomPositionAmount = 0f;
            shape.scale = new Vector3(1, 0, 1); // 2D in xz plane
            // Velocity
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
        }
    }

    protected override void ModulateOcclusion(bool correct, float occlusionAmount)
    {
        // shape.randomPositionAmount += correct ? -0.03f : 0.05f;
        // main.maxParticles += correct ? 300 : -200;
        main.maxParticles = (int)occlusionAmount;
        ps.Clear(); // Re-emit particles with new spacing
        ps.Play();
    }

    protected override void CycleOcclusion(int minParticles, int maxParticles, float timeframe, float pauseDuration)
    {
        StartCoroutine(CyclicOcclusion(minParticles, maxParticles, timeframe, pauseDuration));
    }

    IEnumerator CyclicOcclusion(int minParticles, int maxParticles, float timeframe, float pauseDuration)
    {
        float rampTime = (timeframe / 2) - 3 * pauseDuration;

        // Initial wait before starting
        yield return new WaitForSeconds(pauseDuration);
        while (true)
        {
            // Increase from min to max
            for (float t = 0; t <= 1; t += Time.deltaTime / rampTime)
            {
                main.maxParticles = (int)Mathf.Lerp(minParticles, maxParticles, t);
                ps.Clear(); // Re-emit particles with new spacing
                ps.Play();
                yield return null;
            }

            // Wait at max
            yield return new WaitForSeconds(pauseDuration);

            // Decrease from max to min
            for (float t = 1; t >= 0; t -= Time.deltaTime / rampTime)
            {
                main.maxParticles = (int)Mathf.Lerp(minParticles, maxParticles, t);
                ps.Clear(); // Re-emit particles with new spacing
                ps.Play();
                yield return null;
            }

            // Wait at min
            yield return new WaitForSeconds(pauseDuration);
        }
    }
}