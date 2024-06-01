using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Tobii.XR;

[System.Serializable]
public class Chunk
{
    public List<Vector3> gazePos;
    public List<float> gazeTimes;
    public float startTrialTimes;
    public float preJumpedTimes;
    public float postJumpedTimes;
    public Vector3 jumpedPositions;
    public float jumpedSpeeds;
    public float passCenterTimes;
    public float endTrialTimes;
}

[System.Serializable]
public class ExtendedCobraData : Data
{
    public List<Chunk> chunks;
}

public class Cobra : Stimulus
{
    public float maxScale = 3f;
    [Range(0.1f, 5f)]
    public float timeToCenter = 0.2f; // 200ms for the dot to get to center
    [Range(0.1f, 10f)]
    public float scaleSpeed = 10f;
    public float outward = 20f;
    public int repetitionCount = 0;
    public int repetitionLimit = 400;
    [SerializeField] private Vector2 boundarySize = new Vector2(50f, 50f);


    private GameObject tracker;
    private Vector3 center = new Vector3(0f, 0f, 139f);
    private float[] speeds = new float[] { 16f, 18f, 20f, 22f, 24f }; // in deg/sec
    private float[] distances;
    private float oldTimeToCenter;
    private GameObject xrRig;
    private GameObject backdrop;
    private bool isMoving = false;
    private int index = 0;

    // private List<Vector3> gazePos = new List<Vector3>();
    // private List<float> gazeTimes = new List<float>();
    private List<bool> movementMask = new List<bool>();
    private List<float> startTrialTimes = new List<float>();
    private List<float> preJumpedTimes = new List<float>();
    private List<float> postJumpedTimes = new List<float>();
    private List<Vector3> jumpedPositions = new List<Vector3>();
    private List<float> jumpedSpeeds = new List<float>();
    private List<float> passCenterTimes = new List<float>();
    private List<float> endTrialTimes = new List<float>();

    protected override void OnEnable()
    {
        base.OnEnable();
        // Clear previous lists
        movementMask.Clear();
        startTrialTimes.Clear();
        preJumpedTimes.Clear();
        postJumpedTimes.Clear();
        jumpedPositions.Clear();
        jumpedSpeeds.Clear();
        passCenterTimes.Clear();
        endTrialTimes.Clear();

        tracker = GameObject.Find("Tracker");
        tracker.transform.localScale = Vector3.zero;
        tracker.transform.position = center;

        distances = new float[speeds.Length];
        oldTimeToCenter = timeToCenter;

        xrRig = GameObject.Find("XRRig");
        backdrop = GameObject.Find("Backdrop");
        RecalculateDistances(xrRig.transform.position.z, backdrop.transform.position.z);
        Debug.Log($"calibrated COBRA");
    }

    protected override void Update()
    {
        base.Update();
        Quaternion shiftRotation = this.headingRotation;
        Vector3 cameraOffset = xrRig.transform.position - backdrop.transform.position; // not currently used

        GameObject.Find("COBRA/BackdropHandle").transform.rotation = shiftRotation;

        // Form mask to filter out eye data when tracker is not moving
        movementMask.Add(isMoving);

        // Only begin routine when user clicks mouse
        if (!isMoving && Input.GetKey(KeyCode.RightArrow))
        {
            Debug.Log("Starting routine");
            isMoving = true;
            tracker.transform.position = shiftRotation * center; // Necessary to account current rotation to find new, rotated center
            StartCoroutine(MoveAndScale(GetPredefinedPosition(), center));
        }
    }

    protected override bool ShouldEndStimulus()
    {
        return repetitionCount >= repetitionLimit;
    }

