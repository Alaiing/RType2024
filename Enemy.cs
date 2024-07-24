using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RType2024
{
    public class Enemy : Character
    {
        public const string ANIMATION_IDLE = "Idle";
        public const string EVENT_ENEMY_DIE = "EnemyDie";

        protected virtual int maxHP => 1;
        private int _currentHP;

        protected SpriteSheet _projectileSheet;
        protected Vector2 _spawnPosition;

        public Enemy(SpriteSheet spriteSheet, SpriteSheet projectileSheet, Game game) : base(spriteSheet, game)
        {
            _projectileSheet = projectileSheet;
            game.Components.Add(this);
            Deactivate();
            SetAnimation(ANIMATION_IDLE);
        }

        public virtual void Spawn(Vector2 position)
        {
            _spawnPosition = position;
            MoveTo(position);
            Activate();
        }

        public void TakeHit(int amount)
        {
            _currentHP -= amount;
            if (_currentHP < 0)
            {
                Die();
            }
        }

        public void Die()
        {
            EventsManager.FireEvent(EVENT_ENEMY_DIE, this);
            EventsManager.FireEvent(Explosion.EVENT_SPAWN_EXPLOSION, Position);
            Game.Components.Remove(this);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            //SpriteBatch.DrawRectangle(GetBounds(), Color.Green);
        }
    }
}
