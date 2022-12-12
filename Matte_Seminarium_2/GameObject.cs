using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Matte_Seminarium_2
{
    public class GameObject
    {
        public Vector2 Origin { get; private set; }

        private Vector2 velocity;
        public Vector2 Velocity { get => velocity; }

        public Rectangle HitBox { get; private set; }

        private float radius;
        private float rotation;

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

        public void Update(float rotation)
        {
            HitBox = new((int)Origin.X - HitBox.Width / 2, (int)Origin.Y - HitBox.Height / 2, HitBox.Width, HitBox.Height);
            this.rotation = rotation;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Tex, HitBox, null, Color.White, rotation, Vector2.Zero, SpriteEffects.None, 0);
        }
    }
}
