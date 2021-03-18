/*
 * Name is an overkill by why not
 * Need more work and refactoring
 * Status: working (fragile)
 */


using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
namespace Csnake
{
    public static class ConsoleExtensionWindows
    {
        private const int STD_OUTPUT_HANDLE = -11;
        private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        public static void EnableTerminalProcessingOnWindows()
        {
            if (OperatingSystem.IsWindows())
            {
                var iStdOut = GetStdHandle(STD_OUTPUT_HANDLE);
                GetConsoleMode(iStdOut, out var outConsoleMode);
                SetConsoleMode(iStdOut, outConsoleMode | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
            }
        }
    }

    public static class OperatingSystem
    {
        public static bool IsWindows() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static bool IsMacOS() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static bool IsLinux() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    }
    static class TerminalColor
    {
        public static string Black   { get { return "\u001b[30m"; } }
        public static string Red     { get { return "\u001b[31m"; } }
        public static string Green   { get { return "\u001b[32m"; } }
        public static string Yellow  { get { return "\u001b[33m"; } }
        public static string Blue    { get { return "\u001b[34m"; } }
        public static string Magenta { get { return "\u001b[35m"; } }
        public static string Cyan    { get { return "\u001b[36m"; } }
        public static string White   { get { return "\u001b[37m"; } }
        public static string Reset   { get { return "\u001b[0m"; } }

        public static string Color(int num) { return $"\u001b[{num}m"; }
    }

    class Renderer
    {
        private static Renderer _renderer;
        public static Renderer GetInstance(Board board, string banner = " ", char delimiter_border = '\u2588', bool show_cells = false)
        {
            if (_renderer == null)
                return _renderer = new Renderer(board, banner, delimiter_border, show_cells);
            else
                return _renderer;
        }
   
        public int TopOffset { get; private set; }
        public int LeftOffset { get; private set; }

        public Queue<(int, int)> RedrawQueue;
        StringBuilder str = new StringBuilder();

        private const string _escape = "\u001b[";
        private StringBuilder str_banner;
        private StringBuilder str_canvas;
        private StringBuilder str_top_border;

        private readonly char delimiter_border;
        private readonly int  spacing;

        private readonly int _cursor_top_position;
        private readonly int _cursor_left_position;
        private Renderer(Board board, string banner, char delimiter_border = '\u275A', bool show_cells = false, int spacing = 1)
        {
            //enable ainsi encoding on windows console
            ConsoleExtensionWindows.EnableTerminalProcessingOnWindows();

            str_canvas = new StringBuilder();
            str_banner = new StringBuilder();
            str_top_border = new StringBuilder();

            this.delimiter_border = delimiter_border;
            this.spacing = spacing < 1 ? 1 : spacing;

            str_banner.Append($"                         \n");
            for (int i = 0; i < board.Width + (board.Width * spacing) + 1; ++i)
            {
                str_top_border.Append(this.delimiter_border);
            }
            str_top_border.Append("\n");

            for (int i = 0; i < board.Height; ++i)
            {
                str_canvas.Append(delimiter_border);
                for (int j = 0; j < board.Width; ++j)
                {
                    if (Type(board.WhatAt(i, j)).Item1 == '*')
                        str_canvas.Append('*');
                    else
                        str_canvas.Append(' ', spacing);
                    str_canvas.Append(' ', spacing);
                }
                str_canvas.Remove(str_canvas.Length-1,1);
                str_canvas.Append($"{delimiter_border}\n");
            }
            //translation for the ainsi \u001b[x;yHs start at 1 rather than 0
            _cursor_top_position = Console.CursorTop + 1;
            _cursor_left_position = Console.CursorLeft + 1;
            Console.CursorVisible = false;

            RenderAll();
        }
        private (char, int) Type(int cont)
        {
            if (cont == (int)assets.empty)
                return (' ', 37);
            else if (cont == (int)assets.snake)
                return ('*', 92);

            else //if (cont == (int)assets.food)
                return ('@', 91);

        }
        private void ColorPrint((char, int) c)
        {
            Console.Write($"{_escape}{c.Item2}m{c.Item1}{_escape}0m");
        }
        private void RenderAll()
        {
            Console.Write(str_banner.ToString());
            Console.Write(str_top_border.ToString());
            Console.Write(str_canvas.ToString());
            Console.Write($"{this.delimiter_border}{str_top_border.ToString(0, str_top_border.Length - (spacing +2))}{this.delimiter_border}");
        }
        public void RenderChanged(Board board)
        {
            Console.CursorVisible = false;
            RedrawQueue = board.ChangedStatesQueue;
            BufferChanged(board);
            Console.Write(str.ToString());
            str.Clear();
        }
        public void PrintBanner(string s)
        {
            Console.Write($"\u001b[{_cursor_top_position};{_cursor_left_position}H{s}");
        }
        public void BufferChanged(Board board)
        {
           
            (int, int) tmp;
            (char, int) pack;
            while (RedrawQueue.Count != 0)
            {
                tmp = RedrawQueue.Dequeue();

                str.Append($"{_escape}{tmp.Item1 + _cursor_top_position + 2 };{tmp.Item2 + (tmp.Item2 * spacing) + _cursor_left_position + 1 }H");
                pack = Type(board.WhatAt(tmp.Item1, tmp.Item2));
                //set color
                str.Append($"{TerminalColor.Color(pack.Item2)}{pack.Item1}{TerminalColor.Reset}");
            }
        }
        public void ResetCanvas()
        {
            Console.Write($"{_escape}2J{_escape}{_cursor_top_position};{_cursor_left_position}H");
            RenderAll();
        }
        public void PrintGameOver(int xloc,int yloc)
        {   //only available on windows for now
            if (OperatingSystem.IsWindows())
                Console.Beep(2048, 100);
            Console.Write($"{ _escape}{xloc};{yloc}H Game Over{ _escape}{xloc+2};{yloc}H R   to restart{ _escape}{xloc+3};{yloc}H Esc to exit");
        }

    }
}
