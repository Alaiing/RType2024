using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RType2024
{
    public class Turret : Enemy
    {
        private Ship _ship;
        public Turret(SpriteSheet spriteSheet, Game game, Ship ship) : base(spriteSheet, game)
        {
            _ship = ship;
            _bulletSpawnPositionOffset = new Vector2(12,5);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            float deltaX = _ship.Position.X - _position.X;

            SetScale(new Vector2(MathF.Sign(deltaX), _currentScale.Y));
        }
    }
}
