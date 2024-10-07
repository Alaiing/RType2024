using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RType2024
{
    public class Bonus : Character
    {
        public const string ANIMATION_IDLE = "Idle";
        private Action _onPickup;
        private Level _level;

        public Bonus(SpriteSheet spriteSheet, Game game) : base(spriteSheet, game)
        {
            _collider = new Rectangle(2, 3, 13, 13);
        }

        public void Spawn(Level level, Vector2 position, Action onPickup)
        {
            _level = level;
            MoveTo(position);
            MoveDirection = new Vector2(-1, 0);
            SetBaseSpeed(level.ScrollSpeed);
            SetAnimation(ANIMATION_IDLE);
            _onPickup = onPickup;
            Game.Components.Add(this);
        }

        public void Pickup()
        {
            _onPickup?.Invoke();
            _level.BonusList.Remove(this);
            Game.Components.Remove(this);
        }
    }
}
