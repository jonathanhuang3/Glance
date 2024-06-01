using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Tobii.XR;

public class FixationStation : MonoBehaviour
{
    public GameObject handle;
    private List<Vector3> fixationEyeData = new List<Vector3>();
    private float timeAlive;

    private GazeUtility gazeUtility;
    void Start()
    {
        gazeUtility = new GazeUtility();
        // gazeUtility.Calibrate();
    }

    void Update()
    {
        Quaternion shiftRotation = gazeUtility.HeadingRotation(transform.forward, transform.up);
        handle.transform.rotation = shiftRotation;
        fixationEyeData.Add(Quaternion.Inverse(shiftRotation) * gazeUtility.GetGazeRay());
        timeAlive += Time.deltaTime;

        if (timeAlive >= 300f)
        {
            Quit();
        }
    }

    void Quit()
    {
#if UNITY_STANDALONE
        Application.Quit();
#endif
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    void OnApplicationQuit()
    {
        Vector3ListWrapper fixationFile = new Vector3ListWrapper()
        {
            data = fixationEyeData
        };

        string storagePath = $"Assets/Scripts/SpaceTime/FixationData";
        System.IO.Directory.CreateDirectory(storagePath);
        System.IO.File.WriteAllText($"{storagePath}/eyeTraceFixation.json", JsonUtility.ToJson(fixationFile));
    }
}