using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class StepData
{
    public float stepStart = -1f;
    public float stepEnd = -1f;
    public int frameStart = -1;
    public int frameEnd = -1;
    public string stepColor; // Stripped of 'RGBA' from the beginning.
}
public class DotsSpawner : Stimulus
{
    public GameObject dotPrefab;
    public int totalDots = 300;
    public float spawnRadius = 7f;
    public float amplitude = 1.25f;
    [Range(0.01f, 0.3f)]
    public float flickerInterval = 0.1f;

    public enum Direction { Up, Diagonal, Right, Left, Down }
    public Direction movementDirection = Direction.Up;

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

    private List<GameObject> dotPool = new List<GameObject>();
    private List<float> dotTimes = new List<float>();
    private float _frequency = 2 * Mathf.PI / 15f;
    private List<float> dotsFrequencies = new List<float>();
    private List<float> dotsPhase = new List<float>();
    private int[] coeff = new int[] { 1, 3, 7, 11 };
    private float[] _phase = new float[4];

    protected override void OnEnable()
    {
        base.OnEnable();

        // Clear previous lists
        if (dotPool.Count >= totalDots)
        {
            dotPool.ForEach(dot => Destroy(dot));
            dotPool.Clear();
            dotTimes.Clear();
            dotsFrequencies.Clear();
            dotsPhase.Clear();
            // Contrast specific lists
            stepContrasts.Clear();
        }
        // Run checks
        RunChecks();

        // Set up contrasts
        // i.e. for 3 steps: *-*-*-*-*
        gray = GameObject.Find("Knob/Background").GetComponent<Renderer>().material.color;
        stepContrasts = GenerateStepContrast();
        Debug.Log($"Step contrasts: {stepContrasts.Count} \nand values: {string.Join(", ", stepContrasts.Select(c => c.ToString("F3")))}");
        stepDuration = (this.duration - contrastTransitionSpeed * 2 * (stepContrasts.Count - 1)) / (2 * stepContrasts.Count - 1); // calculating how long each step can be with a given transition speed.
        stepDuration = stepDuration - Time.deltaTime * 3f; // Shorten step duration by 3 frames as a buffer for saving the last contrast step.
        cycleContrast = contrast == Contrast.Cycle;

        dotsPhase.AddRange(Enumerable.Range(0, _phase.Length).Select(_ => UnityEngine.Random.Range(0f, 2 * Mathf.PI)));

        while (dotPool.Count < totalDots)
        {
            var region = RandomEllipse(spawnRadius);
            var position = transform.position + new Vector3(0, region.y, 0);
            var instance = Instantiate(dotPrefab, position, Quaternion.identity);
            instance.transform.SetParent(GameObject.Find("Dot Bag").transform, true);
            instance.transform.localScale = Vector3.one * 0.25f;
            instance.GetComponent<Renderer>().material.color = cycleContrast ? stepContrasts[0] : colors[(int)contrast];
            dotPool.Add(instance);
            dotTimes.Add(UnityEngine.Random.Range(0f, 0.025f));
        }

        if (!cycleContrast)
        {
            dotsFrequencies.AddRange(coeff.Select(c => c * _frequency));
        }
        else
        {
            // Speed up the period when cycling through contrasts
            dotsFrequencies.AddRange(coeff.Select(c => c * _frequency)); // This used to be faster (2pi/5)
        }

    }

    private List<Color> GenerateStepContrast()
    {
        List<Color> steps = new List<Color>();
        float center = gray[0];
        float numSteps = 11f;
        float x_incpt = -Mathf.Pow(center, 1f / 3f);
        float x_max = Mathf.Pow(center, 1f / 3f);
        float stepSize = (float)(x_max - x_incpt) / numSteps;

        steps.Add(Color.black); // Low contrast
        for (float x = x_incpt + stepSize; x <= x_max; x += stepSize)
        {
            float fx = Mathf.Pow(x, 3) + center;
            Color color = (Mathf.Abs(fx - center) <= 0.0004f) ? new Color(fx, fx, fx, 0f) : new Color(fx, fx, fx, 1f);
            steps.Add(color); //Intermediate contrasts
        }

        // steps.Add(Color.white); // High contrast

        return steps;
    }

    private void RunChecks()
    {
        // Check if contrast transition time is too slow for the given duration
        try
        {
            if (contrastTransitionSpeed * (stepContrasts.Count - 1) >= this.duration)
            {
                throw new Exception("Contrast transition time is too slow for the given duration.");

            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
            Debug.Log("Contrast transition time is too slow for the given duration.");
            this.EndStimulus();
        }
    }

    protected override void Update()
    {
        GameObject.Find("Dot Bag").transform.rotation = this.headingRotation;
        GameObject.Find("Knob").transform.rotation = this.headingRotation; //this.gazeUtility.HeadingRotation(transform.forward, transform.up);

        // For contrasts.
        Color currentColor = cycleContrast ? StepContrast() : colors[(int)contrast];
        dotPool.ForEach(dot => dot.GetComponent<Renderer>().material.color = cycleContrast ? currentColor : colors[(int)contrast]);
        // contrastRange.Add(currentColor); // No longer necessary to save the contrast at each frame.

        foreach (var dot in dotPool)
        {
            int index = dotPool.IndexOf(dot);
            dotTimes[index] += Time.deltaTime;

            if (dotTimes[index] >= flickerInterval)
            {
                dot.SetActive(!dot.activeSelf);
                var randomPosition = RandomEllipse(spawnRadius);
                dot.transform.position = this.headingRotation * randomPosition;

                dotTimes[index] = UnityEngine.Random.Range(0f, 0.025f);
            }
            else
            {
                float vx = VelocityNonHarmonic(Time.time, dotsFrequencies.ToArray(), dotsPhase.ToArray());
                Vector3 velocity = Vector3.zero;

                switch (movementDirection)
                {
                    case Direction.Up:
                        velocity = new Vector3(0, vx, 0);
                        break;
                    case Direction.Diagonal:
                        velocity = new Vector3(vx, vx, 0) * MathF.Sqrt(2) / 2f; // normalize based on 1-1-sqrt(2) triangle
                        break;
                    case Direction.Right:
                        velocity = new Vector3(vx, 0, 0);
                        break;
                }

                dot.GetComponent<Rigidbody>().velocity = this.headingRotation * velocity;
            }
        }
        base.Update(); // Added at end to avoid null reference when at the top.
    }

    private Vector3 RandomEllipse(float scale = 1f)
    {
        float minorAxis = 1f;
        float majorAxis = 1f;
        float t = 2 * Mathf.PI * UnityEngine.Random.value;
        float r = MathF.Sqrt(UnityEngine.Random.value) * scale;

        float x = minorAxis * r * Mathf.Cos(t);
        float y = majorAxis * r * Mathf.Sin(t);

        return new Vector3(x, y, 9f);
    }

    private float VelocityHarmonic(float t, float frequency, float phase)
    {
        return this.amplitude * Mathf.Sin(frequency * t + phase);
    }

    private float VelocityNonHarmonic(float t, float[] frequencies, float[] phases)
    {
        float velocity = 0f;
        for (int i = 0; i < frequencies.Length; i++)
        {
            velocity += Mathf.Sin(frequencies[i] * t - phases[i]);
        }
        return this.amplitude * velocity;
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