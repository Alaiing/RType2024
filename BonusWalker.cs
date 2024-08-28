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
        public const string BONUS_SPAWN_EVENT = "BonusSpawn";
        public const string ANIMATION_ONGROUND = "OnGround";

        public BonusWalker(SpriteSheet spriteSheet, Game game) : base(spriteSheet, game)
        {
            SetBaseSpeed(50f);
            _collider = new Rectangle(0, 0, spriteSheet.FrameWidth, 18);
        }

        public override void Spawn(Vector2 position, Level level)
        {
            base.Spawn(position, level);
            MoveDirection = new Vector2(-1, 1);
            MoveDirection.Normalize();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (_level.IsColliding(Position - SpriteSheet.DefaultPivot.ToVector2(), _collider))
            {
                SetAnimation(ANIMATION_ONGROUND);
                MoveDirection = new Vector2(-1, 0);
                SetBaseSpeed(_level.ScrollSpeed);
            }
        }

        public override void Die()
        {
            base.Die();
            EventsManager.FireEvent(BONUS_SPAWN_EVENT, Position);
        }

        protected override void BulletUpdate(GameTime gameTime)
        {
            // DO NOTHING
        }
    }
}
