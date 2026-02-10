using System;
using UnityEngine;

public class MiniTankAnimation : IAnimationModule
{ // TODO: all fields related to rig might be reworked later on
    private readonly BaseController controller;
    private int animationState;
    private readonly Transform[] targets;
    private readonly Transform[] hints;

    public MiniTankAnimation(BaseController controller)
    {
        this.controller = controller;
    }

    public void Tick(){} // TODO
}