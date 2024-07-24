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
        private float _amplitude = 24;

        public Fly(SpriteSheet spriteSheet, SpriteSheet projectileSheet, Game game) : base(spriteSheet, projectileSheet, game)
        {
            SetBaseSpeed(40f);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Vector2 position = new Vector2();
            position.X = Position.X - CurrentSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            position.Y = _spawnPosition.Y + MathF.Sin((float)gameTime.TotalGameTime.TotalSeconds * _ySpeed) * _amplitude;

            MoveTo(position);
        }
    }
}
