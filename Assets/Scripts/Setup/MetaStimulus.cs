using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MetaStimulus
{
    public string name;
    public enum OKRDriver { Dots, Unidirectional, COBRA, TumblingE };
    public OKRDriver driver = OKRDriver.Dots;
    public enum Direction { Up, Diagonal, Horizontal, Down }
    public Direction movementDirection = Direction.Up;
    public bool unidirectionalDots = false; // For unidirectional dots stimulus (for ramping scotomas)
    public int numRepetitions = 1; // For number of repetitions of unidirectional dots stimulus
    public enum Contrast { Low, High, Cycle }
    public Contrast contrast = Contrast.High;

    public enum Scotoma { None, Cone, Central, Peripheral, Grid };
    public Scotoma scotoma = Scotoma.None;
    public bool fixationPoint = false; // For OKR Suppression stimulus

    public float duration; // in seconds
    public int repetitionLimit;
    public bool calibNeeded;

}