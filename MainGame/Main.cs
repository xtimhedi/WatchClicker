using Godot;
using System;
using WatchApp.Scripts;

public partial class Main : Node2D
{
    public override void _Ready()
    {
        GuiManager mgr = new GuiManager();
        GameManager gmgr = new GameManager();
        AddChild(mgr);
        AddChild(gmgr);
        GetTree().Root.GoBackRequested += HandleBack;
    }
    public void OpenDebug()
    {
        var debugUi = GD.Load<PackedScene>("res://UIPanels/Debug.tscn");
        var instance = debugUi.Instantiate();
        GuiManager.Instance.PushScreen((UiScreen)instance);

    }
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey key)
        {
            if (key.Keycode == Key.Alt && key.Pressed)
            {
                HandleBack();
            }
        } 
    }
    public void HandleBack()
    {
        if (GuiManager.Instance.IsMenuOpen)
        {
            GuiManager.Instance.PopScreen(true);

        }
        else
        {
            GuiManager.Instance.PushScreen((UiScreen)GD.Load<PackedScene>("res://UIPanels/IngameMenu.tscn").Instantiate());

        }
    }
}
