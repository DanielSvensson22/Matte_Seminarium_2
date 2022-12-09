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
        public Vector2 Origin { get; private set; }

        private Vector2 velocity;

        public Rectangle HitBox { get; private set; }

        private float radius;

        public Texture2D Tex { get; private set; }

        public GameObject(Vector2 position, Vector2 velocity, Texture2D tex, float radius)
        {
            Origin = position;
            this.velocity = velocity;
            Tex = tex;
            this.radius = radius;

            HitBox = new((int)(Origin.X - radius), (int)(Origin.Y - radius), (int)radius * 2, (int)radius * 2);
        }

        public void Move()
        {
            Origin += velocity;
        }

        public void SetOrigin(Vector2 origin)
        {
            Origin = origin;
        }

        public void Update()
        {
            HitBox = new((int)Origin.X - HitBox.Width / 2, (int)Origin.Y - HitBox.Height / 2, HitBox.Width, HitBox.Height);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Tex, HitBox, Color.White);
        }
    }
}
