using Godot;
using System;
using PetEnums;
using System.Collections.Generic;

public partial class Mima : CharacterBody2D
{
    #region 事件

    public event Action MimaDead; // 死了
    public event Action<string, float> MimaPropertyChanged; // 状态变化
    public event Action<ActionState> MimaStateChanged; // 状态改变

    #endregion

    #region 属性

    [Export(PropertyHint.Range, "0,100,1")]
    protected float health = 50; // 健康值
    public float Health
    {
        get => health;
        set
        {
            health = Mathf.Clamp(value, 0f, 100f);
            if (health <= 0)
            {
                MimaDead?.Invoke();
            }
            else
            {
                MimaPropertyChanged?.Invoke("health", health);
            }
        }
    }

    [Export(PropertyHint.Range, "0,100,1")]
    protected float mood = 50; // 情绪值
    public float Mood
    {
        get => mood;
        set
        {
            mood = Mathf.Clamp(value, 0f, 100f);
            MimaPropertyChanged?.Invoke("mood", mood);
        }
    }

    [Export(PropertyHint.Range, "0,100,1")]
    protected float intimacy = 50; // 亲密值
    public float Intimacy
    {
        get => intimacy;
        set
        {
            intimacy = Mathf.Clamp(value, 0f, 100f);
            MimaPropertyChanged?.Invoke("intimacy", intimacy);
        }
    }

    #endregion

    #region 状态控制

    public ActionState State { get => actionState; }
    public bool is_moving = false; // 是否移动
    public bool is_running = false; // 是否奔跑
    protected bool is_follow = false; // 是否跟随鼠标

    private float Speed = 100; // 移动速度，奔跑时速度为3倍
    private Vector2 screen;
    private Vector2 collisionSize; // 检测区域大小
    private Rect2I boundRect; // 边界
    private bool is_dragging = false; // 是否拖动
    private Vector2 MouseInPos; // 开始拖动的坐标
    private Vector2 MousePos; // 拖动时坐标
    private Vector2 bodyVelocity;
    private Vector2 direction; // 移动方向
    private ActionState actionState = ActionState.Idle; // 初始状态
    private int tickNumber = 0; // 计时器执行次数
    private int wanderTime = 4; // 游荡时间
    private int leaveDistance; // 远离距离 

    #endregion

    private AiComponent aiComponent;
    private AnimationTree animationTree;
    private Timer aiTimer;

    public override void _Ready()
    {
        aiComponent = GetNode<AiComponent>("AiComponent");
        animationTree = GetNode<AnimationTree>("AnimationTree");
        aiTimer = GetNode<Timer>("AiTimer");
        screen = DisplayServer.ScreenGetSize();
        collisionSize = GetWindow().Size; // 按窗口大小，因为我们使用popup来显示额外的内容，所以窗口就是人物大小
        // collisionSize = GetNode<CollisionShape2D>("CollisionShape2D").Shape.GetRect().Size; // 按角色的碰撞区域， 当视窗里需要放其他东西的时候用这个
        GD.Print(collisionSize);
        boundRect = new(0, 0, (int)(screen.X - collisionSize.X), (int)(screen.Y - collisionSize.Y));
        // 启动AI
        aiTimer.Timeout += OnAiTimerTimeout;
        aiTimer.Start();
        aiComponent.GetNetAction();
    }

    public override void _Input(InputEvent @event)
    {
        // 按下鼠标
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
            {
                is_dragging = true;
                MouseInPos = GetViewport().GetMousePosition();
            }
            else
            {
                is_dragging = false;
            }
            // 暂时设置为右键打印属性
            if (mouseButton.ButtonIndex == MouseButton.Right && mouseButton.Pressed)
            {
                GD.Print($"健康：{Health}，情绪：{Mood}，亲密：{Intimacy}");
            }
        }
        // 按下鼠标拖动中
        if (@event is InputEventMouseMotion && is_dragging)
        {
            MousePos = GetViewport().GetMousePosition();
            GetTree().Root.Position += (Vector2I)(MousePos - MouseInPos);
        }
        // 退出软件
        if (@event is InputEventKey && @event.IsActionPressed("quit"))
        {
            GetTree().Quit();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        bodyVelocity = Vector2.Zero;
        // 如果是跟随鼠标，则每次运行都重新获取与鼠标的相对方向
        if (is_follow)
        {
            direction = GetDistanceToMouse();
        }
        if (direction != Vector2.Zero)
        {
            if (is_moving)
            {
                bodyVelocity = direction * Speed * (float)delta;
            }
            else if (is_running)
            {
                bodyVelocity = direction * Speed * 3 * (float)delta;

            }
            animationTree.Set("parameters/Idle/blend_position", bodyVelocity.Normalized());
            animationTree.Set("parameters/Walking/blend_position", bodyVelocity.Normalized());
            animationTree.Set("parameters/Running/blend_position", bodyVelocity.Normalized());
            Move();
        }
    }

    private void OnAiTimerTimeout()
    {
        tickNumber++;
        if (tickNumber >= aiComponent.IntervalTick)
        {
            tickNumber = 0;
            aiComponent.GetNetAction();
        }
        CheckedAction();
    }

    #region 行为相关

