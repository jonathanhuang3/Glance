using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConeScotoma : Scotoma
{
    private ParticleSystem ps;
    private ParticleSystem.ShapeModule shape;
    private ParticleSystem.MainModule main;
    private ParticleSystem.EmissionModule emission;

    private List<int> occlusionSteps = new List<int>();

    protected override void OnEnable()
    {
        base.OnEnable();
        ps = GetComponent<ParticleSystem>();

        if (ps != null)
        {
            SetupParticleSystem();
        }
    }

    protected void SetupParticleSystem(float duration = 5f, float lifetime = 10000f)
    {
        // start size 0.3f
        // For optotype, can get away with 0 - 1000 particles with radius of 1 and 0 dispersion
        // For Dots, will need to increase to 100,000 particles with radius of 5 and 5 dispersion
        // For Dots, the scotoma particle numbers could be reduced if the dispersion for the stimulus particles was set to 0
        ps.Stop();

        main = ps.main;
        shape = ps.shape;
        emission = ps.emission;
        ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = ps.velocityOverLifetime;

        main.duration = duration;
        main.startLifetime = lifetime;
        main.startSize = 0.3f;
        main.startSpeed = 0f;
        main.maxParticles = this.driver == MetaStimulus.OKRDriver.TumblingE ? 0 : 100000; // Tumbling optotype will update and redraw to increase particles. Other stimuli will rely on changing emission rate, so will start at max
                                                                                          // Emission
        emission.rateOverTime = 1000000f;
        // Shape
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = this.driver == MetaStimulus.OKRDriver.TumblingE ? 1f : 5f;
        shape.randomPositionAmount = this.driver == MetaStimulus.OKRDriver.TumblingE ? 0f : 5f;
        shape.scale = new Vector3(1, 0, 1); // 2D in xz plane
                                            // Velocity
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.World;

        ps.Play();
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

        float rampTime = (timeframe - (3f * pauseDuration)) / 2;
        float rate = (maxParticles - minParticles) / rampTime;
        float rateBoost = (maxParticles - (Mathf.Sqrt(rate) * (rampTime))) / (rampTime - Mathf.Sqrt(rampTime));
        float normalizedRampSqrt = Mathf.Sqrt(rampTime) / timeframe;
        float normalizedRampBoost = (rampTime - Mathf.Sqrt(rampTime)) / timeframe;
        // float normalizedRamp = rampTime / timeframe;
        float normalizedPause = pauseDuration / timeframe;

        float time1 = 0;
        float time2 = time1 + normalizedPause;
        // float time3 = time2 + (normalizedRamp); // Ping Pong requires that the curve is symmetric about the midpoint
        float time3 = time2 + normalizedRampSqrt;
        float time4 = time3 + normalizedRampBoost;


        SetupParticleSystem(timeframe, rampTime); // Set scale so that normalized time in animation curve is fraction of duration

        Keyframe[] ks = new Keyframe[4];
        ks[0] = new Keyframe(time1, 0f);
        ks[0].outTangent = 0f;
        ks[1] = new Keyframe(time2, 0f);
        ks[1].inTangent = 0f;
        ks[2] = new Keyframe(time2, 0f);
        ks[2].outTangent = 0f;
        ks[3] = new Keyframe(time3, 1f);
        ks[3].inTangent = 1f;

        AnimationCurve curve = new AnimationCurve(ks);
        curve.preWrapMode = WrapMode.Loop;
        curve.postWrapMode = WrapMode.Loop;

        emission.rateOverTime = new ParticleSystem.MinMaxCurve(Mathf.Sqrt(rate), curve);
        StartCoroutine(InvertCurve(curve, pauseDuration + rampTime, rate)); // End of 1.5 loops (first pause, ramp, second pause) at maximum particles, stop emission to allow particles to die out

    }

    IEnumerator InvertCurve(AnimationCurve curve, float delay, float scale)
    {
        yield return new WaitForSeconds(delay);
        ps.Stop(); // Let particles die out
    }
}