using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum UiLayer
{
    Base,      // Main backgrounds, static menus
    Screen,    // Standard game screens (Main Menu, Inventory, Level Select)
    Popup,     // Overlay popups, pause menus, confirmation dialogues
    Hud,       // Gameplay HUD overlays
    System     // Top-most layer (loading screens, error messages)
}