    private IEnumerator MoveAndScale(Vector3 positionState, Vector3 centerState, Quaternion rotationState = default)
    {
        // To get rotation proper
        // positionState = rotationState * positionState;
        // centerState = rotationState * centerState;
        // Start of a trial
        repetitionCount++;
        startTrialTimes.Add(Time.time);

        float t = 0f;
        Vector3 startingScale = tracker.transform.localScale;

        // Scale up
        while (t < 1f)
        {
            t += Time.deltaTime * scaleSpeed;
            // Exponential ease-in scale
            tracker.transform.localScale = Vector3.Lerp(startingScale, new Vector3(maxScale, startingScale.y, maxScale), t);
            yield return null;
        }

        // Delay
        preJumpedTimes.Add(Time.time); // Save time before jump
        float delay = Mathf.Clamp(RandomNormal(), 0.2f, 5f);
        yield return new WaitForSeconds(delay);

        Debug.DrawRay(center - Vector3.up * 1f, Vector3.up * 2f, Color.yellow, 3f);
        Debug.DrawRay(center - Vector3.right * 1f, Vector3.right * 2f, Color.yellow, 3f);

        // Jump to target position and don't delay
        var rotatedPositionState = this.headingRotation * positionState; // Rotated jump position
        tracker.transform.position = rotatedPositionState;

        // Time spent to delay and new position
        postJumpedTimes.Add(Time.time); // Save time following jump
        jumpedPositions.Add(rotatedPositionState); // Save position jumped to

        // Calculate line from center to target position
        float slope = (positionState.y - centerState.y) / (positionState.x - centerState.x);
        Func<float, Vector3> line = (x) => new Vector3(x, slope * (x - centerState.x) + centerState.y, 139f);

        // Draw a small cross at the jump position
        Debug.DrawRay(positionState - Vector3.up * 1f, Vector3.up * 2f, Color.green, 3f);
        Debug.DrawRay(positionState - Vector3.right * 1f, Vector3.right * 2f, Color.green, 3f);

        Vector3 finalPosition = line(-positionState.x * outward); // Some point off the backdrop
        // Debug.Log($"position : {-positionState.x * outward} | finalPosition: {finalPosition.y} | outward: {outward}");
        // Debug.DrawLine(centerState, rotationState * finalPosition, Color.green, 10f);

        // Calculate move speed
        // Necessary to account for rotation to maintain speed
        float distance = Vector3.Distance(positionState, center);
        float moveSpeed = distance / timeToCenter;
        jumpedSpeeds.Add(moveSpeed); // Save move speed

        // Move from target position through center to outside field of view
        bool passedCenter = false; // state to check if the dot has passed the center
        t = 0f;
        while (t < 1f)
        {
            Debug.DrawRay(center - Vector3.up * 1f, Vector3.up * 2f, Color.red, 1f);
            Debug.DrawRay(center - Vector3.right * 1f, Vector3.right * 2f, Color.red, 1f);

            t += Time.deltaTime;
            var fractionTraveled = (moveSpeed * t) / Vector3.Distance(positionState, finalPosition);
            // Exponential ease-out translation
            // Necessary to account for rotation during Lerp
            tracker.transform.position = this.headingRotation * Vector3.Lerp(positionState, finalPosition, fractionTraveled);
            Debug.Log($"final: {finalPosition}");
            // Passed center
            if (!passedCenter && (tracker.transform.position.x >= centerState.x || tracker.transform.position.y >= centerState.y))
            {
                passCenterTimes.Add(Time.time);
                passedCenter = true;
            }
            yield return null;
        }

        // End of a trial
        endTrialTimes.Add(Time.time);

        // Scale down
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * scaleSpeed;
            // Exponential ease-out scale
            tracker.transform.localScale = Vector3.Lerp(new Vector3(maxScale, startingScale.y, maxScale), Vector3.zero, t);
            yield return null;
        }

