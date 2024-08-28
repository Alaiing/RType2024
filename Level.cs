using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private Ship _ship;

        private List<Enemy> _enemyList = new();
        public List<Enemy> EnemyList => _enemyList;
        private List<Bullet> _bulletList = new();
        public List<Bullet> BulletList => _bulletList;

        private static SpriteSheet _flySheet;
        private static SpriteSheet _turretSheet;
        private static SpriteSheet _bonusWalkerSheet;

        private float _enemyPeriod = 1f;
        private float _enemySpawnTimer;

        public event Action OnLevelReset;

        private struct LevelData
        {
            public string textureName;
            public List<Vector2> turrets;
        }

        private LevelData _levelData;
        private List<Vector2> _currentTurrets;

        public Level(Game game, string levelDataFile, Ship ship) : base(game)
        {
            _ship = ship;
            GetData(levelDataFile);
            _texture = game.Content.Load<Texture2D>(_levelData.textureName);
            _data = new Color[_texture.Width * _texture.Height];
            _texture.GetData(_data);

            if (_flySheet == null)
            {
                _flySheet = new SpriteSheet(game.Content, "fly", 24, 24, new Point(12, 12));
                _flySheet.RegisterAnimation(Enemy.ANIMATION_IDLE, 0, 0, 1);
            }

            if (_turretSheet == null)
            {
                _turretSheet = new SpriteSheet(game.Content, "cannon", 16, 16, new Point(8, 16));
                _turretSheet.RegisterAnimation(Enemy.ANIMATION_IDLE, 0, 0, 1);
            }

            if (_bonusWalkerSheet == null)
            {
                _bonusWalkerSheet = new SpriteSheet(game.Content, "bonus-walker", 16, 24, new Point(8, 18));
                _bonusWalkerSheet.RegisterAnimation(Enemy.ANIMATION_IDLE, 0, 0, 1);
                _bonusWalkerSheet.RegisterAnimation(BonusWalker.ANIMATION_ONGROUND, 1, 1, 1);
            }

            EventsManager.ListenTo<Enemy>(Enemy.EVENT_ENEMY_DIE, OnEnemyDie);

            EnabledChanged += OnLevelEnabledChanged;

            UpdateOrder = 0;
        }

        private void GetData(string levelDataFile)
        {
            if (System.IO.File.Exists(levelDataFile))
            {
                string dataString = System.IO.File.ReadAllText(levelDataFile);
                _levelData = JsonConvert.DeserializeObject<LevelData>(dataString);
            }
        }

        private void OnLevelEnabledChanged(object sender, EventArgs e)
        {
            foreach (Enemy enemy in _enemyList)
            {
                enemy.Enabled = Enabled;
            }

            foreach (Bullet bullet in _bulletList)
            {
                bullet.Enabled = Enabled;
            }
        }

        public void Reset()
        {
            _scrollOffset = 0;
            _enemySpawnTimer = 0;
            ClearEnemies();
            ClearBullets();
            OnLevelReset?.Invoke();
            _currentTurrets = _levelData.turrets.ToList();

            Enemy bonusEnemy = new BonusWalker(_bonusWalkerSheet, Game);
            bonusEnemy.Spawn(new Vector2(2 * RType2024.PLAYGROUND_WIDTH / 3, RType2024.PLAYGROUND_HEIGHT / 3), this);
            _enemyList.Add(bonusEnemy);
        }

        private void UpdateSpawnEnemy(float deltaTime)
        {
            _enemySpawnTimer += deltaTime;

            if (_enemySpawnTimer > _enemyPeriod)
            {
                _enemySpawnTimer = 0;
                Enemy enemy = new Fly(_flySheet, Game);
                enemy.Spawn(new Vector2(RType2024.PLAYGROUND_WIDTH + 50, RType2024.PLAYGROUND_HEIGHT / 2), this);

                _enemyList.Add(enemy);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            _scrollOffset += _scrollSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            UpdateSpawnEnemy((float)gameTime.ElapsedGameTime.TotalSeconds);
            UpdateSpawnTurrets();
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

        public void AddBullet(Bullet bullet)
        {
            _bulletList.Add(bullet);
            Game.Components.Add(bullet);
        }

        public void RemoveBullet(Bullet bullet)
        {
            _bulletList.Remove(bullet);
            Game.Components.Remove(bullet);
        }

        private void ClearBullets()
        {
            foreach (Bullet bullet in _bulletList)
            {
                Game.Components.Remove(bullet);
            }

            _bulletList.Clear();
        }

        private void UpdateSpawnTurrets()
        {
            for (int i = _currentTurrets.Count - 1; i >= 0; i--)
            {
                if (_currentTurrets[i].X <= _scrollOffset + RType2024.PLAYGROUND_WIDTH)
                {
                    Turret newTurret = new Turret(_turretSheet, Game, _ship);

                    newTurret.Spawn(_currentTurrets[i] - new Vector2(_scrollOffset, 0), this);
                    newTurret.SetBaseSpeed(_scrollSpeed);
                    newTurret.MoveDirection = new Vector2(-1, 0);
                    _enemyList.Add(newTurret);
                    _currentTurrets.RemoveAt(i);
                }
            }
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
