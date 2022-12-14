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

        private Timer timer;
        private Timer inputTimer;

        private Texture2D ballTex;
        private Texture2D carTex;
        private Texture2D cannonTex;
        private SpriteFont font;

        private GameObject car;
        private List<GameObject> balls;

        private Vector2 originOfCircle;
        private Vector2 originVector;
        private Vector2 scaledVector;
        private Vector2 position;
        private int circleSize = 100;
        private int waveInterval = 10;
        private int waveScale = 15;
        private int waveMag = 15;
        private int waveSpeed = 5;

        private MouseState previousMouseState;
        private MouseState mouseState;

        private Vector2 ballSpawn;
        private Vector2 ballSpeed;
        private float shootingRotation;

        private int ballRadius;

        States state = States.Preparing;

        private SimplePath carPath;
        private SimplePath wavePath;
        private float carPos;
        private float carSpeed;
        private RenderTarget2D renderTarget;
        private RenderTarget2D ballRenderTarget;

        List<double> hitTimes;
        List<Vector2> ballHitOrigins;
        List<Vector2> carHitOrigins;
        List<Vector2> collisionPoints;

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
            font = Content.Load<SpriteFont>("font");

            balls = new();
            originOfCircle = new(800, 200);
            originVector = new(0, Window.ClientBounds.Height/2);
            ballSpawn = new(20, Window.ClientBounds.Height - 20);

            if(state == States.Preparing)
            {
                carSpeed = 1;
                circleSize = 100;
                ballRadius = 20;

                timer = new();
                inputTimer = new();

                hitTimes = new();
                ballHitOrigins = new();
                carHitOrigins = new();
                collisionPoints = new();

                car = new(new(originOfCircle.X, originOfCircle.Y - circleSize), Vector2.Zero, carTex, 20);
            }
            else if(state == States.Executing)
            {
                renderTarget = new(GraphicsDevice, Window.ClientBounds.Width, Window.ClientBounds.Height);
                ballRenderTarget = new(GraphicsDevice, Window.ClientBounds.Width, Window.ClientBounds.Height);

                carPath = new(GraphicsDevice);
                carPath.Clean();

                //wavePath = new(GraphicsDevice);
                //wavePath.Clean();

                CreateCirclePath(circleSize);
                //CreateWavePath();

                
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            previousMouseState = mouseState;
            mouseState = Mouse.GetState();
            shootingRotation = MouseDirection(4);

            if (Keyboard.GetState().IsKeyDown(Keys.P))
            {
                state = States.Preparing;
                LoadContent();
            }

            if(state == States.Preparing)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Enter))
                {
                    state = States.WavePath;
                    LoadContent();
                }

                inputTimer.Update(gameTime);

                ballRadius += ValueChange(Keys.S, Keys.A, 1);

                circleSize += ValueChange(Keys.X, Keys.Z, 1);

                carSpeed += ValueChange(Keys.V, Keys.C, 1);
            }
            else if(state == States.Executing)
            {
                timer.Update(gameTime);

                DrawCarOnRenderTarget();

                carPos += carSpeed;

                if(carPos >= carPath.endT)
                {
                    carPos = carPath.beginT;
                }

                CheckCarRotation();

                if(mouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
                {
                    balls.Add(new(new(ballSpawn.X + ballTex.Width / 2, ballSpawn.Y - ballTex.Height / 2), ballSpeed, ballTex, ballRadius));
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
                    balls[i].Update();

                    if (HitCar(balls[i]))
                    {
                        balls.Remove(balls[i]);
                    }
                }

                if (Keyboard.GetState().IsKeyDown(Keys.L))
                {
                    state = States.ListCheck;
                    LoadContent();
                }
            }else if(state == States.WavePath)
            {
                //car.SetOrgin(new Vector2(0,Window.ClientBounds.Height/2));
                UpdateWave(gameTime);

                inputTimer.Update(gameTime);

                waveSpeed += ValueChange(Keys.S, Keys.A, 1);

                waveScale += ValueChange(Keys.X, Keys.Z, 1);

                waveMag += ValueChange(Keys.V, Keys.C, 1);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Green);

            // TODO: Add your drawing code here
            _spriteBatch.Begin();

            if(state == States.Preparing)
            {
                _spriteBatch.DrawString(font, $"Radius of ball: {ballRadius}", new(10, 50), Color.Red);

                _spriteBatch.DrawString(font, $"Radius of circle path: {circleSize}", new(300, 50), Color.Gold);

                _spriteBatch.DrawString(font, $"Car speed: {carSpeed}", new(700, 50), Color.Orange);
            }
            else if(state == States.Executing)
            {
                carPath.Draw(_spriteBatch);
                //wavePath.Draw(_spriteBatch);
                //_spriteBatch.Draw(renderTarget, Vector2.Zero, Color.White);

                car.Draw(_spriteBatch);

                foreach(GameObject ball in balls)
                {
                    ball.Draw(_spriteBatch);
                }

                _spriteBatch.Draw(cannonTex, ballSpawn, null, Color.White, shootingRotation, 
                                  new(cannonTex.Width / 2, cannonTex.Height / 2), 1, SpriteEffects.None, 0);
            }
            else if(state == States.ListCheck)
            {
                for (int i = 0; i < hitTimes.Count; i++)
                {
                    _spriteBatch.DrawString(font, $"Hit at {(int)hitTimes[i]} seconds.", new(10, 10 + 20 * i), Color.Gold);
                }

                for (int i = 0; i < ballHitOrigins.Count; i++)
                {
                    _spriteBatch.DrawString(font, $"Ball collided at coordinates: {(int)ballHitOrigins[i].X}; {(int)ballHitOrigins[i].Y}", new(150, 10 + 20 * i), Color.Red);
                }

                for (int i = 0; i < carHitOrigins.Count; i++)
                {
                    _spriteBatch.DrawString(font, $"Car collided at coordinates: {(int)carHitOrigins[i].X}; {(int)carHitOrigins[i].Y}", new(450, 10 + 20 * i), Color.Orange);
                }

                for (int i = 0; i < collisionPoints.Count; i++)
                {
                    _spriteBatch.DrawString(font, $"Ball and car collided at coordinates: {(int)collisionPoints[i].X}; {(int)collisionPoints[i].Y}", new(750, 10 + 20 * i), Color.Purple);
                }
            }

            _spriteBatch.End();

            if (state == States.WavePath)
            {
                DrawCarOnWave();
            }

            base.Draw(gameTime);
        }

        public int ValueChange(Keys increaseKey, Keys decreaseKey, int changeInValue)
        {
            if (inputTimer.time > 0.1)
            {
                if (Keyboard.GetState().IsKeyDown(increaseKey))
                {
                    inputTimer.time = 0;

                    return changeInValue;
                }
                else if (Keyboard.GetState().IsKeyDown(decreaseKey))
                {
                    inputTimer.time = 0;

                    return -changeInValue;
                }
            }

            return 0;
        }

        private float MouseDirection(int ballSpeed)
        {
            Vector2 direction = mouseState.Position.ToVector2() - ballSpawn;
            direction.Normalize();

            this.ballSpeed = direction * ballSpeed;

            return (float)Math.Asin(direction.Y / direction.Length()) + (float)Math.PI / 2;
        }

        private void CheckCarRotation()
        {
            float x = carPath.GetPos(carPos).X - originOfCircle.X;
            float y = carPath.GetPos(carPos).Y - originOfCircle.Y;

            car.SetRotation((-(float)MathF.Atan2(x, y)) + (float)Math.PI);
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
        
        private void DrawCarOnWave()
        {
            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(Color.Transparent);
            _spriteBatch.Begin();

            car.Update();

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

        /*private void CreateWavePath()
        {
            for(int i = 0; i < waveInterval; i++)
            {
                wavePath.AddPoint((i * Window.ClientBounds.Height/waveInterval), (float)Math.Sin(i) * waveSize);
            }
        }*/

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
                {
                    hitTimes.Add(timer.time);
                    ballHitOrigins.Add(ball.Origin);
                    carHitOrigins.Add(car.Origin);
                    collisionPoints.Add(GiveCollisionPoint(ball));

                    return true;
                }
            }

            return false;
        }

        private void UpdateWave(GameTime gameTime)
        {
            scaledVector = new Vector2(waveScale * (float)gameTime.TotalGameTime.TotalSeconds * waveSpeed, waveScale * (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * waveSpeed) * waveMag);

            position = originVector + scaledVector;
            //if(!(position.X < Window.ClientBounds.Width)) { position.X = 0; scaledVector = Vector2.Zero; }
            position.X = position.X % Window.ClientBounds.Width;
            //rect.X = (int)position.X;
            //rect.Y = (int)position.Y;
            car.SetOrigin(position);
        }

        private Vector2 GiveCollisionPoint(GameObject ball)
        {
            float x = ((ball.Origin.X * car.Radius) + (car.Origin.X * ball.Radius)) / (ball.Radius + car.Radius);
            float y = ((ball.Origin.Y * car.Radius) + (car.Origin.Y * ball.Radius)) / (ball.Radius + car.Radius);

            return new(x, y);
        }

        enum States
        {
            Preparing,
            Executing,
            ListCheck,
            WavePath,
        }
    }
}