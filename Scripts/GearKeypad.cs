using Godot;
using System;

public partial class GearKeypad : Control
{
	private Label _pinDisplay;
	private Button _deleteButton;

	// Layout configuration for 320x320 screens
	private float _outerRadius = 160.0f;
	private float _innerRadius = 110.0f;
	private float _numberRadius = 135.0f;
	private Vector2 _center = new Vector2(160, 160);

	private string[] _gearNumbers = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
	private string _currentPin = "";
	private int _selectedIndex = 0; // Tracks which wedge is actively highlighted

	// Styling Colors
	private Color _backgroundColor = new Color(0.05f, 0.05f, 0.05f); // Deep dark background
	private Color _wedgeLineColor = new Color(0.18f, 0.18f, 0.18f); // Soft gray dividers
	private Color _highlightColor = new Color(0.12f, 0.37f, 0.74f); // Samsung Blue

	public override void _Ready()
	{
		// 1. Force the root node sizing to guarantee 320x320 constraints
		CustomMinimumSize = new Vector2(320, 320);
		Size = new Vector2(320, 320);

		// 2. Programmatically generate Center Interface Nodes
		Control centerArea = new Control();
		centerArea.Position = _center;
		AddChild(centerArea);

		_pinDisplay = new Label();
		_pinDisplay.HorizontalAlignment = HorizontalAlignment.Center;
		_pinDisplay.VerticalAlignment = VerticalAlignment.Center;
		_pinDisplay.Position = new Vector2(-60, -30);
		_pinDisplay.Size = new Vector2(120, 30);
		centerArea.AddChild(_pinDisplay);

		_deleteButton = new Button();
		_deleteButton.Text = "⌫";
		_deleteButton.Flat = true;
		_deleteButton.Position = new Vector2(-30, 10);
		_deleteButton.Size = new Vector2(60, 40);
		_deleteButton.Pressed += OnDeletePressed;
		centerArea.AddChild(_deleteButton);

		// 3. Spatially position the text numbers on top of the wedges
		SetupNumberButtons();
		UpdateDisplay();
	}

	private void SetupNumberButtons()
	{
		int totalNumbers = _gearNumbers.Length;

		for (int i = 0; i < totalNumbers; i++)
		{
			int index = i; // Avoid closure allocation trap
			string numStr = _gearNumbers[i];

			Button numBtn = new Button();
			numBtn.Text = numStr;
			numBtn.Flat = true;
			numBtn.Alignment = HorizontalAlignment.Center;

			Vector2 buttonSize = new Vector2(40, 40);
			numBtn.CustomMinimumSize = buttonSize;
			numBtn.Size = buttonSize;

			// Calculate exact polar coordinates for text overlay using Godot's Mathf
			float angle = (i * (Mathf.Tau / totalNumbers)) - (Mathf.Pi / 2.0f);
			float x = Mathf.Cos(angle) * _numberRadius;
			float y = Mathf.Sin(angle) * _numberRadius;

			// Position button relative to screen center
			numBtn.Position = _center + new Vector2(x, y) - (buttonSize / 2.0f);

			// Interaction changes selection AND inputs number
			numBtn.Pressed += () => {
				_selectedIndex = index;
				OnNumberSelected(numStr);
				QueueRedraw(); // Re-render vector slices
			};

			AddChild(numBtn);
		}
	}

	// Handles drawing the graphical background, lines, and active wedge highlights
	public override void _Draw()
	{
		// A. Base Dark Fill
		DrawCircle(_center, _outerRadius, _backgroundColor);

		int totalWedges = _gearNumbers.Length;
		float wedgeAngle = Mathf.Tau / totalWedges;

		for (int i = 0; i < totalWedges; i++)
		{
			// Align starting rotation to 12 o'clock minus half a wedge thickness
			float startAngle = (i * wedgeAngle) - (Mathf.Pi / 2.0f) - (wedgeAngle / 2.0f);
			float endAngle = startAngle + wedgeAngle;

			// B. Draw Active Blue Selection Wedge Graphic
			if (i == _selectedIndex)
			{
				DrawWedgeHighlight(startAngle, endAngle);
			}

			// C. Draw Gray Divider Beams
			Vector2 innerPoint = _center + new Vector2(Mathf.Cos(startAngle), Mathf.Sin(startAngle)) * _innerRadius;
			Vector2 outerPoint = _center + new Vector2(Mathf.Cos(startAngle), Mathf.Sin(startAngle)) * _outerRadius;
			DrawLine(innerPoint, outerPoint, _wedgeLineColor, 1.5f);
		}

		// D. Inner Border Ring to give it that crisp circular bezel frame look
		DrawArc(_center, _innerRadius, 0, Mathf.Tau, 64, _wedgeLineColor, 2.0f);
	}

	private void DrawWedgeHighlight(float startAngle, float endAngle)
	{
		int fidelity = 16; // Level of curvature smoothness
		Vector2[] points = new Vector2[fidelity * 2 + 2];

		// Map Outer Arc Points using Godot's Mathf
		for (int i = 0; i <= fidelity; i++)
		{
			float t = (float)i / fidelity;
			float angle = Mathf.Lerp(startAngle, endAngle, t);
			points[i] = _center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * _outerRadius;
		}

		// Map Inner Arc Points (Reversed to close the polygon loop cleanly)
		for (int i = 0; i <= fidelity; i++)
		{
			float t = (float)i / fidelity;
			float angle = Mathf.Lerp(endAngle, startAngle, t);
			points[fidelity + 1 + i] = _center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * _innerRadius;
		}

		Color[] colors = new Color[] { _highlightColor };
		DrawPolygon(points, colors);
	}

	private void OnNumberSelected(string number)
	{
		if (_currentPin.Length < 4)
		{
			_currentPin += number;
			UpdateDisplay();
		}
	}

	private void OnDeletePressed()
	{
		if (_currentPin.Length > 0)
		{
			_currentPin = _currentPin.Substring(0, _currentPin.Length - 1);
			UpdateDisplay();
		}
	}

	private void UpdateDisplay()
	{
		string displayStr = "";
		for (int i = 0; i < _currentPin.Length; i++)
		{
			if (i == _currentPin.Length - 1)
				displayStr += _currentPin[i];
			else
				displayStr += "• ";
		}

		_pinDisplay.Text = displayStr == "" ? "" : displayStr;
	}

	// Capture mouse scrolling or smartwatch bezel operations to cycle the highlights
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
		{
			if (mouseEvent.ButtonIndex == MouseButton.WheelUp)
			{
				_selectedIndex = (_selectedIndex + 1) % _gearNumbers.Length;
				QueueRedraw();
			}
			else if (mouseEvent.ButtonIndex == MouseButton.WheelDown)
			{
				// Correct negative modulo protection
				_selectedIndex = (_selectedIndex - 1 + _gearNumbers.Length) % _gearNumbers.Length;
				QueueRedraw();
			}
		}
	}
}
