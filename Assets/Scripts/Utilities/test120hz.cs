using UnityEngine;
using ViveSR.anipal.Eye;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Globalization;
using UnityEngine.UI;

/// <summary>
/// Example usage for eye tracking callback
/// Note: Callback runs on a separate thread to report at ~120hz.
/// Unity is not threadsafe and cannot call any UnityEngine api from within callback thread.
/// </summary>
public class test120hz : MonoBehaviour
{
    private static EyeData eyeData = new EyeData();
    private static bool eye_callback_registered = false;

    public Text uiText;
    private float updateSpeed = 0;
    private static float lastTime, currentTime;
    public Vector3 gazeRay;
    private static List<Vector3> gazePositions = new List<Vector3>();
    private static List<float> gazeTimes = new List<float>();


    void Update()
    {
        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING) return;


        if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == true && eye_callback_registered == false)
        {
            SRanipal_Eye.WrapperRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye.CallbackBasic)EyeCallback));
            eye_callback_registered = true;
        }
        else if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == false && eye_callback_registered == true)
        {
            SRanipal_Eye.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye.CallbackBasic)EyeCallback));
            eye_callback_registered = false;
        }

        updateSpeed = currentTime - lastTime;
        uiText.text = updateSpeed.ToString() + " ms";
    }

    public Vector3 GetGazeRay()
    {
        return gazePositions.LastOrDefault();
    }

    private void OnDisable()
    {
        Release();
    }

    void OnApplicationQuit()
    {
        // System.IO.File.WriteAllLines($"gazePositions120hz.txt", ListToString<Vector3>(gazePositions));
        // System.IO.File.WriteAllLines($"gazeTimes120hz.txt", ListToString<float>(gazeTimes));
        Release();
    }

    /// <summary>
    /// Release callback thread when disabled or quit
    /// </summary>
    private static void Release()
    {
        if (eye_callback_registered == true)
        {
            SRanipal_Eye.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye.CallbackBasic)EyeCallback));
            eye_callback_registered = false;
        }
    }

    /// <summary>
    /// Required class for IL2CPP scripting backend support
    /// </summary>
    internal class MonoPInvokeCallbackAttribute : System.Attribute
    {
        public MonoPInvokeCallbackAttribute() { }
    }

    /// <summary>
    /// Eye tracking data callback thread.
    /// Reports data at ~120hz
    /// MonoPInvokeCallback attribute required for IL2CPP scripting backend
    /// </summary>
    /// <param name="eye_data">Reference to latest eye_data</param>
    [MonoPInvokeCallback]
    private static void EyeCallback(ref EyeData eye_data)
    {
        eyeData = eye_data;

        lastTime = currentTime;
        currentTime = eyeData.timestamp;
        gazePositions.Add(eyeData.verbose_data.combined.eye_data.gaze_direction_normalized);
        gazeTimes.Add(currentTime);
    }

    protected string[] ListToString<T>(List<T> logged) where T : IFormattable
    {
        return logged.Select(logps => logps.ToString("F10", CultureInfo.InvariantCulture)).ToArray();
    }
}