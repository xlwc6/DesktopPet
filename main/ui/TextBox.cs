using Godot;
using System;

public partial class TextBox : MarginContainer
{
    public event Action FinshedDisplay; // 完成显示

    private const int MAX_WIDTH = 160;

    private string text = "";
    private int letterIndex = 0;
    private float letterTIme = 0.03f; // 文字输入时间
    private float punctuationTime = 0.2f; // 其他输入时间

    private Label label;
    private Timer timer;

    public void Initialize()
    {
        label = GetNode<Label>("MarginContainer/Label");
        timer = GetNode<Timer>("LetterDisplayTimer");
        timer.Timeout += OnTimerTimeout;
    }

    public async void DisplayText(string msg)
    {
        text = msg;
        label.Text = msg;
        // 重新设置文本框大小
        await ToSignal(this, SignalName.Resized);
        CustomMinimumSize = new Vector2(Mathf.Min(Size.X, MAX_WIDTH), CustomMinimumSize.Y);
        if (Size.X > MAX_WIDTH)
        {
            // 中文要用这个，不然无法正确换行
            label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            await ToSignal(this, SignalName.Resized); // 等待 X 设置完成
            await ToSignal(this, SignalName.Resized); // 等待 Y 设置完成
            CustomMinimumSize = new Vector2(CustomMinimumSize.X, Size.Y);
        }
        // 调整显示位置
        GlobalPosition = new Vector2(GlobalPosition.X - Size.X / 2, GlobalPosition.Y - (Size.Y + 24));
        // 延迟显示
        label.Text = "";
        DisplayLetter();
    }

    private void DisplayLetter()
    {
        label.Text += text[letterIndex];
        letterIndex++;
        if (letterIndex >= text.Length)
        {
            FinshedDisplay?.Invoke();
            return;
        }
        // 判断是否为汉字，否则输入时间不一样
        if (text[letterIndex] >= 0x4e00 && text[letterIndex] <= 0x9fbb)
        {
            timer.Start(letterTIme);
        }
        else
        {
            timer.Start(punctuationTime);
        }
    }

    private void OnTimerTimeout()
    {
        DisplayLetter();
    }

}
