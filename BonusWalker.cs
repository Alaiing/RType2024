using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RType2024
{
    public class BonusWalker : Enemy
    {
        public const string POD_BONUS_SPAWN_EVENT = "BonusSpawn";
        public const string ANIMATION_ONGROUND = "OnGround";

        private const string STATE_FALL = "Fall";
        private const string STATE_JUMP = "Jump";
        private const string STATE_SIT = "Sit";

        private SimpleStateMachine _stateMachine;

        public BonusWalker(SpriteSheet spriteSheet, Game game) : base(spriteSheet, game)
        {
            SetBaseSpeed(50f);
            _collider = new Rectangle(0, 0, spriteSheet.FrameWidth, 18);

            _stateMachine = new SimpleStateMachine();
            _stateMachine.AddState(STATE_FALL, OnEnter: FallEnter, OnUpdate: FallUpdate);
            _stateMachine.AddState(STATE_JUMP, OnEnter: JumpEnter, OnUpdate: JumpUpdate);
            _stateMachine.AddState(STATE_SIT, OnEnter: SitEnter, OnUpdate: SitUpdate);
        }

        public override void Spawn(Vector2 position, Level level)
        {
            base.Spawn(position, level);
            _stateMachine.SetState(STATE_FALL);
        }

        public override void Die()
        {
            base.Die();
            EventsManager.FireEvent(POD_BONUS_SPAWN_EVENT, Position);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            _stateMachine.Update(gameTime);
        }

        protected override void BulletUpdate(GameTime gameTime)
        {
            // DO NOTHING
        }

        #region States
        private void FallEnter()
        {
            SetAnimation(ANIMATION_IDLE);
            MoveDirection = new Vector2(-1, 1);
            MoveDirection.Normalize();
        }

        private void FallUpdate(GameTime time, float arg2)
        {
            if (_level.IsColliding(Position - SpriteSheet.DefaultPivot.ToVector2(), _collider))
            {
                _stateMachine.SetState(STATE_SIT);
            }
        }

        private void SitEnter()
        {
            SetAnimation(ANIMATION_ONGROUND);
            MoveDirection = new Vector2(-1, 0);
            SetBaseSpeed(_level.ScrollSpeed);
        }

        private void SitUpdate(GameTime time, float stateTime)
        {
            if (stateTime > 2f)
            {
                _stateMachine.SetState(STATE_JUMP);
            }
        }
        private void JumpEnter()
        {
            SetAnimation(ANIMATION_IDLE);
            MoveDirection = new Vector2(-1, -1);
            MoveDirection.Normalize();
            SetBaseSpeed(50f);
        }

        private void JumpUpdate(GameTime time, float stateTime)
        {
            if (stateTime > 2f)
            {
                _stateMachine.SetState(STATE_FALL);
            }
        }
        #endregion
    }
}
