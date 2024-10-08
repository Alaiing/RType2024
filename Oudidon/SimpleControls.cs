﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Oudidon
{
    public class SimpleControls
    {
        private static KeyboardState _keyboardState;
        private static MouseState _mouseState;
        private static GamePadState _gamePadStatePlayer1;
        private static GamePadState _gamePadStatePlayer2;

        private static KeyboardState _keyboardPreviousState;
        private static MouseState _mousePreviousState;
        private static GamePadState _gamePadPreviousStatePlayer1;
        private static GamePadState _gamePadPreviousStatePlayer2;

        public static void GetStates()
        {
            _keyboardPreviousState = _keyboardState;
            _mousePreviousState = _mouseState;
            _gamePadPreviousStatePlayer1 = _gamePadStatePlayer1;
            _gamePadPreviousStatePlayer2 = _gamePadStatePlayer2;

            _keyboardState = Keyboard.GetState();
            _mouseState = Mouse.GetState();
            _gamePadStatePlayer1 = GamePad.GetState(PlayerIndex.One);
            _gamePadStatePlayer2 = GamePad.GetState(PlayerIndex.Two);
        }

        public static bool IsLeftMouseButtonDown()
        {
            return _mouseState.LeftButton == ButtonState.Pressed;
        }

        public static bool LeftMouseButtonPressedThisFrame()
        {
            return _mouseState.LeftButton == ButtonState.Pressed && _mousePreviousState.LeftButton == ButtonState.Released;
        }

        public static bool IsRightMouseButtonDown()
        {
            return _mouseState.RightButton == ButtonState.Pressed;
        }

        public static bool RightMouseButtonPressedThisFrame()
        {
            return _mouseState.RightButton == ButtonState.Pressed && _mousePreviousState.RightButton == ButtonState.Released;
        }

        public static Point GetMousePosition()
        {
            return _mouseState.Position;
        }

        private static bool IsKeyPressedThisFrame(Keys key)
        {
            return _keyboardState.IsKeyDown(key) && _keyboardPreviousState.IsKeyUp(key);
        }

        private static bool IsKeyReleasedThisFrame(Keys key)
        {
            return _keyboardState.IsKeyUp(key) && _keyboardPreviousState.IsKeyDown(key);
        }

        private static bool IsButtonPressedThisFrame(GamePadState gamePadState, GamePadState previousGamePadState, Buttons button)
        {
            return gamePadState.IsButtonDown(button) && previousGamePadState.IsButtonUp(button);
        }

        private static bool IsButtonReleasedThisFrame(GamePadState gamePadState, GamePadState previousGamePadState, Buttons button)
        {
            return gamePadState.IsButtonUp(button) && previousGamePadState.IsButtonDown(button);
        }

        public static bool IsAnyMoveKeyDown(PlayerIndex playerNumber)
        {
            return IsLeftDown(playerNumber) || IsRightDown(playerNumber) || IsUpDown(playerNumber) || IsDownDown(playerNumber);
        }

        public static bool IsLeftDown(PlayerIndex playerNumber)
        {
            if (playerNumber == PlayerIndex.One && _keyboardState.IsKeyDown(Keys.Left)
                || playerNumber == PlayerIndex.Two && _keyboardState.IsKeyDown(Keys.G))
            {
                return true;
            }

            GamePadState currentGamepadState = playerNumber == PlayerIndex.One ? _gamePadStatePlayer1 : _gamePadStatePlayer2;

            return currentGamepadState.IsButtonDown(Buttons.DPadLeft) || currentGamepadState.IsButtonDown(Buttons.LeftThumbstickLeft);
        }

        public static bool IsLeftPressedThisFrame(PlayerIndex playerNumber)
        {
            GamePadState previousGamePad;
            GamePadState currentState;
            if (playerNumber == PlayerIndex.One)
            {
                if (IsKeyPressedThisFrame(Keys.Left))
                {
                    return true;
                }
                previousGamePad = _gamePadPreviousStatePlayer1;
                currentState = _gamePadStatePlayer1;
            }
            else
            {
                if (IsKeyPressedThisFrame(Keys.G))
                {
                    return true;
                }
                previousGamePad = _gamePadPreviousStatePlayer2;
                currentState = _gamePadStatePlayer2;
            }

            return IsButtonPressedThisFrame(currentState, previousGamePad, Buttons.DPadLeft) || IsButtonPressedThisFrame(currentState, previousGamePad, Buttons.LeftThumbstickLeft);
        }

        public static bool IsRightDown(PlayerIndex playerNumber)
        {
            if (playerNumber == PlayerIndex.One && _keyboardState.IsKeyDown(Keys.Right)
                || playerNumber == PlayerIndex.Two && _keyboardState.IsKeyDown(Keys.J))
            {
                return true;
            }

            GamePadState currentGamepadState = playerNumber == PlayerIndex.One ? _gamePadStatePlayer1 : _gamePadStatePlayer2;

            if (currentGamepadState.IsConnected)
            {
                if (currentGamepadState.DPad.Right == ButtonState.Pressed)
                {
                    return true;
                }

                if (currentGamepadState.ThumbSticks.Left.X > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsRightPressedThisFrame(PlayerIndex playerNumber)
        {
            GamePadState previousGamePad;
            GamePadState currentState;
            if (playerNumber == PlayerIndex.One)
            {
                if (IsKeyPressedThisFrame(Keys.Right))
                {
                    return true;
                }
                previousGamePad = _gamePadPreviousStatePlayer1;
                currentState = _gamePadStatePlayer1;
            }
            else
            {
                if (IsKeyPressedThisFrame(Keys.J))
                {
                    return true;
                }
                previousGamePad = _gamePadPreviousStatePlayer2;
                currentState = _gamePadStatePlayer2;
            }

            return IsButtonPressedThisFrame(currentState, previousGamePad, Buttons.DPadRight) || IsButtonPressedThisFrame(currentState, previousGamePad, Buttons.LeftThumbstickRight);
        }

        public static bool IsUpDown(PlayerIndex playerNumber)
        {
            if (playerNumber == PlayerIndex.One && _keyboardState.IsKeyDown(Keys.Up)
                || playerNumber == PlayerIndex.Two && _keyboardState.IsKeyDown(Keys.Y))
            {
                return true;
            }

            GamePadState currentGamepadState = playerNumber == PlayerIndex.One ? _gamePadStatePlayer1 : _gamePadStatePlayer2;

            if (currentGamepadState.IsConnected)
            {
                if (_gamePadStatePlayer1.DPad.Up == ButtonState.Pressed)
                {
                    return true;
                }

                if (currentGamepadState.ThumbSticks.Left.Y > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsUpPressedThisFrame(PlayerIndex playerNumber)
        {
            GamePadState previousGamePad;
            GamePadState currentState;
            if (playerNumber == PlayerIndex.One)
            {
                if (IsKeyPressedThisFrame(Keys.Up))
                {
                    return true;
                }
                previousGamePad = _gamePadPreviousStatePlayer1;
                currentState = _gamePadStatePlayer1;
            }
            else
            {
                if (IsKeyPressedThisFrame(Keys.Y))
                {
                    return true;
                }
                previousGamePad = _gamePadPreviousStatePlayer2;
                currentState = _gamePadStatePlayer2;
            }

            return IsButtonPressedThisFrame(currentState, previousGamePad, Buttons.DPadUp) || IsButtonPressedThisFrame(currentState, previousGamePad, Buttons.LeftThumbstickUp);
        }

        public static bool IsDownDown(PlayerIndex playerNumber)
        {
            if (playerNumber == PlayerIndex.One && _keyboardState.IsKeyDown(Keys.Down)
                || playerNumber == PlayerIndex.Two && _keyboardState.IsKeyDown(Keys.H))
            {
                return true;
            }

            GamePadState currentGamepadState = playerNumber == PlayerIndex.One ? _gamePadStatePlayer1 : _gamePadStatePlayer2;

            if (currentGamepadState.IsConnected)
            {
                if (currentGamepadState.DPad.Down == ButtonState.Pressed)
                {
                    return true;
                }

                if (currentGamepadState.ThumbSticks.Left.Y < 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsDownPressedThisFrame(PlayerIndex playerNumber)
        {
            GamePadState previousGamePad;
            GamePadState currentState;
            if (playerNumber == PlayerIndex.One)
            {
                if (IsKeyPressedThisFrame(Keys.Down))
                {
                    return true;
                }
                previousGamePad = _gamePadPreviousStatePlayer1;
                currentState = _gamePadStatePlayer1;
            }
            else
            {
                if (IsKeyPressedThisFrame(Keys.H))
                {
                    return true;
                }
                previousGamePad = _gamePadPreviousStatePlayer2;
                currentState = _gamePadStatePlayer2;
            }

            return IsButtonPressedThisFrame(currentState, previousGamePad, Buttons.DPadDown) || IsButtonPressedThisFrame(currentState, previousGamePad, Buttons.LeftThumbstickDown);
        }

        public static bool IsADown(PlayerIndex playerNumber)
        {
            if (playerNumber == PlayerIndex.One && _keyboardState.IsKeyDown(Keys.Space)
                || playerNumber == PlayerIndex.Two && _keyboardState.IsKeyDown(Keys.F))
            {
                return true;
            }

            GamePadState currentGamepadState = playerNumber == PlayerIndex.One ? _gamePadStatePlayer1 : _gamePadStatePlayer2;

            if (currentGamepadState.IsConnected)
            {
                if (currentGamepadState.Buttons.A == ButtonState.Pressed)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsAPressedThisFrame(PlayerIndex playerNumber)
        {
            GamePadState previousGamePad;
            GamePadState currentState;
            if (playerNumber == PlayerIndex.One)
            {
                if (IsKeyPressedThisFrame(Keys.Space))
                {
                    return true;
                }
                previousGamePad = _gamePadPreviousStatePlayer1;
                currentState = _gamePadStatePlayer1;
            }
            else
            {
                if (IsKeyPressedThisFrame(Keys.F))
                {
                    return true;
                }
                previousGamePad = _gamePadPreviousStatePlayer2;
                currentState = _gamePadStatePlayer2;
            }

            return IsButtonPressedThisFrame(currentState, previousGamePad, Buttons.A);
        }
        public static bool IsAReleasedThisFrame(PlayerIndex playerNumber)
        {
            GamePadState previousGamePad;
            GamePadState currentState;
            if (playerNumber == PlayerIndex.One)
            {
                if (IsKeyReleasedThisFrame(Keys.Space))
                {
                    return true;
                }
                previousGamePad = _gamePadPreviousStatePlayer1;
                currentState = _gamePadStatePlayer1;
            }
            else
            {
                if (IsKeyReleasedThisFrame(Keys.F))
                {
                    return true;
                }
                previousGamePad = _gamePadPreviousStatePlayer2;
                currentState = _gamePadStatePlayer2;
            }

            return IsButtonReleasedThisFrame(currentState, previousGamePad, Buttons.A);
        }

        public static bool IsBDown(PlayerIndex playerNumber)
        {
            if (playerNumber == PlayerIndex.One && _keyboardState.IsKeyDown(Keys.Enter)
                || playerNumber == PlayerIndex.Two && _keyboardState.IsKeyDown(Keys.D))
            {
                return true;
            }

            GamePadState currentGamepadState = playerNumber == PlayerIndex.One ? _gamePadStatePlayer1 : _gamePadStatePlayer2;

            if (currentGamepadState.IsConnected)
            {
                if (currentGamepadState.Buttons.B == ButtonState.Pressed)
                {
                    return true;
                }
            }

            return false;
        }

        // TODO : pressed this frame for B

        public static bool IsStartDown()
        {
            if (_keyboardState.IsKeyDown(Keys.F1))
            {
                return true;
            }

            if (_gamePadStatePlayer1.IsConnected)
            {
                if (_gamePadStatePlayer1.Buttons.Start == ButtonState.Pressed)
                {
                    return true;
                }
            }

            return false;
        }

        // TODO : pressed this frame for Start

        public static bool IsSelectDown()
        {
            if (_keyboardState.IsKeyDown(Keys.F2))
            {
                return true;
            }

            if (_gamePadStatePlayer1.IsConnected)
            {
                if (_gamePadStatePlayer1.Buttons.Back == ButtonState.Pressed)
                {
                    return true;
                }
            }

            return false;
        }

        // TODO : pressed this frame for Select

        public static bool IsEscapeDown()
        {
            return _keyboardState.IsKeyDown(Keys.Escape);
        }

        public static bool IsCheatKillDown()
        {
            return _keyboardState.IsKeyDown(Keys.F3);
        }

    }
}
