using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RType2024
{
    public class Projectile : Character
    {
        public const string IDLE_ANIMATION = "Idle";

        private Level _level;

        private Rectangle _collider;

        public Projectile(SpriteSheet spriteSheet, Game game) : base(spriteSheet, game)
        {
            SetAnimation(IDLE_ANIMATION);
            _collider = new Rectangle(0,0, spriteSheet.FrameWidth, spriteSheet.FrameHeight);
        }

        public void Spawn(Level level, Vector2 position, Vector2 direction, float speed)
        {
            _level = level;
            SetBaseSpeed(speed);
            MoveTo(position);
            MoveDirection = direction;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Move((float)gameTime.ElapsedGameTime.TotalSeconds);

            if (Position.X - SpriteSheet.LeftMargin > RType2024.PLAYGROUND_WIDTH
                || _level.IsColliding(Position - SpriteSheet.DefaultPivot.ToVector2(), _collider))

            {
                Game.Components.Remove(this);
            }

            // TODO: test collisions && out of bounds
        }
    }
}
