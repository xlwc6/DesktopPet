using Godot;
using System;

public partial class Word : Node2D
{
    private Mima mima;
    private Control control;
    private Label hLabel;
    private Label mLabel;
    private Label iLabel;

    public override void _Ready()
    {
        mima = GetTree().CurrentScene.GetNode<Mima>("%Mima");
        control = GetTree().CurrentScene.GetNode<Control>("%AttributePanel");
        hLabel = GetTree().CurrentScene.GetNode<Label>("%HealthLabel");
        mLabel = GetTree().CurrentScene.GetNode<Label>("%MoodLabel");
        iLabel = GetTree().CurrentScene.GetNode<Label>("%IntimacyLabel");

        mima.MimaPropertyChanged += OnMimaPropertyChanged;

        hLabel.Text = mima.Health.ToString();
        mLabel.Text = mima.Mood.ToString();
        iLabel.Text = mima.Intimacy.ToString();
    }

    private void OnMimaPropertyChanged(string arg1, float arg2)
    {
        switch (arg1)
        {
            case "health":
                hLabel.Text = arg2.ToString();
                break;
            case "mood":
                mLabel.Text = arg2.ToString();
                break;
            case "intimacy":
                iLabel.Text = arg2.ToString();
                break;
            default: break;
        }
    }

}
