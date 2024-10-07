using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RType2024
{
    public class Ship : Character
    {
        public const string EVENT_SHIP_DESTROYED = "ShipDestroyed";
        public const string EVENT_GAME_OVER = "GameOver";

        private float _movementSpeed = 50f;
        private float _projectileSpeed = 100f;
        private float _animationSpeed = 16f;
        private float _frameDelta = 0f;

        private Level _level;

        private float _projectileCharge;
        private float _chargeSpeed = 2f;
        private float _maxCharge = 3;
        private bool _isCharging;
        public float ProjectileCharge => _projectileCharge;
        public float MaxCharge => _maxCharge;

        private int _lives;
        public int Lives => _lives;

        private struct ChargeLevel
        {
            public SpriteSheet SpriteSheet;
            public SoundEffect SoundEffect;
            public int Damage;
        }

        private ChargeLevel[] _chargeLevels;

        public Ship(SpriteSheet spriteSheet, Game game) : base(spriteSheet, game)
        {
            _collider = new Rectangle(3, 4, 25, 8);
            _baseSpeed = _movementSpeed;
            Initialize();
            UpdateOrder = 1;
        }

        public void SetLevel(Level level)
        {
            _level = level;
        }

        public override void Initialize()
        {
            base.Initialize();
            DrawOrder = 99;
        }

        public override Rectangle GetBounds()
        {
            return new Rectangle(PixelPositionX - SpriteSheet.LeftMargin + Collider.X, PixelPositionY - SpriteSheet.TopMargin + Collider.Y, Collider.Width, Collider.Height);
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            SpriteSheet baseProjectileSheet = new SpriteSheet(Game.Content, "base-projectile", 12, 4, new Point(12, 2));
            baseProjectileSheet.RegisterAnimation(Projectile.IDLE_ANIMATION, 0, 0, 1f);
            SoundEffect baseProjectileSound = Game.Content.Load<SoundEffect>("piou");

            SpriteSheet projectileSheetPhase2 = new SpriteSheet(Game.Content, "projectile-2", 12, 8, new Point(12, 4));
            projectileSheetPhase2.RegisterAnimation(Projectile.IDLE_ANIMATION, 0, 0, 1f);

            SpriteSheet projectileSheetPhase3 = new SpriteSheet(Game.Content, "projectile-3", 16, 8, new Point(16, 4));
            projectileSheetPhase3.RegisterAnimation(Projectile.IDLE_ANIMATION, 0, 0, 1f);

            SpriteSheet projectileSheetPhase4 = new SpriteSheet(Game.Content, "projectile-4", 24, 12, new Point(24, 6));
            projectileSheetPhase4.RegisterAnimation(Projectile.IDLE_ANIMATION, 0, 3, 10f);

            _chargeLevels = new ChargeLevel[]
            {
                new ChargeLevel { SpriteSheet = baseProjectileSheet, Damage = 1, SoundEffect = baseProjectileSound },
                new ChargeLevel { SpriteSheet = projectileSheetPhase2, Damage = 2, SoundEffect = baseProjectileSound },
                new ChargeLevel { SpriteSheet = projectileSheetPhase3, Damage = 3, SoundEffect = baseProjectileSound },
                new ChargeLevel { SpriteSheet = projectileSheetPhase4, Damage = 4, SoundEffect = baseProjectileSound }
            };

        }

        public override void Reset()
        {
            base.Reset();
            _currentFrame = 2;
            _frameDelta = 0;
            _isCharging = false;
            _projectileCharge = 0;
        }

        public override void Update(GameTime gameTime)
        {
            float xMove = 0;
            float yMove = 0;

            SimpleControls.GetStates();

            if (SimpleControls.IsLeftDown(PlayerIndex.One))
            {
                xMove = -1;
            }
            else if (SimpleControls.IsRightDown(PlayerIndex.One))
            {
                xMove = 1;
            }

            if (SimpleControls.IsUpDown(PlayerIndex.One))
            {
                yMove = -1;
            }
            else if (SimpleControls.IsDownDown(PlayerIndex.One))
            {
                yMove = 1;
            }

            if (_isCharging)
            {
                _projectileCharge = MathF.Min(_maxCharge, _projectileCharge + _chargeSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds);
            }

            if (SimpleControls.IsAPressedThisFrame(PlayerIndex.One))
            {
                _projectileCharge = 0;
                _isCharging = true;
            }

            if (SimpleControls.IsAReleasedThisFrame(PlayerIndex.One))
            {
                FireProjectile(Position + new Vector2(SpriteSheet.RightMargin + 4, 3), _projectileCharge);
                _isCharging = false;
                _projectileCharge = 0;
            }

            MoveDirection = new Vector2(xMove, yMove);

            if (yMove == 0)
            {
                _frameDelta = MathF.Sign(2 - CurrentFrame);
            }
            else
            {
                _frameDelta = MathF.Sign(yMove);
            }

            _currentFrame = Math.Clamp(_currentFrame + _frameDelta * _animationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds, 0, 4);

            Move((float)gameTime.ElapsedGameTime.TotalSeconds);

            TestBorderLimits();

            TestBackgroundCollision();

            TestEnemyCollision();

            TestEnemyBulletCollision();

            TestBonusCollision();
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteSheet.DrawFrame(CurrentFrame, SpriteBatch, new Vector2(PixelPositionX, PixelPositionY), SpriteSheet.DefaultPivot, 0, Vector2.One, Color.White);
            //SpriteBatch.DrawRectangle(GetBounds(), Color.Green);
        }

        private void TestBackgroundCollision()
        {
            if (_level.IsColliding(Position - SpriteSheet.DefaultPivot.ToVector2(), _collider))
            {
                Die();
            }
        }

        private void TestBorderLimits()
        {
            if (Position.X - SpriteSheet.LeftMargin < 0)
            {
                MoveTo(new Vector2(SpriteSheet.LeftMargin, Position.Y));
            }
            else if (Position.X + SpriteSheet.RightMargin > RType2024.PLAYGROUND_WIDTH)
            {
                MoveTo(new Vector2(RType2024.PLAYGROUND_WIDTH - SpriteSheet.RightMargin, Position.Y));
            }

            if (Position.Y - SpriteSheet.TopMargin < 0)
            {
                MoveTo(new Vector2(Position.X, SpriteSheet.TopMargin));
            }
            else if (Position.Y + SpriteSheet.BottomMargin > RType2024.PLAYGROUND_HEIGHT)
            {
                MoveTo(new Vector2(Position.X, RType2024.PLAYGROUND_HEIGHT - SpriteSheet.BottomMargin));
            }
        }

        private void TestEnemyCollision()
        {
            foreach (Enemy enemy in _level.EnemyList)
            {
                if (MathUtils.OverlapsWith(GetBounds(), enemy.GetBounds()))
                {
                    Die();
                }
            }
        }

        private void TestEnemyBulletCollision()
        {
            foreach (Bullet bullet in _level.BulletList)
            {
                if (MathUtils.OverlapsWith(GetBounds(), bullet.GetBounds()))
                {
                    Die();
                }
            }
        }

        private void TestBonusCollision()
        {
            for (int i = _level.BonusList.Count -1; i >= 0; i--)
            {
                Bonus bonus = _level.BonusList[i];
                if (MathUtils.OverlapsWith(GetBounds(), bonus.GetBounds()))
                {
                    bonus.Pickup();
                }
            }
        }

        public void FireProjectile(Vector2 position, float projectileCharge)
        {
            int chargeIndex = 0;
            chargeIndex = (int)MathF.Floor(projectileCharge);

            Projectile projectile = new Projectile(_chargeLevels[chargeIndex].SpriteSheet, Game, _chargeLevels[chargeIndex].SoundEffect, _chargeLevels[chargeIndex].Damage);
            projectile.Spawn(_level, position, new Vector2(1, 0), _projectileSpeed);
            projectile.Activate();
            Game.Components.Add(projectile);
        }

        private void Die()
        {
            _lives--;
            EventsManager.FireEvent(EVENT_SHIP_DESTROYED);
        }

        public void ResetLives()
        {
            _lives = 2;
        }
    }
}
