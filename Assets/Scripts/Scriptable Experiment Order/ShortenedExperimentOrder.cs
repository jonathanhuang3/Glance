using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ShortExperimentOrder", menuName = "ScriptableObjects/ShortenedExperimentOrder", order = 1)]
public class ShortenedExperimentOrder : ExperimentOrder
{
    public ShortenedExperimentOrder()
    {
        MetaStimuli.Clear();
        MetaStimuli.AddRange(
            new List<MetaStimulus>
            {
                new MetaStimulus
                {
                    name = "Stimulus 1",
                    driver = MetaStimulus.OKRDriver.Dots,
                    movementDirection = MetaStimulus.Direction.Up,
                    contrast = MetaStimulus.Contrast.High,
                    scotoma = MetaStimulus.Scotoma.None,
                    duration = 20f,
                    calibNeeded = false
                },
                new MetaStimulus
                {
                    name = "Stimulus 2",
                    driver = MetaStimulus.OKRDriver.Dots,
                    movementDirection = MetaStimulus.Direction.Diagonal,
                    contrast = MetaStimulus.Contrast.Low,
                    scotoma = MetaStimulus.Scotoma.None,
                    duration = 20f,
                    calibNeeded = false
                },
                new MetaStimulus
                {
                    name = "Stimulus 3",
                    driver = MetaStimulus.OKRDriver.Dots,
                    movementDirection = MetaStimulus.Direction.Up,
                    contrast = MetaStimulus.Contrast.High,
                    scotoma = MetaStimulus.Scotoma.Cone,
                    duration = 20f,
                    calibNeeded = false
                },
                new MetaStimulus
                {
                    name = "Stimulus 4",
                    driver = MetaStimulus.OKRDriver.Dots,
                    movementDirection = MetaStimulus.Direction.Up,
                    contrast = MetaStimulus.Contrast.Low,
                    scotoma = MetaStimulus.Scotoma.None,
                    duration = 20f,
                    calibNeeded = false
                },
                new MetaStimulus
                {
                    name = "Stimulus 5",
                    driver = MetaStimulus.OKRDriver.Dots,
                    movementDirection = MetaStimulus.Direction.Right,
                    contrast = MetaStimulus.Contrast.High,
                    scotoma = MetaStimulus.Scotoma.None,
                    duration = 20f,
                    calibNeeded = false
                },
                new MetaStimulus
                {
                    name = "Stimulus 6",
                    driver = MetaStimulus.OKRDriver.Dots,
                    movementDirection = MetaStimulus.Direction.Up,
                    contrast = MetaStimulus.Contrast.High,
                    scotoma = MetaStimulus.Scotoma.Central,
                    duration = 20f,
                    calibNeeded = false
                },
                new MetaStimulus
                {
                    name = "Stimulus 7",
                    driver = MetaStimulus.OKRDriver.Dots,
                    movementDirection = MetaStimulus.Direction.Diagonal,
                    contrast = MetaStimulus.Contrast.High,
                    scotoma = MetaStimulus.Scotoma.None,
                    duration = 20f,
                    calibNeeded = false
                },
                new MetaStimulus
                {
                    name = "Stimulus 8",
                    driver = MetaStimulus.OKRDriver.Dots,
                    movementDirection = MetaStimulus.Direction.Up,
                    contrast = MetaStimulus.Contrast.High,
                    scotoma = MetaStimulus.Scotoma.None,
                    duration = 20f,
                    calibNeeded = false
                },
                new MetaStimulus
                {
                    name = "Stimulus 9",
                    driver = MetaStimulus.OKRDriver.Dots,
                    movementDirection = MetaStimulus.Direction.Up,
                    contrast = MetaStimulus.Contrast.High,
                    scotoma = MetaStimulus.Scotoma.Peripheral,
                    duration = 20f,
                    calibNeeded = false
                },
                new MetaStimulus
                {
                    name = "Stimulus 10",
                    driver = MetaStimulus.OKRDriver.Unidirectional,
                    movementDirection = MetaStimulus.Direction.Up,
                    contrast = MetaStimulus.Contrast.High,
                    scotoma = MetaStimulus.Scotoma.None,
                    duration = 10f,
                    calibNeeded = true,
                    fixate = false
                },
                new MetaStimulus
                {
                    name = "Stimulus 11",
                    driver = MetaStimulus.OKRDriver.Unidirectional,
                    movementDirection = MetaStimulus.Direction.Down,
                    contrast = MetaStimulus.Contrast.High,
                    scotoma = MetaStimulus.Scotoma.None,
                    duration = 10f,
                    calibNeeded = false,
                    fixate = false
                },
                new MetaStimulus
                {
                    name = "Stimulus 12",
                    driver = MetaStimulus.OKRDriver.Unidirectional,
                    movementDirection = MetaStimulus.Direction.Up,
                    contrast = MetaStimulus.Contrast.High,
                    scotoma = MetaStimulus.Scotoma.None,
                    duration = 10f,
                    calibNeeded = false,
                    fixate = true
                },
                new MetaStimulus
                {
                    name = "Stimulus 13",
                    driver = MetaStimulus.OKRDriver.Unidirectional,
                    movementDirection = MetaStimulus.Direction.Down,
                    contrast = MetaStimulus.Contrast.High,
                    scotoma = MetaStimulus.Scotoma.None,
                    duration = 10f,
                    calibNeeded = false,
                    fixate = true
                },
                new MetaStimulus
                {
                    name = "Stimulus 14",
                    driver = MetaStimulus.OKRDriver.COBRA,
                    movementDirection = MetaStimulus.Direction.Up,
                    contrast = MetaStimulus.Contrast.High,
                    scotoma = MetaStimulus.Scotoma.None,
                    duration = 10f,
                    calibNeeded = false
                }
            }
        );
    }


