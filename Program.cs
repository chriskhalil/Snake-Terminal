/*
 * Need more work and refactoring
 * Status: working (fragile)
 */
using System;
using System.Threading;
using System.Text;
using System.Runtime.InteropServices;

namespace Csnake
{

    class Program
    {

        static void Main(string[] args)
        {
            Console.Title = "Snake";
            const int height = 25;
            const int width = 40;
            const int init_game_speed = 150;
            Coordinator cd = new Coordinator(height,width,init_game_speed);

            ConsoleKeyInfo keyinfo;

            bool stop_game_flag = false;

            while (!stop_game_flag)
            {
                keyinfo = Console.ReadKey(true);
                
                if(keyinfo.Key == ConsoleKey.Spacebar)
                {
                    cd.StartGame();
                    while (!cd.GameOver())
                    {
                        
                        keyinfo = Console.ReadKey(true);
                        if (keyinfo.Key == ConsoleKey.W && cd.LastDirection() != direction.up && cd.LastDirection() != direction.down)
                        {
                            cd.Direction(direction.up);
                        }
                        else if (keyinfo.Key == ConsoleKey.D && cd.LastDirection() != direction.right && cd.LastDirection() != direction.left)
                        {
                            cd.Direction(direction.right);
                        }
                        else if (keyinfo.Key == ConsoleKey.S && cd.LastDirection() != direction.down && cd.LastDirection() != direction.up)
                        {
                            cd.Direction(direction.down);
                        }
                        else if (keyinfo.Key == ConsoleKey.A && cd.LastDirection() != direction.left && cd.LastDirection() != direction.right)
                        {
                            cd.Direction(direction.left);
                        }
                        else if(keyinfo.Key ==ConsoleKey.Escape && cd.GameOver())
                        {
                            stop_game_flag = true;
                        }
                        else if(keyinfo.Key ==ConsoleKey.R && cd.GameOver())
                        {
                            cd.Restart();
                            break;
                        }
                    }
                }
                else if( keyinfo.Key == ConsoleKey.Escape)
                {
                    stop_game_flag = true;
                }
            }

            Console.Write($"\u001b[{height + 4};{0}H");
            Console.CursorVisible = true;
        }
      

    }
}
