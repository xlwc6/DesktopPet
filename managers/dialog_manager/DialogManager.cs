using Godot;
using System;
using System.Collections.Generic;

public partial class DialogManager : Node
{
    private PackedScene textBoxScene;
    private List<string> dialogLines;
    private int currentIndex = 0;
    private Vector2 textBoxPos;
    private bool isDialogActive = false;
    private bool canAdvanceLine = false;
    private TextBox textBox;
    private Timer timer;

    public override void _Ready()
    {
        textBoxScene = GD.Load<PackedScene>("res://main/ui/TextBox.tscn");
        timer = GetNode<Timer>("AtuoLineTimer");

        timer.Timeout += OnAtuoLineTimerTimeout;
    }

    /// <summary>
    /// 显示对话框
    /// </summary>
    public void StartDialog(Vector2 position, List<string> lines)
    {
        if (isDialogActive)
        {
            return;
        }
        dialogLines = lines;
        textBoxPos = position;
        ShowTextBox();
        isDialogActive = true;
        timer.Start();
    }

    private void ShowTextBox()
    {
        textBox = textBoxScene.Instantiate<TextBox>();
        textBox.Initialize();
        textBox.FinshedDisplay += OnTextBoxFinshedDisplay;
        GetTree().CurrentScene.GetNode<Node2D>("DialogContainer").AddChild(textBox);
        textBox.GlobalPosition = textBoxPos;
        textBox.DisplayText(dialogLines[currentIndex]);
        canAdvanceLine = false;
    }

    private void OnAtuoLineTimerTimeout()
    {
        if (isDialogActive && canAdvanceLine)
        {
            textBox.QueueFree();
            currentIndex++;
            if (currentIndex >= dialogLines.Count)
            {
                isDialogActive = false;
                currentIndex = 0;
                timer.Stop();
                return;
            }
            ShowTextBox();
        }
    }

    private void OnTextBoxFinshedDisplay()
    {
        canAdvanceLine = true;
    }

}
