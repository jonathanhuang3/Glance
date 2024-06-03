using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConeScotoma : Scotoma
{
    private ParticleSystem ps;
    private ParticleSystem.ShapeModule shape;
    private ParticleSystem.MainModule main;

    protected override void OnEnable()
    {
        base.OnEnable();
        ps = GetComponent<ParticleSystem>();

        if (ps != null)
        {
            main = ps.main;
            shape = ps.shape;
            ParticleSystem.EmissionModule emissionModule = ps.emission;
            ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = ps.velocityOverLifetime;

            mainModule.startLifetime = 10000f;
            mainModule.startSpeed = 0f;
            mainModule.maxParticles = 3000;
            // Emission
            emissionModule.rateOverTime = 10000f;
            // Shape
            shapeModule.shapeType = ParticleSystemShapeType.Sphere;
            shapeModule.radius = this.stimulusType == MetaStimulus.OKRDriver.TumblingE ? 5f : 15f;
            shapeModule.randomPositionAmount = 10f;
            shapeModule.scale = new Vector3(1, 0, 1); // 2D in xz plane
            // Velocity
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
        }
    }

    protected override void ModulateOcclusion(bool correct)
    {
        shape.randomPositionAmount += correct ? -0.03f : 0.05f;
        ps.Clear(); // Re-emit particles with new spacing
        ps.Play();
    }

    protected override void CycleOcclusion(int minParticles, int maxParticles, float timeframe, float pauseDuration)
    {
        StartCoroutine(CycleOcclusion(minParticles, maxParticles, timeframe, pauseDuration));
    }

    IEnumerator CycleOcclusion(int minParticles, int maxParticles, float timeframe, float pauseDuration)
    {
        while (true)
        {
            // Increase from min to max
            for (float t = 0; t <= 1; t += Time.deltaTime / timeframe)
            {
                main.maxParticles = Mathf.Lerp(minParticles, maxParticles, t);
                ps.Clear(); // Re-emit particles with new spacing
                ps.Play();
                yield return null;
            }

            // Wait at max
            yield return new WaitForSeconds(pauseDuration);

            // Decrease from max to min
            for (float t = 1; t >= 0; t -= Time.deltaTime / timeframe)
            {
                main.maxParticles = Mathf.Lerp(minParticles, maxParticles, t);
                ps.Clear(); // Re-emit particles with new spacing
                ps.Play();
                yield return null;
            }

            // Wait at min
            yield return new WaitForSeconds(pauseDuration);
        }
    }
}