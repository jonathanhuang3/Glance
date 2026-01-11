using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;

using Tobii.XR;

public class ExperimentScheduler : MonoBehaviour
{
    // Player info
    public string playerName = "Name";
    public string playerID = "ID";
    public string experimentTag = "ID";
    public ExperimentOrder schedule;
    public IEnumerator<MetaStimulus> scheduleEnumerator;

    // Stimulus setup
    public GameObject XRCamera;
    public GameObject rotationHandle;
    public List<GameObject> drivers = new List<GameObject>();
    private GameObject objectDriver;
    // UI and experiment setup
    public GameObject waitingRoom;


    // Internal setup
    public GameObject fasterTracker;
    private GazeUtility gazeUtility;
    // private FasterTracker gazeUtility;
    private DateTime experimentStartTime;
    private int currentStimulus = 0;

    // Broadcasts
    public delegate void NewOKRDriver(MetaStimulus.OKRDriver driver);
    public static event NewOKRDriver StartDriver;

    void Start()
    {
        PlayerInfo.Initialize(playerName, playerID, experimentTag);
        scheduleEnumerator = schedule.MetaStimuli.GetEnumerator();
        experimentStartTime = DateTime.Now;
        gazeUtility = new GazeUtility();
        // gazeUtility = fasterTracker.GetComponent<FasterTracker>();
        BeginExperiment();
    }

    void Update()
    {
        
        // For remote testing without Tobii SDK initialized
        if (gazeUtility == null) return;
        
        try
        {
            rotationHandle.transform.rotation = gazeUtility.HeadingRotation(transform.forward, transform.up);
        }
        catch (System.NullReferenceException)
        {
            // Tobii SDK not initialized; skip rotation update
        }
    }

    void OnEnable()
    {
        // Subscribe to the end of stimulus event
        Stimulus.StimulusEnded += OnStimulusEnded;
        Stimulus.FailedSave += OnFailedSave;
        Stimulus.SkipStimulus += OnExitStimulus;
        Stimulus.RepeatStimulus += OnRepeatStimulus;
        WaitingRoomController.StimulusCompletedEvent += OnExitWaitingRoom;
        WaitingRoomController.RepeatStimulusEvent += OnRepeatStimulus;
        WaitingRoomController.SkipNextStimulusEvent += OnSkipStimulus;
    }

    void OnDisable()
    {
        // Unsubscribe from the end of stimulus event
        Stimulus.StimulusEnded -= OnStimulusEnded;
        Stimulus.FailedSave -= OnFailedSave;
        Stimulus.SkipStimulus -= OnExitStimulus;
        Stimulus.RepeatStimulus -= OnRepeatStimulus;
        WaitingRoomController.StimulusCompletedEvent -= OnExitWaitingRoom;
        WaitingRoomController.RepeatStimulusEvent -= OnRepeatStimulus;
        WaitingRoomController.SkipNextStimulusEvent -= OnSkipStimulus;
    }

    void OnExitStimulus(string stimulusName)
    {
        // While current stimulus is active, end current stimulus and enter waiting room. Raised by Stimulus component
        EnterWaitingRoom($"\nSkipped {stimulusName}. \nPress \u2192 to begin");
        CloseGameObjects(objectDriver);
        // objectDriver.SetActive(false);
    }
    void OnSkipStimulus()
    {
        // While in waiting room, skip next stimulus. Raised by WaitingRoomController
        waitingRoom.SetActive(false); // To allow reload of waiting room to occurr
        if (scheduleEnumerator.MoveNext())
        {
            currentStimulus++;
            EnterWaitingRoom($"\nSkipped {currentStimulus}.\nPress \u2192 to begin Stimulus {currentStimulus + 1}", calibNeeded: false);
        }
        else
        {
            EndExperiment();
        }

    }

    void OnFailedSave(string stimulusName)
    {
        // If the stimulus fails to save, repeat it. Raised by Stimulus component
        RepeatStimulus();
    }

    void OnExitWaitingRoom(bool calibNeeded, bool repeatStimulus)
    {
        NextStimulus(calibNeeded, repeatStimulus);
        waitingRoom.SetActive(false);
    }

