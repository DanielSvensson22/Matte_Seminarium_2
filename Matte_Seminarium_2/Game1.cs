using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Spline;
using System.Collections.Generic;

namespace Matte_Seminarium_2
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D ballTex;
        private Texture2D carTex;
        private Texture2D cannonTex;

        private GameObject car;
        private List<GameObject> balls;

        private Vector2 startPos;

        States state = States.Executing;

        private SimplePath carPath;
        private float carPos;
        private RenderTarget2D renderTarget;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = 1080;
            _graphics.PreferredBackBufferHeight = 720;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            ballTex = Content.Load<Texture2D>("ball");
            carTex = Content.Load<Texture2D>("car");
            cannonTex = Content.Load<Texture2D>("cannon");

            balls = new();
            startPos = new(700, 100);

            if(state == States.Executing)
            {
                renderTarget = new(GraphicsDevice, Window.ClientBounds.Width, Window.ClientBounds.Height);

                carPath = new(GraphicsDevice);
                carPath.Clean();

                carPath.AddPoint(startPos);
                carPath.SetPos(0, startPos);
                carPath.AddPoint(new(startPos.X + 50, startPos.Y + 50));
                carPath.AddPoint(new(startPos.X, startPos.Y + 100));
                carPath.AddPoint(new(startPos.X - 50, startPos.Y + 50));
                carPath.AddPoint(startPos);

                car = new(startPos, Vector2.Zero, carTex, 1);

            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            if(state == States.Executing)
            {
                DrawCarOnRenderTarget();

                carPos += 5;
            }           

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            _spriteBatch.Begin();

            if(state == States.Executing)
            {
                carPath.Draw(_spriteBatch);

                _spriteBatch.Draw(renderTarget, Vector2.Zero, Color.White);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawCarOnRenderTarget()
        {
            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(Color.Transparent);
            _spriteBatch.Begin();

            _spriteBatch.Draw(carTex, carPath.GetPos(carPos), Color.White);
            car.SetPos(carPath.GetPos(carPos));

            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
        }

        enum States
        {
            Preparing,
            Executing,
        }
    }
}