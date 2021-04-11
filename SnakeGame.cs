﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoSnake.GameObjects;
using MonoSnake.Infrastructure;
using MonoSnake.UI;

namespace MonoSnake
{
    public class SnakeGame : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private FrameCounter _frameCounter = new FrameCounter();

        const int SCREEN_WIDTH = 780;
        const int SCREEN_HEIGHT = 820;

        private Snake _snake;
        private Apple _appleGameObject;
        private SnakeHead _snakeHeadGameObject;

        private InputController _inputController;
        private SpriteFont _gameOverFont;
        private SpriteFont _scoreBoardFont;
        private Texture2D _appleTexture;
        private Texture2D _snakeHeadSpriteSheet;
        private Texture2D _snakeSegmentsSpriteSheet;
        private Texture2D _gameAreaRectangleTexture;
        private Texture2D _snakeHeadRectangleTexture;
        private SoundEffect _eatSoundEffect;
        private Rectangle _gameAreaRectangle;
        private Rectangle _snakeHeadRectangle;
        private Rectangle _appleRectangle;
        private List<Rectangle> _cells;
        private List<Rectangle> _occupiedCells = new List<Rectangle>();
        private List<Rectangle> _unOccupiedCells = new List<Rectangle>();
        private ScoreBoard _scoreBoard;

        // Draw diagnostic grid?
        private bool _drawDiagnosticGrid = false;

        private bool _appleEaten;
        private bool _applePlaced;
        private bool _isGameOver;
        private bool _showFpsMonitor = false;
        private Sprite _snakeCwUpToRightCcwLeftToDownSprite;
        private Sprite _snakeCwRightToDownCcwUpToLeftSprite;
        private Sprite _snakeCwDownToLeftCcwRightToUpSprite;
        private Sprite _snakeCwLeftToUpCcwDownToRightSprite;
        private Sprite _snakeStraightBodySprite;
        private UiFrame _uiFrame;
        const string GAME_OVER_STRING = "Game Over";

        const float GAME_OVER_FONT_SCALE = 1f;
        private const int HIT_BOX_PADDING = 5;
        private const int DEFAULT_SPRITE_SIZE = 42;
        private const int DEFAULT_SPRITE_HALF_SIZE = 21;
        private const string APPLE_SPRITE_SHEET_NAME = "Apple";
        private const string SNAKE_HEAD_SPRITE_SHEET_NAME = "SnakeHead";
        private const string SNAKE_SEGMENTS_SPRITE_SHEET_NAME = "SnakeSegments";
        private const string EAT_SOUND_EFFECT_NAME = "eat";

        public SnakeGame()  
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _scoreBoard = new ScoreBoard(Assembly.GetEntryAssembly().Location);
            _graphics.PreferredBackBufferWidth = SCREEN_WIDTH;
            _graphics.PreferredBackBufferHeight = SCREEN_HEIGHT;
            _graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            LoadAssets();

            InitializeGameObjects();

            InitializeDiagnosticObjects();

            GenerateGrid();

            GenerateApple();

