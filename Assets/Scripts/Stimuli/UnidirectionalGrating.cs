using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ZunidirectionalData : Data
{
    public List<float> fixationSizes;
}
public class UnidirectionalGrating : Stimulus
{
    public GameObject theater;
    public float speed = 22f;
    public float spatialFrequency = 10f;

    public GameObject fixate;
    private float scaleTime;

    private List<float> fixationSizes = new List<float>();

    void Start()
    {
        theater.GetComponent<Renderer>().material.SetFloat("_Speed", speed);
        theater.GetComponent<Renderer>().material.SetFloat("_SpatialFrequency", spatialFrequency);
    }

    protected override void Update()
    {

        scaleTime += Time.deltaTime;
        fixate.transform.localScale = Vector3.Lerp(new Vector3(1, 1, 0), Vector3.zero, scaleTime / this.duration);
        base.Update();
    }

    public override void SaveTrackingData(string stimulusName)
    {
        try
        {
            ZunidirectionalData data = new ZunidirectionalData()
            {
                playerName = PlayerInfo.Instance.PlayerName,
                playerID = PlayerInfo.Instance.PlayerID,
                stimulusName = stimulusName,
                duration = this.duration,
                gazePositions = this.gazePositions.ToArray(),
                rotatedGaze = this.rotatedGaze.ToArray(),
                gazeRotations = this.gazeRotations.ToArray(),
                gazeTimes = this.gazeTimes.ToArray(),
                fixationSizes = fixationSizes
            };
            base.SaveTrackingData(stimulusName);
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