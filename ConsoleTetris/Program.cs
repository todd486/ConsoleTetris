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
            public Tetromino() { }
            private byte[][,] data = { //jagged array storing shape info in hex, to later be converted into binary
                new byte[,] { { 0x0 }, { 0xF } },
                new byte[,] { { 0x8 }, { 0xE } },
                new byte[,] { { 0x2 }, { 0xE } },
                new byte[,] { { 0xC }, { 0xC } },
                new byte[,] { { 0x6 }, { 0xC } },
                new byte[,] { { 0x4 }, { 0xE } },
                new byte[,] { { 0xC }, { 0x6 } }
            };
            private int[] Coords = { 0, 0 };
            public int Xpos { get => Coords[0]; set { Coords[0] = (value >= 0 && value < 10 ? value : 0); } }
            public int Ypos { get => Coords[1]; set { Coords[1] = (value >= 0 && value < 15 ? value : 0); } }
            public int[,] Shape = new int[4, 4];
            public void genShape(int dataIndex) {
                int[,] temp = new int[4, 4];
                int acc = 0;
                foreach (byte shape in data[dataIndex]) {
                    string binaryString = "";
                    binaryString += Convert.ToString(shape, 2).PadLeft(4, '0');
                    foreach (var bin in binaryString.Select((value, i) => new { i, value })) { //credit where credit is due: https://stackoverflow.com/a/11437562
                        var value = bin.value;
                        var index = bin.i;
                        temp[index, acc] = (int)Char.GetNumericValue(value); //this was painful
                    }
                    acc++;
                }
                Shape = temp;
            }
        }
        public static class State {
            public static bool paused { get; set; }
            public static bool lost { get; set; }
        }
        static void Main(string[] args) {
            bool active = false;
            int difficulty = 1;
            int[,] field = new int[10, 16];
            Tetromino current = new Tetromino(); //instantiate a tetromino
            Random random = new Random(); //instantiate a random
            Console.CursorVisible = false;
            bool Occupied(int x, int y) /*TODO: Implement collision detection */ {
                return field[x, y] != 0 ? false : true;
            }
            void Rotate(int direction) {
                switch (direction) {
                    case 0: {

                            break;
                        }
                    case 1: {

                            break;
                        }
                    default: { break; }
                }
            }
            void Controls() {
                while (true) {
                    if (Console.KeyAvailable) {
                        ConsoleKeyInfo key = Console.ReadKey(true);
                        switch (key.Key) {
                            case ConsoleKey.UpArrow: { break; }
                            case ConsoleKey.DownArrow: { current.Ypos++; break; }
                            case ConsoleKey.LeftArrow: { current.Xpos--; break; }
                            case ConsoleKey.RightArrow: { current.Xpos++; break; }
                            case ConsoleKey.Z: { /*Rotate(0)*/; break; }
                            case ConsoleKey.X: { /*Rotate(1);*/ break; }
                            case ConsoleKey.Escape: { State.paused = !State.paused; break; }
                            default: { break; }
                        }
                    }
                }
            }
            void Cascade() {
                int acc = 0;
                int i = 0;
                for (int y = 0; y < 16; y++) {
                    for (int x = 0; x < 10; x++) {
                        acc = field[x, y];
                    }
                    i++;
                    acc = 0;
                }
            }
            
            Thread drawThread = new Thread(() => Draw(field, current));
            Thread controlThread = new Thread(() => Controls());
            
            drawThread.Start();
            controlThread.Start();
            while (!State.lost) { //TODO: create a new tetromino instead of mutating original, apply it to array in set statement, store X & Y somewhere efficiently
                if (!State.paused) {
                    Thread.Sleep(1000 / difficulty); //speed up later to up difficulty
                    current.genShape(random.Next(6));
                    //if (!active) {
                        
                    //}
                    
                    //if (current.Ypos <= 15) { current.Ypos++; }
                    //else {
                    //    active = false;
                    //    for (int y = 0; y < 4; y++) { for (int x = 0; x < 4; x++) {
                    //            if (current.setShape[x, y] == 0) {
                    //                field[current.Xpos, current.Ypos] = current.setShape[x, y];
                    //            }
                    //        } //push the current tetromino piece into field & only modify the field if there is anything there
                    //    }
                    //}
                    //Cascade();
                }
                //if (!active) { //check if a tetromino is active
                //    active = true;
                //    current.Xpos = 0;
                //    current.Ypos = 0;
                //    current.setShape = current.getShape(random.Next(6)); //if not, set a new shape for the tetromino
                //}

                
            }
            drawThread.Join();
            controlThread.Join();
        }
        public static void Draw(int[,] field, Tetromino current) { //TODO: pass the current tetromino into this thread somehow
            while (true) {
                Thread.Sleep(10); //since drawing to console takes roughly 0.25s, delay the thread to reduce flickering
                Console.SetCursorPosition(0, 0); //return cursor to top left
                string toWrite = "";
                for (int y = 0; y < 16; y++) { 
                    for (int x = 0; x < 10; x++) { 
                        toWrite = (field[x, y] == 0 ? ".." : "[]"); 
                        Console.Write(toWrite); 
                    }
                    Console.WriteLine(toWrite);
                }
                Console.SetCursorPosition(current.Xpos * 2, current.Ypos); //move cursor to current tetromino position
                if (!State.paused) {
                    for (int y = 0; y < 4; y++) { for (int x = 0; x < 4; x++) {
                            if (current.Shape[x, y] == 0) { 
                                Console.SetCursorPosition(Console.CursorLeft + 2, Console.CursorTop); 
                            }
                            else { Console.Write("[]"); }
                        }
                        Console.SetCursorPosition(current.Xpos * 2, current.Ypos + 1);
                    }
                }
                if (State.paused) { Console.SetCursorPosition(7, 8); Console.Write("{PAUSED}"); } //draw the pause screen dialog TODO: fix since it broke somehow
            }
        }
    }
}