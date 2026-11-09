using Godot;
using System;

public partial class CookieSprite : Sprite2D
{
    public override void _Ready()
    {
        StartRotating();
    }

    private void StartRotating()
    {
        // Create a tween that loops indefinitely
        Tween tween = CreateTween().SetLoops();

        // Convert 45 degrees to radians
        float angleRad = Mathf.DegToRad(45);

        // Rotate to +45 degrees over 2 seconds, using a smooth ease-in-out transition
        tween.TweenProperty(this, "rotation", angleRad, 3.0f)
             .SetTrans(Tween.TransitionType.Sine)
             .SetEase(Tween.EaseType.InOut);

        // Rotate back to -45 degrees over 2 seconds
        tween.TweenProperty(this, "rotation", -angleRad, 3.0f)
             .SetTrans(Tween.TransitionType.Sine)
             .SetEase(Tween.EaseType.InOut);
    }
}
