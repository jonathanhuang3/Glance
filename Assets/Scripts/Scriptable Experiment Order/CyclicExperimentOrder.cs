using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CyclicExperimentOrder", menuName = "ScriptableObjects/Cyclic Experiment Order", order = 1)]
public class CyclicExperimentOrder : ExperimentOrder
{
    public CyclicExperimentOrder()
    {
        MetaStimuli[1] = new MetaStimulus
        {
            name = "Stimulus 2",
            driver = MetaStimulus.OKRDriver.Dots,
            movementDirection = MetaStimulus.Direction.Up,
            contrast = MetaStimulus.Contrast.Cycle,
            scotoma = MetaStimulus.Scotoma.None,
            duration = 220f,
            calibNeeded = false
        };

        MetaStimuli[3] = new MetaStimulus
        {
            name = "Stimulus 4",
            driver = MetaStimulus.OKRDriver.Dots,
            movementDirection = MetaStimulus.Direction.Up,
            contrast = MetaStimulus.Contrast.Cycle,
            scotoma = MetaStimulus.Scotoma.None,
            duration = 190f,
            calibNeeded = false
        };
    }

}