    // List<(string name, MetaStimulus.OKRDriver driver, MetaStimulus.Direction direction, MetaStimulus.Contrast contrast, MetaStimulus.Scotoma scotoma, float duration, bool calibNeeded)> stimuliData = new List<(string, MetaStimulus.OKRDriver, MetaStimulus.Direction, MetaStimulus.Contrast, MetaStimulus.Scotoma, float, bool)>
    // {
    //     ("Stimulus 1", MetaStimulus.OKRDriver.Dots, MetaStimulus.Direction.Up, MetaStimulus.Contrast.High, MetaStimulus.Scotoma.None, 190f, true),
    //     ("Stimulus 2", MetaStimulus.OKRDriver.Dots, MetaStimulus.Direction.Diagonal, MetaStimulus.Contrast.Low, MetaStimulus.Scotoma.None, 190f, false),
    //     ("Stimulus 3", MetaStimulus.OKRDriver.Dots, MetaStimulus.Direction.Up, MetaStimulus.Contrast.High, MetaStimulus.Scotoma.Cone, 190f, false),
    //     ("Stimulus 4", MetaStimulus.OKRDriver.Dots, MetaStimulus.Direction.Up, MetaStimulus.Contrast.Low, MetaStimulus.Scotoma.None, 190f, false),
    //     ("Stimulus 5", MetaStimulus.OKRDriver.Dots, MetaStimulus.Direction.Right, MetaStimulus.Contrast.High, MetaStimulus.Scotoma.None, 190f, true),
    //     ("Stimulus 6", MetaStimulus.OKRDriver.Dots, MetaStimulus.Direction.Up, MetaStimulus.Contrast.High, MetaStimulus.Scotoma.Central, 190f, false),
    //     ("Stimulus 7", MetaStimulus.OKRDriver.Dots, MetaStimulus.Direction.Diagonal, MetaStimulus.Contrast.High, MetaStimulus.Scotoma.None, 190f, false),
    //     ("Stimulus 8", MetaStimulus.OKRDriver.Dots, MetaStimulus.Direction.Up, MetaStimulus.Contrast.High, MetaStimulus.Scotoma.None, 190f, false),
    //     ("Stimulus 9", MetaStimulus.OKRDriver.Dots, MetaStimulus.Direction.Up, MetaStimulus.Contrast.High, MetaStimulus.Scotoma.Peripheral, 190f, false),
    //     ("Stimulus 10", MetaStimulus.OKRDriver.Unidirectional, MetaStimulus.Direction.Up, MetaStimulus.Contrast.High, MetaStimulus.Scotoma.None, 25f, true),
    //     ("Stimulus 11", MetaStimulus.OKRDriver.Unidirectional, MetaStimulus.Direction.Down, MetaStimulus.Contrast.High, MetaStimulus.Scotoma.None, 25f, false),
    //     ("Stimulus 12", MetaStimulus.OKRDriver.Unidirectional, MetaStimulus.Direction.Up, MetaStimulus.Contrast.High, MetaStimulus.Scotoma.None, 25f, false),
    //     ("Stimulus 13", MetaStimulus.OKRDriver.Unidirectional, MetaStimulus.Direction.Down, MetaStimulus.Contrast.High, MetaStimulus.Scotoma.None, 25f, false),
    //     ("Stimulus 14", MetaStimulus.OKRDriver.COBRA, MetaStimulus.Direction.Up, MetaStimulus.Contrast.High, MetaStimulus.Scotoma.None, 300f, false)
    // };
    // public void AddMultipleMetaStimuli(List<(string name, MetaStimulus.OKRDriver driver, MetaStimulus.Direction direction, MetaStimulus.Contrast contrast, MetaStimulus.Scotoma scotoma, float duration, bool calibNeeded)> stimuliData)
    // {
    //     foreach (var data in stimuliData)
    //     {
    //         MetaStimulus stimulus = new MetaStimulus
    //         {
    //             name = data.name,
    //             driver = data.driver,
    //             movementDirection = data.direction,
    //             contrast = data.contrast,
    //             scotoma = data.scotoma,
    //             duration = data.duration,
    //             calibNeeded = data.calibNeeded
    //         };

    //         MetaStimuli.Add(stimulus);
    //     }
    // }
}