        isMoving = false;
    }

    // Not currently being used
    private Vector3 GetRandomPosition()
    {
        float lowerRangeX = tracker.transform.position.x > 50f ? -boundarySize.x : tracker.transform.position.x + 2f * boundarySize.x;
        float upperRangeX = tracker.transform.position.x < 50f ? boundarySize.x : tracker.transform.position.x + 4f * boundarySize.x;
        float lowerRangeY = tracker.transform.position.y > 50f ? -boundarySize.y : tracker.transform.position.y + 2f * boundarySize.y;
        float upperRangeY = tracker.transform.position.y < 50f ? boundarySize.y : tracker.transform.position.y + 4f * boundarySize.y;

        float x = UnityEngine.Random.Range(lowerRangeX, upperRangeX);
        float y = UnityEngine.Random.Range(lowerRangeY, upperRangeY);
        return new Vector3(x, y, tracker.transform.position.z);
    }

    public float RandomNormal()
    {
        float u1 = UnityEngine.Random.Range(0f, 1f);
        float u2 = UnityEngine.Random.Range(0f, 1f);
        float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
        return (randStdNormal * 0.5f) + 0.7f; // mean of 0.7 and std of 0.5
    }

    private Vector3 GetPredefinedPosition(Quaternion rotationState = default)
    {
        // No need for rotation of this value.
        Vector3 position = UnityEngine.Random.insideUnitCircle.normalized * distances[index];

        index = UnityEngine.Random.Range(0, distances.Length);
        // index = (index + 1) % distances.Length;

        return rotationState * new Vector3(position.x, position.y, 139f);
    }

    // private void OnValidate()
    // {
    //     if (timeToCenter != oldTimeToCenter)
    //     {
    //         xrRig = GameObject.Find("XRRig");
    //         backdrop = GameObject.Find("Backdrop");
    //         RecalculateDistances(xrRig.transform.position.z, backdrop.transform.position.z);
    //         oldTimeToCenter = timeToCenter;
    //     }
    // }

    private void RecalculateDistances(float rigZ, float backdropZ)
    {
        distances = new float[speeds.Length];
        for (int i = 0; i < speeds.Length; i++)
        {
            distances[i] = Mathf.Tan(speeds[i] * 0.2f * Mathf.Deg2Rad) * Mathf.Abs(rigZ - backdropZ);
        }
    }

    public string CombineListsIntoJson()
    {
        ExtendedCobraData data = new ExtendedCobraData();
        data.chunks = new List<Chunk>();

        var groupedGazePositions = new List<List<Vector3>>();
        var currentGroup = new List<Vector3>();

        for (int i = 0; i < this.gazePositions.Count; i++)
        {
            if (movementMask[i])
            {
                currentGroup.Add(this.gazePositions[i]);
            }
            else if (currentGroup.Any())
            {
                // At first point that motion ends, add the just completed group to the groupedGazePositions list
                groupedGazePositions.Add(currentGroup);
                currentGroup = new List<Vector3>();
            }
        }
        // Probably uneccessary, but add the last, just finished group if it exists
        if (currentGroup.Any()) groupedGazePositions.Add(currentGroup);

        var groupedGazeTimes = new List<List<float>>();
        var currentGazeTimes = new List<float>();

        for (int i = 0; i < this.gazeTimes.Count; i++)
        {
            if (movementMask[i])
            {
                currentGazeTimes.Add(this.gazeTimes[i]);
            }
            else if (currentGazeTimes.Any())
            {
                groupedGazeTimes.Add(currentGazeTimes);
                currentGazeTimes = new List<float>();
            }
        }
        if (currentGazeTimes.Any()) groupedGazeTimes.Add(currentGazeTimes);

        // Create a Chunk for each index
        for (int i = 0; i < groupedGazePositions.Count; i++)
        {
            Chunk chunk = new Chunk();
            chunk.gazePos = GetValueOrDefault(groupedGazePositions, i, new List<Vector3>());
            chunk.gazeTimes = GetValueOrDefault(groupedGazeTimes, i, new List<float>() { -1f });
            chunk.startTrialTimes = GetValueOrDefault(startTrialTimes, i, -1f);
            chunk.preJumpedTimes = GetValueOrDefault(preJumpedTimes, i, -1f);
            chunk.postJumpedTimes = GetValueOrDefault(postJumpedTimes, i, -1f);
            chunk.jumpedPositions = GetValueOrDefault(jumpedPositions, i, new Vector3(-1f, -1f, -1f));
            chunk.jumpedSpeeds = GetValueOrDefault(jumpedSpeeds, i, -1f);
            chunk.passCenterTimes = GetValueOrDefault(passCenterTimes, i, -1f);
            chunk.endTrialTimes = GetValueOrDefault(endTrialTimes, i, -1f);

            data.chunks.Add(chunk);
        }
        data.playerName = PlayerInfo.Instance.PlayerName;
        data.playerID = PlayerInfo.Instance.PlayerID;
        data.stimulusName = this.stimulusName;
        data.duration = this.timeAlive;
        data.gazePositions = this.gazePositions.ToArray();
        data.rotatedGaze = this.rotatedGaze.ToArray();
        data.gazeRotations = this.gazeRotations.ToArray();
        data.gazeTimes = this.gazeTimes.ToArray();

        // Convert the Data object to a JSON string
        string dataJson = JsonUtility.ToJson(data, true);

        return dataJson;
    }

    private T GetValueOrDefault<T>(List<T> list, int index, T defaultValue)
    {
        if (index >= 0 && index < list.Count)
        {
            return list[index];
        }
        return defaultValue;
    }

    public override void SaveTrackingData(string stimulusName)
    {
        try
        {
            string dataJson = CombineListsIntoJson();
            System.IO.File.WriteAllText($"{this.storagePath}/{PlayerInfo.Instance.PlayerName}-{stimulusName}.json", dataJson);
        }
        catch (Exception e)
        {
            Debug.Log("Failed to save data for " + stimulusName + " with error: " + e);
            OnFailedSave(stimulusName);
        }
    }
}

