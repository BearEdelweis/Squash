using Godot;
using System;

public class KinematicBody_Player : KinematicBody
{
    [Signal] public delegate void Hit();

    [Export] private int fallAcceleration; //имитация свободного падения
    [Export] private int speed; //скорость игрока
    [Export] private int jumpImpuls;
    [Export] private int bounceImpuls;
    private Vector3 velocity; //направление движения
    private Spatial spatialPivot;
    
    public override void _Ready()
    {
        fallAcceleration = 75;
        speed = 14;
        jumpImpuls = 20;
        bounceImpuls = 16;
        velocity = Vector3.Zero;
        spatialPivot = GetNode<Spatial>("Spatial_Pivot");
    }

    private void Die()
    {
        EmitSignal(nameof(Hit));
        QueueFree();
    }

    private void OnAreaBodyEntered(Node body)
    {
        Die();
    }


    public override void _PhysicsProcess(float delta)
    {
        Vector3 direction = Vector3.Zero;

        if (Input.IsActionPressed("ui_left")) direction.x -= 1;
        if (Input.IsActionPressed("ui_right")) direction.x += 1;
        if (Input.IsActionPressed("ui_down")) direction.z += 1;
        if (Input.IsActionPressed("ui_up")) direction.z -= 1;

        if(direction!=Vector3.Zero)
        {
            direction = direction.Normalized();
            spatialPivot.LookAt(Translation + direction, Vector3.Up);
        }

        velocity.x = direction.x * speed;
        velocity.z = direction.z * speed;
        velocity.y -= fallAcceleration * delta;//персонаж должен постоянно немножко долбиться об пол :-)
                                               //потому что столкновения определяются только в движении

        if (IsOnFloor() && Input.IsActionJustPressed("ui_jump"))
        {
            velocity.y += jumpImpuls;
        }
        
        velocity = MoveAndSlide(velocity,Vector3.Up);//сглаживает столкновения, сбрасывая скорость, чтобы не дать
                                                     //персонажу "пробить" пол, или стены и т.п.

        for(int i=0;i<GetSlideCount();i++)
        {
            KinematicCollision collision = GetSlideCollision(i); //все столкновения кадра
            if(collision.Collider is Mob mob && mob.IsInGroup("mob_group"))
            {
                if (Vector3.Up.Dot(collision.Normal) > 0.1f) //если касание сверху
                {
                    //сквош и оттолкнуться
                    mob.Squash();
                    velocity.y = bounceImpuls;
                    GD.Print("Monster killed!");
                }
                else
                {
                    GD.Print("Fail.Player died.");
                }
            }
        }
        if(direction != Vector3.Zero)
        {
            GetNode<AnimationPlayer>("AnimationPlayer").PlaybackSpeed = 4;
        }
        else
        {
            GetNode<AnimationPlayer>("AnimationPlayer").PlaybackSpeed = 1;
        }
        Spatial pivot = GetNode<Spatial>("Spatial_Pivot");
        pivot.Rotation = new Vector3(Mathf.Pi / 6f * velocity.y / jumpImpuls, pivot.Rotation.y, pivot.Rotation.z);
    }
}
