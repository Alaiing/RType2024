using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Oudidon;
using System;
using System.Diagnostics;

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

        private Texture2D _background;

        private SpriteSheet _flySheet;

        private Level _level;

        protected override void Initialize()
        {
            EventsManager.ListenTo(Ship.EVENT_SHIP_DESTROYED, OnShipDestroyed);

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

            _flySheet = new SpriteSheet(Content, "fly", 24, 24, new Point(12, 12));
            _flySheet.RegisterAnimation(Enemy.ANIMATION_IDLE, 0, 0, 1);

            _background = Content.Load<Texture2D>("test-level-background");

            _level = new Level(this, _background);
            Components.Add(_level);

            _ship.SetLevel(_level);

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
            SetState(STATE_SHIP_DESTROYED);
        }
        #endregion

        #region States
        private void GameEnter()
        {
            _level.Reset();
            _ship.MoveTo(new Vector2(160, 100));
            _ship.Activate();
            _level.Enabled = true;

            Enemy enemy = new Fly(_flySheet, null, this);
            enemy.Spawn(new Vector2(PLAYGROUND_WIDTH + 50, PLAYGROUND_HEIGHT / 2));
        }

        private void GameDraw(SpriteBatch batch, GameTime gameTime)
        {
            DrawComponents(gameTime);

            SpriteBatch.FillRectangle(new Rectangle(0, PLAYGROUND_HEIGHT, PLAYGROUND_WIDTH, HUD_HEIGHT), Color.Blue);
        }

        private void GameUpdate(GameTime time, float arg2)
        {
        }

        private void ShipDestroyedEnter()
        {
            _ship.Enabled = false;
            _level.Enabled = false;
        }

        private void ShipDestroyedUpdate(GameTime time, float stateTime)
        {
            if(stateTime > 5f)
            {
                SetState(STATE_GAME);
            }
        }

        #endregion
    }
}
