using Godot;
using PetEnums;
using System;
using System.Collections.Generic;

// emo值组件，在这里控制弹出对话框、随机事件、状态变化等
public partial class EmoComponent : Node
{
    private string[] walkaroundFronts = new string[]
    {
        "今天也是充满希望的一天",
        "活过来了",
        "啦啦啦",
        "为什么打虎的不是我呢",
        "好想去旅游啊",
        "今天天气真好",
        "好开心，捡到钱了",
        "我发现自己变好看了呢"
    };
    private string[] walkaroundNegativies = new string[]
    {
        "我受伤了",
        "拉肚子了",
        "好饿啊",
        "我好像病了",
         "只有我自己知道孤独的滋味",
        "曾经的快乐变在回忆中独自徘徊",
        "没有人愿意走进我的内心深处",
        "程序永远是程序，没有人类的温度"
    };
    private string[] hinderFronts = new string[]
    {
        "今天别工作了，陪我一起玩吧",
        "工作做完了没？没做完也先停一下，陪我一会儿",
        "我想和你一起放松，别再工作了，好吗",
        "你这么忙，别工作了，赶紧去陪我玩吧",
        "别工作了，陪我玩会吧"
    };
    private string[] runawayNegativies = new string[]
    {
        "滚开",
        "你已经不关心我了",
        "我讨厌你",
        "我们好像没有那么熟吧",
        "你有什么企图吗"
    };

    private Mima mima;
    private DialogManager dialogManager;

    public override void _Ready()
    {
        mima = GetParent<Mima>();
        dialogManager = mima.GetNode<DialogManager>("DialogManager");

        mima.MimaStateChanged += OnMimaStateChanged;
    }

    private void OnMimaStateChanged(ActionState state)
    {
        switch (state)
        {
            case ActionState.WalkAround:
                DoWalkAround();
                break;
            case ActionState.Hinder:
                DoHinder();
                break;
            case ActionState.RunAway:
                DoRunAway();
                break;
            default: break;
        }
    }

    private int GetRnadomChange(int min, int max)
    {
        var random = GD.RandRange(0f, 1f) > 0.5f;
        var value = GD.RandRange(min, max);
        return random ? value : -value;
    }

    private void DoWalkAround()
    {
        // 每次游荡会随机减少或者增加[1-3]：情绪、亲密、健康
        float changeHealth = GetRnadomChange(1, 3);
        mima.Health += changeHealth;
        float changeMood = GetRnadomChange(1, 3);
        mima.Mood += changeMood;
        float changeIntimacy = GetRnadomChange(1, 3);
        mima.Intimacy += changeIntimacy;
        // 根据变化值生成对话，然后调用对话框，目前简单实现吧
        string msg;
        if (changeHealth + changeMood + changeIntimacy > 0)
        {
            msg = walkaroundFronts[GD.RandRange(0, walkaroundFronts.Length - 1)];
        }
        else
        {
            msg = walkaroundNegativies[GD.RandRange(0, walkaroundNegativies.Length - 1)];
        }
        var textboxPos = new Vector2(mima.GlobalPosition.X, mima.GlobalPosition.Y - mima.CollisionSize.Y / 2);
        dialogManager.StartDialog(textboxPos, new List<string>() { msg });
    }

    private void DoHinder()
    {
        // 每次捣乱会随机减少[2-4]：情绪、健康，增加或减少[1-3]：亲密
        mima.Health -= GD.RandRange(2, 4);
        mima.Mood -= GD.RandRange(2, 4);
        mima.Intimacy += GetRnadomChange(1, 3);
        // 随机对话
        string msg = hinderFronts[GD.RandRange(0, hinderFronts.Length - 1)];
        var textboxPos = new Vector2(mima.GlobalPosition.X, mima.GlobalPosition.Y - mima.CollisionSize.Y / 2);
        dialogManager.StartDialog(textboxPos, new List<string>() { msg });
    }

    private void DoRunAway()
    {
        // 每次逃离会随机减少或者增加[5-10]：亲密、情绪
        float changeMood = GetRnadomChange(5, 10);
        mima.Mood += changeMood;
        float changeIntimacy = GetRnadomChange(5, 10);
        mima.Intimacy += changeIntimacy;
        // 根据变化值生成对话，然后调用对话框，这里应该接入ChatGPT，目前简单实现吧
        string msg = runawayNegativies[GD.RandRange(0, runawayNegativies.Length - 1)];
        var textboxPos = new Vector2(mima.GlobalPosition.X, mima.GlobalPosition.Y - mima.CollisionSize.Y / 2);
        dialogManager.StartDialog(textboxPos, new List<string>() { msg });
    }

    private void DoSleep()
    {
        // 每次睡觉会增加[1-5]：健康、情绪
    }

    private void DoEat()
    {
        // 每次吃饭会增加或者减少[2-6]：健康、亲密、情绪
    }

    private void DoPlayWith()
    {
        // 每次陪玩会增加[1-6]：亲密、情绪，减少[1-3]：健康
    }

}
