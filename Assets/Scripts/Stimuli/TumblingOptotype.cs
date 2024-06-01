using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

[System.Serializable]
public class OptotypeData : Data
{
    // Consider saving all textures used.
    public List<UserResponses> userResponses;
}

[System.Serializable]
public class UserResponses
{
    public enum Orientation { Right, Down, Left, Up };
    public Orientation orientation;
    public string letter = "E";
    public float dispersionAmount;
    public float[] fractionVisible = new float[15];
    public bool response;
    public float timeOfResponse; // Time user responded
    public float deliberationTime;
    public float timeOfRotation; // Time new rotation was shown
}

public static class ArrayHelper
{
    public static float[,] Calculate(this float[,] a, float[,] b, Func<float, float, float> operation)
    {
        float[,] result = new float[a.GetLength(0), a.GetLength(1)];
        for (int i = 0; i < a.GetLength(0); i++)
        {
            for (int j = 0; j < a.GetLength(1); j++)
            {
                result[i, j] = operation(a[i, j], b[i, j]);
            }
        }
        return result;
    }

    public static float[,] ReplaceRow(this float[,] a, int row, float[] newRow)
    {
        for (int i = 0; i < a.GetLength(1); i++)
        {
            a[row, i] = newRow[i];
        }
        return a;
    }

    public static IEnumerable<T> GetColumn<T>(this T[,] a, int column)
    {
        for (int i = 0; i < a.GetLength(0); i++)
        {
            yield return a[i, column];
        }
    }

    public static IEnumerable<T> GetRow<T>(this T[,] a, int row)
    {
        for (int i = 0; i < a.GetLength(1); i++)
        {
            yield return a[row, i];
        }
    }
}

public class TumblingOptotype : Stimulus
{
    // Use same prefab as waiting room, changing text on cavas in font style optician sans. 
    // Letters to use are CDEFHKNPRUVZ
    public GameObject handle;
    public TextMeshProUGUI optotypeText;
    public float tiling = 1.0f;
    public int repetitionLimit;
    private int repetitionCount;

    public GameObject audioObject;
    private AudioSource audioData;

    public ParticleSystem ps; // particle system from scotoma
    private ParticleSystem.ShapeModule shape;
    // MetaStimulus for this stimulus stores scotoma information, MetaStimulus.Scotoma.Grid, which is currently not being used.
    private enum ScotomaGrid { S0, S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12, S13, S14, S15, NumStates }; // Corresponds to indices in masks. Finer modulation of noise occurs through `tiling`
    private ScotomaGrid state = ScotomaGrid.S0; // 1.9 - (state) * 0.1 = random dispersion of particles
    private float minParticleDispersion = 0.4f;
    private float maxParticleDispersion = 1.9f;
    // Least to most difficult
    private int numStates = 16;
    private int numGroups = 4;
    private List<float> particleDispersions = new List<float>();
    private List<List<float>> particleDispersionGroups = new List<List<float>>();
    // Move through noise states using BinSort type insertion algorithm, by lumping together groups of states
    // random dispersion from 1.9 to 0.4 in particle system. Step size of 0.1 for 4 groups of 4 states
    // Implement option for either optimized threshold calculation or sequential threshold calculation
    private int groupsIndex = 2; // Start at the second group
    private int intraGroupIndex = 0; // Start at the first state in the group
    private enum ResponseQuality { Miss, FalsePositive, Correct, FalseNegative }; // A false positive is where the user guessed correctly by chance. A false negative would only make sense if the user knew the answer but pressed the wrong button
    private List<bool> responses = new List<bool>();
    private bool correct;
    private bool justAnswered;
    private float timeOfRotation; // Time new rotation is shown
    private float timeOfResponse; // Time user responded
    private float deliberationTime;
    private enum Optotype { Right, Up, Left, Down };
    private float[] angle = { 0f, 90f, 180f, 270f };
    private Optotype direction = Optotype.Right;
    private List<UserResponses> userResponses = new List<UserResponses>();

