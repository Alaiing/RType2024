using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RType2024
{
    public class Level : OudidonGameComponent
    {
        private Texture2D _texture;
        private Color[] _data;

        private float _scrollSpeed = 20;
        public float ScrollSpeed => _scrollSpeed;
        private float _scrollOffset;

        private List<Enemy> _enemyList = new();
        public List<Enemy> EnemyList => _enemyList;

        private static SpriteSheet _flySheet;

        private float _enemyPeriod = 1f;
        private float _enemySpawnTimer;

        public event Action OnLevelReset;

        public Level(Game game, Texture2D texture) : base(game)
        {
            _texture = texture;
            _data = new Color[_texture.Width * _texture.Height];
            _texture.GetData(_data);

            if (_flySheet == null)
            {
                _flySheet = new SpriteSheet(game.Content, "fly", 24, 24, new Point(12, 12));
                _flySheet.RegisterAnimation(Enemy.ANIMATION_IDLE, 0, 0, 1);
            }

            EventsManager.ListenTo<Enemy>(Enemy.EVENT_ENEMY_DIE, OnEnemyDie);

            EnabledChanged += OnLevelEnabledChanged;
        }

        private void OnLevelEnabledChanged(object sender, EventArgs e)
        {
            foreach (Enemy enemy in _enemyList)
            {
                enemy.Enabled = Enabled;
            }
        }

        public void Reset()
        {
            _scrollOffset = 0;
            _enemySpawnTimer = 0;
            ClearEnemies();
            OnLevelReset?.Invoke();
        }

        private void UpdateSpawnEnemy(float deltaTime)
        {
            _enemySpawnTimer += deltaTime;

            if (_enemySpawnTimer > _enemyPeriod)
            {
                _enemySpawnTimer = 0;
                Enemy enemy = new Fly(_flySheet, null, Game);
                enemy.Spawn(new Vector2(RType2024.PLAYGROUND_WIDTH + 50, RType2024.PLAYGROUND_HEIGHT / 2));

                _enemyList.Add(enemy);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            _scrollOffset += _scrollSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            UpdateSpawnEnemy((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch.Draw(_texture, Vector2.Zero, new Rectangle((int)_scrollOffset, 0, RType2024.PLAYGROUND_WIDTH, RType2024.PLAYGROUND_HEIGHT), Color.White);
        }

        public bool IsColliding(Vector2 position, Rectangle collider)
        {

            for (int i = 0; i < collider.Width; i++)
            {
                Vector2 positionInLevel = position + new Vector2(collider.X + i + (int)_scrollOffset, collider.Y);
                if (TestPixelAlpha(positionInLevel))
                    return true;

                positionInLevel = position + new Vector2(collider.X + i + (int)_scrollOffset, collider.Y + collider.Height);
                if (TestPixelAlpha(positionInLevel))
                    return true;
            }

            for (int i = 1; i < collider.Height - 1; i++)
            {
                Vector2 positionInLevel = position + new Vector2(collider.X, collider.Y + i);
                if (TestPixelAlpha(positionInLevel))
                    return true;

                positionInLevel = position + new Vector2(collider.X + collider.Width, collider.Y + i);
                if (TestPixelAlpha(positionInLevel))
                    return true;
            }

            return false;
        }

        private bool TestPixelAlpha(Vector2 positionInLevel)
        {
            int x = (int)positionInLevel.X;
            int y = (int)positionInLevel.Y;

            int index = x + y * _texture.Width;

            Color colorBeneath = _data[index];

            return colorBeneath.A > 0;
        }

        #region Enemies
        private void ClearEnemies()
        {
            foreach (Enemy enemy in _enemyList)
            {
                Game.Components.Remove(enemy);
            }
            _enemyList.Clear();
        }
        #endregion

        #region Events
        private void OnEnemyDie(Enemy enemy)
        {
            _enemyList.Remove(enemy);
        }
        #endregion
    }
}
