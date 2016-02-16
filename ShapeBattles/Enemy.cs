using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ShapeBattles
{
    class Enemy
    {
        //public static Texture2D Texture;
        public static Viewport GraphicsViewport;
        public static Random rand;
        public static int BadassRandomness = 100;

        private bool _chases;   //does this enemy chase the player (or just bounce around aimlessly) - Greens currently just bounce

        private Texture2D _texture;
        public Texture2D Texture
        {
            get { return _texture; }
            set { _texture = value; }
        }

        private string _color;
        public string sColor
        {
            get { return _color; }
            set { _color = value; }
        }
	
        private Vector2 _position;
        public Vector2 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        private bool _damages;
        public bool Damages
        {
            get { return _damages; }
            set { _damages = value; }
        }
        
        private Vector2 _velocity;
        public Vector2 Velocity
        {
            get { return _velocity; }
            set { _velocity = value; }
        }

        private Color _drawingColor;
        public Color DrawingColor
        {
            get { return _drawingColor; }
            set { _drawingColor = value; }
        }

        private bool _living;
        public bool Living
        {
            get { return _living; }
            set { _living = value; }
        }
                	
        public Enemy(ContentManager content)
        {
            _drawingColor = Color.White;
            _living = true;
            _damages = true;
            int randomSpeed = rand.Next(1, 3);
            _velocity = new Vector2(randomSpeed, randomSpeed);

            int randomBall = rand.Next(1, 5);
            int randomSuperEnemy = rand.Next(1, BadassRandomness);
            if (randomSuperEnemy == 1)
                randomBall = 5;
            
            switch (randomBall)
            {
                case 1:
                    _position = new Vector2(rand.Next(1, 50), rand.Next(1, 50));
                    //_velocity = new Vector2(rand.Next(1, 10), rand.Next(1, 10));
                    _texture = content.Load<Texture2D>("Content\\Assets\\blue");
                    _color = "blue";
                    _chases = true;
                    break;
                case 2:
                    _position = new Vector2(rand.Next(1, 50), rand.Next(500, 580));
                    //_velocity = new Vector2(rand.Next(5, 15), rand.Next(5, 15));
                    _texture = content.Load<Texture2D>("Content\\Assets\\green");
                    _color = "green";
                    _chases = false;
                    break;
                case 3:
                    _position = new Vector2(rand.Next(700, 780), rand.Next(500, 580));
                    _velocity = new Vector2(rand.Next(2, 4), rand.Next(2, 4));  //Reds are more DANGEROUS!
                    _texture = content.Load<Texture2D>("Content\\Assets\\red");
                    _color = "red";
                    _chases = true;
                    break;
                case 4:
                    _position = new Vector2(rand.Next(700, 780), rand.Next(1, 50));
                    //_velocity = new Vector2(rand.Next(1, 5), rand.Next(1, 5));
                    _texture = content.Load<Texture2D>("Content\\Assets\\yellow");
                    _color = "yellow";
                    _chases = true;
                    break;
                case 5:     //this is the SUPER RARE purple badass!
                    _position = new Vector2(2, 400);
                    _velocity = new Vector2(5, 5);
                    _texture = content.Load<Texture2D>("Content\\Assets\\purple");
                    _color = "purple";
                    _chases = true;
                    break;
            }
        }

        public Enemy(ContentManager content, bool damages, string color, Vector2 position, Vector2 velocity)
        {
            _drawingColor = Color.White;
            _living = true;
            _damages = damages;
            _position = position;
            _velocity = velocity;
            _texture = content.Load<Texture2D>("Content\\Assets\\" + color);
            _color = color;
        }

        public void UpdateMovement(Player player)
        {
            //adjust for the centered origin of player (since we want to compare based on 0,0)
            Vector2 adjPlayerPos = new Vector2(player.Position.X - Player.Texture.Width / 2, player.Position.Y - Player.Texture.Height / 2);

            if (_living && _damages && _chases)
            {
                if (_position.X > adjPlayerPos.X)
                    _position.X -= _velocity.X;
                else if (_position.X < adjPlayerPos.X)
                    _position.X += _velocity.X;

                if (_position.Y > adjPlayerPos.Y)
                    _position.Y -= _velocity.Y;
                else if (_position.Y < adjPlayerPos.Y)
                    _position.Y += _velocity.Y;
            }
            else
            {
                _position += _velocity;
            }

            //now check to see if its hitting Player:                
            if (_living && _damages)
            {
                //Left Side of enemy hitting Right Side of player?
                if (_position.X <= adjPlayerPos.X + Player.Texture.Width && _position.X >= adjPlayerPos.X && _position.Y >= adjPlayerPos.Y && _position.Y <= adjPlayerPos.Y + Player.Texture.Height)
                    player.Lives = player.Lives - 1;
                //Right Side of enemy hitting Left Side of player?
                else if (_position.X + _texture.Width >= adjPlayerPos.X && _position.X + _texture.Width <= adjPlayerPos.X + Player.Texture.Width && _position.Y >= adjPlayerPos.Y && _position.Y <= adjPlayerPos.Y + Player.Texture.Height)
                    player.Lives = player.Lives - 1;
                //Top of enemy hitting Bottom of player?
                else if (_position.Y <= adjPlayerPos.Y + Player.Texture.Height && _position.Y >= adjPlayerPos.Y && _position.X >= adjPlayerPos.X && _position.X <= adjPlayerPos.X + Player.Texture.Width)
                    player.Lives = player.Lives - 1;
                //Bottom of enemy hitting Top of player?
                else if (_position.Y + _texture.Height >= adjPlayerPos.Y && _position.Y + _texture.Height <= adjPlayerPos.Y + Player.Texture.Height && _position.X >= adjPlayerPos.X && _position.X <= adjPlayerPos.X + Player.Texture.Width)
                    player.Lives = player.Lives - 1;

                if (player.Lives < 0)
                {
                    player.Status = false;
                    player.Lives = 0;
                }
            }
            //make sure it doesnt leave the Viewport (technically it shouldnt since its chasing the player (and he cant leave)
            if (_position.X < 0 || _position.X > (GraphicsViewport.Width - _texture.Width))
            {
                if (_damages)
                {
                    _velocity.X = -_velocity.X;
                    _position.X = _position.X + _velocity.X;
                }
                else
                    _living = false;
            }
            if (_position.Y < 0 || _position.Y > (GraphicsViewport.Height - _texture.Height))
            {
                if (_damages)
                {
                    _velocity.Y = -_velocity.Y;
                    _position.Y = _position.Y + _velocity.Y;
                }
                else
                    _living = false;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if(_damages)
                spriteBatch.Draw(_texture, _position, _drawingColor);
            else
                if(_color == "white")   //player debris, so make them slightly larger
                    spriteBatch.Draw(_texture, _position, null, _drawingColor, 0.0f, new Vector2(_texture.Width / 2, _texture.Height / 2), 0.5f, SpriteEffects.None, 0f);
                else
                    spriteBatch.Draw(_texture, _position, null, _drawingColor, 0.0f, new Vector2(_texture.Width/2, _texture.Height/2), 0.2f, SpriteEffects.None, 0f);
        }
    }
}