    void OnRepeatStimulus()
    {
        // Repeat the current stimulus. Raised by WaitingRoomController
        RepeatStimulus();
        waitingRoom.SetActive(false);
    }
    void OnExitStimulusAndRepeat(string stimulusName)
    {
        // While current stimulus is active, enter waiting room just to repeat stimulus. Raised by Stimulus component
        EnterWaitingRoom($"\nPress \u2192 to repeat Stimulus {currentStimulus + 1}", repeatStimulus: true);
        CloseGameObjects(objectDriver);
        // objectDriver.SetActive(false);
    }
    void BeginExperiment()
    {
        // Set OKR drivers to inactive
        // Debug.Log($"Drivers: {MetaStimulus.OKRDriver.Dots.ToString()}, {MetaStimulus.OKRDriver.Unidirectional.ToString()}, {MetaStimulus.OKRDriver.COBRA.ToString()}");
        drivers.ForEach(driver => driver.SetActive(false));

        // Enter waiting room
        EnterWaitingRoom("\nPress \u2192 to begin");

        // StartCoroutine(WaitForInput()); // This is now raised in an event in the waiting room controller
    }

    void OnStimulusEnded(string stimulusName)
    {
        // Raised by Stimulus component when stimulus completes, to wrap up stimulus and enter waiting room.
        // Save data
        Debug.Log($"Saving data for {stimulusName}");
        var stimulus = objectDriver.GetComponent<Stimulus>();
        try
        {
            stimulus.SaveTrackingData(stimulusName);

        }
        catch (Exception e)
        {
            Debug.Log($"Stimulus component not found on {objectDriver.name}. Caught error: {e}");
        }


        EnterWaitingRoom($"\nPress \u2192 to continue \nPress \u2190 to repeat");
        CloseGameObjects(objectDriver);
        // objectDriver.SetActive(false);
        // StartCoroutine(WaitForInput()); // This is now raised in an event in the waiting room controller
    }

    void NextStimulus(bool calibNeeded = false, bool repeatStimulus = false)
    {
        if (!repeatStimulus && !scheduleEnumerator.MoveNext())
        {
            EndExperiment();
            return;
        }

        // GameObject holding stimulus
        objectDriver = drivers.Find(driver => driver.ToString().Contains(scheduleEnumerator.Current.driver.ToString()));
        // MetaData set from scriptable object containing experiment schedule
        MetaStimulus currentMetaStimulus = scheduleEnumerator.Current;
        string driver = currentMetaStimulus.driver.ToString(); // enum stating which gameobject to use.
        switch (currentMetaStimulus.driver)
        {
            case MetaStimulus.OKRDriver.Dots:
                ParticleController particleController = objectDriver.GetComponent<ParticleController>();
                particleController.stimulusName = currentMetaStimulus.name;
                particleController.driver = currentMetaStimulus.driver;
                particleController.instructions = currentMetaStimulus.name;
                particleController.duration = currentMetaStimulus.duration;
                particleController.saveTracking = currentMetaStimulus.saveTracking;
                particleController.calibNeeded = calibNeeded; // || currentMetaStimulus.calibNeeded Refresh calibration when skipping stimuli.
                particleController.movementDirection = (ParticleController.Direction)currentMetaStimulus.movementDirection;
                particleController.unidirectional = currentMetaStimulus.unidirectional;
                particleController.timeRepetitions = currentMetaStimulus.numRepetitions;
                particleController.contrast = (ParticleController.Contrast)currentMetaStimulus.contrast;
                // Add cycle contrast toggle
                particleController.scotoma = (Stimulus.Scotoma)currentMetaStimulus.scotoma;
                particleController.cycleScotoma = currentMetaStimulus.cycleScotoma;
                objectDriver.SetActive(true);
                break;
            case MetaStimulus.OKRDriver.Unidirectional:
                UnidirectionalHandler unidirectional = objectDriver.GetComponent<UnidirectionalHandler>();
                unidirectional.stimulusName = currentMetaStimulus.name;
                unidirectional.driver = currentMetaStimulus.driver;
                unidirectional.instructions = currentMetaStimulus.name;
                unidirectional.duration = currentMetaStimulus.duration;
                unidirectional.saveTracking = currentMetaStimulus.saveTracking;
                unidirectional.calibNeeded = calibNeeded; //|| currentMetaStimulus.calibNeeded;
                unidirectional.motion = (UnidirectionalHandler.Direction)currentMetaStimulus.movementDirection;
                unidirectional.fixate = currentMetaStimulus.fixate;
                unidirectional.scotoma = (Stimulus.Scotoma)currentMetaStimulus.scotoma;
                objectDriver.SetActive(true);
                break;
            case MetaStimulus.OKRDriver.COBRA:
                Cobra cobra = objectDriver.GetComponent<Cobra>();
                cobra.stimulusName = currentMetaStimulus.name;
                cobra.driver = currentMetaStimulus.driver;
                cobra.instructions = currentMetaStimulus.name;
                cobra.repetitionLimit = currentMetaStimulus.repetitionLimit;
                cobra.saveTracking = currentMetaStimulus.saveTracking;
                cobra.calibNeeded = calibNeeded; //|| currentMetaStimulus.calibNeeded;
                cobra.scotoma = (Stimulus.Scotoma)currentMetaStimulus.scotoma;
                objectDriver.SetActive(true);
                break;
            case MetaStimulus.OKRDriver.TumblingE:
                TumblingOptotype tumblingOptotype = objectDriver.GetComponent<TumblingOptotype>();
                tumblingOptotype.stimulusName = currentMetaStimulus.name;
                tumblingOptotype.driver = currentMetaStimulus.driver;
                tumblingOptotype.numStates = currentMetaStimulus.numStates;
                tumblingOptotype.repetitionLimit = currentMetaStimulus.repetitionLimit;
                tumblingOptotype.saveTracking = currentMetaStimulus.saveTracking;
                tumblingOptotype.calibNeeded = calibNeeded; //|| currentMetaStimulus.calibNeeded
                // Noise and initial optotype direction could be laid here
                tumblingOptotype.scotoma = (Stimulus.Scotoma)currentMetaStimulus.scotoma;
                objectDriver.SetActive(true);
                break;
        }
        StartDriver?.Invoke(currentMetaStimulus.driver); // Invokes after objectDriver is set active.
        if (!repeatStimulus)
        {
            currentStimulus++;
        }
    }

