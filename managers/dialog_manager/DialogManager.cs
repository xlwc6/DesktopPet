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
        // 这里还有一种是弹出popup显示对话和属性面板，但是那样的话得考虑子窗口跟随问题
        // 由于一开始我就没打算采用弹出窗口，所以都是在Mina那个类里重新定位人物得点和边界等，让试图窗口大的足够装下我的面板
        textBox = textBoxScene.Instantiate<TextBox>();
        textBox.Initialize();
        textBox.FinshedDisplay += OnTextBoxFinshedDisplay;
        GetTree().CurrentScene.GetNode<Control>("DialogPanel").AddChild(textBox);
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
