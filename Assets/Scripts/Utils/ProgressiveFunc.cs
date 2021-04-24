﻿using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

public class ProgressiveFunc : IEnumerator {

    public float FrameBudgetMillis = 1f;

    readonly Stopwatch sw = new Stopwatch();
    readonly WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();

    readonly IEnumerator workRoutine;
    readonly IEnumerator progressiveRoutine;

    public ProgressiveFunc(IEnumerator workRoutine) {
        this.workRoutine = workRoutine;
        progressiveRoutine = WorkProgressively();
    }
    
    IEnumerator WorkProgressively() {
        // start timer
        sw.Start();
        // step through work
        while (workRoutine.MoveNext()) {
            // frame break if over budget or yield requests frame break
            var tickBudget = (int)(FrameBudgetMillis * 10000);
            if (sw.ElapsedTicks >= tickBudget || workRoutine.Current is WaitForEndOfFrame) {
                yield return endOfFrame;
                sw.Restart();
            } 
            // handle other non-null yields
            else if (workRoutine.Current is YieldInstruction) {
                yield return workRoutine.Current;
            }
        }
    }

    public bool MoveNext() { return progressiveRoutine.MoveNext(); }
    public void Reset() { progressiveRoutine.Reset(); }
    public object Current { get { return progressiveRoutine.Current; } }
}