    void RepeatStimulus(string stimulusName = null)
    {
        if (stimulusName != null)
        {
            // If errant stimuli name is passed, repeat that stimulus
            MetaStimulus errantStimulus = (MetaStimulus)schedule.MetaStimuli.Where(stimulus => stimulus.name == stimulusName).FirstOrDefault();
            string driver = errantStimulus.driver.ToString();

            switch (errantStimulus.driver)
            {
                case MetaStimulus.OKRDriver.Dots:
                    objectDriver.GetComponent<ParticleController>().stimulusName = errantStimulus.name;
                    objectDriver.GetComponent<ParticleController>().instructions = errantStimulus.name;
                    objectDriver.GetComponent<ParticleController>().duration = errantStimulus.duration;
                    objectDriver.GetComponent<ParticleController>().calibNeeded = errantStimulus.calibNeeded;
                    break;
                case MetaStimulus.OKRDriver.Unidirectional:
                    objectDriver.GetComponent<UnidirectionalHandler>().stimulusName = errantStimulus.name;
                    objectDriver.GetComponent<UnidirectionalHandler>().instructions = errantStimulus.name;
                    objectDriver.GetComponent<UnidirectionalHandler>().duration = errantStimulus.duration;
                    objectDriver.GetComponent<UnidirectionalHandler>().calibNeeded = errantStimulus.calibNeeded;
                    break;
                case MetaStimulus.OKRDriver.COBRA:
                    objectDriver.GetComponent<Cobra>().stimulusName = errantStimulus.name;
                    objectDriver.GetComponent<Cobra>().instructions = errantStimulus.name;
                    objectDriver.GetComponent<Cobra>().repetitionLimit = errantStimulus.repetitionLimit;
                    objectDriver.GetComponent<Cobra>().calibNeeded = errantStimulus.calibNeeded;
                    break;
                case MetaStimulus.OKRDriver.TumblingE:
                    objectDriver.GetComponent<TumblingOptotype>().stimulusName = errantStimulus.name;
                    objectDriver.GetComponent<TumblingOptotype>().repetitionLimit = errantStimulus.repetitionLimit;
                    objectDriver.GetComponent<TumblingOptotype>().calibNeeded = errantStimulus.calibNeeded;
                    break;
            }

            objectDriver.SetActive(true);
        }
        else if (scheduleEnumerator.Current != null)
        {
            // If no errant stimuli name is passed, repeat the current stimulus
            MetaStimulus currentMetaStimulus = scheduleEnumerator.Current;
            string driver = currentMetaStimulus.driver.ToString();

            switch (currentMetaStimulus.driver)
            {
                case MetaStimulus.OKRDriver.Dots:
                    objectDriver.GetComponent<ParticleController>().stimulusName = currentMetaStimulus.name;
                    objectDriver.GetComponent<ParticleController>().instructions = currentMetaStimulus.name;
                    objectDriver.GetComponent<ParticleController>().duration = currentMetaStimulus.duration;
                    objectDriver.GetComponent<ParticleController>().calibNeeded = currentMetaStimulus.calibNeeded;
                    break;
                case MetaStimulus.OKRDriver.Unidirectional:
                    objectDriver.GetComponent<UnidirectionalHandler>().stimulusName = currentMetaStimulus.name;
                    objectDriver.GetComponent<UnidirectionalHandler>().instructions = currentMetaStimulus.name;
                    objectDriver.GetComponent<UnidirectionalHandler>().duration = currentMetaStimulus.duration;
                    objectDriver.GetComponent<UnidirectionalHandler>().calibNeeded = currentMetaStimulus.calibNeeded;
                    break;
                case MetaStimulus.OKRDriver.COBRA:
                    objectDriver.GetComponent<Cobra>().stimulusName = currentMetaStimulus.name;
                    objectDriver.GetComponent<Cobra>().instructions = currentMetaStimulus.name;
                    objectDriver.GetComponent<Cobra>().repetitionLimit = currentMetaStimulus.repetitionLimit;
                    objectDriver.GetComponent<Cobra>().calibNeeded = currentMetaStimulus.calibNeeded;
                    break;
                case MetaStimulus.OKRDriver.TumblingE:
                    objectDriver.GetComponent<TumblingOptotype>().stimulusName = currentMetaStimulus.name;
                    objectDriver.GetComponent<TumblingOptotype>().repetitionLimit = currentMetaStimulus.repetitionLimit;
                    objectDriver.GetComponent<TumblingOptotype>().calibNeeded = currentMetaStimulus.calibNeeded;
                    break;
            }

            objectDriver.SetActive(true);
        }
    }

