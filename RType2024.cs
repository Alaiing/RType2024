using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Oudidon;
using System;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace RType2024
{
    public class RType2024 : OudidonGame
    {
        public const int PLAYGROUND_WIDTH = 320;
        public const int PLAYGROUND_HEIGHT = 200 - HUD_HEIGHT;
        public const int HUD_HEIGHT = 24;

        private const string STATE_GAME = "Game";
        private const string STATE_SHIP_DESTROYED = "ShipDestroyed";

        private Ship _ship;
        private Pod _pod;

        private Texture2D _background;
        private SpriteSheet _explosionSprite;
        private SoundEffect _explosionSound;
        private SpriteSheet _bulletSprite;

        private SoundEffectInstance _ingameMusicInstance;

        private Level _level;

        protected override void Initialize()
        {
            EventsManager.ListenTo(Ship.EVENT_SHIP_DESTROYED, OnShipDestroyed);
            EventsManager.ListenTo<Vector2>(Explosion.EVENT_SPAWN_EXPLOSION, OnSpawnExplosion);
            EventsManager.ListenTo<Enemy>(Bullet.SPAWN_BULLET_EVENT, OnSpawnBullet);

            base.Initialize();
        }

        protected override void InitStateMachine()
        {
            base.InitStateMachine();
            AddState(STATE_GAME, onEnter: GameEnter, onDraw: GameDraw, onUpdate: GameUpdate);
            AddState(STATE_SHIP_DESTROYED, onEnter: ShipDestroyedEnter, onDraw: GameDraw, onUpdate: ShipDestroyedUpdate);
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            SpriteSheet shipSheet = new SpriteSheet(Content, "ship", 32, 16, new Point(16, 8));
            shipSheet.RegisterAnimation("Idle", 0, 0, 1);
            _ship = new Ship(shipSheet, this);
            _ship.SetAnimation("Idle");
            _ship.Deactivate();
            Components.Add(_ship);

            SpriteSheet podSheet = new SpriteSheet(Content, "pod", 24, 24, new Point(12, 12));
            podSheet.RegisterAnimation("Idle", 0, 0, 1);
            _pod = new Pod(podSheet, this, _ship);

            _background = Content.Load<Texture2D>("test-level-background");

            _explosionSprite = new SpriteSheet(Content, "explosion", 16, 16, new Point(8, 8));
            _explosionSprite.RegisterAnimation(Explosion.ANIMATION_IDLE, 0, 13, 30f);

            _bulletSprite = new SpriteSheet(Content, "bullet", 8, 8, new Point(4, 4));
            _bulletSprite.RegisterAnimation(Bullet.ANIMATION_IDLE, 0, 0, 1f);

            _explosionSound = Content.Load<SoundEffect>("prouahou");
            
            _ingameMusicInstance = Content.Load<SoundEffect>("ingame-music").CreateInstance();
            _ingameMusicInstance.IsLooped = true;
            _ingameMusicInstance.Volume = 0.75f;

            _level = new Level(this, "level-test.data", _ship);
            Components.Add(_level);

            SetState(STATE_GAME);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime); // Updates state machine and components, in that order
        }

        protected override void DrawGameplay(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);


            SpriteBatch.Begin(samplerState: SamplerState.PointWrap, blendState: BlendState.AlphaBlend);

            base.DrawGameplay(gameTime); // Draws state machine

            SpriteBatch.End();
        }

        protected override void DrawUI(GameTime gameTime)
        {
            // TODO: Draw your overlay UI here
        }

        #region Events
        private void OnShipDestroyed()
        {
            EventsManager.FireEvent(Explosion.EVENT_SPAWN_EXPLOSION, _ship.Position);
            SetState(STATE_SHIP_DESTROYED);
        }

        private void SpawnExplosion(Vector2 position)
        {
            EventsManager.FireEvent(Explosion.EVENT_SPAWN_EXPLOSION, position);
        }

        private void OnSpawnExplosion(Vector2 position)
        {
            Explosion newExplosion = new Explosion(_explosionSprite, _explosionSound, this, _level);
            newExplosion.Spawn(position);
        }

        private void OnSpawnBullet(Enemy enemy)
        {
            Bullet newBullet = new Bullet(_bulletSprite, this);
            Vector2 direction = _ship.Position - enemy.Position;
            direction.Normalize();
            newBullet.Spawn(enemy.GetBulletSpawnPosition(), direction);
            _level.AddBullet(newBullet);
        }

        #endregion

        #region States
        private void GameEnter()
        {
            _level.Reset();
            _ship.Reset();
            _ship.MoveTo(new Vector2(160, 100));
            _ship.Activate();
            _level.Enabled = true;
            //_ingameMusicInstance.Stop();
            //_ingameMusicInstance.Play();

            _pod.Spawn();

            _ship.SetLevel(_level);
            _pod.SetLevel(_level);
        }

        private Rectangle chargeGauge = new Rectangle(PLAYGROUND_WIDTH / 2 - 32, PLAYGROUND_HEIGHT + 2, 64, 8);
        private void GameDraw(SpriteBatch batch, GameTime gameTime)
        {
            DrawComponents(gameTime);

            Rectangle fillGauge = chargeGauge;
            fillGauge.Width = (int)(_ship.ProjectileCharge * chargeGauge.Width / _ship.MaxCharge);
            SpriteBatch.FillRectangle(fillGauge, Color.Cyan);
            SpriteBatch.DrawRectangle(chargeGauge, Color.Blue);

        }

        private void GameUpdate(GameTime gameTime, float stateTime)
        {
        }

        private void ShipDestroyedEnter()
        {
            _ship.Enabled = false;
            _pod.Despawn();
            _level.Enabled = false;
        }

        private void ShipDestroyedUpdate(GameTime time, float stateTime)
        {
            if (stateTime > 2f)
            {
                SetState(STATE_GAME);
            }
        }


        #endregion
    }
}
