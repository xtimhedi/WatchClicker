using Godot;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatchApp.Scripts
{
    public partial class GameManager : Node
    {
        public static GameManager Instance;

        public int BaseCookieGain = 1;
        public float CookiePerClickMultiplier = 1.0f;
        public float CookiesPerSecond = 0.0f;
        public float Cookies = 0;

        public void Click()
        {
            Cookies = Cookies + (BaseCookieGain * CookiePerClickMultiplier);
        }

        public override void _Ready()
        {
            Instance = this;
        }

    }
}
