#region File Description
//-----------------------------------------------------------------------------
// Player.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace Platformer2D
{
    /// <summary>
    /// Our fearless adventurer!
    /// </summary>
    class Player
    {
        TimeSpan fireTime = TimeSpan.FromSeconds(1.0f) ;
        TimeSpan previousFireTime = TimeSpan.Zero;
        TimeSpan Ultimateskill3rd = TimeSpan.Zero;
        bool previousMove;
        Random rnd = new Random();

        public float health = 100;
        private static readonly Random getrandom = new Random();
        private static readonly object syncLock = new object();
        public static int GetRandomNumber(int min, int max)
        {
            lock (syncLock)
            { // synchronize
                return getrandom.Next(min, max);
            }
        }

        // Animations
        private Animation idleAnimation;
        private Animation runAnimation;
        private Animation jumpAnimation;
        private Animation celebrateAnimation;
        private Animation dieAnimation;
        private SpriteEffects flip = SpriteEffects.None;
        private AnimationPlayer sprite;

        // Sounds
        private SoundEffect killedSound;
        private SoundEffect jumpSound;
        private SoundEffect fallSound;
        private SoundEffect Genji;
        private SoundEffect Pharah;

        public Level Level
        {
            get { return level; }
        }
        Level level;

        public bool IsAlive
        {
            get { return isAlive; }
        }
        bool isAlive;

        // Physics state
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        Vector2 position;

        private float previousBottom;

        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }
        Vector2 velocity;

        // Constants for controling horizontal movement
        private const float MoveAcceleration = 13000.0f;
        private const float MaxMoveSpeed = 1750.0f;
        private const float GroundDragFactor = 0.48f;
        private const float AirDragFactor = 0.58f;

        // Constants for controlling vertical movement
        private const float MaxJumpTime = 0.35f;
        private const float JumpLaunchVelocity = -3500.0f;
        private const float GravityAcceleration = 3400.0f;
        private const float MaxFallSpeed = 400.0f;
        private const float JumpControlPower = 0.12f; 

        // Input configuration
        private const float MoveStickScale = 1.0f;
        private const float AccelerometerScale = 1.5f;
        private const Buttons JumpButton = Buttons.A;

        /// <summary>
        /// Gets whether or not the player's feet are on the ground.
        /// </summary>
        public bool IsOnGround
        {
            get { return isOnGround; }
        }
        bool isOnGround;

        /// <summary>
        /// Current user movement input.
        /// </summary>
        private float movement;

        // Jumping state
        private bool isJumping;
        private bool wasJumping;
        private float jumpTime;

        private Rectangle localBounds;
        /// <summary>
        /// Gets a rectangle which bounds this player in world space.
        /// </summary>
        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }

        /// <summary>
        /// Constructors a new player.
        /// </summary>
        public Player(Level level, Vector2 position, string a, string b, string c, string d, string e)
        {
            this.level = level;

            LoadContent(a,b,c,d,e);

            Reset(position);
        }

        /// <summary>
        /// Loads the player sprite sheet and sounds.
        /// </summary>
        public void LoadContent(string a, string b, string c, string d, string e)
        {
            // Load animated textures.
            idleAnimation = new Animation(Level.Content.Load<Texture2D>(a), 0.1f, true);
            runAnimation = new Animation(Level.Content.Load<Texture2D>(b), 0.1f, true);
            jumpAnimation = new Animation(Level.Content.Load<Texture2D>(c), 0.1f, false);
            celebrateAnimation = new Animation(Level.Content.Load<Texture2D>(d), 0.1f, false);
            dieAnimation = new Animation(Level.Content.Load<Texture2D>(e), 0.1f, false);
            

            // Calculate bounds within texture size.            
            int width = (int)(idleAnimation.FrameWidth * 0.4);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameWidth * 0.8);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);

            // Load sounds.            
            killedSound = Level.Content.Load<SoundEffect>("Sounds/PlayerKilled");
            jumpSound = Level.Content.Load<SoundEffect>("Sounds/PlayerJump");
            fallSound = Level.Content.Load<SoundEffect>("Sounds/PlayerFall");
            Genji = Level.Content.Load<SoundEffect>("Sounds/Genji");
            Pharah = Level.Content.Load<SoundEffect>("Sounds/pharah");
        }

        /// <summary>
        /// Resets the player to life.
        /// </summary>
        /// <param name="position">The position to come to life at.</param>
        public void Reset(Vector2 position)
        {
            Position = position;
            Velocity = Vector2.Zero;
            isAlive = true;
            sprite.PlayAnimation(idleAnimation);
        }

        //-----------------------------------
        

        //-------------------------

        /// <summary>
        /// Handles input, performs physics, and animates the player sprite.
        /// </summary>
        /// <remarks>
        /// We pass in all of the input states so that our game is only polling the hardware
        /// once per frame. We also pass the game's orientation because when using the accelerometer,
        /// we need to reverse our motion when the orientation is in the LandscapeRight orientation.
        /// </remarks>
        public void Update(
            GameTime gameTime, 
            KeyboardState keyboardState, 
            GamePadState gamePadState, 
            AccelerometerState accelState,
            DisplayOrientation orientation)
        {
            //GetInput(keyboardState, gamePadState, accelState, orientation,gameTime);
            //GetInputP2(keyboardState, gamePadState, accelState, orientation, gameTime);

            ApplyPhysics(gameTime);

            if (IsAlive && IsOnGround)
            {
                if (Math.Abs(Velocity.X) - 0.02f > 0)
                {
                    sprite.PlayAnimation(runAnimation);
                }
                else
                {
                    sprite.PlayAnimation(idleAnimation);
                }
            }

           

            // Clear input.
            //movement = 0.0f;
            //isJumping = false;
        }

        /// <summary>
        /// Gets player horizontal movement and jump commands from input.
        /// </summary>
        public void GetInput(
            KeyboardState keyboardState, 
            GamePadState gamePadState,
            AccelerometerState accelState, 
            DisplayOrientation orientation,GameTime gameTime)
        {
            // Get analog horizontal movement.
            movement = gamePadState.ThumbSticks.Left.X * MoveStickScale;

            // Ignore small movements to prevent running in place.
            if (Math.Abs(movement) < 0.5f)
                movement = 0.0f;

            // Move the player with accelerometer
            if (Math.Abs(accelState.Acceleration.Y) > 0.10f)
            {
                // set our movement speed
                movement = MathHelper.Clamp(-accelState.Acceleration.Y * AccelerometerScale, -1f, 1f);

                // if we're in the LandscapeLeft orientation, we must reverse our movement
                if (orientation == DisplayOrientation.LandscapeRight)
                    movement = -movement;
            }

            // If any digital horizontal movement input is found, override the analog movement.
            if (gamePadState.IsButtonDown(Buttons.DPadLeft) ||
                              keyboardState.IsKeyDown(Keys.A))
            {
                movement = -1.0f;
                previousMove = true;
            }
            if (gamePadState.IsButtonDown(Buttons.DPadRight) ||
                     keyboardState.IsKeyDown(Keys.D))
            {
                movement = 1.0f;
                previousMove = false;
            }
            if (keyboardState.IsKeyDown(Keys.J) )
            {
                //movement = 1.0f;
                float speed = 1.5f;
                if (gameTime.TotalGameTime - previousFireTime > fireTime)
                {
                    previousFireTime = gameTime.TotalGameTime;
                    if (previousMove)
                    {
                        speed *= -1;
                    }
                    Level.AddBullet(new Vector2(position.X, position.Y - 30.0f),speed);
                }
            }

            if (keyboardState.IsKeyDown(Keys.K))
            {
                //movement = 1.0f;
                float speed = 3.0f;
                if (gameTime.TotalGameTime - previousFireTime > fireTime)
                {
                    previousFireTime = gameTime.TotalGameTime;
                    if (previousMove)
                    {
                        speed *= -1;
                    }
                    Level.AddBullet(new Vector2(position.X, position.Y - 30.0f), speed);
                }
            }

            if (keyboardState.IsKeyDown(Keys.L))
            {
                //movement = 1.0f;
                float speed = 3.0f;
                if (gameTime.TotalGameTime - previousFireTime > fireTime)
                {
                    Genji.Play();
                    previousFireTime = gameTime.TotalGameTime;
                    if (previousMove)
                    {
                        speed *= -1;
                    }

                    if (speed < 0)
                    {
                        for (float i = 0; i < 1000; i += 20)
                        {
                            Level.AddBullet(new Vector2(1800 - i, i), speed);
                        }
                    }
                    if (speed >= 0)
                    {
                        for (float i = 0; i < 1000; i += 20)
                        {
                            Level.AddBullet(new Vector2(i - 1000, i), speed);
                        }
                    }

                }
            }



            // Check if the player wants to jump.
            isJumping =
                gamePadState.IsButtonDown(JumpButton) ||
                keyboardState.IsKeyDown(Keys.W) 
              ;
        }

        public void GetInputP2(
            KeyboardState keyboardState,
            GamePadState gamePadState,
            AccelerometerState accelState,
            DisplayOrientation orientation, GameTime gameTime)
        {
            // Get analog horizontal movement.
            movement = gamePadState.ThumbSticks.Left.X * MoveStickScale;

            // Ignore small movements to prevent running in place.
            if (Math.Abs(movement) < 0.5f)
                movement = 0.0f;

            // Move the player with accelerometer
            if (Math.Abs(accelState.Acceleration.Y) > 0.10f)
            {
                // set our movement speed
                movement = MathHelper.Clamp(-accelState.Acceleration.Y * AccelerometerScale, -1f, 1f);

                // if we're in the LandscapeLeft orientation, we must reverse our movement
                if (orientation == DisplayOrientation.LandscapeRight)
                    movement = -movement;
            }

            // If any digital horizontal movement input is found, override the analog movement.
            if (gamePadState.IsButtonDown(Buttons.DPadLeft) ||
                keyboardState.IsKeyDown(Keys.Left)
                 )
            {
                movement = -1.0f;
                previousMove = true;
            }
            if (gamePadState.IsButtonDown(Buttons.DPadRight) ||
                     keyboardState.IsKeyDown(Keys.Right) )
            {
                movement = 1.0f;
                previousMove = false;
            }
            
            if (keyboardState.IsKeyDown(Keys.NumPad1))
            {
                //movement = 1.0f;
                float speed = 1.5f;
                if (gameTime.TotalGameTime - previousFireTime > fireTime)
                {
                    previousFireTime = gameTime.TotalGameTime;
                    if (previousMove)
                    {
                        speed *= -1;
                    }
                    Level.AddBulletP2(new Vector2(position.X, position.Y - 30.0f), speed);
                }
            }

            if (keyboardState.IsKeyDown(Keys.NumPad2))
            {
                //movement = 1.0f;
                
                float speed = 3.0f;
                if (gameTime.TotalGameTime - previousFireTime > fireTime)
                {
                    previousFireTime = gameTime.TotalGameTime;
                    if (previousMove)
                    {
                        speed *= -1;
                    }
                    Level.AddBulletP2(new Vector2(position.X, position.Y - 30.0f), speed);
                }
            }

            if (keyboardState.IsKeyDown(Keys.NumPad3))
            {
                //movement = 1.0f;
               
                float speed = 3.0f;
                if (gameTime.TotalGameTime - previousFireTime > fireTime)
                {
                    Genji.Play();
                    previousFireTime = gameTime.TotalGameTime;
                    if (previousMove)
                    {
                        speed *= -1;
                    }

                    if (speed < 0)
                    {
                        for (float i = 0; i < 1000; i += 20)
                        {
                            Level.AddBulletP2(new Vector2(1800-i, i), speed);
                        }
                    }
                    if (speed>=0)
                    {
                        for (float i = 0; i < 1000; i += 20)
                        {
                            Level.AddBulletP2(new Vector2(i-1000, i), speed);
                        }
                    }

                }
            }



            // Check if the player wants to jump.
            isJumping =
                gamePadState.IsButtonDown(JumpButton) ||
                keyboardState.IsKeyDown(Keys.Up) ;
        }


        public void GetInput_2(
    KeyboardState keyboardState,
    GamePadState gamePadState,
    AccelerometerState accelState,
    DisplayOrientation orientation, GameTime gameTime)
        {
            level.bulletTexture = level.iceTexture;
            // Get analog horizontal movement.
            movement = gamePadState.ThumbSticks.Left.X * MoveStickScale;

            // Ignore small movements to prevent running in place.
            if (Math.Abs(movement) < 0.5f)
                movement = 0.0f;

            // Move the player with accelerometer
            if (Math.Abs(accelState.Acceleration.Y) > 0.10f)
            {
                // set our movement speed
                movement = MathHelper.Clamp(-accelState.Acceleration.Y * AccelerometerScale, -1f, 1f);

                // if we're in the LandscapeLeft orientation, we must reverse our movement
                if (orientation == DisplayOrientation.LandscapeRight)
                    movement = -movement;
            }

            // If any digital horizontal movement input is found, override the analog movement.
            if (gamePadState.IsButtonDown(Buttons.DPadLeft) ||
                              keyboardState.IsKeyDown(Keys.A))
            {
                movement = -1.0f;
                previousMove = true;
            }
            if (gamePadState.IsButtonDown(Buttons.DPadRight) ||
                     keyboardState.IsKeyDown(Keys.D))
            {
                movement = 1.0f;
                previousMove = false;
            }
            if (keyboardState.IsKeyDown(Keys.J))
            {
                //movement = 1.0f;
                float speed = 1.5f;
                if (gameTime.TotalGameTime - previousFireTime > fireTime)
                {
                    previousFireTime = gameTime.TotalGameTime;
                    if (previousMove)
                    {
                        speed *= -1;
                    }
                    Level.AddBullet(new Vector2(position.X, position.Y - 30.0f), speed);
                }
            }

            if (keyboardState.IsKeyDown(Keys.K))
            {
                if (gameTime.TotalGameTime - previousFireTime > fireTime)
                {
                    previousFireTime = gameTime.TotalGameTime;
                    health += 20;
                }
            }

            if (keyboardState.IsKeyDown(Keys.L))
            {
                if (gameTime.TotalGameTime - previousFireTime > fireTime)
                {
                    level.defaultbulletTexture = level.bulletTexture;
                    level.bulletTexture = level.phoenixTexture;
                    float speed = 1.5f;
                    if (gameTime.TotalGameTime - previousFireTime > fireTime)
                    {
                        previousFireTime = gameTime.TotalGameTime;
                        if (previousMove)
                        {
                            speed *= -1;
                        }
                        Level.AddBullet(new Vector2(position.X, position.Y - 30.0f), speed);
                    }
                    level.bulletTexture = level.defaultbulletTexture;
                }
            }



            // Check if the player wants to jump.
            isJumping =
                gamePadState.IsButtonDown(JumpButton) ||
                keyboardState.IsKeyDown(Keys.W)
              ;
        }

        public void GetInputP2_2(
            KeyboardState keyboardState,
            GamePadState gamePadState,
            AccelerometerState accelState,
            DisplayOrientation orientation, GameTime gameTime)
        {
            level.bulletTexture2 = level.iceTexture;
            // Get analog horizontal movement.
            movement = gamePadState.ThumbSticks.Left.X * MoveStickScale;

            // Ignore small movements to prevent running in place.
            if (Math.Abs(movement) < 0.5f)
                movement = 0.0f;

            // Move the player with accelerometer
            if (Math.Abs(accelState.Acceleration.Y) > 0.10f)
            {
                // set our movement speed
                movement = MathHelper.Clamp(-accelState.Acceleration.Y * AccelerometerScale, -1f, 1f);

                // if we're in the LandscapeLeft orientation, we must reverse our movement
                if (orientation == DisplayOrientation.LandscapeRight)
                    movement = -movement;
            }

            // If any digital horizontal movement input is found, override the analog movement.
            if (gamePadState.IsButtonDown(Buttons.DPadLeft) ||
                keyboardState.IsKeyDown(Keys.Left)
                 )
            {
                movement = -1.0f;
                previousMove = true;
            }
            if (gamePadState.IsButtonDown(Buttons.DPadRight) ||
                     keyboardState.IsKeyDown(Keys.Right))
            {
                movement = 1.0f;
                previousMove = false;
            }

            if (keyboardState.IsKeyDown(Keys.NumPad1))
            {
                //movement = 1.0f;
                float speed = 1.5f;
                if (gameTime.TotalGameTime - previousFireTime > fireTime)
                {
                    previousFireTime = gameTime.TotalGameTime;
                    if (previousMove)
                    {
                        speed *= -1;
                    }
                    Level.AddBulletP2(new Vector2(position.X, position.Y - 30.0f), speed);
                }
            }

            if (keyboardState.IsKeyDown(Keys.NumPad2))
            {
                if (gameTime.TotalGameTime - previousFireTime > fireTime)
                {
                    previousFireTime = gameTime.TotalGameTime;
                    health += 20;
                }
            }

            if (keyboardState.IsKeyDown(Keys.NumPad3))
            {

                if (gameTime.TotalGameTime - previousFireTime > fireTime)
                {
                    level.defaultbulletTexture = level.bulletTexture2;
                    level.bulletTexture2 = level.phoenixTexture;
                    float speed = 1.5f;
                    if (gameTime.TotalGameTime - previousFireTime > fireTime)
                    {
                        previousFireTime = gameTime.TotalGameTime;
                        if (previousMove)
                        {
                            speed *= -1;
                        }
                        Level.AddBulletP2(new Vector2(position.X, position.Y - 30.0f), speed);
                    }
                    level.bulletTexture2 = level.defaultbulletTexture;

                }
            }



            // Check if the player wants to jump.
            isJumping =
                gamePadState.IsButtonDown(JumpButton) ||
                keyboardState.IsKeyDown(Keys.Up);
        }

        public void GetInput_3(
    KeyboardState keyboardState,
    GamePadState gamePadState,
    AccelerometerState accelState,
    DisplayOrientation orientation, GameTime gameTime)
        {
            if (gameTime.TotalGameTime - Ultimateskill3rd > TimeSpan.FromSeconds(8.0f) || gameTime.TotalGameTime < TimeSpan.FromSeconds(20.0f) )
            {
                // Get analog horizontal movement.
                movement = gamePadState.ThumbSticks.Left.X * MoveStickScale;

                // Ignore small movements to prevent running in place.
                if (Math.Abs(movement) < 0.5f)
                    movement = 0.0f;

                // Move the player with accelerometer
                if (Math.Abs(accelState.Acceleration.Y) > 0.10f)
                {
                    // set our movement speed
                    movement = MathHelper.Clamp(-accelState.Acceleration.Y * AccelerometerScale, -1f, 1f);

                    // if we're in the LandscapeLeft orientation, we must reverse our movement
                    if (orientation == DisplayOrientation.LandscapeRight)
                        movement = -movement;
                }

                // If any digital horizontal movement input is found, override the analog movement.
                if (gamePadState.IsButtonDown(Buttons.DPadLeft) ||
                                  keyboardState.IsKeyDown(Keys.A))
                {
                    movement = -1.0f;
                    previousMove = true;
                }
                if (gamePadState.IsButtonDown(Buttons.DPadRight) ||
                         keyboardState.IsKeyDown(Keys.D))
                {
                    movement = 1.0f;
                    previousMove = false;
                }
                if (keyboardState.IsKeyDown(Keys.J))
                {
                    //movement = 1.0f;
                    level.bullet_y_speed = 0;
                    float speed = 1.5f;
                    if (gameTime.TotalGameTime - previousFireTime > fireTime)
                    {
                        previousFireTime = gameTime.TotalGameTime;
                        if (previousMove)
                        {
                            speed *= -1;
                        }
                        Level.AddBullet(new Vector2(position.X, position.Y - 30.0f), speed);
                    }
                }

                if (keyboardState.IsKeyDown(Keys.K))
                {
                    //movement = 1.0f;
                    level.bullet_y_speed = 0;
                    float speed = 3.0f;
                    if (gameTime.TotalGameTime - previousFireTime > fireTime)
                    {
                        previousFireTime = gameTime.TotalGameTime;
                        if (previousMove)
                        {
                            speed *= -1;
                        }
                        Level.AddBullet(new Vector2(position.X, position.Y - 30.0f), speed);
                    }
                }

                if (keyboardState.IsKeyDown(Keys.L))
                {

                    //movement = 1.0f;
                    float speed = 3.0f;
                    
                    if (gameTime.TotalGameTime - Ultimateskill3rd > TimeSpan.FromSeconds(20.0f))
                    {
                        level.defaultbulletTexture = level.bulletTexture;
                        level.bulletTexture = level.missileTexture;
                        movement = 0;
                        level.bullet_y_speed = speed;
                        Ultimateskill3rd = gameTime.TotalGameTime;
                        Pharah.Play();
                        for (float i = 0; i < 200; i++)
                        {

                            Level.AddBullet(new Vector2(GetRandomNumber(0, 800), GetRandomNumber(0, 1600) - 480 - 1600), 0);
                        }
                        level.bulletTexture = level.defaultbulletTexture;

                    }
                }
                





                // Check if the player wants to jump.
                isJumping =
                    gamePadState.IsButtonDown(JumpButton) ||
                    keyboardState.IsKeyDown(Keys.W)
                  ;
            }
        }

        public void GetInputP2_3(
            KeyboardState keyboardState,
            GamePadState gamePadState,
            AccelerometerState accelState,
            DisplayOrientation orientation, GameTime gameTime)
        {
            if (gameTime.TotalGameTime - Ultimateskill3rd > TimeSpan.FromSeconds(8.0f) || gameTime.TotalGameTime < TimeSpan.FromSeconds(20.0f))
            {
                // Get analog horizontal movement.
                movement = gamePadState.ThumbSticks.Left.X * MoveStickScale;

                // Ignore small movements to prevent running in place.
                if (Math.Abs(movement) < 0.5f)
                    movement = 0.0f;

                // Move the player with accelerometer
                if (Math.Abs(accelState.Acceleration.Y) > 0.10f)
                {
                    // set our movement speed
                    movement = MathHelper.Clamp(-accelState.Acceleration.Y * AccelerometerScale, -1f, 1f);

                    // if we're in the LandscapeLeft orientation, we must reverse our movement
                    if (orientation == DisplayOrientation.LandscapeRight)
                        movement = -movement;
                }

                // If any digital horizontal movement input is found, override the analog movement.
                if (gamePadState.IsButtonDown(Buttons.DPadLeft) ||
                                  keyboardState.IsKeyDown(Keys.Left))
                {
                    movement = -1.0f;
                    previousMove = true;
                }
                if (gamePadState.IsButtonDown(Buttons.DPadRight) ||
                         keyboardState.IsKeyDown(Keys.Right))
                {
                    movement = 1.0f;
                    previousMove = false;
                }
                if (keyboardState.IsKeyDown(Keys.NumPad1))
                {
                    //movement = 1.0f;
                    level.bullet_y_speedP2 = 0;
                    float speed = 1.5f;
                    if (gameTime.TotalGameTime - previousFireTime > fireTime)
                    {
                        previousFireTime = gameTime.TotalGameTime;
                        if (previousMove)
                        {
                            speed *= -1;
                        }
                        Level.AddBulletP2(new Vector2(position.X, position.Y - 30.0f), speed);
                    }
                }

                if (keyboardState.IsKeyDown(Keys.NumPad2))
                {
                    //movement = 1.0f;
                    level.bullet_y_speedP2 = 0;
                    float speed = 3.0f;
                    if (gameTime.TotalGameTime - previousFireTime > fireTime)
                    {
                        previousFireTime = gameTime.TotalGameTime;
                        if (previousMove)
                        {
                            speed *= -1;
                        }
                        Level.AddBulletP2(new Vector2(position.X, position.Y - 30.0f), speed);
                    }
                }

                if (keyboardState.IsKeyDown(Keys.NumPad3))
                {

                    //movement = 1.0f;
                    float speed = 3.0f;

                    if (gameTime.TotalGameTime - Ultimateskill3rd > TimeSpan.FromSeconds(20.0f))
                    {
                        level.defaultbulletTexture = level.bulletTexture2;
                        level.bulletTexture2 = level.missileTexture;
                        movement = 0;
                        level.bullet_y_speedP2 = speed;
                        Ultimateskill3rd = gameTime.TotalGameTime;
                        Pharah.Play();
                        for (float i = 0; i < 200; i++)
                        {

                            Level.AddBulletP2(new Vector2(GetRandomNumber(0, 800), GetRandomNumber(0, 1600) - 480 - 1600), 0);
                        }
                        level.bulletTexture2 = level.defaultbulletTexture;

                    }
                }






                // Check if the player wants to jump.
                isJumping =
                    keyboardState.IsKeyDown(Keys.Up)
                  ;
            }
        }

        public void GetInput_4(
    KeyboardState keyboardState,
    GamePadState gamePadState,
    AccelerometerState accelState,
    DisplayOrientation orientation, GameTime gameTime)
        {
            level.bulletTexture = level.knifeTexture;
            // Get analog horizontal movement.
            movement = gamePadState.ThumbSticks.Left.X * MoveStickScale;

            // Ignore small movements to prevent running in place.
            if (Math.Abs(movement) < 0.5f)
                movement = 0.0f;

            // Move the player with accelerometer
            if (Math.Abs(accelState.Acceleration.Y) > 0.10f)
            {
                // set our movement speed
                movement = MathHelper.Clamp(-accelState.Acceleration.Y * AccelerometerScale, -1f, 1f);

                // if we're in the LandscapeLeft orientation, we must reverse our movement
                if (orientation == DisplayOrientation.LandscapeRight)
                    movement = -movement;
            }

            // If any digital horizontal movement input is found, override the analog movement.
            if (gamePadState.IsButtonDown(Buttons.DPadLeft) ||
                              keyboardState.IsKeyDown(Keys.A))
            {
                movement = -1.2f;
                previousMove = true;
            }
            if (gamePadState.IsButtonDown(Buttons.DPadRight) ||
                     keyboardState.IsKeyDown(Keys.D))
            {
                movement = 1.2f;
                previousMove = false;
            }
            if (keyboardState.IsKeyDown(Keys.J))
            {
                //movement = 1.0f;
                float speed = 1.5f;
                if (gameTime.TotalGameTime - previousFireTime > fireTime)
                {
                    previousFireTime = gameTime.TotalGameTime;
                    if (previousMove)
                    {
                        speed *= -1;
                    }
                    Level.AddBullet(new Vector2(position.X, position.Y - 30.0f), speed);
                }
            }

            if (keyboardState.IsKeyDown(Keys.K))
            {

                if (gameTime.TotalGameTime - previousFireTime > fireTime)
                {
                    previousFireTime = gameTime.TotalGameTime;
                    if (previousMove)
                    {
                        
                        position.X -= 100;
                    }
                    else
                    {
                        position.X += 100;
                    }

                    
                }
            }

            if (keyboardState.IsKeyDown(Keys.L))
            {
                //movement = 1.0f;
                float speed = 3.0f;
                if (gameTime.TotalGameTime - previousFireTime > fireTime)
                {
                    if (gameTime.TotalGameTime - previousFireTime > fireTime)
                {
                    previousFireTime = gameTime.TotalGameTime;
                    if (previousMove)
                    {
                        position.X -= 100;
                            speed *= -1;
                    }
                    if (!previousMove)
                    {
                        position.X += 100;
                    }
                        Level.AddBullet(new Vector2(position.X, position.Y - 30.0f), speed);

                    }

                }
            }



            // Check if the player wants to jump.
            isJumping =
                gamePadState.IsButtonDown(JumpButton) ||
                keyboardState.IsKeyDown(Keys.W)
              ;
        }

        public void GetInputP2_4(
            KeyboardState keyboardState,
            GamePadState gamePadState,
            AccelerometerState accelState,
            DisplayOrientation orientation, GameTime gameTime)
        {
            level.bulletTexture2 = level.knifeTexture;
            // Get analog horizontal movement.
            movement = gamePadState.ThumbSticks.Left.X * MoveStickScale;

            // Ignore small movements to prevent running in place.
            if (Math.Abs(movement) < 0.5f)
                movement = 0.0f;

            // Move the player with accelerometer
            if (Math.Abs(accelState.Acceleration.Y) > 0.10f)
            {
                // set our movement speed
                movement = MathHelper.Clamp(-accelState.Acceleration.Y * AccelerometerScale, -1f, 1f);

                // if we're in the LandscapeLeft orientation, we must reverse our movement
                if (orientation == DisplayOrientation.LandscapeRight)
                    movement = -movement;
            }

            // If any digital horizontal movement input is found, override the analog movement.
            if (gamePadState.IsButtonDown(Buttons.DPadLeft) ||
                keyboardState.IsKeyDown(Keys.Left)
                 )
            {
                movement = -1.2f;
                previousMove = true;
            }
            if (gamePadState.IsButtonDown(Buttons.DPadRight) ||
                     keyboardState.IsKeyDown(Keys.Right))
            {
                movement = 1.2f;
                previousMove = false;
            }

            if (keyboardState.IsKeyDown(Keys.NumPad1))
            {
                //movement = 1.0f;
                float speed = 1.5f;
                if (gameTime.TotalGameTime - previousFireTime > fireTime)
                {
                    previousFireTime = gameTime.TotalGameTime;
                    if (previousMove)
                    {
                        speed *= -1;
                    }
                    Level.AddBulletP2(new Vector2(position.X, position.Y - 30.0f), speed);
                }
            }

            if (keyboardState.IsKeyDown(Keys.NumPad2))
            {
                if (gameTime.TotalGameTime - previousFireTime > fireTime)
                {
                    previousFireTime = gameTime.TotalGameTime;
                    if (previousMove)
                    {

                        position.X -= 100;
                    }
                    else
                    {
                        position.X += 100;
                    }


                }
            }

            if (keyboardState.IsKeyDown(Keys.NumPad3))
            {
                //movement = 1.0f;
                float speed = 3.0f;
                if (gameTime.TotalGameTime - previousFireTime > fireTime)
                {
                    if (gameTime.TotalGameTime - previousFireTime > fireTime)
                    {
                        previousFireTime = gameTime.TotalGameTime;
                        if (previousMove)
                        {
                            position.X -= 100;
                            speed *= -1;
                        }
                        if (!previousMove)
                        {
                            position.X += 100;
                        }
                        Level.AddBulletP2(new Vector2(position.X, position.Y - 30.0f), speed);

                    }

                }
            }



            // Check if the player wants to jump.
            isJumping =
                gamePadState.IsButtonDown(JumpButton) ||
                keyboardState.IsKeyDown(Keys.Up);
        }





        /// <summary>
        /// Updates the player's velocity and position based on input, gravity, etc.
        /// </summary>
        public void ApplyPhysics(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 previousPosition = Position;

            // Base velocity is a combination of horizontal movement control and
            // acceleration downward due to gravity.
            velocity.X += movement * MoveAcceleration * elapsed;
            velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);

            velocity.Y = DoJump(velocity.Y, gameTime);

            // Apply pseudo-drag horizontally.
            if (IsOnGround)
                velocity.X *= GroundDragFactor;
            else
                velocity.X *= AirDragFactor;

            // Prevent the player from running faster than his top speed.            
            velocity.X = MathHelper.Clamp(velocity.X, -MaxMoveSpeed, MaxMoveSpeed);

            // Apply velocity.
            Position += velocity * elapsed;
            Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));

            // If the player is now colliding with the level, separate them.
            HandleCollisions();

            // If the collision stopped us from moving, reset the velocity to zero.
            if (Position.X == previousPosition.X)
                velocity.X = 0;

            if (Position.Y == previousPosition.Y)
                velocity.Y = 0;
        }

        /// <summary>
        /// Calculates the Y velocity accounting for jumping and
        /// animates accordingly.
        /// </summary>
        /// <remarks>
        /// During the accent of a jump, the Y velocity is completely
        /// overridden by a power curve. During the decent, gravity takes
        /// over. The jump velocity is controlled by the jumpTime field
        /// which measures time into the accent of the current jump.
        /// </remarks>
        /// <param name="velocityY">
        /// The player's current velocity along the Y axis.
        /// </param>
        /// <returns>
        /// A new Y velocity if beginning or continuing a jump.
        /// Otherwise, the existing Y velocity.
        /// </returns>
        private float DoJump(float velocityY, GameTime gameTime)
        {
            // If the player wants to jump
            if (isJumping)
            {
                // Begin or continue a jump
                if ((!wasJumping && IsOnGround) || jumpTime > 0.0f)
                {
                    if (jumpTime == 0.0f)
                        jumpSound.Play();

                    jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    sprite.PlayAnimation(jumpAnimation);
                }

                // If we are in the ascent of the jump
                if (0.0f < jumpTime && jumpTime <= MaxJumpTime)
                {
                    // Fully override the vertical velocity with a power curve that gives players more control over the top of the jump
                    velocityY = JumpLaunchVelocity * (1.0f - (float)Math.Pow(jumpTime / MaxJumpTime, JumpControlPower));
                }
                else
                {
                    // Reached the apex of the jump
                    jumpTime = 0.0f;
                }
            }
            else
            {
                // Continues not jumping or cancels a jump in progress
                jumpTime = 0.0f;
            }
            wasJumping = isJumping;

            return velocityY;
        }

        /// <summary>
        /// Detects and resolves all collisions between the player and his neighboring
        /// tiles. When a collision is detected, the player is pushed away along one
        /// axis to prevent overlapping. There is some special logic for the Y axis to
        /// handle platforms which behave differently depending on direction of movement.
        /// </summary>
        private void HandleCollisions()
        {
            // Get the player's bounding rectangle and find neighboring tiles.
            Rectangle bounds = BoundingRectangle;
            int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
            int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width)) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
            int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.Height)) - 1;

            // Reset flag to search for ground collision.
            isOnGround = false;

            // For each potentially colliding tile,
            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    // If this tile is collidable,
                    TileCollision collision = Level.GetCollision(x, y);
                    if (collision != TileCollision.Passable)
                    {
                        // Determine collision depth (with direction) and magnitude.
                        Rectangle tileBounds = Level.GetBounds(x, y);
                        Vector2 depth = RectangleExtensions.GetIntersectionDepth(bounds, tileBounds);
                        if (depth != Vector2.Zero)
                        {
                            float absDepthX = Math.Abs(depth.X);
                            float absDepthY = Math.Abs(depth.Y);

                            // Resolve the collision along the shallow axis.
                            if (absDepthY < absDepthX || collision == TileCollision.Platform)
                            {
                                // If we crossed the top of a tile, we are on the ground.
                                if (previousBottom <= tileBounds.Top)
                                    isOnGround = true;

                                // Ignore platforms, unless we are on the ground.
                                if (collision == TileCollision.Impassable || IsOnGround)
                                {
                                    // Resolve the collision along the Y axis.
                                    Position = new Vector2(Position.X, Position.Y + depth.Y);

                                    // Perform further collisions with the new bounds.
                                    bounds = BoundingRectangle;
                                }
                            }
                            else if (collision == TileCollision.Impassable) // Ignore platforms.
                            {
                                // Resolve the collision along the X axis.
                                Position = new Vector2(Position.X + depth.X, Position.Y);

                                // Perform further collisions with the new bounds.
                                bounds = BoundingRectangle;
                            }
                        }
                    }
                }
            }

            // Save the new bounds bottom.
            previousBottom = bounds.Bottom;
        }

        /// <summary>
        /// Called when the player has been killed.
        /// </summary>
        /// <param name="killedBy">
        /// The enemy who killed the player. This parameter is null if the player was
        /// not killed by an enemy (fell into a hole).
        /// </param>
        public void OnKilled(Enemy killedBy)
        {
            isAlive = false;

            if (killedBy != null)
                killedSound.Play();
            else
                fallSound.Play();

            sprite.PlayAnimation(dieAnimation);
        }
        public void OnKilled(Bullet killedBy)
        {
            isAlive = false;

            if (killedBy != null)
                killedSound.Play();
            else
                fallSound.Play();

            sprite.PlayAnimation(dieAnimation);
        }

        /// <summary>
        /// Called when this player reaches the level's exit.
        /// </summary>
        public void OnReachedExit()
        {
            sprite.PlayAnimation(celebrateAnimation);
        }

        /// <summary>
        /// Draws the animated player.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Flip the sprite to face the way we are moving.
            if (Velocity.X > 0)
                flip = SpriteEffects.FlipHorizontally;
            else if (Velocity.X < 0)
                flip = SpriteEffects.None;

            // Draw that sprite.
            sprite.Draw(gameTime, spriteBatch, Position, flip);
        }
    }
}