            // Initialize InputController
            _inputController = new InputController(_snake);
            _inputController.ExitEvent += _inputController_ExitEvent;
            _inputController.RestartEvent += _inputController_RestartEvent;
        }

        private void _inputController_RestartEvent(object sender, EventArgs e)
        {
            InitializeGameObjects();
            // Initialize InputController
            _inputController = new InputController(_snake);
            _inputController.ExitEvent += _inputController_ExitEvent;
            _inputController.RestartEvent += _inputController_RestartEvent;
            GenerateGrid();
            _appleEaten = false;
            _applePlaced = false;
            GenerateApple();
            _isGameOver = false;
        }

        private void _inputController_ExitEvent(object sender, EventArgs e)
        {
            Exit();
        }

        private void InitializeGameObjects()
        {
            // Create Sprite Objects
            Sprite appleSprite = new Sprite(_appleTexture, 0, 0, _appleTexture.Width, _appleTexture.Height)
            {
                Origin = new Vector2(_appleTexture.Width / 2f, _appleTexture.Height / 2f)
            };
            Sprite snakeHeadSprite = new Sprite(_snakeHeadSpriteSheet, 0, 0, _snakeHeadSpriteSheet.Width, DEFAULT_SPRITE_SIZE)
            {
                Origin = new Vector2(DEFAULT_SPRITE_HALF_SIZE, DEFAULT_SPRITE_HALF_SIZE)
            };
            Sprite snakeTailSprite = new Sprite(_snakeSegmentsSpriteSheet, 84, 0, DEFAULT_SPRITE_SIZE, DEFAULT_SPRITE_SIZE)
            {
                Origin = new Vector2(DEFAULT_SPRITE_HALF_SIZE, DEFAULT_SPRITE_HALF_SIZE)
            };
            _snakeStraightBodySprite = new Sprite(_snakeSegmentsSpriteSheet, 84, DEFAULT_SPRITE_SIZE, DEFAULT_SPRITE_SIZE,
                DEFAULT_SPRITE_SIZE)
            {
                Origin = new Vector2(DEFAULT_SPRITE_HALF_SIZE, DEFAULT_SPRITE_HALF_SIZE)
            };
            _snakeCwUpToRightCcwLeftToDownSprite = new Sprite(_snakeSegmentsSpriteSheet, 0, 0, DEFAULT_SPRITE_SIZE, DEFAULT_SPRITE_SIZE)
            {
                Origin = new Vector2(DEFAULT_SPRITE_HALF_SIZE, DEFAULT_SPRITE_HALF_SIZE)
            };
            _snakeCwRightToDownCcwUpToLeftSprite = new Sprite(_snakeSegmentsSpriteSheet, 0, DEFAULT_SPRITE_SIZE,
                DEFAULT_SPRITE_SIZE, DEFAULT_SPRITE_SIZE)
            {
                Origin = new Vector2(DEFAULT_SPRITE_HALF_SIZE, DEFAULT_SPRITE_HALF_SIZE)
            };
            _snakeCwDownToLeftCcwRightToUpSprite = new Sprite(_snakeSegmentsSpriteSheet, DEFAULT_SPRITE_SIZE,
                DEFAULT_SPRITE_SIZE, DEFAULT_SPRITE_SIZE, DEFAULT_SPRITE_SIZE)
            {
                Origin = new Vector2(DEFAULT_SPRITE_HALF_SIZE, DEFAULT_SPRITE_HALF_SIZE)
            };
            _snakeCwLeftToUpCcwDownToRightSprite = new Sprite(_snakeSegmentsSpriteSheet, DEFAULT_SPRITE_SIZE, 0,
                DEFAULT_SPRITE_SIZE, DEFAULT_SPRITE_SIZE)
            {
                Origin = new Vector2(DEFAULT_SPRITE_HALF_SIZE, DEFAULT_SPRITE_HALF_SIZE)
            };

            // Create GameObjects
            _snakeHeadGameObject = new SnakeHead(snakeHeadSprite, new Vector2(53, 83));
            _appleGameObject = new Apple(appleSprite,
                new Vector2(_graphics.PreferredBackBufferWidth / 2f + DEFAULT_SPRITE_HALF_SIZE,
                    _graphics.PreferredBackBufferHeight / 2f + DEFAULT_SPRITE_HALF_SIZE));
            _appleGameObject.Sprite.Scale = new Vector2(0.65f, 0.65f);

            // Initialize Snake
            _snake = new Snake
            (
                _snakeHeadGameObject,
                _graphics.PreferredBackBufferWidth,
                _graphics.PreferredBackBufferHeight,
                _scoreBoardFont,
                snakeTailSprite,
                _snakeStraightBodySprite,
                _snakeCwUpToRightCcwLeftToDownSprite,
                _snakeCwRightToDownCcwUpToLeftSprite,
                _snakeCwDownToLeftCcwRightToUpSprite,
                _snakeCwLeftToUpCcwDownToRightSprite
            );
        }

        private void LoadAssets()
        {
            // Load Font
            _scoreBoardFont = Content.Load<SpriteFont>("score");
            _gameOverFont = Content.Load<SpriteFont>("GameOver");
            // Load Textures
            _appleTexture = Content.Load<Texture2D>(APPLE_SPRITE_SHEET_NAME);
            _snakeHeadSpriteSheet = Content.Load<Texture2D>(SNAKE_HEAD_SPRITE_SHEET_NAME);
            _snakeSegmentsSpriteSheet = Content.Load<Texture2D>(SNAKE_SEGMENTS_SPRITE_SHEET_NAME);
            _eatSoundEffect = Content.Load<SoundEffect>(EAT_SOUND_EFFECT_NAME);
        }

        private void InitializeDiagnosticObjects()
        {
            _gameAreaRectangleTexture = new Texture2D(GraphicsDevice, 2, 2);
            _snakeHeadRectangleTexture = new Texture2D(GraphicsDevice, 1, 1);
            _gameAreaRectangleTexture.SetData(new[] {Color.White, Color.White, Color.White, Color.White,});
            _snakeHeadRectangleTexture.SetData(new[] {Color.White});
            _gameAreaRectangle = new Rectangle
            (
                20,
                50,
                _graphics.PreferredBackBufferWidth - 35,
                _graphics.PreferredBackBufferHeight - 70
            );
            _snakeHeadRectangle = new Rectangle
            (
                (int) Math.Round(_snake.SnakeHead.Position.X -
                                 _snake.SnakeHead.Sprite.Width / 2f * _snake.SnakeHead.Sprite.Scale.X) - HIT_BOX_PADDING,
                (int) Math.Round(_snake.SnakeHead.Position.Y -
                                 _snake.SnakeHead.Sprite.Height / 2f * _snake.SnakeHead.Sprite.Scale.Y) - HIT_BOX_PADDING,
                (int) (_snake.SnakeHead.Sprite.Width * _snake.SnakeHead.Sprite.Scale.X),
                (int) (_snake.SnakeHead.Sprite.Height * _snake.SnakeHead.Sprite.Scale.Y)
            );
        }

        protected override void Update(GameTime gameTime)
        {
            // Process Input
            _inputController.ProcessInput();

            if (_isGameOver)
                return;

            // Update GameObjects
            _snake.Update(gameTime);

            _snakeHeadRectangle.X = (int)Math.Round(_snake.SnakeHead.Position.X - _snake.SnakeHead.Sprite.Width / 2f * _snake.SnakeHead.Sprite.Scale.X);
            _snakeHeadRectangle.Y = (int)Math.Round(_snake.SnakeHead.Position.Y - _snake.SnakeHead.Sprite.Height / 2f * _snake.SnakeHead.Sprite.Scale.Y);

            // Hit edge of play area
            if (!_gameAreaRectangle.Contains(_snake.SnakeHead.Position))
            {
                // GAME OVER!
                EndGameAndRecordScore();
            }

            if (_snake.SnakeSegments.Any(s =>
                s.Position.X == _snake.SnakeHead.Position.X && s.Position.Y == _snake.SnakeHead.Position.Y))
            {
                EndGameAndRecordScore();
            }

            if (_snakeHeadRectangle.Intersects(_appleRectangle))
            {
                _appleEaten = true;
                _applePlaced = false;
                _eatSoundEffect.Play(1f, 0f, 0f);
                _snake.AddSegment();
            }

            GenerateGrid();

            if (_appleEaten)
            {
                GenerateApple();
            }

            base.Update(gameTime);
        }

        private void EndGameAndRecordScore()
        {
            _scoreBoard.HighScores.AddHighScore(new ScoreEntry("SeVeN", _snake.Score));

            _scoreBoard.SaveHighScores();

            _isGameOver = true;
        }

        private void GenerateGrid()
        {
            // Draw Grid (Temp)
            _cells = new List<Rectangle>();
            //Occupied Cells
            _occupiedCells = new List<Rectangle>();
            // Columns
            // Rows
            for (int i = 0; i < 18; i++)
            {
                for (int j = 0; j < 18; j++)
                {
                    Rectangle rectangle = new Rectangle(i * 40 + 32, j * 40 + 62, 42, 42);
                    _cells.Add(rectangle);
                }
            }

            foreach (Rectangle cell in _cells)
            {
                if (Math.Round(_snake.SnakeHead.Position.X) == cell.X + 21 &&
                    Math.Round(_snake.SnakeHead.Position.Y) == cell.Y + 21)
                {
                    _occupiedCells.Add(cell);
                }

                if (_snake.SnakeSegments.Any(s =>
                    Math.Round(s.Position.X) == cell.X + 21 && Math.Round(s.Position.Y) == cell.Y + 21))
                {
                    _occupiedCells.Add(cell);
                }
            }

            _unOccupiedCells = _cells.Except(_occupiedCells).ToList();
        }

        private void GenerateApple()
        {
            Random randomApple = new Random();

            int nextIndex = randomApple.Next(0, _unOccupiedCells.Count);

            if (!_applePlaced)
            {
                if (_unOccupiedCells.Count > 0)
                {
                    Vector2 nextAppleLoc = new Vector2(_unOccupiedCells[nextIndex].X + 22, _unOccupiedCells[nextIndex].Y + 22);

                    _appleGameObject.Position = nextAppleLoc;
                }

                _appleRectangle = new Rectangle
                (
                    (int) Math.Round(_appleGameObject.Position.X -
                                     _appleGameObject.Sprite.Width / 2f * _appleGameObject.Sprite.Scale.X) - HIT_BOX_PADDING,
                    (int) Math.Round(_appleGameObject.Position.Y -
                                     _appleGameObject.Sprite.Height / 2f * _appleGameObject.Sprite.Scale.Y) - HIT_BOX_PADDING,
                    (int) (_appleGameObject.Sprite.Width * _appleGameObject.Sprite.Scale.X) + HIT_BOX_PADDING * 2,
                    (int) (_appleGameObject.Sprite.Height * _appleGameObject.Sprite.Scale.Y) + HIT_BOX_PADDING * 2
                );

                _applePlaced = true;
                _appleEaten = false;
            }

            _uiFrame = new UiFrame
            (new Vector2(20, 20),
                15,
                15,
                _snakeStraightBodySprite,
                _snakeStraightBodySprite,
                _snakeCwUpToRightCcwLeftToDownSprite,
                _snakeCwRightToDownCcwUpToLeftSprite,
                _snakeCwDownToLeftCcwRightToUpSprite,
                _snakeCwLeftToUpCcwDownToRightSprite
            );
        }

        protected override void Draw(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _frameCounter.Update(deltaTime);

            var fps = $"FPS: {_frameCounter.CurrentFramesPerSecond}";

            if (_isGameOver)
            {
                _spriteBatch.Begin();
                DrawGameOverText();
                _spriteBatch.End();
                return;
            }

            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

            //DrawUiFrame(gameTime);

            if(_showFpsMonitor)
                _spriteBatch.DrawString
                    (
                        _scoreBoardFont,
                        fps,
                        new Vector2(20, 10),
                        Color.Blue,
                        0f,
                        Vector2.Zero,
                        new Vector2(2, 2),
                        SpriteEffects.None,
                    0f
                    );

            if (_isGameOver)
                DrawGameOverText();

            // Draw Game Area
            foreach (Vector2 outlinePixel in _gameAreaRectangle.OutlinePixels())
            {
                _spriteBatch.Draw(_gameAreaRectangleTexture, outlinePixel, Color.Green);
            }

            _snake.Draw(_spriteBatch, gameTime);

            _appleGameObject.Draw(_spriteBatch, gameTime);
            
            if (_drawDiagnosticGrid)
            {
                foreach (Rectangle rectangle in _cells)
                {
                    foreach (Vector2 outlinePixel in rectangle.OutlinePixels())
                    {
                        _spriteBatch.Draw(_snakeHeadRectangleTexture, outlinePixel, Color.Blue);
                    }
                }

                // Rectangle around Snake Head
                foreach (Vector2 outlinePixel in _snakeHeadRectangle.OutlinePixels())
                {
                    _spriteBatch.Draw(_snakeHeadRectangleTexture, outlinePixel, Color.Red);
                }

                // Rectangle around Apple
                foreach (Vector2 outlinePixel in _appleRectangle.OutlinePixels())
                {
                    _spriteBatch.Draw(_snakeHeadRectangleTexture, outlinePixel, Color.Red);
                }
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawUiFrame(GameTime gameTime)
        {
            _uiFrame.Draw(_spriteBatch, gameTime);
        }

        private void DrawGameOverText()
        {
            Vector2 gameOverStringWidth = _gameOverFont.MeasureString(GAME_OVER_STRING) * GAME_OVER_FONT_SCALE;
            float gameOverX = SCREEN_WIDTH / 2 - gameOverStringWidth.X / 2;
            float gameOverY = SCREEN_HEIGHT / 2 - gameOverStringWidth.Y / 2;
            Vector2 gameOverPosition = new Vector2(gameOverX, gameOverY);

            _spriteBatch.DrawString
            (
                _gameOverFont,
                GAME_OVER_STRING,
                gameOverPosition,
                Color.White,
                0f,
                Vector2.Zero,
                GAME_OVER_FONT_SCALE,
                SpriteEffects.None,
                0f
            );
        }
    }
}
