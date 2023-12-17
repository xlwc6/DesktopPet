using Godot;
using System;
using PetEnums;

public partial class Hinder : Node, IBehavior
{
    private string BehaviorName => "Hinder";

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
        mima.SetAction(ActionState.Hinder);
    }

}
