using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoXEngine
{
    public class InputManager
    {
        [Flags]
        public enum Input
        {
            Up,
            Down,
            Left,
            Right,
            Action1,
            Action2,
            L1,
            L2,
            Special1,
            Special2
        }

        public enum InputType
        {
            Keyboard,
            Controller
        }

        public InputType CurrentInputType;
        public Dictionary<Keys, Input> KeyboardInput;
        public Dictionary<Buttons, Input> ControllerInput;

        private Dictionary<Input, bool> EmptyInput;
        private Dictionary<Input, bool> PressedThisFrame;
        private Dictionary<Input, bool> HeldLastFrame;
        private Dictionary<Input, bool> HeldThisFrame;

        public InputManager(InputType inputType)
        {
            // Default input type
            CurrentInputType = inputType;

            // PressedThisFrame & HeldLastFrame & HeldThisFrame
            EmptyInput = new Dictionary<Input, bool>() {
                { Input.Up,      false },
                { Input.Down,    false },
                { Input.Left,    false },
                { Input.Right,   false },
                { Input.Action1, false },
                { Input.Action2, false },
                { Input.L1,      false },
                { Input.L2,      false },
                { Input.Special1,false },
                { Input.Special2,false }
            };
            PressedThisFrame = EmptyInput.Copy();
            HeldThisFrame = PressedThisFrame.Copy();

            // Keyboard input
            KeyboardInput = new Dictionary<Keys, Input>() {
                { Keys.Up,      Input.Up },
                { Keys.Down,    Input.Down },
                { Keys.Left,    Input.Left },
                { Keys.Right,   Input.Right },
                { Keys.Z,       Input.Action1 },
                { Keys.X,       Input.Action2 },
                { Keys.A,       Input.L1 },
                { Keys.S,       Input.L2 },
                { Keys.OemComma,Input.Special1 },
                { Keys.OemPeriod,Input.Special2 }
            };

            // Controller input
            ControllerInput = new Dictionary<Buttons, Input>() {
                { Buttons.DPadUp,      Input.Up },
                { Buttons.DPadDown,    Input.Down },
                { Buttons.DPadLeft,    Input.Left },
                { Buttons.DPadRight,   Input.Right },
                { Buttons.A,           Input.Action1 },
                { Buttons.B,           Input.Action2 },
                { Buttons.LeftShoulder,Input.L1 },
                { Buttons.RightShoulder,Input.L2 },
                { Buttons.LeftTrigger,  Input.Special1 },
                { Buttons.RightTrigger,  Input.Special2 },
            };
        }

        public void Update()
        {
            HeldLastFrame = HeldThisFrame.Copy();
            HeldThisFrame = EmptyInput.Copy();
            PressedThisFrame = EmptyInput.Copy();

            if (CurrentInputType == InputType.Keyboard)
            {
                foreach (var key in Keyboard.GetState().GetPressedKeys())
                {
                    if (KeyboardInput.ContainsKey(key))
                    {
                        HeldThisFrame[KeyboardInput[key]] = true;

                        if(HeldThisFrame[KeyboardInput[key]] != HeldLastFrame[KeyboardInput[key]])
                            PressedThisFrame[KeyboardInput[key]] = true;
                    }
                }
            }
            else if (CurrentInputType == InputType.Controller)
            {
                GamePadState gps = GamePad.GetState(Microsoft.Xna.Framework.PlayerIndex.One);
                foreach (var btn in ControllerInput)
                {
                    if (gps.IsButtonDown(btn.Key))
                    {
                        HeldThisFrame[ControllerInput[btn.Key]] = true;

                        if (HeldThisFrame[ControllerInput[btn.Key]] != HeldLastFrame[ControllerInput[btn.Key]])
                            PressedThisFrame[ControllerInput[btn.Key]] = true;
                    }
                }
            }
        }

        public bool Pressed(Input input)
        {
            return PressedThisFrame[input];
        }

        public bool Held(params Input[] input)
        {
            foreach(Input i in input)
                if (!HeldThisFrame[i])
                    return false;

            return true;
        }
    }
}