    protected override void OnEnable()
    {
        base.OnEnable();
        audioData = audioObject.GetComponent<AudioSource>();
        repetitionCount = 0;
        userResponses.Clear();

        if (particleDispersions.Count == 0)
        {
            shape = ps.shape;
            particleDispersions.AddRange(Enumerable.Range(0, numStates).Select(i => maxParticleDispersion - i * (maxParticleDispersion - minParticleDispersion) / (numStates - 1)));
            particleDispersionGroups.AddRange(Enumerable.Range(0, numGroups).Select(i => particleDispersions.Skip(i * numGroups).Take(numGroups).ToList()));
        }
    }
    protected override void Update()
    {
        base.Update();
        // Keep stimulus in front of user
        Quaternion shiftRotation = this.headingRotation;
        handle.transform.rotation = shiftRotation;

        optotypeText.color = Color.Lerp(optotypeText.color, new Color(optotypeText.color.r, optotypeText.color.g, optotypeText.color.b, 1f), (Time.time - timeOfResponse) / 0.5f);
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            correct = CheckDirection();
            responses.Add(correct);
            deliberationTime = Time.time - timeOfResponse; // timeOfResponse is set in CheckDirection()
            justAnswered = true;
        }
        // Split this up into two blocks, one for checking input and one for updating the stimulus only when the user has answered
        if (justAnswered)
        {
            string answer = correct ? "Correct" : "Incorrect";
            Debug.Log($"{answer} - Intended Rotation {direction.ToString()} - Optotype Rotation {optotypeText.transform.rotation.eulerAngles.z}");

            // Choose groupIndex based on correctness. A metric of correctness could take into account `correct`, time taken to answer, and known difiiulty of the current group
            // state = groups[groupsIndex][Random.Range(0, groups[groupsIndex].Length)];
            // float dispersion = CalculateDispersion(correct, deliberationTime, groupsIndex, intraGroupIndex);
            repetitionCount++;
            this.ScotomaModulateOcclusion(correct);
            optotypeText.transform.rotation = Quaternion.Lerp(optotypeText.transform.rotation, Quaternion.Euler(0, 0, RandomOptotypeRotation()), 1f);

            optotypeText.color = new Color(optotypeText.color.r, optotypeText.color.g, optotypeText.color.b, 0f); // Change alpha to hide optotype while particles re-emit, to avoid giving away optotype rotation
            userResponses.Add(
                new UserResponses()
                {
                    orientation = (UserResponses.Orientation)direction,
                    letter = optotypeText.text,
                    dispersionAmount = shape.randomPositionAmount,
                    fractionVisible = this.fractionVisible.TakeLast(15).ToArray(),
                    response = correct,
                    timeOfResponse = timeOfResponse,
                    deliberationTime = deliberationTime,
                    timeOfRotation = timeOfRotation
                });
            audioData.Play();
            justAnswered = false;
        }
    }

    private bool CheckDirection()
    {

        // Change key bindings from arrow keys to YGHJ, or some other key set that is not bound
        timeOfResponse = Time.time;
        if (Input.GetKeyDown(KeyCode.J))
        {
            return direction == Optotype.Right;
        }
        else if (Input.GetKeyDown(KeyCode.H))
        {
            return direction == Optotype.Down;
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            return direction == Optotype.Left;
        }
        else if (Input.GetKeyDown(KeyCode.Y))
        {
            return direction == Optotype.Up;
        }
        return false;
    }

    private float RandomOptotypeRotation()
    {
        int rotation = UnityEngine.Random.Range(0, angle.Length);
        direction = (Optotype)rotation;
        return angle[rotation];
    }


    public int RandomFromDistribution(float[] values)
    {
        float r = UnityEngine.Random.value;
        float cdf = 0;
        for (int i = 0; i < values.Length; i++)
        {
            cdf += values[i];
            if (r <= cdf)
            {
                return i;
            }
        }
        return values.Length - 1;
    }

    float CalculateDispersion(bool correct, float deliberationTime, int groupsIndex, int intraGroupIndex)
    {
        switch (MeasureResponseQuality(correct, deliberationTime, groupsIndex, intraGroupIndex))
        {
            case ResponseQuality.Miss:
                if (intraGroupIndex <= (numStates / numGroups) / 2)
                {
                    if (intraGroupIndex == 0) groupsIndex = Mathf.Clamp(groupsIndex - 1, 0, numGroups - 1); // Change group only when on the first state in the group when a convervative step is not required
                    intraGroupIndex = (intraGroupIndex - 1) % (numStates / numGroups);
                    break;
                }
                intraGroupIndex = (intraGroupIndex - 1) % (numStates / numGroups);
                break;
            case ResponseQuality.FalsePositive:
                // if (intraGroupIndex <= (numStates / numGroups) / 2)
                // {
                //     // Try state again

                //     break;
                // }
                intraGroupIndex += 1;
                if (intraGroupIndex >= numStates / numGroups)
                {
                    groupsIndex = Mathf.Clamp(groupsIndex + 1, 0, numGroups - 1);
                    intraGroupIndex = intraGroupIndex % (numStates / numGroups);
                }
                break;
            case ResponseQuality.Correct:
                intraGroupIndex += 2;
                if (intraGroupIndex >= numStates / numGroups)
                {
                    groupsIndex = Mathf.Clamp(groupsIndex + 1, 0, numGroups - 1);
                    intraGroupIndex = intraGroupIndex % (numStates / numGroups);
                }
                break;
        }
        float dispersion = particleDispersionGroups[groupsIndex][intraGroupIndex];
        return dispersion;
    }

    private ResponseQuality MeasureResponseQuality(bool correct, float deliberationTime, int groupsIndex, int intraGroupIndex)
    {
        if (!correct)
        {
            return ResponseQuality.Miss;
        }

        if (groupsIndex <= numGroups / 2)
        {
            return ResponseQuality.Correct;
        }

        if (groupsIndex < numGroups - 1)
        {
            if (intraGroupIndex <= (numStates / numGroups) / 2 || deliberationTime < 5f)
            {
                return ResponseQuality.Correct;
            }
            return ResponseQuality.FalsePositive;
        }

        if (this.fractionVisible.LastOrDefault() != 0)
        {
            if (intraGroupIndex == (numStates / numGroups) / 2 || deliberationTime < 7f)
            {
                return ResponseQuality.Correct;
            }
            return ResponseQuality.FalsePositive;
        }

        return ResponseQuality.Miss;
    }

    protected override bool ShouldEndStimulus()
    {
        return repetitionCount >= particleDispersions.Count * 30; // 30 repetitions per state
    }

    public override void SaveTrackingData(string stimulusName)
    {
        try
        {
            base.SaveTrackingData(stimulusName);

            OptotypeData optotypeData = new OptotypeData()
            {
                playerName = PlayerInfo.Instance.PlayerName,
                playerID = PlayerInfo.Instance.PlayerID,
                stimulusName = stimulusName,
                duration = this.duration,
                gazePositions = this.gazePositions.ToArray(),
                rotatedGaze = this.rotatedGaze.ToArray(),
                gazeRotations = this.gazeRotations.ToArray(),
                gazeTimes = this.gazeTimes.ToArray(),
                userResponses = userResponses
            };
            string json = JsonUtility.ToJson(optotypeData);
            System.IO.File.WriteAllText($"{this.storagePath}/{PlayerInfo.Instance.PlayerName}-{stimulusName}.json", json);

        }
        catch (Exception e)
        {
            Debug.Log("Failed to save data for " + stimulusName + " with error: " + e);
            OnFailedSave(stimulusName);
        }
    }
}