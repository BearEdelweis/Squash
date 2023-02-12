using Godot;
using System;

public class KinematicBody_Player : KinematicBody
{
    [Signal] public delegate void Hit();

    private int fallAcceleration; //имитация свободного падения
    private int speed; //скорость игрока
    private int jumpImpuls;
    private int bounceImpuls;
    private Vector3 velocity; //направление движения
    private Spatial spatialPivot;

    private bool waitForDoubleTap;
    private bool isJumping;

    private Timer doubleTapAwaiter;
    
    public override void _Ready()
    {
        fallAcceleration = 75;
        speed = 5;
        jumpImpuls = 20;
        bounceImpuls = 16;
        velocity = Vector3.Zero;
        spatialPivot = GetNode<Spatial>("Spatial_Pivot");
        waitForDoubleTap = false;
        isJumping = false;
        doubleTapAwaiter = GetNode<Timer>("DoubleTapAwaiter");
    }

    private void Die()
    {
        EmitSignal(nameof(Hit));
        //Hide();
        //if()
        //await ToSignal(GetTree().CreateTimer(0.3f), "timeout");
        QueueFree();
    }

    private void OnAreaBodyEntered(Node body)
    {
        Die();
    }

    public override async void _Input(InputEvent iE)
    {
        if(iE is InputEventScreenTouch sT)
        {
            if(sT.Pressed && !waitForDoubleTap)
            {
                waitForDoubleTap = true;
                doubleTapAwaiter.Start();
                await ToSignal(doubleTapAwaiter, "timeout");
                waitForDoubleTap = false;
                //GD.Print(waiter.GetType());
            }
            if(sT.Pressed && waitForDoubleTap) //&& IsOnFloor())
            {
                isJumping = true;
                //velocity.y += jumpImpuls;
                GD.Print("DoubleTap!");
                waitForDoubleTap = false;
            }
            
            if(!sT.Pressed)
            {
                //velocity.x = 0;
                velocity = Vector3.Zero;
            }
        }
        
        if (iE is InputEventScreenDrag sD)
        {
            velocity.x = sD.Speed.x;
            velocity.z = sD.Speed.y;
        }
    }

    private void ConstraintsPlayerMove()
    {
        Vector3 pos = Translation;

        if (Translation.z < -12)
        {
            pos.z = -12;
            Translation = pos;
        }
        if (Translation.z > 14)
        {
            pos.z = 14;
            Translation = pos;
        }

        if (Translation.x < -5)
        {
            pos.x = -5;
            Translation = pos;
        }
        if (Translation.x > 5)
        {
            pos.x = 5;
            Translation = pos;
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        ConstraintsPlayerMove();
        
        /*Vector3 direction = Vector3.Zero;

        if (Input.IsActionPressed("ui_left")) direction.x -= 1;
        if (Input.IsActionPressed("ui_right")) direction.x += 1;
        if (Input.IsActionPressed("ui_down")) direction.z += 1;
        if (Input.IsActionPressed("ui_up")) direction.z -= 1;*/

        if (velocity!=Vector3.Zero) // в этом блоке было direction
        {
            velocity = velocity.Normalized();
            spatialPivot.LookAt(Translation + velocity, Vector3.Up);
        }

        velocity.x *= speed;
        velocity.z *= speed;

        if(!IsOnFloor()) velocity.y -= 100 * delta;//персонаж должен постоянно немножко долбиться об пол :-)
                                               //потому что столкновения определяются только в движении

        if (isJumping)//(Input.IsActionJustPressed("ui_jump"))
        {
            velocity.y += 100;
            isJumping = false;
        }
        
        
        //else 

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
        if(velocity.z!=0 || velocity.x!=0) //вместо velocity было direction
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
