using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Tobii.XR;

public class Scotoma : MonoBehaviour
{
    public bool gazeContingent = false;
    protected float speed = 3f;
    public bool toggleMotionJitter = true;
    public Color color;

    public MetaStimulus.OKRDriver driver = MetaStimulus.OKRDriver.TumblingE;
    protected GazeUtility gazeUtility;
    protected Vector3 rayDirection { get; set; }
    protected Vector3 rayOrigin { get; set; }

    protected virtual void OnEnable()
    {
        gazeUtility = new GazeUtility();
        Stimulus.ModulateScotomaOcclusion += ModulateOcclusion;
        Stimulus.CycleScotomaOcclusion += CycleOcclusion;
        ExperimentScheduler.StartDriver += SetOKRDriver;
    }

    void OnDisable()
    {
        Stimulus.ModulateScotomaOcclusion -= ModulateOcclusion;
        Stimulus.CycleScotomaOcclusion -= CycleOcclusion;
        ExperimentScheduler.StartDriver -= SetOKRDriver;
    }

    protected virtual void Update()
    {
        // Note that the heading rotation of the scotomas are conducted in the Stimulus class
        if (gazeContingent)
        {
            rayDirection = gazeUtility.GetGazeRay();
            rayOrigin = gazeUtility.rayOrigin;

            TrackGaze();
        }

        if (toggleMotionJitter)
        {
            Vector3 motionJitter = MotionJitter();
            motionJitter.z = 0f;
            // if (Time.frameCount % 30 == 0) Debug.Log(motionJitter);
            // transform.position += motionJitter;
            transform.Translate(motionJitter, Space.World);
        }
    }

    protected virtual void SetOKRDriver(MetaStimulus.OKRDriver newDriver)
    {
        Debug.Log($"Driver switched to: {driver.ToString()}");
        driver = newDriver;
    }
    /// <summary>
    /// Modulates the occlusion of the scene.
    /// </summary>
    /// <param name="correct">A boolean value indicating whether the occlusion should be corrected.</param>
    /// <param name="occlusionAmount">The amount of occlusion to apply.</param>
    protected virtual void ModulateOcclusion(bool correct, float occlusionAmount)
    {
        // Occlude varying amounts of the scene  either changing scale or modifying shader (Scotoma specific)
        // Generic type - either int or 
    }

    /// <summary>
    /// Cycle from fractional occlusion from zero to one to zero, in `timeframe` amount of time, with n second pause at minimum and maximum occlusions
    /// </summary>
    /// <param name="timeframe">The amount of time it takes to complete one cycle of occlusion</param>
    /// <param name="pauseDuration">The duration of the pause at minimum and maximum occlusions</param>
    protected virtual void CycleOcclusion(int min, int max, float timeframe, float pauseDuration, int repetitions)
    {

    }

    /// <summary>
    /// Calculates a jittered motion for the scotoma.
    /// </summary>
    /// <returns>A new position for the scotoma.</returns>
    protected virtual Vector3 MotionJitter()
    {
        // Jitter particles uniformly
        // shape.position = Vector3.Lerp(shape.position, new Vector3(Random.Range(-bound, bound), 0f, Random.Range(-bound, bound)), 0.1f);
        float bound = 0.25f; //0.25f
        Vector3 translation = new Vector3(Random.Range(-bound, bound), Random.Range(-bound, bound), 0f);
        // translation.x = Mathf.Clamp(translation.x, -bound, bound); // Keep within bounds
        // translation.y = Mathf.Clamp(translation.y, -bound, bound);
        // return Random.insideUnitCircle * bound * Time.deltaTime;
        return translation * Time.deltaTime;
    }

    /// <summary>
    /// Tracks the gaze and occludes the scene based on the gaze.
    /// </summary>
    protected virtual void TrackGaze()
    {
        // Track gaze and occlude scene based on gaze
        // Shift the quad (faster than adjusting the shader)
        // transform.position = new Vector3(transform.InverseTransformVector(rayOrigin + rayDirection).x, transform.InverseTransformVector(rayOrigin + rayDirection).y, 2);
        Vector3 rotatedGaze = gazeUtility.GazeTrackingRotation(10f, transform.forward) * (rayOrigin + rayDirection);
        transform.position = new Vector3(rotatedGaze.x, rotatedGaze.y, 2);
    }
}