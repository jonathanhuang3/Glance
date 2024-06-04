using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using System.Linq;
using UnityEngine;

public class ParticleController : Stimulus
{
    // Particle system
    public ParticleSystem ps;
    public float amplitude = 1.25f;
    private ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime;
    private ParticleSystem.ShapeModule shapeModule;
    private ParticleSystem.EmissionModule emissionModule;

    // Velocity parameters
    private float _frequency = 2 * Mathf.PI / 15f;
    private List<int> coeff = new List<int> { 1, 3, 7, 11 };
    private List<float> dotsFrequencies = new List<float>();
    private float[] _phase = new float[4];
    private List<float> dotsPhase = new List<float>();
    // For unidirectional case to replicate mouse stimulus
    public bool unidirectional = false;
    public float defaultVelocity = 3f;

    // Direction parameters
    public enum Direction { Up, Diagonal, Horizontal, Down }
    public Direction movementDirection = Direction.Up;

    // Contrast parameters
    public enum Contrast { Low, High, Cycle }
    public Contrast contrast = Contrast.High;
    private List<Color> colors = new List<Color>()
    {
        new Color32(60,60,60,0), // Low contrast
        Color.white // High contrast
    };
    private List<Color> stepContrasts = new List<Color>()
    {
        // Steps will be filled in programmatically with GenerateStepContrast()
    };
    private List<StepData> steps = new List<StepData>(); // Start and Stop times, as well as color at given step
    private StepData step = new StepData(); // holds current step data
    private int contrastCurrent = 0;
    private float stepTimeAlive = 0f;
    private float rampTimeAlive = 0f;
    private float contrastTransitionSpeed = 3f;
    private float stepDuration;
    private bool stepDown = false;
    private Color gray;
    public bool cycleContrast = false; // When toggled, ignore contrast and cycle through Lerp from 0 to 1

    protected override void OnEnable()
    {
        base.OnEnable();
        if (ps != null)
        {
            emissionModule = ps.emission;
            shapeModule = ps.shape;
            velocityOverLifetime = ps.velocityOverLifetime;
            // Emission
            emissionModule.rateOverTime = 1000f;
            // Shape
            shapeModule.shapeType = ParticleSystemShapeType.Sphere;
            shapeModule.radius = 5f;
            shapeModule.randomPositionAmount = 10f;
            shapeModule.scale = new Vector3(1, 0, 1); // 2D in xz plane
            // Velocity
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        }
        dotsPhase.AddRange(Enumerable.Range(0, _phase.Length).Select(_ => UnityEngine.Random.Range(0f, 2 * Mathf.PI)));
        dotsFrequencies.AddRange(coeff.Select(c => c * _frequency));
    }

    protected override void Update()
    {
        base.Update();

        if (unidirectional) this.ScotomaCycleOcclusion(0, 9000, this.duration, 1f);
        float v = unidirectional ? defaultVelocity : VelocityNonHarmonic(Time.time, dotsFrequencies, dotsPhase);
        Vector3 velocity = Vector3.zero;

        switch (movementDirection)
        {
            case Direction.Up:
                velocity = new Vector3(0, 0, v); // z is up for the particle system, due to its initial rotation and local space
                break;
            case Direction.Diagonal:
                velocity = new Vector3(v, 0, v) * MathF.Sqrt(2) / 2f; // normalize based on 1-1-sqrt(2) triangle
                break;
            case Direction.Horizontal:
                velocity = new Vector3(v, 0, 0);
                break;
        }
        velocityOverLifetime.x = velocity.x;
        velocityOverLifetime.y = velocity.y; // In world space, y is upwards
        velocityOverLifetime.z = velocity.z;
    }

    private float VelocityNonHarmonic(float t, List<float> frequencies, List<float> phases)
    {
        float velocity = frequencies.Select((f, i) => Mathf.Sin(f * t - phases[i])).Sum();
        return amplitude * velocity;
    }

