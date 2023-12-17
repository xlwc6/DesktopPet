using Godot;
using System;

public partial class AiAgentDebugger : Node2D
{
    [Export]
    public Godot.Collections.Array<UtilityAIOption> Options; // 决策配置
    public Godot.Collections.Dictionary Context = new();

    private HSlider _healthBar;
    private HSlider _moodBar;
    private HSlider _intimacyBar;
    private Label _actionLabel;

    public override void _Ready()
    {
        _healthBar = GetNode<HSlider>("CanvasLayer/VBoxContainer/HealthBox/HealthSlider");
        _moodBar = GetNode<HSlider>("CanvasLayer/VBoxContainer/MoodBox/MoodSlider");
        _intimacyBar = GetNode<HSlider>("CanvasLayer/VBoxContainer/IntimacyBox/IntimacySlider");
        _actionLabel = GetNode<Label>("CanvasLayer/VBoxContainer/ChooseBox/ChooseLabel");
        // 事件
        _healthBar.ValueChanged += HealthBar_ValueChanged;
        _moodBar.ValueChanged += MoodBar_ValueChanged;
        _intimacyBar.ValueChanged += IntimacyBar_ValueChanged;
        // 初始化参数
        _healthBar.Value = 50;
        _moodBar.Value = 50;
        _intimacyBar.Value = 50;
        // 初始化AI
        foreach (var option in Options)
        {
            option.Context = Context;
        }
        // 初始化条件
        Context["health"] = .5f;
        Context["mood"] = .5f;
        Context["intimacy"] = .5f;
    }

    public override void _Process(double delta)
    {
        string bestAction = "idle";
        var bestOption = UtilityAI.ChooseHighest(Options);
        if (bestOption != null)
        {
            bestAction = bestOption.Action["Name"].As<string>();
        }
        _actionLabel.Text = "行为：" + bestAction;
    }

    private void HealthBar_ValueChanged(double value)
    {
        Context["health"] = value / 100f;
    }

    private void MoodBar_ValueChanged(double value)
    {
        Context["mood"] = value / 100f;
    }

    private void IntimacyBar_ValueChanged(double value)
    {
        Context["intimacy"] = value / 100f;
    }

}
