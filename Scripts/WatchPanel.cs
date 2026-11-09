using Godot;
using System;

[Tool]
[GlobalClass]
public partial class WatchPanel : PanelContainer
{
    public override void _Ready()
    {
        UpdateRadius();
        Resized += UpdateRadius;
    }

    public void UpdateRadius()
    {
        StyleBoxFlat stylebox = (StyleBoxFlat)GetThemeStylebox("panel");

        if (stylebox != null)
        {
            int _maxRadius = (int)Mathf.Min(Size.X, Size.Y) / 2;

            stylebox.SetCornerRadiusAll(_maxRadius);

            stylebox.CornerDetail = 20;
        }
    }
}
