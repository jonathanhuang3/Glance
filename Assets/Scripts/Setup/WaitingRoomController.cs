using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tobii.XR;

public class WaitingRoomController : MonoBehaviour
{
    public Slider statusSlider;
    public TextMeshProUGUI stimuliLeftText;
    public string addendum = "";

    public int totalStimuli;
    public int stimuliCompleted;
    public bool calibNeeded;
    public bool repeatStimulus;
    private float animationDuration = 1.0f;

    public delegate void StimulusCompleted(bool calibNeeded, bool repeatStimulus);
    public static event StimulusCompleted StimulusCompletedEvent;
    public delegate void RepeatStimulus();
    public static event RepeatStimulus RepeatStimulusEvent;
    public delegate void SkipNextStimulus();
    public static event SkipNextStimulus SkipNextStimulusEvent;

    private GazeUtility gazeUtility;

    private void OnEnable()
    {
        gazeUtility = new GazeUtility();
        UpdateProgress();
    }

    void Update()
    {
        // Issue with perpetual rotation occurs when rotating the attached gameobject. solution is to create nested gameobject 'Handle', and rotate, thus rotating the actual canvas.
        // GameObject.Find("Handle").transform.rotation = gazeUtility.HeadingRotation(transform.forward, transform.up);

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            StimulusCompletedEvent?.Invoke(calibNeeded, repeatStimulus);
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            RepeatStimulusEvent?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            SkipNextStimulusEvent?.Invoke();
        }
    }

    public void UpdateProgress()
    {
        // stimuliCompleted++; // set by Experiment Scheduler
        StartCoroutine(AnimateStatus());
        UpdateStimuliLeftText();
    }

    private IEnumerator AnimateStatus()
    {
        float initialProgress = statusSlider.value;
        float targetProgress = (float)stimuliCompleted / totalStimuli;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            statusSlider.value = Mathf.Lerp(initialProgress, targetProgress, elapsedTime / animationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        statusSlider.value = targetProgress; // Ensure the final value is set
    }

    private void UpdateStimuliLeftText()
    {
        int stimuliLeft = totalStimuli - stimuliCompleted;
        stimuliLeftText.text = $"Stimuli Left: {stimuliLeft} \n{addendum}";
    }
}
