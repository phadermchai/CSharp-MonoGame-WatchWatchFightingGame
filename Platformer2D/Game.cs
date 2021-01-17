#region File Description
//-----------------------------------------------------------------------------
// PlatformerGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input.Touch;
using System.Collections;
using System.Collections.Generic;




namespace Platformer2D
{

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class PlatformerGame : Microsoft.Xna.Framework.Game
    {
        Random rnd = new Random();
        bool menuDraw = true;
        bool menuMain = true;
        bool menu2 = false;
        bool menuStageSelect = false;
        public static int p1select = 1, p2select = 1;
        public static int stageSelect = 1;


        public enum Gamestate
        {
            MainMenu,
            GamePlay,
            EndOfGame,
        }
        Gamestate _state = Gamestate.MainMenu;

        // Resources for drawing.
        GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        Vector2 baseScreenSize = new Vector2(800, 480);
        private Matrix globalTransformation;

        public Viewport viewport;

        // Global content.
        private SpriteFont hudFont;
  
        private Texture2D winOverlay;
        private Texture2D loseOverlay;
        private Texture2D diedOverlay;

        // Meta-level game state.
        private int levelIndex;
        private Level level;
        private bool wasContinuePressed;

        // When the time remaining is less than the warning time, it blinks on the hud
        private static readonly TimeSpan WarningTime = TimeSpan.FromSeconds(30);

        // We store our input states so that we only poll once per frame, 
        // then we use the same input state wherever needed
        private GamePadState gamePadState;
        private KeyboardState keyboardState;
        private KeyboardState previousKS;
        private KeyboardState currentKS;
        InputHelper input;
        private TouchCollection touchState;
        private AccelerometerState accelerometerState;

        private VirtualGamePad virtualGamePad;

        // The number of levels in the Levels directory of our content. We assume that
        // levels in our content are 0-based and that all numbers under this constant
        // have a level file present. This allows us to not need to check for the file
        // or handle exceptions, both of which can add unnecessary time to level loading.
        private const int numberOfLevels = 3;
      

        public PlatformerGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

#if WINDOWS_PHONE
            TargetElapsedTime = TimeSpan.FromTicks(333333);
#endif
            graphics.IsFullScreen = false;

            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 480;
     

            Accelerometer.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load fonts
            hudFont = Content.Load<SpriteFont>("Fonts/Hud");

            // Load overlay textures
            winOverlay = Content.Load<Texture2D>("Overlays/you_win");
            loseOverlay = Content.Load<Texture2D>("Overlays/you_lose");
            diedOverlay = Content.Load<Texture2D>("Overlays/you_died");
            

            //Work out how much we need to scale our graphics to fill the screen
            float horScaling = GraphicsDevice.PresentationParameters.BackBufferWidth / baseScreenSize.X;
            float verScaling = GraphicsDevice.PresentationParameters.BackBufferHeight / baseScreenSize.Y;
            Vector3 screenScalingFactor = new Vector3(horScaling, verScaling, 1);
            globalTransformation = Matrix.CreateScale(screenScalingFactor);
          
            virtualGamePad = new VirtualGamePad(baseScreenSize, globalTransformation, Content.Load<Texture2D>("Sprites/VirtualControlArrow"));

            //Known issue that you get exceptions if you use Media PLayer while connected to your PC
            //See http://social.msdn.microsoft.com/Forums/en/windowsphone7series/thread/c8a243d2-d360-46b1-96bd-62b1ef268c66
            //Which means its impossible to test this from VS.
            //So we have to catch the exception and throw it away
            try
            {
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(Content.Load<Song>("Sounds/Music"));
            }
            catch { }

           // LoadNextLevel();
        }

    
     

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            keyboardState = Keyboard.GetState();
            //currentKS = Keyboard.GetState();
            //previousKS = keyboardState;

            // Handle polling for our input and handling high-level input
            switch (_state)
            {
                case Gamestate.MainMenu:
                    UpdateMainMenu(gameTime);
                    break;
                case Gamestate.GamePlay:
                    UpdateGameplay(gameTime);
                    break;
                case Gamestate.EndOfGame:
                    UpdateEndOfGame(gameTime);
                    break;
            }
            previousKS = keyboardState;

