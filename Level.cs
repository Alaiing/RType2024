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
        public float ScrollOffset => _scrollOffset;

        private Ship _ship;

        private List<Enemy> _enemyList = new();
        public List<Enemy> EnemyList => _enemyList;
        private List<Bullet> _bulletList = new();
        public List<Bullet> BulletList => _bulletList;
        private List<Bonus> _bonusList = new();
        public List<Bonus> BonusList => _bonusList;

        private static SpriteSheet _flySheet;
        private static SpriteSheet _dartSheet;
        private static SpriteSheet _turretSheet;
        private static SpriteSheet _bonusWalkerSheet;
        private static SpriteSheet _podBonusSheet;

        private float _enemyPeriod = 1f;
        private float _enemySpawnTimer;

        public event Action OnLevelReset;

        private enum EnemyType { Fly, Dart, BonusWalker }

        private struct TurretData
        {
            public Vector2 position;
            public bool top;
        }

        private struct EnemyData
        {
            public EnemyType type;
            public Vector2 position;
            public int amount;
        }

        private struct LevelData
        {
            public string textureName;
            public List<TurretData> turrets;
            public List<EnemyData> enemies;
        }

        private LevelData _levelData;
        private List<TurretData> _currentTurrets;

        private class EnemySpawnData
        {
            public Vector2 position;
            public EnemyType enemyType;
            public int amount;
            public float timer;
        }
        private List<EnemySpawnData> _currentEnemiesSpawnData;

        public Level(Game game, string levelDataFile, Ship ship) : base(game)
        {
            _ship = ship;
            GetData(levelDataFile);
            _texture = game.Content.Load<Texture2D>(_levelData.textureName);
            _data = new Color[_texture.Width * _texture.Height];
            _texture.GetData(_data);

            _currentEnemiesSpawnData = new();

            if (_flySheet == null)
            {
                _flySheet = new SpriteSheet(game.Content, "fly", 24, 24, new Point(12, 12));
                _flySheet.RegisterAnimation(Enemy.ANIMATION_IDLE, 0, 0, 1);
            }

            if (_dartSheet == null)
            {
                _dartSheet = new SpriteSheet(game.Content, "dart", 24, 16, new Point(12, 8));
                _dartSheet.RegisterAnimation(Enemy.ANIMATION_IDLE, 0, 3, 4);
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

            if (_podBonusSheet == null)
            {
                _podBonusSheet = new SpriteSheet(game.Content, "pod-bonus", 16, 16, new Point(8, 8));
                _podBonusSheet.RegisterAnimation(Enemy.ANIMATION_IDLE, 0, 0, 1);
            }

            EventsManager.ListenTo<Enemy>(Enemy.EVENT_ENEMY_DIE, OnEnemyDie);
            EventsManager.ListenTo<Vector2>(BonusWalker.POD_BONUS_SPAWN_EVENT, OnPodBonusSpawn);

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
            GenerateEnemySpawnData();
        }

        private void GenerateEnemySpawnData()
        {
            _currentEnemiesSpawnData.Clear();
            foreach (EnemyData enemyData in _levelData.enemies)
            {
                EnemySpawnData enemySpawnData = new EnemySpawnData()
                {
                    position = enemyData.position,
                    enemyType = enemyData.type,
                    amount = enemyData.amount,
                    timer = 0f
                };
                _currentEnemiesSpawnData.Add(enemySpawnData);
            }
        }

        private void UpdateSpawnEnemy(float deltaTime)
        {
            for (int i = _currentEnemiesSpawnData.Count - 1; i >= 0; i--)
            {
                if (_currentEnemiesSpawnData[i].position.X <= _scrollOffset + RType2024.PLAYGROUND_WIDTH)
                {
                    if (_currentEnemiesSpawnData[i].timer <= 0)
                    {
                        Vector2 spawnPosition = new Vector2(RType2024.PLAYGROUND_WIDTH + 50, _currentEnemiesSpawnData[i].position.Y);
                        Enemy enemy = null;
                        switch (_currentEnemiesSpawnData[i].enemyType)
                        {
                            case EnemyType.Fly:
                                enemy = new Fly(_flySheet, Game);
                                break;

                            case EnemyType.Dart:
                                enemy = new Dart(_dartSheet, Game);
                                break;

                            case EnemyType.BonusWalker:
                                enemy = new BonusWalker(_bonusWalkerSheet, Game);
                                break;
                        }

                        if (enemy == null)
                            continue;

                        enemy.Spawn(spawnPosition, this);

                        _enemyList.Add(enemy);

                        _currentEnemiesSpawnData[i].amount--;

                        if (_currentEnemiesSpawnData[i].amount <= 0)
                        {
                            _currentEnemiesSpawnData.RemoveAt(i);
                            continue;
                        }

                        _currentEnemiesSpawnData[i].timer = _enemyPeriod;
                        continue;
                    }

                    _currentEnemiesSpawnData[i].timer -= deltaTime;
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            _scrollOffset += _scrollSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            UpdateSpawnEnemy((float)gameTime.ElapsedGameTime.TotalSeconds);
            UpdateSpawnTurrets();
            ClearOutOfBoundsEnemies();
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
                if (IsPixelColliding(positionInLevel))
                    return true;

                positionInLevel = position + new Vector2(collider.X + i + (int)_scrollOffset, collider.Y + collider.Height);
                if (IsPixelColliding(positionInLevel))
                    return true;
            }

            for (int i = 1; i < collider.Height - 1; i++)
            {
                Vector2 positionInLevel = position + new Vector2(collider.X, collider.Y + i);
                if (IsPixelColliding(positionInLevel))
                    return true;

                positionInLevel = position + new Vector2(collider.X + collider.Width, collider.Y + i);
                if (IsPixelColliding(positionInLevel))
                    return true;
            }

            return false;
        }

        public bool IsPixelColliding(Vector2 positionInLevel)
        {
            int x = (int)positionInLevel.X;
            int y = (int)positionInLevel.Y;

            int index = x + y * _texture.Width;

            Color colorBeneath = _data[index];

            return colorBeneath.A > 0;
        }

        public bool IsOnScreen(Vector2 positionInLevel)
        {
            return positionInLevel.X - _scrollOffset > 0 && positionInLevel.X - _scrollOffset < RType2024.PLAYGROUND_WIDTH;
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
                if (_currentTurrets[i].position.X <= _scrollOffset + RType2024.PLAYGROUND_WIDTH)
                {
                    Turret newTurret = new Turret(_turretSheet, Game, _ship);

                    newTurret.Spawn(_currentTurrets[i].position - new Vector2(_scrollOffset, 0), this);
                    newTurret.SetBaseSpeed(_scrollSpeed);
                    newTurret.MoveDirection = new Vector2(-1, 0);
                    newTurret.SetScale(new Vector2(1, _currentTurrets[i].top ? -1 : 1));
                    _enemyList.Add(newTurret);
                    _currentTurrets.RemoveAt(i);
                }
            }
        }

        private void ClearOutOfBoundsEnemies()
        {
            for (int i = _enemyList.Count - 1; i >= 0; i--)
            {
                Enemy enemy = _enemyList[i];
                if (enemy.Position.X + enemy.SpriteSheet.RightMargin < 0)
                {
                    Game.Components.Remove(enemy);
                    _enemyList.RemoveAt(i);
                }
            }
        }
        #endregion

        #region Events
        private void OnEnemyDie(Enemy enemy)
        {
            _enemyList.Remove(enemy);
        }

        private void OnPodBonusSpawn(Vector2 position)
        {
            Bonus podBonus = new Bonus(_podBonusSheet, Game);
            podBonus.Spawn(this, position, () => EventsManager.FireEvent(Pod.SPAWN_POD_EVENT));
            _bonusList.Add(podBonus);
        }
        #endregion
    }
}
