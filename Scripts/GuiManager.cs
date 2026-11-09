using Godot;
using System.Collections.Generic;
using WatchApp.Scripts;
using static Godot.Control;

public partial class GuiManager : CanvasLayer
{
    public static GuiManager Instance { get; private set; }

    // Dictionary tracking our layer container roots
    private readonly Dictionary<UiLayer, Control> _layerContainers = new();

    // The history stack preserving active screen references
    private readonly Stack<UiScreen> _screenHistory = new();

    public override void _Ready()
    {
        Instance = this;
        CreateLayerContainers();
    }

    /// <summary> 
    /// Returns true if ANY interactive menu or popup is currently active in the history stack.
    /// Excludes non-modal layers like ambient HUDs or low-level backgrounds.
    /// </summary>
    public bool IsMenuOpen
    {
        get
        {
            if (_screenHistory.Count == 0) return false;

            // Iterate through the active stack to see if any screen matches our interactive criteria
            foreach (var screen in _screenHistory)
            {
                if (screen.TargetLayer == UiLayer.Screen || screen.TargetLayer == UiLayer.Popup)
                {
                    return true;
                }
            }
            return false;
        }
    }

    /// <summary> 
    /// Returns true if a modal dialogue box or pause menu (Popup layer explicitly) is currently active.
    /// Useful for pausing background gameplay or blocking player inputs.
    /// </summary>
    public bool IsPopupOpen
    {
        get
        {
            if (_screenHistory.Count == 0) return false;

            // Quick shortcut: check if the top-most active element is a popup
            return _screenHistory.Peek().TargetLayer == UiLayer.Popup;
        }
    }

    /// <summary> 
    /// Returns the total number of UI instances currently preserved in the stack.
    /// </summary>
    public int TotalOpenScreens => _screenHistory.Count;

    private void CreateLayerContainers()
    {
        // Dynamically instantiate clean control anchors for each layer to enforce rendering order
        foreach (UiLayer layer in System.Enum.GetValues(typeof(UiLayer)))
        {
            var container = new Control
            {
                Name = $"{layer}Layer",
                AnchorsPreset = (int)LayoutPreset.FullRect,
                MouseFilter = MouseFilterEnum.Pass
            };
            AddChild(container);
            _layerContainers[layer] = container;
        }
    }

    /// <summary> Pushes a brand new screen instance onto the stack and displays it. </summary>
    public void PushScreen(UiScreen screenInstance)
    {
        if (screenInstance == null) return;

        // 1. Notify the current top screen that it is being covered
        if (_screenHistory.Count > 0)
        {
            _screenHistory.Peek().OnScreenCovered();
        }

        // 2. Parent the new screen to its designated rendering layer container
        var targetContainer = _layerContainers[screenInstance.TargetLayer];
        if (screenInstance.GetParent() == null)
        {
            targetContainer.AddChild(screenInstance);
        }

        // 3. Update the tracking history stack and fire entry hooks
        _screenHistory.Push(screenInstance);
        screenInstance.OnScreenEnter();
    }

    /// <summary> Pops the top screen off the stack, completely closing it. </summary>
    public void PopScreen(bool deleteInstance = false)
    {
        if (_screenHistory.Count == 0) return;

        // 1. Remove the current top screen from the stack
        UiScreen closedScreen = _screenHistory.Pop();
        closedScreen.OnScreenExit();

        if (deleteInstance)
        {
            closedScreen.QueueFree();
        }

        // 2. Resume the screen that was sitting underneath it
        if (_screenHistory.Count > 0)
        {
            _screenHistory.Peek().OnScreenResumed();
        }
    }

    /// <summary> Pops multiple screens sequentially until a target screen type is reached. </summary>
    public void GoBackToScreen<T>(bool deletePoppedInstances = false) where T : UiScreen
    {
        while (_screenHistory.Count > 0)
        {
            if (_screenHistory.Peek() is T)
            {
                // Target screen reached! Resume it.
                _screenHistory.Peek().OnScreenResumed();
                return;
            }

            // Keep popping intermediate screens out of the stack
            UiScreen closedScreen = _screenHistory.Pop();
            closedScreen.OnScreenExit();

            if (deletePoppedInstances)
            {
                closedScreen.QueueFree();
            }
        }

        GD.PrintErr($"Failed to go back: Screen of type {typeof(T).Name} was not found in history.");
    }

    /// <summary> Closes every screen in the stack, resetting the system. </summary>
    public void ClearHistory(bool deleteInstances = true)
    {
        while (_screenHistory.Count > 0)
        {
            UiScreen screen = _screenHistory.Pop();
            screen.OnScreenExit();
            if (deleteInstances)
            {
                screen.QueueFree();
            }
        }
    }
}
