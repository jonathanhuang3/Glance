using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Tobii.XR;

[System.Serializable]
public class Data
{
    public string playerName;
    public string playerID;
    public string stimulusName;
    public float duration;
    public Vector3[] gazePositions;
    public Vector3[] rotatedGaze;
    public Quaternion[] gazeRotations;
    public float[] gazeTimes;
}

public class Stimulus : MonoBehaviour
{
    // Player info and stimulus metadata
    public string stimulusName;
    public string instructions;
    public float duration; // in seconds
    protected float timeAlive = 0f; // in seconds

    // Stimulus setup
    // protected List<GameObject> stimObjs = new List<GameObject>(); // Current plan to attach children of Stimulus class to enabeld/disabled gameobjects in scene
    public delegate void StimulusEventHandler(string stimulusName);
    public static event StimulusEventHandler StimulusEnded;
    public delegate void FailedSaveHandler(string stimulusName);
    public static event FailedSaveHandler FailedSave;
    public delegate void SkipStimulusHandler(string stimulusName);
    public static event SkipStimulusHandler SkipStimulus;
    public delegate void RepeatStimulusHandler();
    public static event RepeatStimulusHandler RepeatStimulus;
    // Scotoma setup
    public delegate void ModulateScotomaOcclusionHandler(bool correct, float occlusionAmount); // Generic delegate for occlusion amount, to be passed either a float or an int
    public static event ModulateScotomaOcclusionHandler ModulateScotomaOcclusion;
    public delegate void CycleOcclusionHandler(int minParticles, int maxParticles, float timeframe, float pauseDuration, int repetitions);
    public static event CycleOcclusionHandler CycleScotomaOcclusion;
    public MetaStimulus.OKRDriver driver;

    // Stimulus associated objects
    public GameObject scotomaHandler;
    public enum Scotoma { None, Cone, Central, Peripheral, Grid };
    public Scotoma scotoma = Scotoma.None;

    // Eye tracking and data setup
    protected GazeUtility gazeUtility;
    public bool saveTracking = true;
    public bool calibNeeded = true;
    protected string storagePath;
    protected Vector3 rayDirection { get; set; }
    protected Quaternion headingRotation { get; set; }
    protected Quaternion towardGazeRotation { get; set; }
    protected List<Vector3> gazePositions = new List<Vector3>();
    protected List<Quaternion> gazeRotations = new List<Quaternion>();
    protected List<Vector3> rotatedGaze = new List<Vector3>();
    protected List<float> gazeTimes = new List<float>();

    public RenderTexture renderTexture; // Set to the default layer camera's target texture
    public RenderTexture stimulusRenderTexture; // Set to the stimulus layer camera's target texture
    private Texture2D tex;
    public ComputeShader countWhitePixelsShader;
    private ComputeBuffer resultBuffer;
    protected List<float> fractionVisible = new List<float>();

    // public void ShowInstructions()
    // {
    //     breakRoom.SetActive(true);
    //     instructionsText.text = instructions;
    // }

    // public void HideInstructions()
    // {
    //     breakRoom.SetActive(false);
    // }

    protected void OnFailedSave(string stimulusName)
    {
        FailedSave?.Invoke(stimulusName);
    }

    public void ShowStimulus()
    {
        // Managed in ExperimentScheduler
    }

    public void EndStimulus()
    {
        if (scotomaHandler != null) scotomaHandler.SetActive(false);
        StimulusEnded?.Invoke(this.stimulusName);
    }

    public void SkipThisStimulus()
    {
        scotomaHandler.SetActive(false);
        SkipStimulus?.Invoke(this.stimulusName);
    }

    public void RepeatThisStimulus()
    {
        scotomaHandler.SetActive(false);
        RepeatStimulus?.Invoke();
    }

    protected virtual void OnEnable()
    {
        timeAlive = 0f;
        this.gazeUtility = new GazeUtility();
        this.storagePath = $"Assets/Scripts/SpaceTime/{PlayerInfo.Instance.PlayerName}/{stimulusName}";
        if (scotoma != Scotoma.None)
        {
            ToggleScotoma(true);
            // renderTexture.width = Screen.width;
            // renderTexture.height = Screen.height;
            // stimulusRenderTexture.width = Screen.width;
            // stimulusRenderTexture.height = Screen.height;

            tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        }

        TryToCalibrate();
    }

    protected virtual void OnDisable()
    {
        if (resultBuffer != null) resultBuffer.Release();
    }

    protected virtual void Update()
    {
        this.rayDirection = this.gazeUtility.GetGazeRay();
        this.headingRotation = this.gazeUtility.HeadingRotation(transform.forward, transform.up); // Useful to rotate stimulus to directly in front of XR rig

        // Rotate the stimulus system. Hopefully, all other components will rotate as well, if they are children of the rotation handle.
        // ps.transform.rotation = this.headingRotation; // This won't be necessary if the particle system is a child of the rotation handle

        // this.towardGazeRotation = this.gazeUtility.GazeTrackingRotation(transform.forward, transform.up); // Useful for gaze contingent scotoma, but as been changed to let ScotomaHandler poll this value.
        // if (this.scotoma != Scotoma.None) scotomaHandler.transform.rotation = this.headingRotation; // Rotate scotoma to match XR rig rotation

        this.gazePositions.Add(this.rayDirection); // Absolute gaze vector in world space (shifted due to rotation of XR rig)
        this.rotatedGaze.Add(Quaternion.Inverse(this.headingRotation) * this.rayDirection); // Rotates gaze vector back in front of origin
        this.gazeRotations.Add(Quaternion.Inverse(this.headingRotation)); //Rotation to get gaze vector back in front of origin
        this.gazeTimes.Add(Time.time);

        timeAlive += Time.deltaTime; // Time-based control of stimuli length for most stimuli except COBRA.
        if (this.scotoma != Scotoma.None) fractionVisible.Add(PixelsVisible(renderTexture) / PixelsVisible(stimulusRenderTexture));

        if (ShouldEndStimulus())
        {
            EndStimulus();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            SkipThisStimulus();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            RepeatThisStimulus();
        }
    }

