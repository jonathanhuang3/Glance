using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PatientExperimentOrder", menuName = "ScriptableObjects/Patient Experiment Order for 202407", order = 1)]

public class PatientExperimentOrder : ExperimentOrder
{
    public PatientExperimentOrder()
    {
        MetaStimuli = new List<MetaStimulus>();

        MetaStimuli.Add(new MetaStimulus
        {
            name = "Practice Oscillating Dots",
            driver = MetaStimulus.OKRDriver.Dots,
            movementDirection = MetaStimulus.Direction.Up,
            contrast = MetaStimulus.Contrast.High,
            scotoma = MetaStimulus.Scotoma.None,
            duration = 15f,
            saveTracking = false,
            calibNeeded = false


        });

        MetaStimuli.Add(new MetaStimulus
        {
            name = "High Contrast Up Unidirectional Dots 1",
            driver = MetaStimulus.OKRDriver.Dots,
            movementDirection = MetaStimulus.Direction.Up,
            unidirectional = true,
            contrast = MetaStimulus.Contrast.High,
            scotoma = MetaStimulus.Scotoma.None,
            duration = 40f,
            calibNeeded = true

        });



        MetaStimuli.Add(new MetaStimulus
        {
            name = "Cyclic Contrast Vertical Unidirectional Dots 1",
            driver = MetaStimulus.OKRDriver.Dots,
            movementDirection = MetaStimulus.Direction.Up,
            unidirectional = true,
            contrast = MetaStimulus.Contrast.Cycle,
            scotoma = MetaStimulus.Scotoma.None,
            duration = 40f,
            calibNeeded = false

        });

        MetaStimuli.Add(new MetaStimulus
        {
            name = "Cyclic Contrast Vertical Unidirectional Dots 2",
            driver = MetaStimulus.OKRDriver.Dots,
            movementDirection = MetaStimulus.Direction.Up,
            unidirectional = true,
            contrast = MetaStimulus.Contrast.Cycle,
            scotoma = MetaStimulus.Scotoma.None,
            duration = 40f,
            calibNeeded = false

        });

        MetaStimuli.Add(new MetaStimulus
        {
            name = "Cyclic Contrast Vertical Unidirectional Dots 3",
            driver = MetaStimulus.OKRDriver.Dots,
            movementDirection = MetaStimulus.Direction.Up,
            unidirectional = true,
            contrast = MetaStimulus.Contrast.Cycle,
            scotoma = MetaStimulus.Scotoma.None,
            duration = 40f,
            calibNeeded = false

        });

        MetaStimuli.Add(new MetaStimulus
        {
            name = "Optotype Practice",
            driver = MetaStimulus.OKRDriver.TumblingE,
            scotoma = MetaStimulus.Scotoma.Cone,
            repetitionLimit = 3,
            numStates = 5,
            saveTracking = false,
            calibNeeded = false

        });

        MetaStimuli.Add(new MetaStimulus
        {
            name = "Tumbling E",
            driver = MetaStimulus.OKRDriver.TumblingE,
            scotoma = MetaStimulus.Scotoma.Cone,
            repetitionLimit = 15,
            numStates = 16, // z: 4.648 changed to 4.6 with patient due to scotomas rotating behind optotype.
            calibNeeded = false

        });

        MetaStimuli.Add(new MetaStimulus
        {
            name = "Cyclic Scotoma 1",
            driver = MetaStimulus.OKRDriver.Dots,
            movementDirection = MetaStimulus.Direction.Up,
            unidirectional = true,
            contrast = MetaStimulus.Contrast.High,
            scotoma = MetaStimulus.Scotoma.Cone,
            cycleScotoma = true,
            duration = 190f, //423f
            numRepetitions = 1,
            calibNeeded = true

        });

        MetaStimuli.Add(new MetaStimulus
        {
            name = "Cyclic Scotoma 2",
            driver = MetaStimulus.OKRDriver.Dots,
            movementDirection = MetaStimulus.Direction.Up,
            unidirectional = true,
            contrast = MetaStimulus.Contrast.High,
            scotoma = MetaStimulus.Scotoma.Cone,
            cycleScotoma = true,
            duration = 190f, //423f
            numRepetitions = 1,
            calibNeeded = false

        });

        MetaStimuli.Add(new MetaStimulus
        {
            name = "Cyclic Scotoma 3",
            driver = MetaStimulus.OKRDriver.Dots,
            movementDirection = MetaStimulus.Direction.Up,
            unidirectional = true,
            contrast = MetaStimulus.Contrast.High,
            scotoma = MetaStimulus.Scotoma.Cone,
            cycleScotoma = true,
            duration = 190f, //423f
            numRepetitions = 1,
            calibNeeded = false

        });

        MetaStimuli.Add(new MetaStimulus
        {
            name = "High Contrast Vertical Oscillating Dots",
            driver = MetaStimulus.OKRDriver.Dots,
            movementDirection = MetaStimulus.Direction.Up,
            contrast = MetaStimulus.Contrast.High,
            scotoma = MetaStimulus.Scotoma.None,
            duration = 190f,
            calibNeeded = true

        });

        MetaStimuli.Add(new MetaStimulus
        {
            name = "High Contrast Up Unidirectional Dots 2",
            driver = MetaStimulus.OKRDriver.Dots,
            movementDirection = MetaStimulus.Direction.Up,
            unidirectional = true,
            contrast = MetaStimulus.Contrast.High,
            scotoma = MetaStimulus.Scotoma.None,
            duration = 40f,
            calibNeeded = false

        });

        MetaStimuli.Add(new MetaStimulus
        {
            name = "High Contrast Right Unidirectional Dots 1",
            driver = MetaStimulus.OKRDriver.Dots,
            movementDirection = MetaStimulus.Direction.Right,
            unidirectional = true,
            contrast = MetaStimulus.Contrast.High,
            scotoma = MetaStimulus.Scotoma.None,
            duration = 40f,
            calibNeeded = false

        });

        MetaStimuli.Add(new MetaStimulus
        {
            name = "High Contrast Diagonal Unidirectional Dots 1",
            driver = MetaStimulus.OKRDriver.Dots,
            movementDirection = MetaStimulus.Direction.Diagonal,
            unidirectional = true,
            contrast = MetaStimulus.Contrast.High,
            scotoma = MetaStimulus.Scotoma.None,
            duration = 40f,
            calibNeeded = false

        });

        MetaStimuli.Add(new MetaStimulus
        {
            name = "High Contrast Down Unidirectional Dots 1",
            driver = MetaStimulus.OKRDriver.Dots,
            movementDirection = MetaStimulus.Direction.Down,
            unidirectional = true,
            contrast = MetaStimulus.Contrast.High,
            scotoma = MetaStimulus.Scotoma.None,
            duration = 40f,
            calibNeeded = false

        });

        MetaStimuli.Add(new MetaStimulus
        {
            name = "High Contrast Left Unidirectional Dots 1",
            driver = MetaStimulus.OKRDriver.Dots,
            movementDirection = MetaStimulus.Direction.Left,
            unidirectional = true,
            contrast = MetaStimulus.Contrast.High,
            scotoma = MetaStimulus.Scotoma.None,
            duration = 40f,
            calibNeeded = false

        });

        MetaStimuli.Add(new MetaStimulus
        {
            name = "OKR Suppression 1",
            driver = MetaStimulus.OKRDriver.Dots,
            scotoma = MetaStimulus.Scotoma.Fixation,
            movementDirection = MetaStimulus.Direction.Up,
            unidirectional = true,
            fixate = true,
            duration = 40f,
            calibNeeded = true

        });

        MetaStimuli.Add(new MetaStimulus
        {
            name = "OKR Suppression 2",
            driver = MetaStimulus.OKRDriver.Dots,
            scotoma = MetaStimulus.Scotoma.Fixation,
            movementDirection = MetaStimulus.Direction.Up,
            unidirectional = true,
            fixate = true,
            duration = 40f,
            calibNeeded = false

        });

        MetaStimuli.Add(new MetaStimulus
        {
            name = "OKR Suppression 3",
            driver = MetaStimulus.OKRDriver.Dots,
            scotoma = MetaStimulus.Scotoma.Fixation,
            movementDirection = MetaStimulus.Direction.Up,
            unidirectional = true,
            fixate = true,
            duration = 40f,
            calibNeeded = false

        });
    }

}
