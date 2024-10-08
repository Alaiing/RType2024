﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Oudidon
{
    public class Character : OudidonGameComponent
    {
        public string name;
        protected SpriteSheet _spriteSheet;
        public SpriteSheet SpriteSheet => _spriteSheet;
        protected Vector2 _position;
        public Vector2 Position => _position;
        private Point _pivotOffset;
        public int PixelPositionX => (int)MathF.Floor(_position.X);
        public int PixelPositionY => (int)MathF.Floor(_position.Y);
        protected float _currentFrame;
        public virtual int CurrentFrame => (int)MathF.Floor(_currentFrame);
        protected Color[] _colors;
        public Color[] Colors => _colors;
        protected Color _mainColor;
        public Color Color => _mainColor;
        private Color[] _finalColors;
        public Vector2 MoveDirection;
        protected float _currentRotation;
        public float CurrentRotation => _currentRotation;
        protected Vector2 _currentScale;
        public Vector2 CurrentScale => _currentScale;
        protected float _baseSpeed;
        public float BaseSpeed => _baseSpeed;
        protected float _speedMultiplier;
        public float CurrentSpeed => _baseSpeed * _speedMultiplier;
        protected float _animationSpeedMultiplier;
        private float _animationDirection;
        private string _currentAnimationName;
        public string CurrentAnimationName => _currentAnimationName;
        private SpriteSheet.Animation _currentAnimation;

        protected Rectangle _collider;
        public Rectangle Collider => _collider;

        private Action _onAnimationEnd;
        protected Action<int> _onAnimationFrame;

        public Character(SpriteSheet spriteSheet, Game game) : base(game)
        {
            _collider = new Rectangle(0,0, spriteSheet.FrameWidth, spriteSheet.FrameHeight);
            _spriteSheet = spriteSheet;
            _colors = new Color[_spriteSheet.LayerCount];
            _finalColors = new Color[_colors.Length];
            Reset();
            Enabled = true;
            Visible = true;
        }

        public virtual Rectangle GetBounds()
        {
            int x = PixelPositionX - (CurrentScale.X > 0 ? SpriteSheet.LeftMargin - _collider.X : SpriteSheet.RightMargin + (SpriteSheet.FrameWidth - _collider.X - _collider.Width));
            int y = PixelPositionY - (CurrentScale.Y > 0 ? SpriteSheet.TopMargin - _collider.Y: SpriteSheet.BottomMargin + (SpriteSheet.FrameHeight - _collider.Y - _collider.Height));
            return new Rectangle(x, y, Collider.Width, Collider.Height);
        }

        public void Activate()
        {
            Enabled = true;
            Visible = true;
        }

        public void Deactivate()
        {
            Enabled = false;
            Visible = false;
        }

        public virtual void Reset()
        {
            _currentScale = Vector2.One;
            _currentFrame = 0;
            ResetColors();
            LookTo(new Vector2(1, 0));
            MoveDirection = Vector2.Zero;
            _animationSpeedMultiplier = 1f;
            _speedMultiplier = 1f;
        }

        public virtual void ResetColors()
        {
            for (int i = 0; i < _colors.Length; i++)
            {
                _colors[i] = Color.White;
            }
            _mainColor = Color.White;
            UpdateFinalColors();
        }

        public void SetMainColor(Color color)
        {
            _mainColor = color;
            UpdateFinalColors();
        }

        public void SetLayerColor(Color color, int layer)
        {
            _colors[layer] = color;
            UpdateFinalColors();
        }

        public void SetColors(Color[] colors)
        {
            _colors = colors;
            UpdateFinalColors();
        }

        protected void UpdateFinalColors()
        {
            for (int i = 0; i < _colors.Length; i++)
            {
                _finalColors[i] = new Color(_colors[i].ToVector4() * _mainColor.ToVector4());
            }
        }

        public void SetBaseSpeed(float speed)
        {
            _baseSpeed = speed;
        }

        public void SetSpeedMultiplier(float speedMutilplier)
        {
            _speedMultiplier = speedMutilplier;
        }

        public void SetAnimationSpeedMultiplier(float animationSpeedMultiplier)
        {
            _animationSpeedMultiplier = animationSpeedMultiplier;
        }

        public void SetFrame(int frameIndex)
        {
            if (!string.IsNullOrEmpty(_currentAnimationName) && frameIndex >= 0 && frameIndex < _currentAnimation.FrameCount)
            {
                _currentFrame = frameIndex;
            }
        }

        public void SetAnimation(string animationName, Action onAnimationEnd = null, Action<int> onAnimationFrame = null)
        {
            if (_currentAnimationName != animationName)
            {
                if (_spriteSheet.TryGetAnimation(animationName, out _currentAnimation))
                {
                    _currentAnimationName = animationName;

                    _currentFrame = 0;

                    _onAnimationEnd = onAnimationEnd;
                    _onAnimationFrame = onAnimationFrame;
                }
                else
                {
                    _currentAnimationName = null;
                    _onAnimationEnd = null;
                    _onAnimationFrame = null;
                }
                _animationDirection = 1;
            }
        }

        public void SetScale(Vector2 scale)
        {
            _currentScale = scale;
        }

        public void SetRotation(float rotation) 
        {
            _currentRotation = rotation;
        }

        public void MoveTo(Vector2 position)
        {
            _position = position;
        }

        public void MoveBy(Vector2 translation)
        {
            _position += translation;
        }

        public void LookTo(Vector2 direction)
        {
            direction.Normalize();
            _currentRotation = MathF.Sign(direction.Y) * MathF.Acos(direction.X);
        }

        public void SetPivotOffset(Point offset)
        {
            _pivotOffset = offset;
        }

        public void ResetPivotOffset()
        {
            _pivotOffset = Point.Zero;
        }

        public virtual void Move(float deltaTime)
        {
            _position += MoveDirection * CurrentSpeed * deltaTime;
        }

        public void Animate(float deltaTime)
        {
            if (string.IsNullOrEmpty(_currentAnimationName))
                return;

            int previousFrame = (int)MathF.Floor(_currentFrame);
            _currentFrame += deltaTime * _currentAnimation.speed * _animationSpeedMultiplier * _animationDirection;
            if (_animationDirection > 0 && _currentFrame > _currentAnimation.FrameCount
                || _animationDirection < 0 && _currentFrame < 0)
            {
                switch (_currentAnimation.type)
                {
                    case SpriteSheet.AnimationType.Loop:
                        _currentFrame = 0;
                        break;
                    case SpriteSheet.AnimationType.Once:
                        _currentFrame = _currentAnimation.FrameCount - 1;
                        break;
                    case SpriteSheet.AnimationType.PingPong:
                        _currentFrame = Math.Clamp(_currentFrame, 0, _currentAnimation.FrameCount - 1);
                        _animationDirection = -_animationDirection;
                        break;
                }

                if (_onAnimationEnd != null)
                {
                    _onAnimationEnd?.Invoke();
                }
            }
            int newFrame = (int)MathF.Floor(_currentFrame);
            if (previousFrame != newFrame)
            {
                _onAnimationFrame?.Invoke(newFrame);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Animate(deltaTime);
            Move(deltaTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (string.IsNullOrEmpty(_currentAnimationName))
                return;
            _spriteSheet.DrawAnimationFrame(_currentAnimationName, CurrentFrame, SpriteBatch, new Vector2(PixelPositionX + _pivotOffset.X, PixelPositionY + _pivotOffset.Y), _currentRotation, _currentScale, _finalColors);
        }

        public Color GetPixel(int x, int y, int layer = 0)
        {
            if (string.IsNullOrEmpty(_currentAnimationName))
                return new Color(0, 0, 0, 0);

            int scaledX;
            if (_currentScale.X < 0)
                scaledX = SpriteSheet.FrameWidth - x - 1;
            else
                scaledX = x;
            scaledX = (int)MathF.Floor(scaledX * MathF.Abs(_currentScale.X));

            int scaledY;
            if (_currentScale.Y < 0)
                scaledY = SpriteSheet.FrameHeight - y - 1;
            else
                scaledY = y;
            scaledY = (int)MathF.Floor(scaledY * MathF.Abs(_currentScale.Y));

            // TODO: take rotation into account

            return SpriteSheet.GetPixel(_spriteSheet.GetAbsoluteFrameIndex(_currentAnimationName, CurrentFrame), scaledX, scaledY, layer);
        }
    }
}