    private Color StepContrast()
    {
        stepTimeAlive += Time.deltaTime;

        if (stepTimeAlive <= stepDuration)
        {
            HandleStepStart();
            return stepContrasts[contrastCurrent];
        }
        else if (rampTimeAlive <= contrastTransitionSpeed)
        {
            HandleStepEnd();
            rampTimeAlive += Time.deltaTime;
            Color color = Color.Lerp(stepContrasts[contrastCurrent], stepContrasts[GetNextContrastIndex()], rampTimeAlive / contrastTransitionSpeed);
            return color;
        }
        else
        {
            PrintStepData();
            ResetStepData();
            contrastCurrent = GetNextContrastIndex();
            ToggleStepDirection();
            return stepContrasts[contrastCurrent];
        }
    }

    private void HandleStepStart()
    {
        if (step.frameStart == -1)
        {
            step.stepStart = Time.time;
            step.frameStart = Time.frameCount;
        }
    }

    private void HandleStepEnd()
    {
        if (step.frameEnd == -1)
        {
            step.stepEnd = Time.time;
            step.frameEnd = Time.frameCount - 1;
            step.stepColor = stepContrasts[contrastCurrent].ToString("F5").Substring(4);
            steps.Add(step);
        }
    }

    private void PrintStepData()
    {
        Debug.Log($"Upcoming Step: {stepContrasts[GetNextContrastIndex()].ToString("F9")} \nPrevious step start: {Time.time - stepTimeAlive} Previous step stop: {Time.time - rampTimeAlive} \nPrevious step duration: {stepTimeAlive - rampTimeAlive} Previous ramp duration: {rampTimeAlive}");
    }

    private void ResetStepData()
    {
        stepTimeAlive = 0f;
        rampTimeAlive = 0f;
        step = new StepData();
    }

    private int GetNextContrastIndex()
    {
        return stepDown ? (contrastCurrent - 1) % stepContrasts.Count : (contrastCurrent + 1) % stepContrasts.Count;
    }

    private void ToggleStepDirection()
    {
        if ((stepDown && contrastCurrent == 0) || (!stepDown && contrastCurrent == stepContrasts.Count - 1))
        {
            stepDown = !stepDown;
        }
    }

    public override void SaveTrackingData(string stimulusName)
    {
        try
        {
            DotsData data = new DotsData()
            {
                playerName = PlayerInfo.Instance.PlayerName,
                playerID = PlayerInfo.Instance.PlayerID,
                stimulusName = stimulusName,
                duration = this.duration,
                gazePositions = this.gazePositions.ToArray(),
                rotatedGaze = this.rotatedGaze.ToArray(),
                gazeRotations = this.gazeRotations.ToArray(),
                gazeTimes = this.gazeTimes.ToArray(),
                dotsFrequencies = this.dotsFrequencies.ToArray(),
                dotsPhase = this.dotsPhase.ToArray(),
                stepData = cycleContrast ? steps : null,
                fractionVisible = this.scotoma != Scotoma.None ? this.fractionVisible : null
            };

            base.SaveTrackingData(stimulusName);

            System.IO.File.WriteAllLines($"{this.storagePath}/dotsFrequency.txt", this.ListToString<float>(this.dotsFrequencies));
            System.IO.File.WriteAllLines($"{this.storagePath}/dotsPhase.txt", this.ListToString<float>(this.dotsPhase));
            if (cycleContrast)
            {
                System.IO.File.WriteAllLines($"{this.storagePath}/contrastSteps.txt", this.ListToString<Color>(stepContrasts));
            }

            string dataJson = JsonUtility.ToJson(data);
            System.IO.File.WriteAllText($"{this.storagePath}/{PlayerInfo.Instance.PlayerName}-{stimulusName}.json", dataJson);
        }
        catch (Exception e)
        {
            Debug.Log($"Failed to save data for {stimulusName}. Will rerun.");
            Debug.Log(e);
            OnFailedSave(stimulusName);
        }

    }
}
