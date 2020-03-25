using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
namespace ConsoleTetris {
    class Program {
        public class Tetromino {
            public Tetromino() { }
            private readonly byte[][,] data = { //jagged array storing shape info in hex
                new byte[,] { { 0x0 }, { 0xF } }, //I
                new byte[,] { { 0x8 }, { 0xE } }, //J
                new byte[,] { { 0x2 }, { 0xE } }, //L
                new byte[,] { { 0xC }, { 0xC } }, //O
                new byte[,] { { 0x6 }, { 0xC } }, //S
                new byte[,] { { 0x4 }, { 0xE } }, //T
                new byte[,] { { 0xC }, { 0x6 } }  //Z
            };
            private int[] Coords = { 0, 0 };
            public bool Active { get; set; }
            public int Xpos { get => Coords[0]; set { Coords[0] = value >= 0 && value < 9 ? value : Coords[0]; } }
            public int Ypos { get => Coords[1]; set { Coords[1] = value >= 0 && value < 15 ? value : Coords[1]; } }
            public int[,] Shape = new int[4, 4];
            public void GenerateShape(int shapeIndex) {
                int[,] _Shape = new int[4, 4]; 
                int acc = 0;
                foreach (byte shape in data[shapeIndex]) {
                    string binaryString = "";
                    binaryString += Convert.ToString(shape, 2).PadLeft(4, '0');
                    foreach (var bin in binaryString.Select((value, i) => new { i, value })) {
                        var value = bin.value; var index = bin.i;
                        _Shape[index, acc] = (int)Char.GetNumericValue(value); //cast char to int
                    } //credit where credit is due: https://stackoverflow.com/a/11437562
                    acc++;
                }
                Shape = _Shape;
            }
            public void RotateShape(bool clockwise) { //TODO

            }
        }
        public class PlayField {
            public PlayField() { }
            private static readonly int sizeX = 10;
            private static readonly int sizeY = 16;
            public int[,] State { get; private set; } = new int[sizeX, sizeY];
            public void MutateState(int x, int y, int _value) => State[x >= 0 && x < sizeX ? x : 0, y >= 0 && y < sizeY ? y : 0] = _value;
            public bool Occupied(int x, int y) {
                bool result = true;
                if (x >= 0 && x < sizeX && y >= 0 && y < sizeY) {
                    if (State[x, y] == 0) { result = false; }
                }
                return result;
            }
            public int Cascade() {
                List<int> lines = new List<int>();
                for (int y = 0; y < sizeY; y++) {
                    int acc = 0;
                    for (int x = 0; x < sizeX; x++) { State[x, y] += acc; }
                    if (acc == sizeX - 1) { lines.Add(y); }
                }
                return lines.Count();
            }
        }
        public static class G {
            public static bool Paused { get; set; }
            public static bool Lost { get; set; }
            public static int Score { get; set; }
            public static int Lines { get; set; }
            private static int _Level = 1;
            public static int Level { get => _Level; set { _Level = value > 0 && value <= 20 ? value : _Level; } }
            public static int[] ShapeBuffer { get; set; } = new int[2];
        }
        static void Main(string[] args) {
            int[] scoreMap = { 0, 40, 100, 300, 1200 };
            Tetromino current = new Tetromino(); PlayField field = new PlayField(); Random random = new Random(); //create objects
            void Controls() {
                while (true) {
                    Thread.Sleep(30); //yield
                    if (Console.KeyAvailable) {
                        ConsoleKeyInfo key = Console.ReadKey(true);
                        switch (key.Key) {
                            case ConsoleKey.DownArrow: { Movement(0); break; }
                            case ConsoleKey.LeftArrow: { Movement(1); break; }
                            case ConsoleKey.RightArrow: { Movement(2); break; }
                            case ConsoleKey.UpArrow: { Movement(3); break; }
                            case ConsoleKey.Z: { break; }
                            case ConsoleKey.X: { break; }
                            case ConsoleKey.Escape: { G.Paused = !G.Paused; break; }
                            default: { break; }
                        }
                    }
                }
            }
            bool Movement(int direction) {
                List<bool> result = new List<bool>();
                for (int y = 0; y < 4; y++) {
                    for (int x = 0; x < 4; x++) {
                        if (current.Shape[x, y] != 0) {
                            switch (direction) {
                                case 0: { result.Add(field.Occupied(current.Xpos + x, current.Ypos + y + 1)); break; } //down
                                case 1: { result.Add(field.Occupied(current.Xpos + x - 1, current.Ypos + y)); break; } //left
                                case 2: { result.Add(field.Occupied(current.Xpos + x + 1, current.Ypos + y)); break; } //right
                                case 3: { //TODO: find lowest possible movement

                                        break;
                                    } //drop
                                default: { break; }
                            }
                        }
                    }
                }
                if (!result.Contains(true)) {
                    switch (direction) {
                        case 0: { current.Ypos++; break; }
                        case 1: { current.Xpos--; break; }
                        case 2: { current.Xpos++; break; }
                    }
                }
                return !result.Contains(true);
            }
            Thread drawThread = new Thread(() => Draw(field, current)); drawThread.Start(); //create and start threads
            Thread controlThread = new Thread(() => Controls()); controlThread.Start();
            foreach (int i in G.ShapeBuffer) { G.ShapeBuffer[i] = random.Next(6); }
            while (!G.Lost) {
                Thread.Sleep(1000 / G.Level); //use deltaTime instead?
                if (!G.Paused) {
                    if (!current.Active) { 
                        current.GenerateShape(G.ShapeBuffer[0]);
                        G.ShapeBuffer[0] = G.ShapeBuffer[1]; G.ShapeBuffer[1] = random.Next(6); //shift first element out of array, then replace it.
                        current.Xpos = 0; current.Ypos = 0; 
                    }
                    if (Movement(0)) { current.Active = true; } 
                    else { current.Active = false;
                        for (int y = 0; y < 4; y++) {
                            for (int x = 0; x < 4; x++) {
                                if (current.Shape[x, y] != 0) { field.MutateState(current.Xpos + x, current.Ypos + y, current.Shape[x, y]); }
                            }
                        }
                        int i = field.Cascade(); G.Lines += i; 
                        G.Score += scoreMap[i] * G.Level + 100; //score calculation
                    }
                }
            }
            drawThread.Join(); controlThread.Join(); //join the threads back into main
            Console.SetCursorPosition(0, 0); Console.Write("YOU LOSE!"); Console.ReadKey(true);
        }
        public static void Draw(PlayField field, Tetromino current) {
            Console.CursorVisible = false;
            while (true) {
                Thread.Sleep(30); //since drawing to console takes roughly 0.25s, sleep to reduce flickering
                Console.SetCursorPosition(0, 0); //return cursor to top left
                for (int y = 0; y < 16; y++) {
                    for (int x = 0; x < 10; x++) {
                        string toWrite = field.State[x, y] == 0 ? ".." : "[]";
                        Console.Write(toWrite);
                    }
                    Console.SetCursorPosition(0, Console.CursorTop + 1);
                }
                Console.SetCursorPosition(current.Xpos * 2, current.Ypos); //move cursor to current tetromino position
                if (!G.Paused) {
                    for (int y = 0; y < 4; y++) {
                        for (int x = 0; x < 4; x++) {
                            if (current.Shape[x, y] == 0) { Console.SetCursorPosition(Console.CursorLeft + 2, Console.CursorTop); } else { Console.Write("[]"); }
                        }
                        Console.SetCursorPosition(current.Xpos * 2, current.Ypos + 1);
                    }
                }
                if (G.Paused) { Console.SetCursorPosition(6, 8); Console.Write("{PAUSED}"); } //draw the pause screen dialog
                Console.SetCursorPosition(24, 0); Console.Write("[ SCORE: {0} ][ LEVEL: {1} ][ LINES: {2} ]", G.Score, G.Level, G.Lines); //draw score
                //Console.SetCursorPosition(24, 15); Console.Write("[X: {0}, Y: {1}][ACTIVE: {2}]", current.Xpos, current.Ypos, current.Active); //draw debug info
            }
        }
    }
}