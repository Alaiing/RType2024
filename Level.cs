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
        private float _scrollOffset;

        public Level(Game game, Texture2D texture) : base(game)
        {
            _texture = texture;
            _data = new Color[_texture.Width * _texture.Height];
            _texture.GetData(_data);
        }

        public void Reset()
        {
            _scrollOffset = 0;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            _scrollOffset += _scrollSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch.Draw(_texture, Vector2.Zero, new Rectangle((int)_scrollOffset, 0, RType2024.PLAYGROUND_WIDTH, RType2024.PLAYGROUND_HEIGHT), Color.White);
        }

        public bool IsColliding(Vector2 position, Rectangle collider)
        {

            for (int i =  0; i < collider.Width;i++)
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
    }
}
