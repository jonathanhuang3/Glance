using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MetaStimulus
{
    public string name;
    public enum OKRDriver { Dots, Unidirectional, COBRA, TumblingE };
    public OKRDriver driver = OKRDriver.Dots;
    public enum Direction { Up, Diagonal, Right, Left, Down }
    public Direction movementDirection = Direction.Up;
    public bool unidirectional = false; // For unidirectional dots stimulus
    public int numRepetitions = 1; // For number of repetitions of unidirectional dots stimulus
    public enum Contrast { Low, High, Cycle }
    public Contrast contrast = Contrast.High;

    public enum Scotoma { None, Cone, Central, Peripheral, Grid, Fixation, SuperiorHemifield, InferiorHemifield };
    public Scotoma scotoma = Scotoma.None;
    public bool fixate = false; // For OKR Suppression stimulus
    public bool cycleScotoma = false; // For OKR scotoma growth/decay stimulus

    public float duration; // in seconds
    public int repetitionLimit; // Used to generate scotoma states in TumblingOptotype, or number of trials in COBRA
    public int numStates = 16; // Number of scotoma states in TumblingOptotype
    public bool saveTracking = true;
    public bool calibNeeded;

}