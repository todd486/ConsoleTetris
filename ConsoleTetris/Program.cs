using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
namespace ConsoleTetris {
    class Program {
        public class Tetromino {
            public Tetromino() { }
            private readonly byte[][,] data = { //jagged array storing shape info in hex, to later be converted into binary
                new byte[,] { { 0x0 }, { 0xF } }, //I
                new byte[,] { { 0x8 }, { 0xE } }, //J
                new byte[,] { { 0x2 }, { 0xE } }, //L
                new byte[,] { { 0xC }, { 0xC } }, //O
                new byte[,] { { 0x6 }, { 0xC } }, //S
                new byte[,] { { 0x4 }, { 0xE } }, //T
                new byte[,] { { 0xC }, { 0x6 } }  //Z
            }; //TODO: store each rotation iteration rather than wasting cpu cycles calculating each rotation
            private int[] Coords = { 0, 0 };
            public bool Active { get; set; }
            public int Xpos { get => Coords[0]; set { Coords[0] = value >= 0 && value < 9 ? value : Coords[0]; } }
            public int Ypos { get => Coords[1]; set { Coords[1] = value >= 0 && value < 15 ? value : Coords[1]; } }
            public int[,] Shape = new int[4, 4];
            public void GenerateShape(int dataIndex) {
                int[,] temp = new int[4, 4];
                int acc = 0;
                foreach (byte shape in data[dataIndex]) {
                    string binaryString = "";
                    binaryString += Convert.ToString(shape, 2).PadLeft(4, '0');
                    foreach (var bin in binaryString.Select((value, i) => new { i, value })) {
                        var value = bin.value; var index = bin.i;
                        temp[index, acc] = (int)Char.GetNumericValue(value); //cast char to int
                    } //credit where credit is due: https://stackoverflow.com/a/11437562
                    acc++;
                }
                Shape = temp;
            }
        }
        public class PlayField {
            public PlayField() { }
            public static readonly int sizeX = 10;
            public static readonly int sizeY = 16;
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
                int i = 0;
                int acc = 0;
                List<int> rowsToCascade = new List<int>();
                for (int y = 0; y < 16; y++) {
                    for (int x = 0; x < 10; x++) {
                        i++;
                        acc += State[x, y];
                        if (i == 10) {
                            if (acc == 10) {
                                rowsToCascade.Add(i);
                            }
                            i = 0;
                            acc = 0;
                        }
                    }
                }
                return 0;
            }
        }
        public static class State {
            public static bool Paused { get; set; }
            public static bool Lost { get; set; }
            public static int Score { get; set; }
            private static int _Level = 1;
            public static int Level { get => _Level; set { _Level = value > 0 && value <= 20 ? value : _Level; } }
        }
        static void Main(string[] args) {
            Console.CursorVisible = false;
            Tetromino current = new Tetromino(); PlayField field = new PlayField(); Random random = new Random(); //create objects
            void Controls() {
                while (true) {
                    if (Console.KeyAvailable) {
                        ConsoleKeyInfo key = Console.ReadKey(true);
                        switch (key.Key) {
                            case ConsoleKey.UpArrow: { break; } //calculate lowest possible movement
                            case ConsoleKey.DownArrow: { Collision(0); break; }
                            case ConsoleKey.LeftArrow: { Collision(1); break; }
                            case ConsoleKey.RightArrow: { Collision(2); break; }
                            case ConsoleKey.Z: { break; }
                            case ConsoleKey.X: { break; }
                            case ConsoleKey.Escape: { State.Paused = !State.Paused; break; }
                            default: { break; }
                        }
                    }
                }
            }
            bool Collision(int direction) {
                List<bool> result = new List<bool>();
                for (int y = 0; y < 4; y++) {
                    for (int x = 0; x < 4; x++) {
                        if (current.Shape[x, y] != 0) {
                            switch (direction) {
                                case 0: { result.Add(field.Occupied(current.Xpos + x, current.Ypos + y + 1)); break; } //down
                                case 1: { result.Add(field.Occupied(current.Xpos + x - 1, current.Ypos + y)); break; } //left
                                case 2: { result.Add(field.Occupied(current.Xpos + x + 1, current.Ypos + y)); break; } //right
                                case 3: {

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

            int[] scoreMap = { 0, 40, 100, 300, 1200 };
            while (!State.Lost) {
                Thread.Sleep(1000 / State.Level); //TODO: use deltaTime
                if (!State.Paused) {
                    if (!current.Active) {
                        current.GenerateShape(random.Next(6));
                        current.Xpos = 0; current.Ypos = 0;
                    }
                    if (Collision(0)) { current.Active = true; }
                    else {
                        current.Active = false;
                        for (int y = 0; y < 4; y++) {
                            for (int x = 0; x < 4; x++) {
                                if (current.Shape[x, y] != 0) {
                                    field.MutateState(current.Xpos + x, current.Ypos + y, current.Shape[x, y]);
                                }
                            }
                        }
                        State.Score += scoreMap[field.Cascade()] * State.Level + 100;
                    }
                }
            }
            drawThread.Join(); controlThread.Join();
        }
        public static void Draw(PlayField field, Tetromino current) { //TODO: pass the current tetromino into this thread somehow
            while (true) {
                Thread.Sleep(30); //since drawing to console takes roughly 0.25s, sleep the thread to reduce flickering
                Console.SetCursorPosition(0, 0); //return cursor to top left
                for (int y = 0; y < 16; y++) {
                    for (int x = 0; x < 10; x++) {
                        string toWrite = field.State[x, y] == 0 ? ".." : "[]";
                        Console.Write(toWrite);
                    }
                    Console.SetCursorPosition(0, Console.CursorTop + 1);
                }
                Console.SetCursorPosition(current.Xpos * 2, current.Ypos); //move cursor to current tetromino position
                if (!State.Paused) {
                    for (int y = 0; y < 4; y++) {
                        for (int x = 0; x < 4; x++) {
                            if (current.Shape[x, y] == 0) {
                                Console.SetCursorPosition(Console.CursorLeft + 2, Console.CursorTop);
                            } else { Console.Write("[]"); }
                        }
                        Console.SetCursorPosition(current.Xpos * 2, current.Ypos + 1);
                    }
                }
                if (State.Paused) { Console.SetCursorPosition(6, 8); Console.Write("{PAUSED}"); } //draw the pause screen dialog
                Console.SetCursorPosition(24, 0); Console.Write("[SCORE: {0}]", State.Score); //draw score
                Console.SetCursorPosition(24, 1); Console.Write("[X: {0}, Y: {1}]", current.Xpos, current.Ypos); //draw debug info
                Console.SetCursorPosition(24, 2); Console.Write("[ACTIVE: {0}]", current.Active);
            }
        }
    }
}