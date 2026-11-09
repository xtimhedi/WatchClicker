using Godot;
using System;

public partial class IngameMenu : UiScreen
{
    public void Stats()
    {
        GuiManager.Instance.PushScreen((UiScreen)GD.Load<PackedScene>("res://UIPanels/Stats.tscn").Instantiate());
    }
}
