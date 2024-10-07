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
        private float _currentHP;

        protected Vector2 _spawnPosition;

        protected float _bulletCooldown = 10;
        protected float _bulletTimer;
        protected Vector2 _bulletSpawnPositionOffset;
        public Vector2 BulletOffset => _bulletSpawnPositionOffset;

        protected Level _level;

        public Enemy(SpriteSheet spriteSheet, Game game) : base(spriteSheet, game)
        {
            game.Components.Add(this);
            Deactivate();
            SetAnimation(ANIMATION_IDLE);
        }

        public virtual void Spawn(Vector2 position, Level level)
        {
            _level = level;
            _spawnPosition = position;
            _currentHP = maxHP;
            MoveTo(position);
            Activate();
            _bulletTimer = CommonRandom.Random.NextSingle() * _bulletCooldown;
        }

        public float TakeHit(float amount)
        {
            _currentHP -= amount;
            if (_currentHP <= 0)
            {
                Die();
            }

            return -_currentHP;
        }

        protected void FireBullet()
        {
            EventsManager.FireEvent(Bullet.SPAWN_BULLET_EVENT, this);
        }

        public Vector2 GetBulletSpawnPosition()
        {
            Vector2 spawnOffset = _bulletSpawnPositionOffset;
            if (CurrentScale.X < 0)
            {
                spawnOffset.X = SpriteSheet.FrameWidth - spawnOffset.X;
            }

            return _position - _spriteSheet.DefaultPivot.ToVector2() + spawnOffset;
        }

        public virtual void Die()
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

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            BulletUpdate(gameTime);
        }

        protected virtual void BulletUpdate(GameTime gameTime)
        {
            _bulletTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_bulletTimer > _bulletCooldown)
            {
                _bulletTimer = 0;
                FireBullet();
            }
        }
    }
}
