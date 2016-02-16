using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ShapeBattles
{
    class Projectile
    {
        public static Viewport GraphicsViewport;
        public static Texture2D Texture;

        private Vector2 _position;
        public Vector2 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        private Vector2 _velocity;
        public Vector2 Velocity
        {
            get { return _velocity; }
            set { _velocity = value; }
        }

        private float _angle;
        public float Angle
        {
            get { return _angle; }
            set { _angle = value; }
        }

        private string _ammocolor;
        public string AmmoColor
        {
            get { return _ammocolor; }
            set { _ammocolor = value; }
        }

        private float _ammosize;
        public float AmmoSize
        {
            get { return _ammosize; }
            set { _ammosize = value; }
        }
	
        private bool _fired;
        public bool ShotFired
        {
            get { return _fired; }
            set { _fired = value; }
        }

        private bool _isbigbomb;
        public bool IsBigBomb
        {
            get { return _isbigbomb; }
            set { _isbigbomb = value; }
        }

        //constructor
        public Projectile(Vector2 startPos, Vector2 startVel, string aColor, float size)
        {
            _position = startPos;
            _velocity = startVel;
            _ammocolor = aColor;
            _ammosize = size;
            _fired = false;
            _isbigbomb = false;
        }

        public void UpdateMovement()
        {
            _position += _velocity;
            
            //if its out of the bounds of the viewport, stop its movement (TO DO: the object should be destroyed, not just stopped)
            if ((_position.Y + Texture.Height > GraphicsViewport.Height || _position.Y + Texture.Height < 0) || (_position.X + Texture.Width > GraphicsViewport.Width || _position.X + Texture.Width < 0))
            {
                _velocity.X = 0;
                _velocity.Y = 0;
                _fired = true;
                _position.X = -0.20f;
                _position.Y = -0.20f;
            }
        }

        public void CheckCollision(Enemy enemy)
        {
            if (enemy.Living)   //make sure we kill only bad guys and not any exploded parts of their dead self
            {
                if(_ammocolor == "white")
                {
                    if (_position.X <= enemy.Position.X + enemy.Texture.Width && _position.X >= enemy.Position.X && _position.Y >= enemy.Position.Y && _position.Y <= enemy.Position.Y + enemy.Texture.Height)
                    {
                        _fired = true;
                        enemy.Living = false;
                    }
                    //Right Side of enemy hitting Left Side of player?
                    if (_position.X + Texture.Width >= enemy.Position.X && _position.X + Texture.Width <= enemy.Position.X + enemy.Texture.Width && _position.Y >= enemy.Position.Y && _position.Y <= enemy.Position.Y + enemy.Texture.Height)
                    {
                        _fired = true;
                        enemy.Living = false;
                    }
                    //Top of enemy hitting Bottom of player?
                    if (_position.Y <= enemy.Position.Y + enemy.Texture.Height && _position.Y >= enemy.Position.Y && _position.X >= enemy.Position.X && _position.X <= enemy.Position.X + enemy.Texture.Width)
                    {
                        _fired = true;
                        enemy.Living = false;
                    }
                    //Bottom of enemy hitting Top of player?
                    if (_position.Y + Texture.Height >= enemy.Position.Y && _position.Y + Texture.Height <= enemy.Position.Y + enemy.Texture.Height && _position.X >= enemy.Position.X && _position.X <= enemy.Position.X + enemy.Texture.Width)
                    {
                        _fired = true;
                        enemy.Living = false;
                    }
                }
                else if (_ammocolor == enemy.sColor)
                {
                    if (_position.X <= enemy.Position.X + enemy.Texture.Width && _position.X >= enemy.Position.X && _position.Y >= enemy.Position.Y && _position.Y <= enemy.Position.Y + enemy.Texture.Height)
                    {
                        _fired = true;
                        enemy.Living = false;
                    }
                    //Right Side of enemy hitting Left Side of player?
                    if (_position.X + Texture.Width >= enemy.Position.X && _position.X + Texture.Width <= enemy.Position.X + enemy.Texture.Width && _position.Y >= enemy.Position.Y && _position.Y <= enemy.Position.Y + enemy.Texture.Height)
                    {
                        _fired = true;
                        enemy.Living = false;
                    }
                    //Top of enemy hitting Bottom of player?
                    if (_position.Y <= enemy.Position.Y + enemy.Texture.Height && _position.Y >= enemy.Position.Y && _position.X >= enemy.Position.X && _position.X <= enemy.Position.X + enemy.Texture.Width)
                    {
                        _fired = true;
                        enemy.Living = false;
                    }
                    //Bottom of enemy hitting Top of player?
                    if (_position.Y + Texture.Height >= enemy.Position.Y && _position.Y + Texture.Height <= enemy.Position.Y + enemy.Texture.Height && _position.X >= enemy.Position.X && _position.X <= enemy.Position.X + enemy.Texture.Width)
                    {
                        _fired = true;
                        enemy.Living = false;
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //draw the ammo - if the ammo's color was changed to that of one of the enemy colors, draw using that TINT)
            if (_ammocolor == "white")
                spriteBatch.Draw(Texture, _position, null, Color.White, _angle, new Vector2(Texture.Width / 2, Texture.Height / 2), _ammosize, SpriteEffects.None, 0f);    
            else if (_ammocolor == "red")
                spriteBatch.Draw(Texture, _position, null, Color.Red, _angle, new Vector2(Texture.Width / 2, Texture.Height / 2), _ammosize, SpriteEffects.None, 0f);    
            else if (_ammocolor == "yellow")
                spriteBatch.Draw(Texture, _position, null, Color.Yellow, _angle, new Vector2(Texture.Width / 2, Texture.Height / 2), _ammosize, SpriteEffects.None, 0f);    
            else if (_ammocolor == "blue")
                spriteBatch.Draw(Texture, _position, null, Color.Blue, _angle, new Vector2(Texture.Width / 2, Texture.Height / 2), _ammosize, SpriteEffects.None, 0f);    
            else if (_ammocolor == "green")
                spriteBatch.Draw(Texture, _position, null, Color.Green, _angle, new Vector2(Texture.Width / 2, Texture.Height / 2), _ammosize, SpriteEffects.None, 0f);    
            if (_ammocolor == "purple")
                spriteBatch.Draw(Texture, _position, null, Color.Purple, _angle, new Vector2(Texture.Width / 2, Texture.Height / 2), _ammosize, SpriteEffects.None, 0f);    
        }
    }
}
