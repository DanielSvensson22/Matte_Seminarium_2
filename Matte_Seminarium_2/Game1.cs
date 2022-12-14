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
        private int circleSize = 100;

        private MouseState previousMouseState;
        private MouseState mouseState;

        private Vector2 ballSpawn;
        private Vector2 ballSpeed;
        private int ballSpeedMultiplier;
        private float shootingRotation;

        public static int ballRadius;

        States state = States.Preparing;

        private SimplePath carPath;
        private float carPos;
        private float carSpeed;
        private RenderTarget2D renderTarget;
        private RenderTarget2D ballRenderTarget;

        private bool pathIsCurved;

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
            ballSpawn = new(20, Window.ClientBounds.Height - 20);

            if(state == States.Preparing)
            {
                carSpeed = 1;
                ballSpeedMultiplier = 4;
                circleSize = 100;
                ballRadius = 20;

                timer = new();
                inputTimer = new();

                pathIsCurved = false;

                hitTimes = new();
                ballHitOrigins = new();
                carHitOrigins = new();
                collisionPoints = new();

                carPath = new(GraphicsDevice);
                carPath.Clean();
            }
            else if(state == States.Executing)
            {
                renderTarget = new(GraphicsDevice, Window.ClientBounds.Width, Window.ClientBounds.Height);
                ballRenderTarget = new(GraphicsDevice, Window.ClientBounds.Width, Window.ClientBounds.Height);

                CreateCirclePath(circleSize);

                car = new(new(originOfCircle.X, originOfCircle.Y - circleSize), Vector2.Zero, carTex, 20);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            previousMouseState = mouseState;
            mouseState = Mouse.GetState();
            shootingRotation = MouseDirection(ballSpeedMultiplier);

            if (Keyboard.GetState().IsKeyDown(Keys.P))
            {
                state = States.Preparing;
                LoadContent();
            }

            if(state == States.Preparing)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Enter))
                {
                    state = States.Executing;
                    LoadContent();
                }

                inputTimer.Update(gameTime);

                ballRadius += ValueChange(Keys.S, Keys.A, 1);

                circleSize += ValueChange(Keys.X, Keys.Z, 1);

                carSpeed += ValueChange(Keys.V, Keys.C, 1);

                ballSpeedMultiplier += ValueChange(Keys.G, Keys.F, 1);
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

                if (!pathIsCurved)
                {
                    CheckCarRotation(originOfCircle);
                }
                else
                {
                    CheckCarRotationOnCurve();
                }

                if(mouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
                {
                    balls.Add(new(new(ballSpawn.X + ballRadius * 2, ballSpawn.Y - ballRadius * 2), ballSpeed, ballTex, ballRadius));
                }

                for(int i = 0; i < balls.Count; i++)
                {
                    Rectangle screen = new(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height);

                    if (!screen.Contains(balls[i].DrawHitBox.Location - new Point(10 + ballSpeedMultiplier, 10 + ballSpeedMultiplier)) || 
                        !screen.Contains(balls[i].DrawHitBox.Location + new Point(balls[i].DrawHitBox.Width + 10 + ballSpeedMultiplier, 
                                                                              balls[i].DrawHitBox.Height + 10 + ballSpeedMultiplier)))
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

                if (Keyboard.GetState().IsKeyDown(Keys.D2) && !pathIsCurved)
                {
                    CreateCurvePath();
                }
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

                _spriteBatch.DrawString(font, $"Speed of ball: {ballSpeedMultiplier}", new(10, 100), Color.Blue);

                _spriteBatch.DrawString(font, $"Radius of circle path: {circleSize}", new(300, 50), Color.Gold);

                _spriteBatch.DrawString(font, $"Car speed: {carSpeed}", new(700, 50), Color.Orange);
            }
            else if(state == States.Executing)
            {
                carPath.Draw(_spriteBatch);

                //_spriteBatch.Draw(renderTarget, Vector2.Zero, Color.White);

                car.Draw(_spriteBatch);

                foreach(GameObject ball in balls)
                {
                    ball.Draw(_spriteBatch);
                }

                _spriteBatch.Draw(cannonTex, new Rectangle(ballSpawn.ToPoint() + new Point(20, -20), new(ballRadius * 4, ballRadius * 4)), null, Color.White, shootingRotation, 
                                  new(cannonTex.Width / 2, cannonTex.Height / 2), SpriteEffects.None, 0);
            }
            else if(state == States.ListCheck)
            {
                for (int i = 0; i < hitTimes.Count; i++)
                {
                    _spriteBatch.DrawString(font, $"Hit at {(int)hitTimes[i]} seconds.", new(10, 10 + 20 * i), Color.Gold);
                }

                for (int i = 0; i < ballHitOrigins.Count; i++)
                {
                    _spriteBatch.DrawString(font, $"Ball collided at coordinates: {(int)ballHitOrigins[i].X}; {(int)ballHitOrigins[i].Y}", new(160, 10 + 20 * i), Color.Red);
                }

                for (int i = 0; i < carHitOrigins.Count; i++)
                {
                    _spriteBatch.DrawString(font, $"Car collided at coordinates: {(int)carHitOrigins[i].X}; {(int)carHitOrigins[i].Y}", new(450, 10 + 20 * i), Color.Orange);
                }

                for (int i = 0; i < collisionPoints.Count; i++)
                {
                    _spriteBatch.DrawString(font, $"Ball and car collided at coordinates: {(int)collisionPoints[i].X}; {(int)collisionPoints[i].Y}", new(750, 10 + 20 * i), Color.Pink);
                }
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        //Changes values of an integer;
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

        //Determines angle of mouse position to bottom left corner and where to shoot a ball.
        private float MouseDirection(int ballSpeed)
        {
            Vector2 direction = mouseState.Position.ToVector2() - ballSpawn;
            direction.Normalize();

            this.ballSpeed = direction * ballSpeed;

            return (float)Math.Asin(direction.Y / direction.Length()) + (float)Math.PI / 2;
        }

        //Gets rotation of car in relation to a point.
        private void CheckCarRotation(Vector2 point)
        {
            float x = carPath.GetPos(carPos).X - point.X;
            float y = carPath.GetPos(carPos).Y - point.Y;

            car.SetRotation((-(float)MathF.Atan2(x, y)) + (float)Math.PI);
        }

        //Gives car rotation on the curve.
        private void CheckCarRotationOnCurve()
        {
            if(car.Origin.X > 108 * 2 - 15 && car.Origin.X < 108 * 2 + 15)
            {
                CheckCarRotation(new(108 * 2, 450));
            }
            else if(car.Origin.X < 108 * 3)
            {
                CheckCarRotation(new(108, 450));
            }
            else if (car.Origin.X > 108 * 6 - 15 && car.Origin.X < 108 * 6 + 15)
            {
                CheckCarRotation(new(108 * 6, 450));
            }
            else if(car.Origin.X < 108 * 7)
            {
                CheckCarRotation(new(108 * 5, 450));
            }
            else if(car.Origin.X > 108 * 10 - 15 && car.Origin.X < 108 * 10 + 15)
            {
                CheckCarRotation(new(108 * 10, 450));
            }
            else if(car.Origin.X < 108 * 11)
            {
                CheckCarRotation(new(108 * 9, 450));
            }
        }

        //Draws the car on the render target so pixel perfect collision can be done with the car.
        private void DrawCarOnRenderTarget()
        {
            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(Color.Transparent);
            _spriteBatch.Begin();

            car.SetOrigin(carPath.GetPos(carPos));
            car.Update();

            car.AltDraw(_spriteBatch);

            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
        }

        //Draws a ball on ball render target.
        private void DrawBallOnRenderTarget(GameObject ball)
        {
            GraphicsDevice.SetRenderTarget(ballRenderTarget);
            GraphicsDevice.Clear(Color.Transparent);
            _spriteBatch.Begin();

            ball.Draw(_spriteBatch);

            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
        }

        //Creates a path in the shape of a circle.
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

        //Creates a curvy path.
        private void CreateCurvePath()
        {
            carPath.Clean();

            carPath.AddPoint(new(0, 200));
            carPath.AddPoint(new(108, 100));
            carPath.AddPoint(new(108 * 2 - 10, 200));
            carPath.AddPoint(new(108 * 2, 200));
            carPath.AddPoint(new(108 * 2 + 10, 200));
            carPath.AddPoint(new(108 * 3, 300));
            carPath.AddPoint(new(108 * 4, 200));
            carPath.AddPoint(new(108 * 5, 100));
            carPath.AddPoint(new(108 * 6 - 10, 200));
            carPath.AddPoint(new(108 * 6, 200));
            carPath.AddPoint(new(108 * 6 + 10, 200));
            carPath.AddPoint(new(108 * 7, 300));
            carPath.AddPoint(new(108 * 8, 200));
            carPath.AddPoint(new(108 * 9, 100));
            carPath.AddPoint(new(108 * 10 - 10, 200));
            carPath.AddPoint(new(108 * 10, 200));
            carPath.AddPoint(new(108 * 10 + 10, 200));

            pathIsCurved = true;

            car = new(new(0, 200), Vector2.Zero, carTex, 20)
            {
                spriteEffect = SpriteEffects.FlipHorizontally
            };
        }

        //Pixel perfect bollision with the car and a ball.
        public bool HitCar(GameObject ball)
        {
            Color[] pixels = new Color[ball.DrawHitBox.Width * ball.DrawHitBox.Height];
            Color[] pixels2 = new Color[ball.DrawHitBox.Width * ball.DrawHitBox.Height];

            DrawBallOnRenderTarget(ball);

            ballRenderTarget.GetData(0, ball.DrawHitBox, pixels2, 0, pixels2.Length);

            renderTarget.GetData(0, ball.DrawHitBox, pixels, 0, pixels.Length);

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

        //Return the collision point between the car and a ball.
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
        }
    }
}