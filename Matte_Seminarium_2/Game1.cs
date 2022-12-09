using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Spline;
using System;
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

        private Vector2 originOfCircle;

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
            originOfCircle = new(600, 400);

            if(state == States.Executing)
            {
                renderTarget = new(GraphicsDevice, Window.ClientBounds.Width, Window.ClientBounds.Height);

                carPath = new(GraphicsDevice);
                carPath.Clean();

                CreateCirclePath();

                car = new(new(originOfCircle.X, originOfCircle.Y - 100), Vector2.Zero, carTex, 20);

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

                if(carPos >= carPath.endT)
                {
                    carPos = carPath.beginT;
                }
            }           

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Green);

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

            car.SetOrigin(carPath.GetPos(carPos));
            car.Update();

            car.Draw(_spriteBatch);

            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
        }

        private void CreateCirclePath()
        {
            carPath.AddPoint(new(originOfCircle.X + 100 * (float)Math.Cos(MathHelper.ToRadians(90)),
                                 originOfCircle.Y + 100 * (float)Math.Sin(MathHelper.ToRadians(90))));
            carPath.AddPoint(new(originOfCircle.X + 100 * (float)Math.Cos(MathHelper.ToRadians(60)),
                             originOfCircle.Y + 100 * (float)Math.Sin(MathHelper.ToRadians(60))));
            carPath.AddPoint(new(originOfCircle.X + 100 * (float)Math.Cos(MathHelper.ToRadians(45)),
                             originOfCircle.Y + 100 * (float)Math.Sin(MathHelper.ToRadians(45))));
            carPath.AddPoint(new(originOfCircle.X + 100 * (float)Math.Cos(MathHelper.ToRadians(30)),
                             originOfCircle.Y + 100 * (float)Math.Sin(MathHelper.ToRadians(30))));
            carPath.AddPoint(new(originOfCircle.X + 100 * (float)Math.Cos(MathHelper.ToRadians(0)),
                             originOfCircle.Y + 100 * (float)Math.Sin(MathHelper.ToRadians(0))));
            carPath.AddPoint(new(originOfCircle.X + 100 * (float)Math.Cos(MathHelper.ToRadians(330)),
                             originOfCircle.Y + 100 * (float)Math.Sin(MathHelper.ToRadians(330))));
            carPath.AddPoint(new(originOfCircle.X + 100 * (float)Math.Cos(MathHelper.ToRadians(315)),
                             originOfCircle.Y + 100 * (float)Math.Sin(MathHelper.ToRadians(315))));
            carPath.AddPoint(new(originOfCircle.X + 100 * (float)Math.Cos(MathHelper.ToRadians(300)),
                             originOfCircle.Y + 100 * (float)Math.Sin(MathHelper.ToRadians(300))));
            carPath.AddPoint(new(originOfCircle.X + 100 * (float)Math.Cos(MathHelper.ToRadians(270)),
                             originOfCircle.Y + 100 * (float)Math.Sin(MathHelper.ToRadians(270))));
            carPath.AddPoint(new(originOfCircle.X + 100 * (float)Math.Cos(MathHelper.ToRadians(240)),
                             originOfCircle.Y + 100 * (float)Math.Sin(MathHelper.ToRadians(240))));
            carPath.AddPoint(new(originOfCircle.X + 100 * (float)Math.Cos(MathHelper.ToRadians(225)),
                             originOfCircle.Y + 100 * (float)Math.Sin(MathHelper.ToRadians(225))));
            carPath.AddPoint(new(originOfCircle.X + 100 * (float)Math.Cos(MathHelper.ToRadians(210)),
                             originOfCircle.Y + 100 * (float)Math.Sin(MathHelper.ToRadians(210))));
            carPath.AddPoint(new(originOfCircle.X + 100 * (float)Math.Cos(MathHelper.ToRadians(180)),
                             originOfCircle.Y + 100 * (float)Math.Sin(MathHelper.ToRadians(180))));
            carPath.AddPoint(new(originOfCircle.X + 100 * (float)Math.Cos(MathHelper.ToRadians(150)),
                             originOfCircle.Y + 100 * (float)Math.Sin(MathHelper.ToRadians(150))));
            carPath.AddPoint(new(originOfCircle.X + 100 * (float)Math.Cos(MathHelper.ToRadians(135)),
                             originOfCircle.Y + 100 * (float)Math.Sin(MathHelper.ToRadians(135))));
            carPath.AddPoint(new(originOfCircle.X + 100 * (float)Math.Cos(MathHelper.ToRadians(120)),
                             originOfCircle.Y + 100 * (float)Math.Sin(MathHelper.ToRadians(120))));
            carPath.AddPoint(new(originOfCircle.X + 100 * (float)Math.Cos(MathHelper.ToRadians(90)),
                             originOfCircle.Y + 100 * (float)Math.Sin(MathHelper.ToRadians(90))));
        }

        enum States
        {
            Preparing,
            Executing,
        }
    }
}