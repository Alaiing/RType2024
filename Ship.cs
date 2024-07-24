using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RType2024
{
    public class Ship : Character
    {
        public const string EVENT_SHIP_DESTROYED = "ShipDestroyed";

        private float _movementSpeed = 50f;
        private float _projectileSpeed = 100f;
        private SpriteSheet _baseProjectileSheet;

        private Rectangle _collider = new Rectangle(2,4,28,8);
        public Rectangle Collider => _collider;

        private Level _level;

        public Ship(SpriteSheet spriteSheet, Game game) : base(spriteSheet, game)
        {
            _baseSpeed = _movementSpeed;
            Initialize();
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

        protected override void LoadContent()
        {
            base.LoadContent();

            _baseProjectileSheet = new SpriteSheet(Game.Content, "base-projectile", 12, 4, new Point(12, 2));
            _baseProjectileSheet.RegisterAnimation(Projectile.IDLE_ANIMATION, 0, 0, 1f);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
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

            if (SimpleControls.IsAPressedThisFrame(PlayerIndex.One))
            {
                FireBaseProjectile();
            }

            MoveDirection = new Vector2(xMove, yMove);

            Move((float)gameTime.ElapsedGameTime.TotalSeconds);

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

            if (_level.IsColliding(Position - SpriteSheet.DefaultPivot.ToVector2(), _collider))
            {
                EventsManager.FireEvent(EVENT_SHIP_DESTROYED);
            }
        }

        private void FireBaseProjectile()
        {
            Projectile projectile = new Projectile(_baseProjectileSheet, Game);
            projectile.Spawn(_level, Position + new Vector2(SpriteSheet.RightMargin + 4, 3), new Vector2(1, 0), _projectileSpeed);
            projectile.Activate();
            Game.Components.Add(projectile);
        }
    }
}
