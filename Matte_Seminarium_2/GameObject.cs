using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Matte_Seminarium_2
{
    public class GameObject
    {
        public Vector2 Position { get; private set; }

        private Vector2 velocity;

        public Rectangle HitBox { get; private set; }

        private int radius;

        public Texture2D Tex { get; private set; }

        public GameObject(Vector2 position, Vector2 velocity, Texture2D tex, int radius)
        {
            Position = position;
            this.velocity = velocity;
            Tex = tex;
            this.radius = radius;

            HitBox = new(Position.ToPoint(), new(radius * 2, radius * 2));
        }

        public void Move()
        {
            Position += velocity;
        }

        public void SetPos(Vector2 Pos)
        {
            Position = Pos;
        }

        public void Update()
        {
            HitBox = new(Position.ToPoint(), HitBox.Size);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Tex, HitBox, Color.White);
        }
    }
}
