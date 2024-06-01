using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConeScotoma : Scotoma
{
    private ParticleSystem ps;
    private ParticleSystem.ShapeModule shape;

    protected override void OnEnable()
    {
        base.OnEnable();
        ps = GetComponent<ParticleSystem>();
        shape = ps.shape;

        if (ps != null)
        {
            ParticleSystem.EmissionModule emissionModule = ps.emission;
            ParticleSystem.ShapeModule shapeModule = ps.shape;
            ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = ps.velocityOverLifetime;

            ps.startSpeed = 0f;
            ps.startLifetime = 10000f;
            ps.maxParticles = 3000;
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
}