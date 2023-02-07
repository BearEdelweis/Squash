using Godot;
using System;

public class Mob : KinematicBody
{
    [Signal] public delegate void Squashed();
    
    [Export] private int minSpeed = 10;
    [Export] private int maxSpeed = 18;
    private Vector3 velocity = Vector3.Zero;

    public override void _Ready()
    {
        
    }

    public void Squash()
    {
        EmitSignal(nameof(Squashed));
        QueueFree();
    }

    public void Initialize(Vector3 startPosition, Vector3 playerPosition)
    {
        LookAtFromPosition(startPosition, playerPosition, Vector3.Up); //посмотреть в направлении
        RotateY((float)GD.RandRange(-Mathf.Pi/4,Mathf.Pi/4)); //немного отклониться

        float randomSpeed = (float)GD.RandRange(minSpeed, maxSpeed);
        velocity = Vector3.Forward*randomSpeed;
        velocity = velocity.Rotated(Vector3.Up, Rotation.y);

        GetNode<AnimationPlayer>("AnimationPlayer").PlaybackSpeed = randomSpeed/minSpeed;
    }

    public override void _PhysicsProcess(float delta)
    {
        MoveAndSlide(velocity);
    }

    public void OnVisibilityNotifierScreenExited()
    {
        QueueFree();
    }
}
