/*
 *  Change how the snake grows specially around the corners
 *  it is impossible to catch the food  at (n,n) (0,0) (0,n) (n,0) 
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
namespace Csnake
{
    enum assets { empty = 0, snake = 1, food = 2, obstacle = 3 }
    enum direction { up = 0, right = 1, down = 2, left = 3 }
    class Board
    {
        private int[,] _board;
        // private Renderer _renderer;

        public Queue<(int, int)> ChangedStatesQueue { get; private set; }

        //Properties
        public int Height { get; private set; }
        public int Width { get; private set; }

        //ctor
        public Board(int x, int y)
        {
            Height = x < 5 ? 5 : x;
            Width = y < 5 ? 5 : y;

            _board = new int[Height, Width];
            // _renderer = Renderer.GetInstance(this);
            ChangedStatesQueue = new Queue<(int, int)>();
        }
        public void SetAt(int x, int y, int value)
        {
            _board[x, y] = value;
            //   _renderer.RedrawQueue.Enqueue((x, y));
            ChangedStatesQueue.Enqueue((x, y));
        }
        public void RemoveAt(int x, int y)
        {
            _board[x, y] = (int)assets.empty;
            //_renderer.RedrawQueue.Enqueue((x, y));
            ChangedStatesQueue.Enqueue((x, y));
        }
        public int WhatAt(int x, int y)
        {
            return _board[x, y];
        }
        public void Reset()
        {
            for (int i = 0; i < Height; ++i)
                for (int j = 0; j < Width; ++j)
                    _board[i, j] = (int)assets.empty;
        }
    }

    class Controller
    {
        private Board _board;
        private Random _rand;
        private LinkedList<(int, int)> _snake;
        private (int, int) _food;

        public direction last_direction { get; private set; }
        public bool GameStateOver { get; private set; }
        public int Score { get; private set; }
        public void SetDirection(direction dir)
        {
            last_direction = dir;
        }
        private (int, int) generateRandomPosition(int min_height, int max_height, int min_width, int max_width)
        { //regenate number if generated position is not empty
            (int, int) pos = (_rand.Next(min_height, max_height), _rand.Next(min_width, max_width));
            while (_board.WhatAt(pos.Item1, pos.Item2) != (int)assets.empty)
            {
                pos = (_rand.Next(min_height, max_height), _rand.Next(min_width, max_width));
            }
            return pos;
        }
        private void SetFoodPosition()
        {
            _food = generateRandomPosition(0, _board.Height, 0, _board.Width);
            _board.SetAt(_food.Item1, _food.Item2, (int)assets.food);
        }
        private bool isValidMove((int, int) next)
        {
            if (next.Item1 < 0 || next.Item1 > _board.Height - 1)
                return false;
            if (next.Item2 < 0 || next.Item2 > _board.Width - 1)
                return false;
            //true if move is valid ie: next position for head is empty or food cell else return false not a valid move
            if (_board.WhatAt(next.Item1, next.Item2) == (int)assets.empty || _board.WhatAt(next.Item1, next.Item2) == (int)assets.food)
                return true;
            else
                return false;
        }
        private void growSnake((int, int) new_head)
        {
            _snake.AddFirst(new_head);
            _board.SetAt(new_head.Item1, new_head.Item2, (int)assets.snake);
        }
        private void moveSnake((int, int) next)
        {
            //move the snake by 1 block by adding 1 to the head and removing 1 from the tail
            _snake.AddFirst(next);
            _board.SetAt(next.Item1, next.Item2, (int)assets.snake);

            var tail = _snake.Last;
            //remove the tail
            _board.RemoveAt(tail.Value.Item1, tail.Value.Item2);
            _snake.RemoveLast();
        }
        private bool harmonizer((int, int) next)
        {
            //return true if snake grows
            //false otherwise
            if (isValidMove(next))
            {
                if (_board.WhatAt(next.Item1, next.Item2) == (int)assets.food)
                {
                    growSnake(next);
                    SetFoodPosition();

                    Score += 1;
                    return true;
                }
                else
                {
                    moveSnake(next);
                }
            }
            else
            {
                //game over
                GameStateOver = true;
                // Console.WriteLine("GameOver");
            }
            return false;
        }
        //ctor
        public Controller(ref Board board)
        {
            _board = board;
            _rand = new Random();
            _snake = new LinkedList<(int, int)>();

            init();
        }
        private void init()
        {
            Score = 0;
            last_direction = direction.right;
            GameStateOver = false;

            //Generate Default state
            _board.SetAt(_board.Height / 2, _board.Width / 2, (int)assets.snake);
            _snake.AddFirst((_board.Height / 2, _board.Width / 2));
            SetFoodPosition();
        }
        //commands
        public bool Up()
        {
            if (!GameStateOver)
            {
                if (last_direction != direction.down)
                {
                    var next = (_snake.First.Value.Item1 - 1, _snake.First.Value.Item2);
                    last_direction = direction.up;
                    return harmonizer(next);
                }
            }
            return false;
        }
        public bool Right()
        {
            if (!GameStateOver)
            {
                if (last_direction != direction.left)
                {
                    var next = (_snake.First.Value.Item1, _snake.First.Value.Item2 + 1);
                    last_direction = direction.right;
                    return harmonizer(next);
                }
            }
            return false;
        }
        public bool Down()
        {
            if (!GameStateOver)
            {
                if (last_direction != direction.up)
                {
                    var next = (_snake.First.Value.Item1 + 1, _snake.First.Value.Item2);
                    last_direction = direction.down;
                    return harmonizer(next);
                }
            }
            return false;
        }
        public bool Left()
        {
            if (!GameStateOver)
            {
                if (last_direction != direction.right)
                {
                    var next = (_snake.First.Value.Item1, _snake.First.Value.Item2 - 1);
                    last_direction = direction.left;
                    return harmonizer(next);
                }
            }
            return false;
        }

        public void ResetGameState() { 
            GameStateOver = false;
            //must reset the board before resetting the controller
            _board.Reset();
            _snake.Clear();
            init();
        }

    }

    class Coordinator
    {
        private Board _board;
        private Controller _controller;
        private Renderer _renderer;
        private Timer _clockSpeed;

        public int ClockSpeed { get; private set; }
        public int Score { get; private set; }

        public Coordinator(int x, int y, int clock_speed = 400)
        {
            _board = new Board(x, y);
            _controller = new Controller(ref _board);
            _renderer = Renderer.GetInstance(_board);

            ClockSpeed = clock_speed < 50 ? 400 : clock_speed;
            Score = 0;

        }


        //game Clock speed (snake movement speed)
        private void ChangeTimeSpeed(bool x)
        {
            if (x)
            {
                if (Score < 17 && ClockSpeed > 30)
                {
                    ClockSpeed -= 5;

                }
                else if (Score < 30 && ClockSpeed > 15)
                {
                    ClockSpeed -= 2;
                }
                _clockSpeed.Change(0, ClockSpeed);
                //update score
                Score += 1;
            }

        }
        private void TimerCallback(Object o)
        {
            if (!_controller.GameStateOver)
            {
                if (LastDirection() == direction.up)
                    ChangeTimeSpeed(_controller.Up());
                else if (LastDirection() == direction.right)
                    ChangeTimeSpeed(_controller.Right());
                else if (LastDirection() == direction.down)
                    ChangeTimeSpeed(_controller.Down());
                else if (LastDirection() == direction.left)
                    ChangeTimeSpeed(_controller.Left());

                _renderer.PrintBanner($"      Score:{Score}      Clock Speed:{ClockSpeed}      version 0.1       Press space to start");
                _renderer.RenderChanged(_board);

            }
            else
            {
                _clockSpeed.Dispose();
                _renderer.PrintGameOver((int)_board.Height/2 ,(int)_board.Width -5);
            }
        }

        //controller interface
        public void Direction(direction dir) => _controller.SetDirection(dir);
        public void StartGame() => _clockSpeed = new Timer(TimerCallback, null, 0, ClockSpeed);
        public direction LastDirection() => _controller.last_direction;
        public bool GameOver() => _controller.GameStateOver;
        public void Restart()
        {
            Score = 0;
            ClockSpeed = 150;
            _controller.ResetGameState();
            _renderer.ResetCanvas(); 

        }

    }
}
