using Godot;
using System;

[GlobalClass]
public partial class WatchMenuContainer : Control
{
    public int index = 0;

    [ExportGroup("Layout Configurations")]
    [Export] public float ItemSpacing = 100.0f;    // Vertical distance between item centers
    [Export] public float SelectedScale = 1.4f;    // Center pop size
    [Export] public float UnselectedScale = 0.8f;  // Shrunk outer bounds

    [ExportGroup("Hyper Physics Settings")]
    [Export] public float LayoutSpeed = 22.0f;     // Snappiness of item movement
    [Export] public float ScaleSpeed = 25.0f;      // Snappiness of scale changes
    [Export] public float Friction = 0.88f;        // Decay speed of free kinetic scrolling
    [Export] public float SnapStiffness = 18.0f;   // Snap force to lock onto an item

    private float _virtualScrollY = 0.0f;
    private float _scrollVelocity = 0.0f;
    private bool _isSnapping = true;
    private float _targetSnapY = 0.0f;

    public override void _Ready()
    {
        ClipContents = true;

        var children = GetChildren();
        for (int i = 0; i < children.Count; i++)
        {
            if (children[i] is Control child)
            {
                // FORCE THE BUTTON TO LOGICAL TOP-LEFT ANCHORS. 
                // We handle 100% of the placement math manually now.
                child.AnchorsPreset = (int)LayoutPreset.TopLeft;

                // Keep the scaling pivot dead center
                child.PivotOffset = child.Size / 2.0f;
                child.Resized += () => { child.PivotOffset = child.Size / 2.0f; };
            }
        }
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        var children = GetChildren();
        if (children.Count == 0) return;

        // Visual horizontal and vertical midpoints of this container window
        float menuCenterX = Size.X / 2.0f;
        float menuCenterY = Size.Y / 2.0f;

        // 1. Kinetic Physics & Snapping Calculations
        if (!_isSnapping)
        {
            _virtualScrollY += _scrollVelocity * dt;
            _scrollVelocity *= Mathf.Pow(Friction, dt * 60f);

            if (Mathf.Abs(_scrollVelocity) < 30.0f)
            {
                _scrollVelocity = 0.0f;
                _isSnapping = true;
                _targetSnapY = index * ItemSpacing;
            }
        }
        else
        {
            _virtualScrollY = Mathf.Lerp(_virtualScrollY, _targetSnapY, dt * SnapStiffness);
            if (Mathf.Abs(_virtualScrollY - _targetSnapY) < 0.05f)
            {
                _virtualScrollY = _targetSnapY;
            }
        }

        // Clamp boundaries based on the list length
        float maxScrollLimit = (children.Count - 1) * ItemSpacing;
        _virtualScrollY = Mathf.Clamp(_virtualScrollY, 0.0f, maxScrollLimit);
        if (_isSnapping) _targetSnapY = Mathf.Clamp(_targetSnapY, 0.0f, maxScrollLimit);

        int closestIndex = 0;
        float shortestDistance = float.MaxValue;

        // 2. Position Positioning & Scaling Loop
        for (int i = 0; i < children.Count; i++)
        {
            if (children[i] is Control child)
            {
                // Compute where the button center SHOULD sit vertically relative to the container's center line
                float itemCenterTargetY = menuCenterY + (i * ItemSpacing) - _virtualScrollY;

                // Absolute local coordinates required to put the button's visual midpoint exactly where we want it
                float absoluteX = menuCenterX - (child.Size.X / 2.0f);
                float absoluteY = itemCenterTargetY - (child.Size.Y / 2.0f);

                Vector2 targetPos = new Vector2(absoluteX, absoluteY);
                child.Position = child.Position.Lerp(targetPos, dt * LayoutSpeed);

                // Focus tracking calculations based on proximity to visual midpoint
                float distanceToCenter = Mathf.Abs(itemCenterTargetY - menuCenterY);
                if (distanceToCenter < shortestDistance)
                {
                    shortestDistance = distanceToCenter;
                    closestIndex = i;
                }

                // Smooth exponential layout scale alterations
                float maxRadius = Size.Y / 1.4f;
                float proximity = Mathf.Clamp(1.0f - (distanceToCenter / maxRadius), 0.0f, 1.0f);
                float exponentialCurve = Mathf.Pow(proximity, 3);

                float targetScaleFactor = Mathf.Lerp(UnselectedScale, SelectedScale, exponentialCurve);
                child.Scale = child.Scale.Lerp(new Vector2(targetScaleFactor, targetScaleFactor), dt * ScaleSpeed);
            }
        }

        index = closestIndex;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseWheelEvent && mouseWheelEvent.Pressed)
        {
            float accelerationImpulse = 450.0f;
            _isSnapping = false;

            if (mouseWheelEvent.ButtonIndex == MouseButton.WheelUp)
            {
                _scrollVelocity -= accelerationImpulse;
                GetViewport().SetInputAsHandled();
            }
            else if (mouseWheelEvent.ButtonIndex == MouseButton.WheelDown)
            {
                _scrollVelocity += accelerationImpulse;
                GetViewport().SetInputAsHandled();
            }
        }

        if (@event is InputEventMouseButton clickEvent && clickEvent.ButtonIndex == MouseButton.Left && !clickEvent.Pressed)
        {
            if (GetGlobalRect().HasPoint(clickEvent.GlobalPosition))
            {
                var children = GetChildren();
                if (index >= 0 && index < children.Count && children[index] is Button centerButton)
                {
                    centerButton.EmitSignal(Button.SignalName.Pressed);
                    GD.Print($"Custom OS Watch Press: {centerButton.Name} at index {index}");
                    GetViewport().SetInputAsHandled();
                }
            }
        }
    }
}
