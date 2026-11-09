using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    using Godot;

    [GlobalClass]
    public partial class UiScreen : Control
    {
        // Allows you to assign what layer this specific screen belongs to in the inspector
        [Export] public UiLayer TargetLayer { get; private set; } = UiLayer.Screen;

        /// <summary> Called when the screen is freshly instantiated or pushed to the top of the stack. </summary>
        public virtual void OnScreenEnter()
        {
            Show();
            SetProcessUnhandledInput(true);
        }

        /// <summary> Called when a new screen is pushed on top of this one, covering it. </summary>
        public virtual void OnScreenCovered()
        {
            // Keep it alive, but hide it and disable input so it retains its state
            Hide();
            SetProcessUnhandledInput(false);
        }

        /// <summary> Called when the screen on top is popped, restoring this screen to focus. </summary>
        public virtual void OnScreenResumed()
        {
            Show();
            SetProcessUnhandledInput(true);
        }

        /// <summary> Called when this screen is popped off the stack completely. </summary>
        public virtual void OnScreenExit()
        {
            Hide();
            SetProcessUnhandledInput(false);
        }
    }

