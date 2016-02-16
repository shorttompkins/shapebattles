using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace ShapeBattles
{
    class Player
    {
        public static Texture2D Texture;
        public static Viewport GraphicsViewport;

        private float _score;
        public float Score
        {
            get { return _score; }
            set { _score = value; }
        }

        private float _multiplier;
        public float Multiplier
        {
            get { return _multiplier; }
            set { _multiplier = value; }
        }
	
        private bool _status;
        public bool Status
        {
            get { return _status; }
            set { _status = value; }
        }

        private float _angle;
        public float Angle
        {
            get { return _angle; }
            set { _angle = value; }
        }

        private float _speed;
        public float Speed
        {
            get { return _speed; }
            set { _speed = value; }
        }

        private int _maxshots;
        public int MaxShots
        {
            get { return _maxshots; }
            set { _maxshots = value; }
        }

        private int _maxbombs;
        public int MaxBombs
        {
            get { return _maxbombs; }
            set { _maxbombs = value; }
        }

        private int _lives;
        public int Lives
        {
            get { return _lives; }
            set { _lives = value; }
        }

        private int _shotsfired;
        public int ShotsFired
        {
            get { return _shotsfired; }
            set { _shotsfired = value; }
        }

        private string _ammocolor;
        public string AmmoColor
        {
            get { return _ammocolor; }
            set { _ammocolor = value; }
        }
	
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

        private Vector2 origin;
                
        public Player()
        {
            _score = 00000000f;
            _position = new Vector2(390f, 290f);
            _speed = 7f;
            _lives = 3;
            _ammocolor = "white";
            _shotsfired = 0;
            _status = true;
            _maxshots = 35;
            _maxbombs = 3;
            _angle = 0.0f;
            origin.X = Texture.Width / 2;
            origin.Y = Texture.Height / 2;
            _multiplier = 1.0f;
        }

        public void UpdateMovement(GamePadState gamepadState)
        {
            Vector2 holdLeftStick = new Vector2(0.0f, 0.0f);
            Vector2 leftStick = gamepadState.ThumbSticks.Right;
            leftStick.Normalize();
            if ((leftStick.X >= 0 || leftStick.X <= 0) || (leftStick.Y >= 0 || leftStick.Y <= 0))
            {
                float angle = (float)Math.Acos(leftStick.Y);
                if (leftStick.X < 0.0f)
                    angle = -angle;

                _angle = angle;
                holdLeftStick = leftStick;
            }

            //Character Movement - only if the player is INBOUNDS!
            float futureX = _position.X + gamepadState.ThumbSticks.Left.X * 6;
            float futureY = _position.Y + -gamepadState.ThumbSticks.Left.Y * 6;
            if (futureX > 10 && futureX < GraphicsViewport.Width-10 && futureY > 10 && futureY < GraphicsViewport.Height-10)
            {
                _position.X += gamepadState.ThumbSticks.Left.X * 6;
                _position.Y += -gamepadState.ThumbSticks.Left.Y * 6;
            }

            //return holdLeftStick;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, _position, null, Color.White, _angle, origin, 1.0f, SpriteEffects.None, 0f);
        }
    }
}
