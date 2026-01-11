using UnityEngine;
using System.Collections.Generic;

// Test schedule for hemifield scotoma with dots stimulus.
// This schedule contains only superior and inferior hemifield dot tests.
// Dots move unidirectionally (like "High Contrast Left Unidirectional Dots 1").
[CreateAssetMenu(fileName = "HemifieldTestOrder", menuName = "ScriptableObjects/HemifieldTestOrder", order = 2)]
public class HemifieldTestOrder : ExperimentOrder
{
    public HemifieldTestOrder()
    {
        MetaStimuli = new List<MetaStimulus>();

        MetaStimuli.Add(new MetaStimulus
        {
            name = "Superior Hemifield Dots",
            driver = MetaStimulus.OKRDriver.Dots,
            movementDirection = MetaStimulus.Direction.Left,
            contrast = MetaStimulus.Contrast.High,
            scotoma = MetaStimulus.Scotoma.SuperiorHemifield,
            unidirectional = true,
            numRepetitions = 1,
            duration = 30f,
            calibNeeded = true,
            saveTracking = true
        });

        MetaStimuli.Add(new MetaStimulus
        {
            name = "Inferior Hemifield Dots",
            driver = MetaStimulus.OKRDriver.Dots,
            movementDirection = MetaStimulus.Direction.Left,
            contrast = MetaStimulus.Contrast.High,
            scotoma = MetaStimulus.Scotoma.InferiorHemifield,
            unidirectional = true,
            numRepetitions = 1,
            duration = 30f,
            calibNeeded = false,
            saveTracking = true
        });
    }
}
