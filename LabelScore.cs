using Godot;
using System;

public class LabelScore : Label
{
    private int score;

    public override void _Ready()
    {
        score = 0;
    }

    public void OnMobSquashed()
    {
        score++;
        Text = string.Format("Score: {0}", score);
    }
}
