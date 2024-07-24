using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RType2024
{
    public class Fly : Enemy
    {
        private float _ySpeed = 2f;
        private float _amplitude = 12;
        private float _timer;

        public Fly(SpriteSheet spriteSheet, SpriteSheet projectileSheet, Game game) : base(spriteSheet, projectileSheet, game)
        {
            SetBaseSpeed(40f);
        }

        public override void Spawn(Vector2 position)
        {
            base.Spawn(position);
            _timer = 0;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 position = new Vector2();
            position.X = Position.X - CurrentSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            float alpha = _timer * _ySpeed;

            position.Y = _spawnPosition.Y + MathF.Sin(alpha) * _amplitude;

            float tangent = MathF.Cos(alpha);

            SetRotation(-MathF.Atan(tangent));
            
            MoveTo(position);
        }
    }
}
