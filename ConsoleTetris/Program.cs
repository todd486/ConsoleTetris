using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleTetris
{
    class Program
    {
        public class Tetromino
        {
            private byte[][,] shapes = { //jagged array storing shape info in hex, to later be converted into binary
                new byte[,] { { 0x0 }, { 0xF } },
                new byte[,] { { 0x8 }, { 0xE } },
                new byte[,] { { 0x2 }, { 0xE } },
                new byte[,] { { 0xC }, { 0xC } },
                new byte[,] { { 0x6 }, { 0xC } },
                new byte[,] { { 0x4 }, { 0xE } },
                new byte[,] { { 0xC }, { 0x6 } }
            };
            public Tetromino() { }
            public int[,] getShape(int shapeIndex)
            {
                int[,] returnBuffer = new int[4, 2];
                int acc = 0;
                foreach (byte shape in shapes[shapeIndex])
                {
                    string binaryString = "";
                    binaryString += Convert.ToString(shape, 2).PadLeft(4, '0');
                    foreach (var bin in binaryString.Select((value, i) => new { i, value }))
                    { //credit where credit is due: https://stackoverflow.com/a/11437562
                        var value = bin.value;
                        var index = bin.i;
                        returnBuffer[index, acc] = (int)Char.GetNumericValue(value); //this was painful
                    }
                    acc++;
                }
                return returnBuffer;
            }
            public int[,] setShape { get; set; } //getShape should always be pushed into setShape
            private int[] Coords = { 0, 0 };
            public int Xpos { get => Coords[0]; set { Coords[0] = (value >= 0 && value < 10 ? value : 0); } }
            public int Ypos { get => Coords[1]; set { Coords[1] = (value >= 0 && value < 10 ? value : 0); } }
        }
        public static class GlobalValues
        {
            public static bool paused { get; set; }
            public static bool lost = false;
        }
        static void Main(string[] args)
        {
            Console.CursorVisible = false;

            int[,] field = new int[10, 16];
            bool active = false;

            Tetromino current = new Tetromino(); //instantiate a tetromino
            Random random = new Random();

            Thread drawThread = new Thread(() => Draw(field, current));
            Thread controlThread = new Thread(() => Controls());

            bool Occupied(int x, int y) //TODO: Implement collision detection
            {
                bool result = (field[x, y] == 0 ? true : false);
                return result;
            }
            void Controls()
            {
                while (true)
                {
                    if (Console.KeyAvailable)
                    {
                        ConsoleKeyInfo key = Console.ReadKey(true);
                        switch (key.Key)
                        {
                            case ConsoleKey.UpArrow: { break; }
                            case ConsoleKey.DownArrow: { current.Ypos++; break; }
                            case ConsoleKey.LeftArrow: { current.Xpos--; break; }
                            case ConsoleKey.RightArrow: { current.Xpos++; break; }
                            case ConsoleKey.Escape: { GlobalValues.paused = !GlobalValues.paused; break; }
                        }
                    }
                }
            }
            void Cascade()
            {
                for (int y = 0; y < 16; y++)
                {
                    for (int x = 0; x < 10; x++) { 
                                                }
                }
            }

            drawThread.Start();
            controlThread.Start();

            while (!GlobalValues.lost) //main loop 
            {
                if (!GlobalValues.paused)
                { //&& !Occupied(current.Xpos + 1, current.Ypos + 1)
                    if (current.Ypos < 16) { current.Ypos++; } //debug why this is isn't workingughggughuhggguh
                    else
                    {
                        active = false;
                        for (int y = 0; y < 2; y++)
                        {
                            for (int x = 0; x < 4; x++)
                            {
                                field[x + current.Xpos, y + current.Ypos] = (
                                    current.setShape[x, y] == 0 ? field[x + current.Xpos, y + current.Ypos] : current.setShape[x, y]);
                            } //push the current tetromino piece into field & only modify the field if there is anything there
                        }
                        current.Xpos = 0;
                        current.Ypos = 0;
                    }
                }
                if (!active)
                { //check if a tetromino is active
                    active = true;
                    current.setShape = current.getShape(random.Next(6)); //if not, set a new shape for the tetromino
                }
                Thread.Sleep(500); //speed up later to up difficulty
            }
            drawThread.Join();

        }
        public static void Draw(int[,] field, Tetromino current)
        {
            while (true)
            {
                Console.SetCursorPosition(0, 0); //return cursor to top left
                string toWrite = "";
                for (int y = 0; y < 16; y++)
                {
                    for (int x = 0; x < 10; x++) { toWrite = (field[x, y] == 0 ? ".." : "[]"); Console.Write(toWrite); }
                    Console.WriteLine(toWrite);
                }

                Console.SetCursorPosition(current.Xpos * 2, current.Ypos); //move cursor to current tetromino position
                for (int y = 0; y < 2; y++)
                {
                    for (int x = 0; x < 4; x++)
                    {
                        if (current.setShape[x, y] == 0) { Console.SetCursorPosition(Console.CursorLeft + 2, Console.CursorTop); }
                        else { Console.Write("[]"); }
                    }
                    Console.SetCursorPosition(current.Xpos * 2, current.Ypos + 1);
                }

                if (GlobalValues.paused) { Console.SetCursorPosition(7, 8); Console.Write("{PAUSED}"); } //draw the pause screen dialog TODO: fix since it broke somehow
                Thread.Sleep(50); //since drawing to console takes roughly 0.25s, delay the thread to reduce flickering
            }
        }
    }
}