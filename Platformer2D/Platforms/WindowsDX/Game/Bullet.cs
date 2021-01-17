using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer2D
{
    class Bullet
    {
        public Texture2D Texture;

        private SpriteEffects flip = SpriteEffects.None;
       // Vector2 origin;
        public bool Active;

        public int Damage;

        public float Speed;


        Random rnd = new Random();
        public Level Level
        {
            get { return level; }
        }
        Level level;
        public Vector2 Position
        {
            get { return position; }
        }
        public Vector2 position;

        private Rectangle localBounds;
        public Vector2 Origin
        {
            get { return new Vector2(Texture.Width / 2.0f, Texture.Height); }
        }
        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }
        public int Width
        {
            get { return Texture.Width; }
        }

        public int Height
        {
            get { return Texture.Height; }
        }

        public Bullet(Texture2D texture,Vector2 pos,Level level,float speed)
        {

            position = pos;
            //this.viewport = viewport;
            Active = true;
            Damage = 5;
            this.level = level;
            Speed = speed;
            Texture = texture;
            //LoadContent();
            int width = (int)(texture.Width * 0.35);
            int left = (texture.Width - width) / 2;
            int height = (int)(texture.Width * 0.2);
            int top = texture.Height - height;
            localBounds = new Rectangle(left, top, width, height);
        }

        /*public void LoadContent()
        {
            Texture = Level.Content.Load<Texture2D>("Weapon/bullet");
           // origin = new Vector2(Texture.Width / 2.0f, Texture.Height / 2.0f);
         //   collectedSound = Level.Content.Load<SoundEffect>("Sounds/GemCollected");
        }*/



        public void Update()
        {
            position.X += Speed;
          
            if ((Position.X > 3000 && Speed > 0) || (Position.X < -3000  && Speed < 0 ))
            {
                Active = false;
            }
            if ((Position.Y > 3000 && Speed > 0) || (Position.Y < -3000))
            {
                Active = false;
            }
        }

        public void Draw(GameTime gameTime,SpriteBatch sb)
        {
            if (Speed < 0)
            {
                flip = SpriteEffects.FlipHorizontally;
            }
            else if (Speed >= 0) { flip = SpriteEffects.None; }
            sb.Draw(Texture, Position, null, Color.White, 0.0f,
   
            new Vector2(Texture.Width / 2, Texture.Height / 2), 1.0f, flip, 0f);
        }

    }
}