    void CloseGameObjects(params GameObject[] gameObjects)
    {
        foreach (GameObject gameObject in gameObjects)
        {
            gameObject.SetActive(false);
        }
    }

    public void EnterWaitingRoom(string addendum = "", bool calibNeeded = false, bool repeatStimulus = false)
    {
        // Set up waiting room
        var waitingRoomController = waitingRoom.GetComponent<WaitingRoomController>();
        waitingRoomController.totalStimuli = schedule.MetaStimuli.Count();
        waitingRoomController.stimuliCompleted = currentStimulus;
        waitingRoomController.addendum = addendum;
        waitingRoomController.calibNeeded = calibNeeded;
        waitingRoomController.repeatStimulus = repeatStimulus;
        waitingRoom.SetActive(true);
    }

    public void EndExperiment()
    {
        // End experiment
        var playerData = $"Assets/Scripts/SpaceTime/{PlayerInfo.Instance.PlayerName}";
        // Get list of saved stimuli
        List<string> savedStimuli = Directory.GetDirectories(playerData)
            .Select(stimulus => stimulus.Split('/').Last())
            .ToList();
        // Get list of unsaved stimuli or stimuli with no tracking data
        var unsavedStimuliQuery = schedule.MetaStimuli
            .Where(stimulus =>
            {
                var isSaved = savedStimuli.Contains(stimulus.name);
                // var hasFiles = Directory.EnumerateFileSystemEntries($"{playerData}/{stimulus.name}").Any();

                return !isSaved || !Directory.EnumerateFileSystemEntries($"{playerData}/{stimulus.name}").Any();
            });
        // Get stimuli names as string
        List<string> unsavedStimuli = unsavedStimuliQuery
            .Select(incompleteStim => incompleteStim.name)
            .ToList();

        if (unsavedStimuli.Count() == 0)
        {
            Debug.Log("End of experiment");
            EnterWaitingRoom("End of experiment");
        }
        else
        {
            Debug.Log($"End of experiment. Missing {unsavedStimuli.Count()} stimuli: {unsavedStimuli}");
            EnterWaitingRoom($"End of experiment. Missing stimuli: {string.Join("\n", unsavedStimuli)}\nPress \u2192 to redo missing stimuli.");
        }
    }


}