using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoystickControl
{
    /// <summary>
    /// Event arguments for the joystick
    /// </summary>
    public class JoystickEventArgs : EventArgs
    {
        /// <summary>
        /// X direction of the joystick
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// Y direction of the joystick
        /// </summary>
        public float Y { get; set; }
    }
}
