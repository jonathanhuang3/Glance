using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Tobii.XR;

public class Scotoma : MonoBehaviour
{
    public bool gazeContingent = false;
    protected float speed = 3f;
    public bool toggleMotionJitter = false;

    public MetaStimulus.OKRDriver stimulusType;
    protected GazeUtility gazeUtility;
    protected Vector3 rayDirection { get; set; }
    protected Vector3 rayOrigin { get; set; }

    protected virtual void OnEnable()
    {
        gazeUtility = new GazeUtility();
        Stimulus.ModulateScotomaOcclusion += ModulateOcclusion;
    }

    void OnDisable()
    {
        Stimulus.ModulateScotomaOcclusion -= ModulateOcclusion;
    }

    protected void Update()
    {
        // Note that the heading rotation of the scotomas are conducted in the Stimulus class
        if (gazeContingent)
        {
            rayDirection = gazeUtility.GetGazeRay();
            rayOrigin = gazeUtility.rayOrigin;

            TrackGaze();
        }

        if (toggleMotionJitter) transform.position = MotionJitter();
    }
    protected virtual void ModulateOcclusion(bool correct)
    {
        // Occlude varying amounts of the scene by either changing scale or modifying shader (Scotoma specific)
        // Generic type - either int or float
    }

    protected virtual Vector3 MotionJitter()
    {
        // Jitter particles uniformly
        // shape.position = Vector3.Lerp(shape.position, new Vector3(Random.Range(-0.25f, 0.25f), 0f, Random.Range(-0.25f, 0.25f)), 0.1f);
        Vector3 translation = new Vector3(Random.Range(-0.25f, 0.25f), Random.Range(-0.25f, 0.25f), 0f) * Time.deltaTime;
        Vector3 newPosition = transform.position + translation;
        newPosition.x = Mathf.Clamp(newPosition.x, -0.25f, 0.25f); // Keep within bounds
        newPosition.y = Mathf.Clamp(newPosition.y, -0.25f, 0.25f);
        return newPosition;
    }

    protected virtual void TrackGaze()
    {
        // Track gaze and occlude scene based on gaze
        // Shift the quad (faster than adjusting the shader)
        // transform.position = new Vector3(transform.InverseTransformVector(rayOrigin + rayDirection).x, transform.InverseTransformVector(rayOrigin + rayDirection).y, 2);
        Vector3 rotatedGaze = gazeUtility.GazeTrackingRotation(10f, transform.forward) * (rayOrigin + rayDirection);
        transform.position = new Vector3(rotatedGaze.x, rotatedGaze.y, 2);
    }
}