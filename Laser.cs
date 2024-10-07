using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RType2024
{
    public class Laser : OudidonGameComponent
    {
        private const float speed = 100f;
        private const float maxDistance = 100f;
        private const float damagePerSecond = 2f;
        private const int maxRebounds = 3;

        private Color _color = new Color(54, 30, 185);
        private Vector2 _worldPosition;
        private Vector2 _moveDirection;

        private Level _level;

        private List<Vector2> _positions = new();
        private List<Vector2> _activePositions = new();

        private int _rebounds;

        public Laser(Game game, Level level) : base(game)
        {
            _level = level;
        }

        public void Spawn(Vector2 position, Vector2 direction)
        {
            _moveDirection = direction;
            _worldPosition = position + new Vector2(_level.ScrollOffset, 0);
            Game.Components.Add(this);
            _positions.Clear();
            _positions.Add(_worldPosition);
            _rebounds = 0;
        }

        public void Despawn()
        {
            Game.Components.Remove(this);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Vector2 nextPosition = _worldPosition + _moveDirection * speed * Game.DeltaTime;

            bool directionChanged = false;
            if (_level.IsPixelColliding(new Vector2(MathF.Floor(nextPosition.X), MathF.Floor(_worldPosition.Y))))
            {
                _moveDirection.X = -_moveDirection.X;
                directionChanged = true;
            }
            if (_level.IsPixelColliding(new Vector2(MathF.Floor(_worldPosition.X), MathF.Floor(nextPosition.Y))))
            {
                _moveDirection.Y = -_moveDirection.Y;
                directionChanged = true;
            }

            if (directionChanged)
            {
                _rebounds++;
                _positions.Add(_worldPosition);
            }

            _worldPosition += _moveDirection * speed * Game.DeltaTime;

            ComputeSegments();

            TestEnemyCollisions();
        }

        private void ComputeSegments()
        {
            Vector2 lastPosition = _worldPosition;
            float currentMaxdistance = maxDistance;

            _activePositions.Clear();
            _activePositions.Add(_worldPosition);

            for (int i = _positions.Count - 1; i >= 0; i--)
            {
                float distance = Vector2.Distance(lastPosition, _positions[i]);
                if (currentMaxdistance - distance < 0)
                {
                    float remainingDistance = distance - maxDistance;
                    Vector2 direction = _positions[i] - lastPosition;
                    direction.Normalize();
                    direction *= currentMaxdistance;

                    _activePositions.Add(lastPosition + direction);

                    break;
                }
                else
                {
                    currentMaxdistance -= distance;
                    _activePositions.Add(_positions[i]);
                    lastPosition = _positions[i];
                }
            }
        }

        private void TestEnemyCollisions(bool debug = false)
        {
            for (int i = _level.EnemyList.Count - 1; i >= 0; i--)
            {
                Rectangle enemyBounds = _level.EnemyList[i].GetBounds();
                enemyBounds.X += (int)_level.ScrollOffset;

                for (int j = 1; j < _activePositions.Count; j++)
                {
                    Vector2 firstPoint = _activePositions[j - 1];
                    Vector2 secondPoint = _activePositions[j];

                    Rectangle segmentBoundingBox = new Rectangle((int)Math.Min(firstPoint.X, secondPoint.X), (int)Math.Min(firstPoint.Y, secondPoint.Y), (int)Math.Abs(secondPoint.X - firstPoint.X), (int)Math.Abs(secondPoint.Y - firstPoint.Y));

                    if (debug)
                    {
                        Rectangle debugBounds = segmentBoundingBox;
                        debugBounds.X -= (int)_level.ScrollOffset;
                        SpriteBatch.DrawRectangle(debugBounds, Color.Green);
                    }

                    if (MathUtils.OverlapsWith(segmentBoundingBox, enemyBounds))
                    {
                        if (enemyBounds.Contains(firstPoint) || enemyBounds.Contains(secondPoint))
                        {
                            if (!debug)
                            {
                                HitEnemy(_level.EnemyList[i]);
                            }
                        }
                        else
                        {
                            Vector2 highestPoint = firstPoint.Y <= secondPoint.Y ? firstPoint : secondPoint;
                            Vector2 direction = secondPoint - firstPoint;

                            if (debug)
                            {
                                SpriteBatch.DrawRectangle(highestPoint - new Vector2(_level.ScrollOffset, 0), Vector2.One, Color.White);
                            }


                            float bottomIntersectionX = Math.Abs(enemyBounds.Bottom - highestPoint.Y);
                            if (Math.Sign(direction.X * direction.Y) > 0)
                            {
                                bottomIntersectionX = highestPoint.X + bottomIntersectionX;
                            }
                            else
                            {
                                bottomIntersectionX = highestPoint.X - bottomIntersectionX;
                            }

                            if (debug)
                            {
                                SpriteBatch.DrawRectangle(new Vector2(bottomIntersectionX - _level.ScrollOffset, enemyBounds.Bottom), Vector2.One, Color.Red);
                            }


                            if (bottomIntersectionX >= enemyBounds.Left && bottomIntersectionX <= enemyBounds.Right)
                            {
                                if (!debug)
                                {
                                    HitEnemy(_level.EnemyList[i]);
                                }
                            }
                            else
                            {
                                float topIntersectionX = Math.Abs(enemyBounds.Y - highestPoint.Y);
                                if (Math.Sign(direction.X * direction.Y) > 0)
                                {
                                    topIntersectionX = highestPoint.X + topIntersectionX;
                                }
                                else
                                {
                                    topIntersectionX = highestPoint.X - topIntersectionX;
                                }

                                if (debug)
                                {
                                    SpriteBatch.DrawRectangle(new Vector2(topIntersectionX - _level.ScrollOffset, enemyBounds.Top), Vector2.One, Color.Red);
                                }

                                if (topIntersectionX >= enemyBounds.Left && topIntersectionX <= enemyBounds.Right)
                                {
                                    if (!debug)
                                    {
                                        HitEnemy(_level.EnemyList[i]);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void HitEnemy(Enemy enemy)
        {
            enemy.TakeHit(damagePerSecond * Game.DeltaTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (_activePositions.Count == 0)
                return;

            Vector2 offset = new Vector2(_level.ScrollOffset, 0);
            bool isStillOnScreen = false;
            for (int i = 0; i < _activePositions.Count - 1; i++)
            {
                if (_level.IsOnScreen(_activePositions[i]))
                {
                    isStillOnScreen = true;
                }

                SpriteBatch.DrawLine(_activePositions[i] - offset, _activePositions[i + 1] - offset, _color);
            }

            if (!isStillOnScreen && !_level.IsOnScreen(_activePositions[_activePositions.Count - 1]))
            {
                Despawn();
            }
        }
    }
}