    protected virtual bool ShouldEndStimulus()
    {
        return timeAlive >= duration;
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return; // Gets Unity to stop trying to update Gizmos after it is done playing.

        Gizmos.color = Color.green;
        Gizmos.DrawRay(gazeUtility.rayOrigin, rayDirection * 3.0f);
        Gizmos.DrawWireSphere(rayDirection * 3.0f, 0.5f);
    }

    protected void ToggleScotoma(bool enable)
    {
        if (enable)
        {
            if (scotomaHandler == null) scotomaHandler = GameObject.Find("Scotomas");
            if (scotomaHandler != null)
            {
                var handlerComponent = scotomaHandler.GetComponent<ScotomaHandler>();
                handlerComponent.scotoma = (ScotomaHandler.ScotomaTypes)this.scotoma;
                handlerComponent.driver = this.driver;
                if (scotomaHandler.activeSelf)
                {
                    handlerComponent.Initialize();
                }
                else
                {
                    scotomaHandler.SetActive(enable);
                }
            }
        }
    }

    protected void ScotomaModulateOcclusion(bool correct, float occlusionAmount)
    {
        ModulateScotomaOcclusion?.Invoke(correct, occlusionAmount);
    }

    protected void ScotomaCycleOcclusion(int minParticles, int maxParticles, float timeframe, float pauseDuration, int repetitions = 1)
    {
        CycleScotomaOcclusion?.Invoke(minParticles, maxParticles, timeframe, pauseDuration, repetitions);
    }

    private float PixelsVisible(RenderTexture rendTex)
    {
        int kernelInit = countWhitePixelsShader.FindKernel("CSInit");
        int kernelMain = countWhitePixelsShader.FindKernel("CountWhitePixels");
        resultBuffer = new ComputeBuffer(1, sizeof(int));
        int[] whitePixels = new int[1];

        countWhitePixelsShader.SetTexture(kernelInit, "InputTexture", rendTex);
        countWhitePixelsShader.SetTexture(kernelMain, "InputTexture", rendTex);
        countWhitePixelsShader.SetBuffer(kernelInit, "ResultBuffer", resultBuffer);
        countWhitePixelsShader.SetBuffer(kernelMain, "ResultBuffer", resultBuffer);
        countWhitePixelsShader.Dispatch(kernelInit, 1, 1, 1);
        countWhitePixelsShader.Dispatch(kernelMain, rendTex.width / 8, rendTex.height / 8, 1);

        resultBuffer.GetData(whitePixels);
        resultBuffer.Release();
        resultBuffer = null;

        return (float)whitePixels[0];
        // tex.ReadPixels(new Rect(0, 0, rendTex.width, rendTex.height), 0, 0);
        // tex.Apply();
        // RenderTexture.active = null;

        // // List<Color> pixels = tex.GetPixels().ToList();
        // // int whitePixels = pixels.Count(pixel => pixel.r > 0.9f && pixel.g > 0.9f && pixel.b > 0.9f); // This is the same as the loop below, but the loop is probably faster
        // Color[] pixels = tex.GetPixels();
        // int whitePixels = 0;
        // foreach (Color pixel in pixels)
        // {
        //     if (pixel.r > 0.9f && pixel.g > 0.9f && pixel.b > 0.9f)
        //     {
        //         whitePixels++;
        //     }
        // }

    }

    protected void TryToCalibrate()
    {
        if (this.calibNeeded)
        {
            Debug.Log($"Calibration needed");
            this.gazeUtility.Calibrate();
            Debug.Log($"Calibration complete");
        }
    }

    public virtual void SaveTrackingData(string stimulusName)
    {
        if (saveTracking)
        {
            // To save gaze data, keep meta file with current date and time.
            string gazeFile = $"gazeSpace.txt";
            string rotatedGazeFile = $"rotatedGaze.txt";
            string rotationsForGazeFile = $"gazeRotations.txt"; // Quaternion of gaze ray rotation
            string timeFile = $"gazeTime.txt";

            System.IO.Directory.CreateDirectory(this.storagePath);
            System.IO.File.WriteAllLines($"{this.storagePath}/{gazeFile}", ListToString<Vector3>(this.gazePositions));
            System.IO.File.WriteAllLines($"{this.storagePath}/{rotatedGazeFile}", ListToString<Vector3>(this.rotatedGaze));
            System.IO.File.WriteAllLines($"{this.storagePath}/{rotationsForGazeFile}", ListToString<Quaternion>(this.gazeRotations));
            System.IO.File.WriteAllLines($"{this.storagePath}/{timeFile}", ListToString<float>(this.gazeTimes));
        }
    }

    // Add generic List type to access all List types.
    protected string[] ListToString<T>(List<T> logged) where T : IFormattable
    {
        return logged.Select(logps => logps.ToString("F10", CultureInfo.InvariantCulture)).ToArray();
    }
}