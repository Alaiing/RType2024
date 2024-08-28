using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Oudidon;

namespace RType2024
{
    public class Bullet : Character
    {
        public const string SPAWN_BULLET_EVENT = "SpawnBullet";

        public const string ANIMATION_IDLE = "Idle";
        private float _speed = 40f;
        public Bullet(SpriteSheet spriteSheet, Game game) : base(spriteSheet, game)
        {
            _collider = new Rectangle(1, 1, 6, 6);

            SetBaseSpeed(_speed);

            SetAnimation(ANIMATION_IDLE);
        }

        public void Spawn(Vector2 position, Vector2 direction)
        {
            MoveTo(position);
            MoveDirection = direction;
        }
    }
}
