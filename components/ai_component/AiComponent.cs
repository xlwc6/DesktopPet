using Godot;
using System;
using System.Collections.Generic;
using PetEnums;

[GlobalClass]
public partial class AiComponent : Node
{
    [Export]
    public float IntervalTick = 10f; // 执行间隔，单位秒
    [Export]
    public Godot.Collections.Array<UtilityAIOption> Options; // 决策配置
    public Godot.Collections.Dictionary Context = new();

    private Dictionary<string, IBehavior> behaviorsDict = new();
    private Mima mima;

    public override void _Ready()
    {
        mima = GetParent<Mima>();
        mima.MimaPropertyChanged += OnMimaPropertyChanged;
        // 初始化行为列表
        for (int i = 0; i < GetChildCount(); i++)
        {
            var child = GetChild(i);
            if (child is IBehavior behavior)
            {
                string lowerName = behavior.GetName().ToLower();
                if (!behaviorsDict.ContainsKey(lowerName))
                {
                    behavior.Initialize();
                    behaviorsDict.Add(lowerName, behavior);
                }
            }
        }
        // 初始化AI
        foreach (var option in Options)
        {
            option.Context = Context;
        }
        // 初始化条件
        Context["health"] = mima.Health / 100f;
        Context["mood"] = mima.Mood / 100f;
        Context["intimacy"] = mima.Intimacy / 100f;
    }

    public void GetNetAction()
    {
        // 这里的行为逻辑有点问题，不想改了，所有后续行为只能在Idle下才能执行，即其他行为执行完毕
        if (mima.State != ActionState.Idle)
        {
            return;
        }
        string bestAction = "walkaround";
        var bestOption = UtilityAI.ChooseHighest(Options);
        if (bestOption != null)
        {
            bestAction = bestOption.Action["Name"].As<string>();
        }
        GD.Print($"选择行为{bestAction}");
        HandleAction(bestAction);
    }

    private void HandleAction(string name)
    {
        if (behaviorsDict.ContainsKey(name))
        {
            behaviorsDict[name].Execute();
        }
    }

    private void OnMimaPropertyChanged(string arg1, float arg2)
    {
        SetProperty(arg1, arg2);
    }

    private void SetProperty(string name, float value)
    {
        switch (name)
        {
            case "health":
                Context["health"] = value / 100f;
                break;
            case "mood":
                Context["mood"] = value / 100f;
                break;
            case "intimacy":
                Context["intimacy"] = value / 100f;
                break;
            default: break;
        }
    }

}
