using Godot;
using System;
using PetEnums;

public partial class RunAway : Node, IBehavior
{
    private string BehaviorName => "RunAway";

    private Mima mima;

    public string GetName()
    {
        return BehaviorName;
    }

    public void Initialize()
    {
        mima = GetTree().CurrentScene.GetNode<Mima>("%Mima");
    }

    public void Execute()
    {
        mima.SetAction(ActionState.RunAway);
    }

}
