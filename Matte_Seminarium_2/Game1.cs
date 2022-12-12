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
        private int circleSize = 100;

        private MouseState previousMouseState;
        private MouseState mouseState;
        private Vector2 ballSpawn;
        private Vector2 ballSpeed;
        private float shootingRotation;

        private int radius;

        States state = States.Executing;

        private SimplePath carPath;
        private float carPos;
        private RenderTarget2D renderTarget;
        private RenderTarget2D ballRenderTarget;

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
            originOfCircle = new(800, 200);
            ballSpawn = new(20, Window.ClientBounds.Height - 20);

            if(state == States.Executing)
            {
                renderTarget = new(GraphicsDevice, Window.ClientBounds.Width, Window.ClientBounds.Height);
                ballRenderTarget = new(GraphicsDevice, Window.ClientBounds.Width, Window.ClientBounds.Height);

                carPath = new(GraphicsDevice);
                carPath.Clean();

                CreateCirclePath(circleSize);

                car = new(new(originOfCircle.X, originOfCircle.Y - circleSize), Vector2.Zero, carTex, 20);

                radius = 20;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            previousMouseState = mouseState;
            mouseState = Mouse.GetState();
            shootingRotation = MouseDirection(8);

            if(state == States.Executing)
            {
                DrawCarOnRenderTarget();

                carPos += 5;

                if(carPos >= carPath.endT)
                {
                    carPos = carPath.beginT;
                }

                if(mouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
                {
                    balls.Add(new(new(ballSpawn.X + ballTex.Width / 2, ballSpawn.Y - ballTex.Height / 2), ballSpeed, ballTex, radius));
                }

                for(int i = 0; i < balls.Count; i++)
                {
                    Rectangle screen = new(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height);

                    if (!screen.Contains(balls[i].HitBox.Location - new Point(10, 10)) || 
                        !screen.Contains(balls[i].HitBox.Location + new Point(balls[i].HitBox.Width + 10, balls[i].HitBox.Height + 10)))
                    {
                        balls.Remove(balls[i]);

                        continue;
                    }

                    balls[i].Move();
                    balls[i].Update(0);

                    if (HitCar(balls[i]))
                    {
                        balls.Remove(balls[i]);
                    }
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

                foreach(GameObject ball in balls)
                {
                    ball.Draw(_spriteBatch);
                }

                _spriteBatch.Draw(cannonTex, ballSpawn, null, Color.White, shootingRotation, 
                                  new(cannonTex.Width / 2, cannonTex.Height / 2), 1, SpriteEffects.None, 0);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private float MouseDirection(int ballSpeed)
        {
            Vector2 direction = mouseState.Position.ToVector2() - ballSpawn;
            direction.Normalize();

            this.ballSpeed = direction * ballSpeed;

            return (float)Math.Asin(direction.Y / direction.Length()) + (float)Math.PI / 2;
        }

        private void DrawCarOnRenderTarget()
        {
            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(Color.Transparent);
            _spriteBatch.Begin();

            car.SetOrigin(carPath.GetPos(carPos));
            car.Update(0);

            car.Draw(_spriteBatch);

            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
        }

        private void DrawBallOnRenderTarget(GameObject ball)
        {
            GraphicsDevice.SetRenderTarget(ballRenderTarget);
            GraphicsDevice.Clear(Color.Transparent);
            _spriteBatch.Begin();

            ball.Draw(_spriteBatch);

            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
        }

        private void CreateCirclePath(int size)
        {
            carPath.AddPoint(new(originOfCircle.X + size * (float)Math.Cos(MathHelper.ToRadians(90)),
                             originOfCircle.Y + size * (float)Math.Sin(MathHelper.ToRadians(90))));
            carPath.AddPoint(new(originOfCircle.X + size * (float)Math.Cos(MathHelper.ToRadians(60)),
                             originOfCircle.Y + size * (float)Math.Sin(MathHelper.ToRadians(60))));
            carPath.AddPoint(new(originOfCircle.X + size * (float)Math.Cos(MathHelper.ToRadians(45)),
                             originOfCircle.Y + size * (float)Math.Sin(MathHelper.ToRadians(45))));
            carPath.AddPoint(new(originOfCircle.X + size * (float)Math.Cos(MathHelper.ToRadians(30)),
                             originOfCircle.Y + size * (float)Math.Sin(MathHelper.ToRadians(30))));
            carPath.AddPoint(new(originOfCircle.X + size * (float)Math.Cos(MathHelper.ToRadians(0)),
                             originOfCircle.Y + size * (float)Math.Sin(MathHelper.ToRadians(0))));
            carPath.AddPoint(new(originOfCircle.X + size * (float)Math.Cos(MathHelper.ToRadians(330)),
                             originOfCircle.Y + size * (float)Math.Sin(MathHelper.ToRadians(330))));
            carPath.AddPoint(new(originOfCircle.X + size * (float)Math.Cos(MathHelper.ToRadians(315)),
                             originOfCircle.Y + size * (float)Math.Sin(MathHelper.ToRadians(315))));
            carPath.AddPoint(new(originOfCircle.X + size * (float)Math.Cos(MathHelper.ToRadians(300)),
                             originOfCircle.Y + size * (float)Math.Sin(MathHelper.ToRadians(300))));
            carPath.AddPoint(new(originOfCircle.X + size * (float)Math.Cos(MathHelper.ToRadians(270)),
                             originOfCircle.Y + size * (float)Math.Sin(MathHelper.ToRadians(270))));
            carPath.AddPoint(new(originOfCircle.X + size * (float)Math.Cos(MathHelper.ToRadians(240)),
                             originOfCircle.Y + size * (float)Math.Sin(MathHelper.ToRadians(240))));
            carPath.AddPoint(new(originOfCircle.X + size * (float)Math.Cos(MathHelper.ToRadians(225)),
                             originOfCircle.Y + size * (float)Math.Sin(MathHelper.ToRadians(225))));
            carPath.AddPoint(new(originOfCircle.X + size * (float)Math.Cos(MathHelper.ToRadians(210)),
                             originOfCircle.Y + size * (float)Math.Sin(MathHelper.ToRadians(210))));
            carPath.AddPoint(new(originOfCircle.X + size * (float)Math.Cos(MathHelper.ToRadians(180)),
                             originOfCircle.Y + size * (float)Math.Sin(MathHelper.ToRadians(180))));
            carPath.AddPoint(new(originOfCircle.X + size * (float)Math.Cos(MathHelper.ToRadians(150)),
                             originOfCircle.Y + size * (float)Math.Sin(MathHelper.ToRadians(150))));
            carPath.AddPoint(new(originOfCircle.X + size * (float)Math.Cos(MathHelper.ToRadians(135)),
                             originOfCircle.Y + size * (float)Math.Sin(MathHelper.ToRadians(135))));
            carPath.AddPoint(new(originOfCircle.X + size * (float)Math.Cos(MathHelper.ToRadians(120)),
                             originOfCircle.Y + size * (float)Math.Sin(MathHelper.ToRadians(120))));
            carPath.AddPoint(new(originOfCircle.X + size * (float)Math.Cos(MathHelper.ToRadians(90)),
                             originOfCircle.Y + size * (float)Math.Sin(MathHelper.ToRadians(90))));
        }

        public bool HitCar(GameObject ball)
        {
            Color[] pixels = new Color[ball.HitBox.Width * ball.HitBox.Height];
            Color[] pixels2 = new Color[ball.HitBox.Width * ball.HitBox.Height];

            DrawBallOnRenderTarget(ball);

            ballRenderTarget.GetData(0, ball.HitBox, pixels2, 0, pixels2.Length);

            renderTarget.GetData(0, ball.HitBox, pixels, 0, pixels.Length);

            for (int i = 0; i < pixels.Length; ++i)
            {
                if (pixels[i].A > 0.0f && pixels2[i].A > 0.0f)
                    return true;
            }
            return false;
        }

        enum States
        {
            Preparing,
            Executing,
        }
    }
}