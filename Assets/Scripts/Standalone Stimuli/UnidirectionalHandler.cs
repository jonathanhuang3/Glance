using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[System.Serializable]
public class UnidirectionalData : Data
{
    public List<Vector3> fixationSizes;

}
public class UnidirectionalHandler : Stimulus
{
    public float speed = 0.1f;
    public enum Direction { Up, Down };
    public Direction motion = Direction.Up;

    public bool fixationPoint = false; // For OKR Suppression stimulus
    // public GameObject fixationRegionPrefab;
    // private GameObject fixationPointPrefab;
    private List<Vector3> fixationSizes = new List<Vector3>();
    private Vector3 fixationPointScale;
    public Vector3 minimumRegionSize;
    public Vector3 maximumRegionSize;

    protected override void OnEnable()
    {
        base.OnEnable();

    }

    protected override void Update()
    {

        // Set the speed variable of the shader
        theater.GetComponent<Renderer>().material.SetFloat("_Speed", speed);

        // Set the direction variable of the shader
        bool up = (motion == Direction.Up);
        float upValue = up ? 1.0f : 0.0f;
        theater.GetComponent<Renderer>().material.SetFloat("_movingUp", upValue);

        // Lerp the fixation point size
        if (fixationPoint)
        {
            fixationRegionPrefab.transform.localScale = LerpFixationPointSize();
            fixationSizes.Add(fixationRegionPrefab.transform.localScale);
            fixationPointPrefab.transform.localScale = fixationPointScale;
        }
        base.Update(); // Added at end to avoid null reference when at the top.

    }

    private Vector3 LerpFixationPointSize()
    {
        float asymptote = this.duration + 5f;
        float nonlinearLerp = (asymptote * Time.time) / (Time.time + 7f); // (t)/(t+1) slows down lerp near the end
        Debug.Log(nonlinearLerp / asymptote);
        Vector3 size = Vector3.Lerp(maximumRegionSize, minimumRegionSize, nonlinearLerp / this.duration);
        return size;
    }

    public override void SaveTrackingData(string stimulusName)
    {
        try
        {
            base.SaveTrackingData(stimulusName);
            UnidirectionalData data = new UnidirectionalData()
            {
                playerName = PlayerInfo.Instance.PlayerName,
                playerID = PlayerInfo.Instance.PlayerID,
                stimulusName = stimulusName,
                duration = duration,
                gazePositions = gazePositions.ToArray(),
                gazeRotations = gazeRotations.ToArray(),
                rotatedGaze = rotatedGaze.ToArray(),
                gazeTimes = gazeTimes.ToArray(),
                fixationSizes = fixationSizes.ToArray()
            };

            string dataJson = JsonUtility.ToJson(data);
            System.IO.File.WriteAllText($"{this.storagePath}/{PlayerInfo.Instance.PlayerName}-{stimulusName}.json", dataJson);
            System.IO.File.WriteAllLines($"{this.storagePath}/fixationSizes.txt", this.ListToString<Vector3>(fixationSizes));
        }
        catch (Exception e)
        {
            Debug.Log("Failed to save data for " + stimulusName + " with error: " + e);
            OnFailedSave(stimulusName);
        }
    }


}