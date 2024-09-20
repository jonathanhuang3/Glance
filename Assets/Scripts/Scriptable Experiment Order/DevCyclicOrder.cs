using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DevCyclicOrder", menuName = "ScriptableObjects/Cyclic Order for Dev", order = 1)]
public class DevCyclicOrder : ExperimentOrder
{
    public DevCyclicOrder()
    {
        MetaStimuli = new List<MetaStimulus>();

        // MetaStimuli.Add(new MetaStimulus
        // {
        //     name = "Cycle Contrast",
        //     driver = MetaStimulus.OKRDriver.Dots,
        //     movementDirection = MetaStimulus.Direction.Up,
        //     contrast = MetaStimulus.Contrast.Cycle,
        //     scotoma = MetaStimulus.Scotoma.None,
        //     duration = 40f,
        //     calibNeeded = false

        // });

        // MetaStimuli.Add(new MetaStimulus
        // {
        //     name = "Unidirectional Dots Fixation",
        //     driver = MetaStimulus.OKRDriver.Dots,
        //     scotoma = MetaStimulus.Scotoma.Fixation,
        //     movementDirection = MetaStimulus.Direction.Up,
        //     unidirectional = true,
        //     fixate = true,
        //     duration = 40f,
        //     calibNeeded = false

        // });
        MetaStimuli.Add(new MetaStimulus
        {
            name = "Tumbling 1",
            driver = MetaStimulus.OKRDriver.TumblingE,
            scotoma = MetaStimulus.Scotoma.Cone,
            repetitionLimit = 15,
            calibNeeded = false
        });

        MetaStimuli.Add(new MetaStimulus
        {
            name = "OKR 1",
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
            name = "OKR 2",
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
            name = "OKR 3",
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
            name = "OKR 4",
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
            name = "OKR 5",
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

        // MetaStimuli[2] = new MetaStimulus
        // {
        //     name = "Stimulus 3",
        //     driver = MetaStimulus.OKRDriver.TumblingE,
        //     scotoma = MetaStimulus.Scotoma.Cone,
        //     repetitionLimit = 15,
        //     calibNeeded = false
        // };

        // MetaStimuli[1] = new MetaStimulus
        // {
        //     name = "Stimulus 2",
        //     driver = MetaStimulus.OKRDriver.Dots,
        //     movementDirection = MetaStimulus.Direction.Up,
        //     unidirectional = true,
        //     contrast = MetaStimulus.Contrast.High,
        //     scotoma = MetaStimulus.Scotoma.Cone,
        //     duration = 190f, //423f
        //     calibNeeded = false
        // };

        // MetaStimuli[3] = new MetaStimulus
        // {
        //     name = "Stimulus 4",
        //     driver = MetaStimulus.OKRDriver.Dots,
        //     movementDirection = MetaStimulus.Direction.Up,
        //     contrast = MetaStimulus.Contrast.Cycle,
        //     scotoma = MetaStimulus.Scotoma.None,
        //     duration = 190f,
        //     calibNeeded = false
        // };
    }

}