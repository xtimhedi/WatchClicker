using Godot;
using System;
using WatchApp.Scripts;

public partial class CookieArea : Area2D
{
    public override void _InputEvent(Viewport viewport, InputEvent @event, int shapeIdx)
    {
        var isClick = @event is InputEventMouseButton ev && ev.Pressed;
        var isTouch = @event is InputEventScreenTouch evt && evt.Pressed;

        if (isClick || isTouch)
        {
            GameManager.Instance.Click();
        }
    }
}
