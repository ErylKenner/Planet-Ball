using ClientServerPrediction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedPhysics : IRunnable
{
    public void Run(RunContext runContext)
    {
        Physics2D.Simulate(runContext.dt);
    }

}
