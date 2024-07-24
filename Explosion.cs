using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Oudidon;

namespace RType2024
{
    public class Explosion : Character
    {
        public const string EVENT_SPAWN_EXPLOSION = "SpawnExplosion";
        public const string ANIMATION_IDLE = "Idle";

        private Level _level;
        private SoundEffectInstance _soundEffectInstance;

        public Explosion(SpriteSheet spriteSheet, SoundEffect soundEffect, Game game, Level level) : base(spriteSheet, game)
        {
            DrawOrder = 99;
            _level = level;
            _level.EnabledChanged += LevelEnabledChanged;
            _soundEffectInstance = soundEffect.CreateInstance();
            MoveDirection = new Vector2(-1, 0);
            SetBaseSpeed(level.ScrollSpeed);
        }

        private void LevelEnabledChanged(object sender, EventArgs e)
        {
            SetSpeedMultiplier(_level.Enabled ? 1f : 0);
        }

        public void Spawn(Vector2 position)
        {
            MoveTo(position);
            Game.Components.Add(this);
            SetAnimation(ANIMATION_IDLE, onAnimationEnd: Die);
            _soundEffectInstance.Play();
        }

        private void Die()
        {
            Game.Components.Remove(this);
        }
    }
}