    private void Move()
    {
        var windowPos = DisplayServer.WindowGetPosition() + (Vector2I)bodyVelocity;
        // 边界判断，如果到达边界立马停止动作
        if (!boundRect.HasPoint(windowPos))
        {
            SetIdle();
            GD.Print("触碰到边界,强行结束,进入idle");
            return;
        }
        GetTree().Root.Position = windowPos;
    }

    private Vector2 GetDistanceToMouse()
    {
        // 当前位置
        var currentPos = DisplayServer.WindowGetPosition() + (Vector2I)Position;
        // 获取鼠标位置
        var targetPos = DisplayServer.MouseGetPosition();
        // 这里的模长判断比check的小一点，避免移动不到位
        var posMode = targetPos - currentPos;
        if (posMode.Length() <= 2)
        {
            return Vector2.Zero;
        }
        // 计算向量
        Vector2 vector = new(targetPos.X - currentPos.X, targetPos.Y - currentPos.Y);
        return vector.Normalized();
    }

    public void SetAction(ActionState action)
    {
        GD.Print($"开始处理{action}");
        actionState = action;
        MimaStateChanged?.Invoke(actionState);
        if (action == ActionState.WalkAround)
        {
            is_moving = true;
            // 随机获取一个方向移动
            direction = GetRandomDirection();
            // 随机获取游荡时间
            wanderTime = GD.RandRange(3, 7);
            GD.Print($"计划游荡{wanderTime}秒");
        }
        if (action == ActionState.Hinder)
        {
            // 获取鼠标位置
            var targetPos = DisplayServer.MouseGetPosition();
            // 判断目标位置是否可达
            if (!boundRect.HasPoint(targetPos))
            {
                SetIdle();
                GD.Print("目标位置不可达,强行结束hinder,进入idle");
                return;
            }
            is_running = true;
            is_follow = true;
            GD.Print($"开始向鼠标位置移动");
        }
        if (action == ActionState.RunAway)
        {
            // 当前位置
            var currentPos = (Vector2)DisplayServer.WindowGetPosition();
            // 获取鼠标位置
            var targetPos = (Vector2)DisplayServer.MouseGetPosition();
            // 随机获取远离距离
            leaveDistance = GD.RandRange(100, 300);
            GD.Print($"计划远离{leaveDistance}");
            // 获取距离
            var distance = targetPos.DistanceTo(currentPos);
            if (distance < leaveDistance)
            {
                is_running = true;
                // 随机获取一个方向移动
                direction = GetRandomDirection();
            }
        }
    }

    private void CheckedAction()
    {
        // 根据行为来检查是否执行结束，然后把动画设置为Idle
        if (actionState == ActionState.WalkAround)
        {
            if (tickNumber >= wanderTime)
            {
                SetIdle();
                GD.Print("walkaround结束,进入idle");
            }
        }
        else if (actionState == ActionState.Hinder)
        {
            // 当前位置
            var currentPos = DisplayServer.WindowGetPosition() + (Vector2I)Position;
            // 获取鼠标位置
            var targetPos = DisplayServer.MouseGetPosition();
            // 判断是否到达了鼠标位置，这里根据模长计算准确性高点
            var posMode = targetPos - currentPos;
            if (posMode.Length() <= 5)
            {
                SetIdle();
                GD.Print("hinder结束,进入idle");
            }
        }
        else if (actionState == ActionState.RunAway)
        {
            // 当前位置
            var currentPos = (Vector2)DisplayServer.WindowGetPosition();
            // 获取鼠标位置
            var targetPos = (Vector2)DisplayServer.MouseGetPosition();
            // 获取距离
            var distance = targetPos.DistanceTo(currentPos);
            if (distance > leaveDistance)
            {
                SetIdle();
                GD.Print("runaway结束,进入idle");
            }
        }
    }

    private void SetIdle()
    {
        is_moving = false;
        is_running = false;
        is_follow = false;
        actionState = ActionState.Idle;
        leaveDistance = 0;
        MimaStateChanged?.Invoke(actionState);
        direction = Vector2.Zero;
    }

    private Vector2 GetRandomDirection()
    {
        // 只有上下左右4个方向移动即 -1,0  1,0  0,-1  0,1
        List<Vector2> allDirection = new()
        {
            new Vector2(-1, 0), // 左
            new Vector2(1, 0),  // 右
            new Vector2(0, 1),  // 下
            new Vector2(0, -1)  // 上
        };
        // 根据边界判断方向, 即在下方时只有左, 上, 右可以移动
        var currentPos = GetTree().Root.Position;
        if (currentPos.X - collisionSize.X <= 10)
        {
            allDirection.Remove(new Vector2(-1, 0));
        }
        if (currentPos.X + collisionSize.X >= screen.X - 10)
        {
            allDirection.Remove(new Vector2(1, 0));
        }
        if (currentPos.Y - collisionSize.Y <= 10)
        {
            allDirection.Remove(new Vector2(0, -1));
        }
        if (currentPos.Y + collisionSize.Y >= screen.Y - 10)
        {
            allDirection.Remove(new Vector2(0, 1));
        }
        // 随机获取一个
        var randomIndex = GD.RandRange(0, allDirection.Count - 1);
        return allDirection[randomIndex];
    }

    #endregion
}
