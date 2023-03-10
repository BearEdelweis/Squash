using Godot;
using System;

public class Main : Node
{
    [Export] public PackedScene mobScene;
    private Control rect;


    public override void _Ready()
    {
        GD.Randomize();
        rect = GetNode<Control>("Control/ColorRect");
        rect.Hide();
    }

    public override void _Input(InputEvent iE)
    {
        if((iE is InputEventScreenTouch sT) && rect.Visible)
        {
            GetTree().ReloadCurrentScene();
        }
    }

    /*public override void _UnhandledInput(InputEvent @event)
    {
        if(@event.IsActionPressed("ui_accept") && GetNode<Control>("Control/ColorRect").Visible)
        {
            GetTree().ReloadCurrentScene();
        }
    }*/

    public void OnPlayerHit()
    {
        GetNode<Timer>("Timer").Stop();
        GetNode<Control>("Control/ColorRect").Show();
    }

    public void OnMobTimerTimeout()
    {
        Mob mob = (Mob)mobScene.Instance();

        PathFollow mobSpawnLocation = GetNode<PathFollow>("Path/PathFollow");
        mobSpawnLocation.UnitOffset = GD.Randf();

        Vector3 playerPosition = GetNode<KinematicBody_Player>("KinematicBody_Player").Transform.origin;

        mob.Initialize(mobSpawnLocation.Translation, playerPosition);

        AddChild(mob);

        mob.Connect(nameof(Mob.Squashed), GetNode<LabelScore>("Control/Label_Score"), nameof(LabelScore.OnMobSquashed));
    }
}
