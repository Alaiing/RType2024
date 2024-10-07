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

        private const string STATE_TITLE = "Title";
        private const string STATE_START_GAME = "StartGame";
        private const string STATE_GAME = "Game";
        private const string STATE_SHIP_DESTROYED = "ShipDestroyed";

        private Ship _ship;
        private Pod _pod;

        private Texture2D _background;
        private SpriteSheet _titleSheet;
        private SpriteSheet _digitsSheet;
        private Texture2D _lifeIcon;
        private SpriteSheet _explosionSprite;
        private SoundEffect _explosionSound;
        private SpriteSheet _bulletSprite;

        private SoundEffectInstance _ingameMusicInstance;
        private SoundEffectInstance _introMusicInstance;

        private Level _currentLevel;

        protected override void Initialize()
        {
            EventsManager.ListenTo(Ship.EVENT_SHIP_DESTROYED, OnShipDestroyed);
            EventsManager.ListenTo<Vector2>(Explosion.EVENT_SPAWN_EXPLOSION, OnSpawnExplosion);
            EventsManager.ListenTo<Enemy>(Bullet.SPAWN_BULLET_EVENT, OnSpawnBullet);
            EventsManager.ListenTo(Pod.SPAWN_POD_EVENT, OnSpawnPod);

            base.Initialize();
        }

        protected override void InitStateMachine()
        {
            base.InitStateMachine();
            AddState(STATE_TITLE, onEnter: TitleEnter, onDraw: TitleDraw, onUpdate: TitleUpdate, onExit: TitleExit);
            AddState(STATE_START_GAME, onEnter: StartGameEnter);
            AddState(STATE_GAME, onEnter: GameEnter, onDraw: GameDraw, onUpdate: GameUpdate);
            AddState(STATE_SHIP_DESTROYED, onEnter: ShipDestroyedEnter, onDraw: GameDraw, onUpdate: ShipDestroyedUpdate);
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            _titleSheet = new SpriteSheet(Content, "title", 40,64, new Point(0,32));
            _digitsSheet = new SpriteSheet(Content, "digits", 8, 8, new Point(0,0));
            _lifeIcon = Content.Load<Texture2D>("life-icon");

            SpriteSheet shipSheet = new SpriteSheet(Content, "ship", 32, 16, new Point(16, 8));
            shipSheet.RegisterAnimation("Idle", 0, 0, 1);
            _ship = new Ship(shipSheet, this);
            _ship.SetAnimation("Idle");
            _ship.Deactivate();
            Components.Add(_ship);

            SpriteSheet podSheet = new SpriteSheet(Content, "pod", 24, 24, new Point(12, 12));
            podSheet.RegisterAnimation("Idle", 0, 3, 10);
            _pod = new Pod(podSheet, this, _ship);

            _background = Content.Load<Texture2D>("test-level-background");


            _explosionSprite = new SpriteSheet(Content, "explosion", 16, 16, new Point(8, 8));
            _explosionSprite.RegisterAnimation(Explosion.ANIMATION_IDLE, 0, 13, 30f);

            _bulletSprite = new SpriteSheet(Content, "bullet", 8, 8, new Point(4, 4));
            _bulletSprite.RegisterAnimation(Bullet.ANIMATION_IDLE, 0, 0, 1f);

            _explosionSound = Content.Load<SoundEffect>("prouahou");
            
            _ingameMusicInstance = Content.Load<SoundEffect>("ingame-music").CreateInstance();
            _ingameMusicInstance.IsLooped = true;
            _ingameMusicInstance.Volume = 0.5f;

            _introMusicInstance = Content.Load<SoundEffect>("intro-music").CreateInstance();
            _introMusicInstance.IsLooped = true;

            _currentLevel = new Level(this, "level-test.data", _ship);
            Components.Add(_currentLevel);
            _currentLevel.Enabled = _currentLevel.Visible = false;


            SetState(STATE_TITLE);
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
            Explosion newExplosion = new Explosion(_explosionSprite, _explosionSound, this, _currentLevel);
            newExplosion.Spawn(position);
        }

        private void OnSpawnBullet(Enemy enemy)
        {
            Bullet newBullet = new Bullet(_bulletSprite, this);
            Vector2 direction = _ship.Position - enemy.Position;
            direction.Normalize();
            newBullet.Spawn(enemy.GetBulletSpawnPosition(), direction);
            _currentLevel.AddBullet(newBullet);
        }

        private void OnSpawnPod()
        {
            if (_pod.IsSpawned)
            {
                _pod.IncreaseRank();
            }
            else
            {
                _pod.Spawn();
            }
        }

        #endregion

        #region States
        private bool _titleArriving;
        private float _arriveDuration = 1f;
        private float _titleTimer;
        private float _spreadDuration = 1f;
        private bool _introDone;
        private void TitleEnter()
        {
            if (_currentLevel != null)
            {
                _currentLevel.Enabled = _currentLevel.Visible = false;
            }
            _titleArriving = true;
            _titleTimer = 0;
            _introDone = false;

            _ingameMusicInstance.Stop();
            _introMusicInstance.Stop();
            _introMusicInstance.Play();
        }

        private void TitleDraw(SpriteBatch batch, GameTime time)
        {
            float yPos = 100 - _titleSheet.FrameHeight;
            float baseXPos = 160 + _titleSheet.LeftMargin;
            if (_titleArriving)
            {
                for (int i = 0; i < _titleSheet.FrameCount; i++)
                {
                    _titleSheet.DrawFrame(i, batch, new Vector2(MathHelper.Lerp(- _titleSheet.RightMargin, baseXPos, _titleTimer / _arriveDuration), yPos), _titleSheet.DefaultPivot, 0, Vector2.One, Color.White);
                }
                _titleTimer += (float)time.ElapsedGameTime.TotalSeconds;

                if (_titleTimer > _arriveDuration)
                {
                    _titleArriving = false;
                    _titleTimer = 0;
                }
            }
            else
            {
                for (int i = 0; i < _titleSheet.FrameCount; i++)
                {
                    float targetXPos = baseXPos + (i - 3) * (_titleSheet.FrameWidth + 2);
                    float xPos = MathHelper.Lerp(baseXPos, targetXPos,  _titleTimer / _spreadDuration);

                    _titleSheet.DrawFrame(i, batch, new Vector2(xPos, yPos), _titleSheet.DefaultPivot, 0, Vector2.One, Color.White);
                }
                _titleTimer = Math.Min(_spreadDuration, _titleTimer + (float)time.ElapsedGameTime.TotalSeconds);
                if (_titleTimer >= _spreadDuration)
                {
                    _introDone = true;
                }
            }
        }

        private void TitleUpdate(GameTime time, float stateTime)
        {
            if (!_introDone)
            {
                return;
            }

            SimpleControls.GetStates();
            if (SimpleControls.IsAPressedThisFrame(PlayerIndex.One))
            {
                SetState(STATE_START_GAME);
            }
        }

        private void TitleExit()
        {
            _introMusicInstance.Stop();
        }

        private void StartGameEnter()
        {
            _ship.ResetLives();
            SetState(STATE_GAME);
        }

        private void GameEnter()
        {
            SetTimeScale(1f);
            _currentLevel.Enabled = _currentLevel.Visible = true;
            _currentLevel.Reset();
            _ship.Reset();
            _ship.MoveTo(new Vector2(160, 100));
            _ship.Activate();
            _currentLevel.Enabled = true;
            _ingameMusicInstance.Stop();
            _ingameMusicInstance.Play();

            _ship.SetLevel(_currentLevel);
            _pod.SetLevel(_currentLevel);
        }

        private Rectangle chargeGauge = new Rectangle(PLAYGROUND_WIDTH / 2 - 32, PLAYGROUND_HEIGHT + 2, 64, 8);
        private void GameDraw(SpriteBatch batch, GameTime gameTime)
        {
            DrawComponents(gameTime);

            Rectangle fillGauge = chargeGauge;
            fillGauge.Width = (int)(_ship.ProjectileCharge * chargeGauge.Width / _ship.MaxCharge);
            SpriteBatch.FillRectangle(fillGauge, Color.Cyan);
            SpriteBatch.DrawRectangle(chargeGauge, Color.Blue);

            for (int i = 0; i < _ship.Lives; i++)
            {
                SpriteBatch.Draw(_lifeIcon, new Vector2(100 - i * (_lifeIcon.Width + 1), PLAYGROUND_HEIGHT + 12), Color.White);
            }

        }

        private void GameUpdate(GameTime gameTime, float stateTime)
        {
        }

        private void ShipDestroyedEnter()
        {
            _ship.Enabled = false;
            _pod.Despawn();
            _currentLevel.Enabled = false;
        }

        private void ShipDestroyedUpdate(GameTime time, float stateTime)
        {
            if (stateTime > 2f)
            {
                if (_ship.Lives >= 0)
                {
                    SetState(STATE_GAME);
                }
                else
                {
                    SetState(STATE_TITLE);
                }
            }
        }


        #endregion
    }
}
