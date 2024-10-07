using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RType2024
{
    public class Dart : Enemy
    {
        private float _ySpeed = 6f;
        private float _amplitude = 12;
        private float _timer;

        public Dart(SpriteSheet spriteSheet, Game game) : base(spriteSheet, game)
        {
            SetBaseSpeed(40f);
            _collider = new Rectangle(3, 2, 19, 12);
            _bulletSpawnPositionOffset = new Vector2(0, spriteSheet.FrameHeight / 2);
        }

        public override void Spawn(Vector2 position, Level level)
        {
            base.Spawn(position, level);
            _timer = CommonRandom.Random.NextSingle() * MathF.PI * 2 / _ySpeed;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 position = new Vector2();
            position.X = Position.X - CurrentSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            float alpha = _timer * _ySpeed;

            position.Y = _spawnPosition.Y + MathF.Sin(alpha) * _amplitude;

            MoveTo(position);
            Animate((float)gameTime.ElapsedGameTime.TotalSeconds);
        }
    }
}
