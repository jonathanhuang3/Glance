using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DevCyclicOrder", menuName = "ScriptableObjects/Cyclic Order for Dev", order = 1)]
public class DevCyclicOrder : ExperimentOrder
{
    public DevCyclicOrder()
    {
        MetaStimuli[0] = new MetaStimulus
        {
            name = "Stimulus 1",
            driver = MetaStimulus.OKRDriver.TumblingE,
            scotoma = MetaStimulus.Scotoma.Cone,
            repetitionLimit = 15,
            calibNeeded = false
        };

        MetaStimuli[1] = new MetaStimulus
        {
            name = "Stimulus 2",
            driver = MetaStimulus.OKRDriver.Dots,
            movementDirection = MetaStimulus.Direction.Up,
            unidirectionalDots = true,
            contrast = MetaStimulus.Contrast.High,
            scotoma = MetaStimulus.Scotoma.Cone,
            duration = 100f, //423f
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