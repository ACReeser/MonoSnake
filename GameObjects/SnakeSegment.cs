﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoSnake.Infrastructure;

namespace MonoSnake.GameObjects
{
    public class SnakeSegment : IGameObject
    {

        public int DrawOrder => 0;
        public Sprite Sprite { get; set; }
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        public Vector2 PreviousSegmentPosition { get; set; }
        public float PreviousSegmentRotation { get; set; }
        public SnakeDirection Direction { get; set; }

        public SnakeSegment(Sprite sprite, Vector2 position, float rotation)
        {
            Sprite = sprite;
            Position = position;
            Rotation = rotation;
        }

        public void Update(GameTime gameTime)
        {
            Position = PreviousSegmentPosition;
            Rotation = PreviousSegmentRotation;
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            Sprite.Draw(spriteBatch, Position, Rotation);
        }
    }
}
