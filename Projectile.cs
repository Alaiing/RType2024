using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
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

        private SoundEffectInstance _soundInstance;

        private int _damage;

        public Projectile(SpriteSheet spriteSheet, Game game, SoundEffect sound, int damage) : base(spriteSheet, game)
        {
            SetAnimation(IDLE_ANIMATION);
            _collider = new Rectangle(0, 0, spriteSheet.FrameWidth, spriteSheet.FrameHeight);
            _soundInstance = sound.CreateInstance();
            _damage = damage;
        }

        public void Spawn(Level level, Vector2 position, Vector2 direction, float speed)
        {
            _level = level;
            _level.EnabledChanged += OnLevelEnableChanged;
            _level.OnLevelReset += OnLevelReset;
            SetBaseSpeed(speed);
            MoveTo(position);
            MoveDirection = direction;
            _soundInstance.Play();
        }

        private void OnLevelReset()
        {
            Die();
        }

        private void OnLevelEnableChanged(object sender, EventArgs e)
        {
            Enabled = _level.Enabled;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Move((float)gameTime.ElapsedGameTime.TotalSeconds);

            TestBackgroundCollision();

            TestEnemyCollision();
        }

        private void TestBackgroundCollision()
        {
            if (Position.X - SpriteSheet.LeftMargin > RType2024.PLAYGROUND_WIDTH
                || _level.IsColliding(Position - SpriteSheet.DefaultPivot.ToVector2(), _collider))

            {
                Die();
            }
        }

        private void TestEnemyCollision()
        {
            foreach (Enemy enemy in _level.EnemyList)
            {
                if (MathUtils.OverlapsWith(GetBounds(), enemy.GetBounds()))
                {
                    enemy.TakeHit(_damage);
                    Die();
                    return;
                }
            }
        }

        private void Die()
        {
            Game.Components.Remove(this);
        }
    }
}