            base.Update(gameTime);
        }

        void UpdateMainMenu(GameTime gameTime)
        {
            // Respond to user input for menu selections, etc

            if (menuDraw == false)
                 _state = Gamestate.GamePlay;

 
            if (menu2 == true)
            {
                if (keyboardState.IsKeyDown(Keys.A) && !previousKS.IsKeyDown(Keys.A) && p1select > 1)
                {
                    p1select--;
                }
                if (keyboardState.IsKeyDown(Keys.R))
                {
                    p1select = rnd.Next(1, 5);
                }
                if (keyboardState.IsKeyDown(Keys.NumPad5))
                {
                    p2select = rnd.Next(1, 5);
                }
                if (keyboardState.IsKeyDown(Keys.D) && !previousKS.IsKeyDown(Keys.D) && p1select < 4)
                {
                    p1select++;
                }
                if (keyboardState.IsKeyDown(Keys.Left) && !previousKS.IsKeyDown(Keys.Left) && p2select > 1)
                {
                    p2select--;
                }
                if (keyboardState.IsKeyDown(Keys.Right) && !previousKS.IsKeyDown(Keys.Right) && p2select < 4)
                {
                    p2select++;
                }

            }
            if(menuStageSelect == true)
            {
                if (keyboardState.IsKeyDown(Keys.A) && !previousKS.IsKeyDown(Keys.A) && stageSelect > 1)
                {
                    stageSelect--;
                }
                if (keyboardState.IsKeyDown(Keys.D) && !previousKS.IsKeyDown(Keys.D) && stageSelect < 3)
                {
                    stageSelect++;
                }
                if (keyboardState.IsKeyDown(Keys.R))
                {
                    stageSelect = rnd.Next(1, 4);
                }
                if (keyboardState.IsKeyDown(Keys.Enter) && !previousKS.IsKeyDown(Keys.Enter))
                {
                    menuDraw = false;
                    LoadNextLevel();
                }

            }
            if (menu2 == true && keyboardState.IsKeyDown(Keys.Enter) && !previousKS.IsKeyDown(Keys.Enter))
            {
                menuStageSelect = true;
                menu2 = false;
            }
            if (keyboardState.IsKeyDown(Keys.Enter) && !previousKS.IsKeyDown(Keys.Enter) && menuStageSelect == false)
            {
                menu2 = true;
                menuMain = false;
            }
            

        }

        void UpdateGameplay(GameTime gameTime)
        {
            // Respond to user actions in the game.
            // Update enemies
            // Handle collisions
            HandleInput(gameTime);

            // update our level, passing down the GameTime along with all of our input states
            level.Update(gameTime, keyboardState, gamePadState,
                         accelerometerState, Window.CurrentOrientation);

            if (level.Player.Velocity != Vector2.Zero)
                virtualGamePad.NotifyPlayerIsMoving();

            //--------------------------------------
            if (level.Player.IsAlive == false || level.Player2.IsAlive == false)
                _state = Gamestate.EndOfGame;
        }

        void UpdateEndOfGame(GameTime gameTime)
        {
            // Update scores
            // Do any animations, effects, etc for getting a high score
            // Respond to user input to restart level, or go back to main menu
            if (keyboardState.IsKeyDown(Keys.F5))
            {
                Mainmenu();
                _state = Gamestate.MainMenu;
            }
            if (keyboardState.IsKeyDown(Keys.Enter) && !previousKS.IsKeyDown(Keys.Enter))
            {
                level.StartNewLife();
                _state = Gamestate.GamePlay;
                //LoadNextLevel();
            }
        }

        private void HandleInput(GameTime gameTime)
        {
            // get all of our input states
            //keyboardState = Keyboard.GetState();
            touchState = TouchPanel.GetState();
            gamePadState = virtualGamePad.GetState(touchState, GamePad.GetState(PlayerIndex.One));
            accelerometerState = Accelerometer.GetState();
            

#if !NETFX_CORE
            // Exit the game when back is pressed.
            if (gamePadState.Buttons.Back == ButtonState.Pressed)
                Exit();
#endif
            bool continuePressed =
                keyboardState.IsKeyDown(Keys.Space) ||
                gamePadState.IsButtonDown(Buttons.A) ||
                touchState.AnyTouch();

            // Perform the appropriate action to advance the game and
            // to get the player back to playing.
            if (!wasContinuePressed && continuePressed)
            {
                if (!level.Player.IsAlive)
                {
                    level.StartNewLife();
                }
                else if (level.TimeRemaining == TimeSpan.Zero)
                {
                    if (level.ReachedExit)
                        LoadNextLevel();
                    else
                        ReloadCurrentLevel();
                }
            }

            wasContinuePressed = continuePressed;

            virtualGamePad.Update(gameTime);
        }

        private void LoadNextLevel()
        {
            // move to the next level
            levelIndex = stageSelect - 1;
            // Unloads the content for the current level before loading the next one.
            if (level != null)
                level.Dispose();

            // Load the level.
            string levelPath = string.Format("Content/Levels/{0}.txt", levelIndex);
            using (Stream fileStream = TitleContainer.OpenStream(levelPath))
                level = new Level(Services, fileStream, levelIndex);
        }

        private void ReloadCurrentLevel()
        {
            --levelIndex;
            LoadNextLevel();
        }

        /// <summary>
        /// Draws the game from background to foreground.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            viewport = GraphicsDevice.Viewport;
            graphics.GraphicsDevice.Clear(Color.Blue);
            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, globalTransformation);
            base.Draw(gameTime);
            switch (_state)
            {
                case Gamestate.MainMenu:
                    DrawMainMenu(gameTime);
                    break;
                case Gamestate.GamePlay:
                    DrawGameplay(gameTime);
                    break;
                case Gamestate.EndOfGame:
                    DrawEndOfGame(gameTime);
                    break;
            }


            //level.Draw(gameTime, spriteBatch);

            //DrawHud();

            spriteBatch.End();

            //base.Draw(gameTime);
        }

        void DrawMainMenu(GameTime gameTime)
        {
            // Draw the main menu, any active selections, etc
      
            if (menuMain == true)
            {
                spriteBatch.Draw(Content.Load<Texture2D>("Menu/BG_Watch"), new Rectangle(0, 0, 800, 480), Color.White);
            }
            if (menu2 == true)
            {
                spriteBatch.Draw(Content.Load<Texture2D>("Menu/BG_char"), new Rectangle(0, 0, 800, 480), Color.White);
                if (p1select == 1)
                {
                    spriteBatch.Draw(Content.Load<Texture2D>("PlayerSelect/Rew"), new Rectangle(60, 50, 300, 250), Color.White);
                }
                if (p1select == 2)
                {
                    spriteBatch.Draw(Content.Load<Texture2D>("PlayerSelect/Ken"), new Rectangle(60, 50, 300, 250), Color.White);
                }
                if (p1select == 3)
                {
                    spriteBatch.Draw(Content.Load<Texture2D>("PlayerSelect/Kuy"), new Rectangle(60, 50, 300, 250), Color.White);
                }
                if (p1select == 4)
                {
                    spriteBatch.Draw(Content.Load<Texture2D>("PlayerSelect/New"), new Rectangle(60, 50, 300, 250), Color.White);
                }
                if (p2select == 1)
                {
                    spriteBatch.Draw(Content.Load<Texture2D>("PlayerSelect/Rew"), new Rectangle(800-300-60, 50, 300, 250), Color.White);
                }
                if (p2select == 2)
                {
                    spriteBatch.Draw(Content.Load<Texture2D>("PlayerSelect/Ken"), new Rectangle(800 - 300 - 60, 50, 300, 250), Color.White);
                }
                if (p2select == 3)
                {
                    spriteBatch.Draw(Content.Load<Texture2D>("PlayerSelect/Kuy"), new Rectangle(800 - 300 - 60, 50, 300, 250), Color.White);
                }
                if (p2select == 4)
                {
                    spriteBatch.Draw(Content.Load<Texture2D>("PlayerSelect/New"), new Rectangle(800 - 300 - 60, 50, 300, 250), Color.White);
                }
            }
            if (menuStageSelect == true)
            {
                if (stageSelect == 1)
                {
                    spriteBatch.Draw(Content.Load<Texture2D>("Backgrounds/Layer0_0"), new Rectangle(0,0, 800, 480), Color.White);
                }
                if (stageSelect == 2)
                {
                    spriteBatch.Draw(Content.Load<Texture2D>("Backgrounds/Layer0_1"), new Rectangle(0, 0, 800, 480), Color.White);
                }
                if (stageSelect == 3)
                {
                    spriteBatch.Draw(Content.Load<Texture2D>("Backgrounds/Layer0_2"), new Rectangle(0, 0, 800, 480), Color.White);
                }
            }

        }

        void DrawGameplay(GameTime gameTime)
        {
            // Draw the background the level
            // Draw enemies
            // Draw the player
            // Draw particle effects, etc

            level.Draw(gameTime, spriteBatch);
            DrawHud();
        }

        void DrawEndOfGame(GameTime gameTime)
        {
            // Draw text and scores
            if (level.Player.IsAlive == false)
                spriteBatch.Draw(Content.Load<Texture2D>("Player2Win"), new Rectangle(0, 0, 800, 480), Color.White);
            if (level.Player2.IsAlive == false)
                spriteBatch.Draw(Content.Load<Texture2D>("Player1Win"), new Rectangle(0, 0, 800, 480), Color.White);


            // Draw menu for restarting level or going back to main menu
        }
        private void DrawHud()
        {
            Rectangle titleSafeArea = GraphicsDevice.Viewport.TitleSafeArea;
            Vector2 hudLocation = new Vector2(titleSafeArea.X, titleSafeArea.Y);
            //Vector2 center = new Vector2(titleSafeArea.X + titleSafeArea.Width / 2.0f,
            //                             titleSafeArea.Y + titleSafeArea.Height / 2.0f);

            Vector2 center = new Vector2(baseScreenSize.X / 2, baseScreenSize.Y / 2);

            // Draw time remaining. Uses modulo division to cause blinking when the
            // player is running out of time.
            string timeString = "TIME: " + level.TimeRemaining.Minutes.ToString("00") + ":" + level.TimeRemaining.Seconds.ToString("00");
            Color timeColor;
            if (level.TimeRemaining > WarningTime ||
                level.ReachedExit ||
                (int)level.TimeRemaining.TotalSeconds % 2 == 0)
            {
                timeColor = Color.Yellow;
            }
            else
            {
                timeColor = Color.Red;
            }
          //  DrawShadowedString(hudFont, timeString, hudLocation, timeColor);

            // Draw score
            float timeHeight = hudFont.MeasureString(timeString).Y;
           // DrawShadowedString(hudFont, "SCORE: " + level.Score.ToString(), hudLocation + new Vector2(0.0f, timeHeight * 1.2f), Color.Yellow);

            // Draw Health
            DrawShadowedString(hudFont, "PLAYER 1: " + level.Player.health.ToString(), hudLocation + new Vector2(0.0f, timeHeight * 1.2f), Color.Yellow);
            DrawShadowedString(hudFont, "PLAYER 2: " + level.Player2.health.ToString(), hudLocation + new Vector2(650.0f, timeHeight * 1.2f), Color.Yellow);

            // Determine the status overlay message to show.
            Texture2D status = null;
            if (level.TimeRemaining == TimeSpan.Zero)
            {
                if (level.ReachedExit)
                {
                    status = winOverlay;
                }
                else
                {
                    status = loseOverlay;
                }
            }
            else if (!level.Player.IsAlive)
            {
                status = diedOverlay;
            }

            if (status != null)
            {
                // Draw status message.
                Vector2 statusSize = new Vector2(status.Width, status.Height);
                spriteBatch.Draw(status, center - statusSize / 2, Color.White);
            }

            if (touchState.IsConnected)
                virtualGamePad.Draw(spriteBatch);
        }

        private void DrawShadowedString(SpriteFont font, string value, Vector2 position, Color color)
        {
            spriteBatch.DrawString(font, value, position + new Vector2(1.0f, 1.0f), Color.Black);
            spriteBatch.DrawString(font, value, position, color);
          
        }

        private void Mainmenu()
        {
            menuDraw = true;
            menuMain = true;
            menu2 = false;
            menuStageSelect = false;
            p1select = 1;
            p2select = 1;
            stageSelect = 1;
        }


    }
}
