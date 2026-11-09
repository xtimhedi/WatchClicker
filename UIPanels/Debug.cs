using Godot;
using System;

public partial class Debug : WatchMenuContainer
{
    public void QuitGame()
    {
        GetTree().Quit();
    }
}
