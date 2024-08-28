using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RType2024
{
    public class Pod : Character
    {
        private enum AttachedMode { None, Front, Back }

        private const float CHASING_SPEED = 50f;
        private const float DETACH_SPEED = 200f;

        private Ship _ship;
        private Level _level;

        private Vector2 _startingPosition;
        private Vector2 _targetPosition;
        private bool _isChasingShipPosition;

        private float _chasingSpeed = 50f;
        private float _chasingDuration = 1f;
        private float _chasingTimer;

        private AttachedMode _attachedMode;

        public Pod(SpriteSheet spriteSheet, Game game, Ship ship) : base(spriteSheet, game)
        {
            _collider = new Rectangle(4,3,16,18);
            _ship = ship;
            SetAnimation("Idle");
            Game.Components.Add(this);
            Deactivate();
            DrawOrder = 99;
            UpdateOrder = 2;
        }

        public void SetLevel(Level level)
        {
            _level = level;
        }

        public void Spawn()
        {
            MoveTo(new Vector2(-50, RType2024.PLAYGROUND_HEIGHT / 2));
            Activate();
            SetNewTargetPosition(GetTargetPositionFromShip(), CHASING_SPEED);
        }

        public void Despawn()
        {
            DetachFromShip();
            Deactivate();
        }

        private void AttachToShip(bool front)
        {
            _attachedMode = front ? AttachedMode.Front : AttachedMode.Back;
        }

        private void DetachFromShip()
        {
            Vector2 targetPosition = Position + (_attachedMode == AttachedMode.Front ? 1 : -1) * new Vector2(100, 0);
            SetNewTargetPosition(targetPosition, DETACH_SPEED);
            _attachedMode = AttachedMode.None;
        }

        public override void Update(GameTime gameTime)
        {
            if (_attachedMode != AttachedMode.None)
            {
                Vector2 newPosition = _ship.Position;
                newPosition.X += _attachedMode == AttachedMode.Front ? 22 : -22;
                MoveTo(newPosition);
                if (SimpleControls.IsBDown(PlayerIndex.One))
                {
                    DetachFromShip();
                }
            }
            else
            {
                _chasingTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (_chasingTimer > _chasingDuration)
                {
                    SetNewTargetPosition(GetTargetPositionFromShip(), CHASING_SPEED);
                }

                Vector2 newPosition = Vector2.Lerp(_startingPosition, _targetPosition, _chasingTimer / _chasingDuration);
                MoveTo(newPosition);

                if (MathUtils.OverlapsWith(GetBounds(), _ship.GetBounds()))
                {
                    AttachToShip(MathF.Sign(Position.X - _ship.Position.X) >= 0);
                }

                if (SimpleControls.IsAReleasedThisFrame(PlayerIndex.One))
                {
                    _ship.FireProjectile(Position + new Vector2(8,0), 0);
                }
            }

            base.Update(gameTime);

            TestEnemyBulletCollision();
        }

        private void SetNewTargetPosition(Vector2 targetPosition, float speed)
        {
            _chasingTimer = 0;
            _chasingSpeed = speed;
            _startingPosition = Position;
            _targetPosition = targetPosition;
            _chasingDuration = Vector2.Distance(_targetPosition, _startingPosition) / _chasingSpeed;
        }

        private Vector2 GetTargetPositionFromShip()
        {
            return new Vector2(RType2024.PLAYGROUND_WIDTH - _ship.Position.X, RType2024.PLAYGROUND_HEIGHT - _ship.Position.Y);
        }

        private void TestEnemyBulletCollision()
        {
            for (int i = _level.BulletList.Count-1; i >=0; i--)
            {
                Bullet bullet = _level.BulletList[i];
                if (MathUtils.OverlapsWith(GetBounds(), bullet.GetBounds()))
                {
                    _level.RemoveBullet(bullet);
                }
            }
        }
    }
}
