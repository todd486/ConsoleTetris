using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
namespace ConsoleTetris {
	class Program {
		public class Tetromino {
			public Tetromino() { }
			private readonly byte[][,] data = {
				new byte[,] { { 0xF } },		  //I //shape data is stored like this:
				new byte[,] { { 0x8 }, { 0xE } }, //J //0010 -- encoded as: 0x8
				new byte[,] { { 0x2 }, { 0xE } }, //L //1110 -- encoded as: 0xE
				new byte[,] { { 0xC }, { 0xC } }, //O //with each bit representing if a block is present or not. 
				new byte[,] { { 0x6 }, { 0xC } }, //S 
				new byte[,] { { 0x4 }, { 0xE } }, //T
				new byte[,] { { 0xC }, { 0x6 } }  //Z
			};
			public bool Active { get; set; }
			private int[] Coords = { 0, 0 };
			public int Xpos { get => Coords[0]; set { Coords[0] = value >= 0 && value < 10 ? value : Coords[0]; } }
			public int Ypos { get => Coords[1]; set { Coords[1] = value >= 0 && value < 16 ? value : Coords[1]; } }
			private int _Rotation { get; set; }
			public int Rotation { get => _Rotation; set { int _ = value < 0 ? value + 4 : value; _Rotation = _ % 4; } }
			public int[][,] Shapes = new int[4][,] { new int[4, 4], new int[4, 4], new int[4, 4], new int[4, 4] };
			public void GenerateShape(int shapeIndex) {
				int[][,] _Shapes = Shapes; string bin = ""; int acc = 0;
				foreach (byte a in data[shapeIndex]) { bin += Convert.ToString(a, 2).PadLeft(4, '0'); }
				for (int y = 0; y < 4; y++) {
					for (int x = 0; x < 4; x++) {
						int current = acc < bin.Length ? bin[acc] == 48 ? 0 : 1 : 0;
						for (int r = 0; r < _Shapes.Length; r++) {
							switch (r) {
								case 0: { _Shapes[r][x, y] = current; break; }
								case 1: { _Shapes[r][y, x] = current; break; }
								case 2: { _Shapes[r][x, Math.Abs(y - 3)] = current; break; }
								case 3: { _Shapes[r][Math.Abs(y - 3), x] = current; break; }
							}
						}
						acc++;
					}
				}
				Shapes = _Shapes;
			}
			public int DataLength { get => data.Length; }
			public void Reset() { Xpos = 0; Ypos = 0; Rotation = 0; }
		}
		public class PlayField {
			public PlayField() { }
			private static readonly int sizeX = 10; private static readonly int sizeY = 16;
			public int[,] State { get; private set; } = new int[sizeX, sizeY];
			public void MutateState(int x, int y, int newValue) => State[x >= 0 && x < sizeX ? x : 0, y >= 0 && y < sizeY ? y : 0] = newValue;
			public bool Occupied(int x, int y) => x >= 0 && x < sizeX && y >= 0 && y < sizeY ? State[x, y] != 0 ? true : false : true;
			public int Cascade() {
				int[] GetRow(int[,] matrix, int row) => Enumerable.Range(0, matrix.GetLength(0)).Select(a => matrix[a, row]).ToArray();
				int rows = 0;
				for (int y = 0; y < sizeY; y++) {
					int[] _ = GetRow(State, y);
					if (_.All(i => i != 0)) {
						rows++;
						for (int x = 0; x < sizeX; x++) {
							MutateState(x, y, 0);
							for (int i = 0; i < sizeY; i++) { if (y - i > 0 && y - i - 1 > 0) { MutateState(x, y - i, State[x, y - i - 1]); } }
						}
					}
				}
				return rows;
			}
		}
		public static class Game {
			public static bool Paused { get; set; }
			public static bool Lost { get; set; }
			public static int Score { get; set; }
			public static int Lines { get; set; }
			private static int _Level = 1;
			public static int Level { get => _Level; set => _Level = value > 0 && value <= 20 ? value : _Level; }
			public static int[] ShapeBuffer { get; set; } = new int[2];
		}
		static void Main(string[] args) {
			//Console.WriteLine("Kontroller:\nNed: Nedåtpil\nVänster: Vänsterpil\nHöger: Högerpil\nSläpp: Uppåtpil\nRotera Medurs: Z\nRotera Moturs: X\nPausa: Escape"); Console.ReadKey(true);
			int[] scoreMap = { 0, 40, 100, 300, 1200 };
			Tetromino current = new Tetromino(); PlayField field = new PlayField(); Random random = new Random(); //create objects
			void Controls() {
				while (!Game.Lost) {
					Thread.Sleep(10); //sleep for 10ms to limit input repetition
					if (Console.KeyAvailable) {
						ConsoleKeyInfo key = Console.ReadKey(true);
						if (current.Active) {
							switch (key.Key) {
								case ConsoleKey.DownArrow: { Movement(0); break; }
								case ConsoleKey.LeftArrow: { Movement(1); break; }
								case ConsoleKey.RightArrow: { Movement(2); break; }
								case ConsoleKey.UpArrow: { Movement(3); break; }
								case ConsoleKey.Z: { current.Rotation++; break; }
								case ConsoleKey.X: { current.Rotation--; break; }
								case ConsoleKey.Escape: { Game.Paused = !Game.Paused; break; }
								default: { break; }
							}
						}
					}
				}
			} //using local functions since i need to have constant access to the objects i created
			bool Movement(int direction) {
				List<bool> result = new List<bool>();
				for (int y = 0; y < 4; y++) {
					for (int x = 0; x < 4; x++) {
						if (current.Shapes[current.Rotation][x, y] != 0) {
							switch (direction) {
								case 0: { result.Add(field.Occupied(current.Xpos + x, current.Ypos + y + 1)); break; } //down
								case 1: { result.Add(field.Occupied(current.Xpos + x - 1, current.Ypos + y)); break; } //left
								case 2: { result.Add(field.Occupied(current.Xpos + x + 1, current.Ypos + y)); break; } //right
								case 3: { break; } //drop
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
			foreach (int i in Game.ShapeBuffer) { Game.ShapeBuffer[i] = random.Next(current.DataLength); } //init shapeBuffer
			DateTime st = DateTime.Now; DateTime et = DateTime.Now;
			while (!Game.Lost) {
				et = DateTime.Now;
				TimeSpan t = et.Subtract(st);
				if (!Game.Paused && t.TotalMilliseconds >= 1000 / Game.Level) {
					st = et;
					if (!current.Active) {
						current.GenerateShape(Game.ShapeBuffer[0]);
						Game.ShapeBuffer[0] = Game.ShapeBuffer[1]; Game.ShapeBuffer[1] = random.Next(current.DataLength); //shift first element out of array, then replace it.
						current.Reset(); //reset values
					}
					if (Movement(0)) { current.Active = true; } else {
						current.Active = false;
						for (int y = 0; y < 4; y++) {
							for (int x = 0; x < 4; x++) {
								if (current.Shapes[current.Rotation][x, y] != 0) {
									if (field.State[current.Xpos + x, current.Ypos + y] == 0) {
										field.MutateState(current.Xpos + x, current.Ypos + y, current.Shapes[current.Rotation][x, y]);
									} else { Game.Lost = true; } //if a piece would be overwritten, assume player has lost
								}
							}
						}
						int i = field.Cascade(); Game.Lines += i;
						Game.Score += scoreMap[i] * Game.Level + 10; //score calculation
						Game.Level = Game.Lines / 2;
					}
				}
			}
			drawThread.Join(); controlThread.Join(); //join the threads back into main
			Console.SetCursorPosition(0, 0);
			for (int y = 0; y < 16; y++) {
				for (int x = 0; x < 10; x++) { Console.Write("░░"); }
				Thread.Sleep(50); Console.SetCursorPosition(0, Console.CursorTop + 1);
			} //losing animation 
			Console.SetCursorPosition(5, 8); Console.Write("Game Over!"); Console.ReadKey(true);
		}
		public static void Draw(PlayField field, Tetromino current) {
			Console.CursorVisible = false;
			while (!Game.Lost) {
				Console.SetCursorPosition(0, 0); //return cursor to top left
				if (!Game.Paused) {
					for (int y = 0; y < 16; y++) {
						for (int x = 0; x < 10; x++) {
							Console.Write(field.State[x, y] == 0 ? ".." : "██"); //TODO: reduce flicker by not drawing the background which will be covered by active piece
						}
						Console.SetCursorPosition(0, Console.CursorTop + 1);
					}
				}
				Console.SetCursorPosition(current.Xpos * 2, current.Ypos); //move cursor to current tetromino position
				if (!Game.Paused && current.Active) {
					for (int y = 0; y < 4; y++) {
						for (int x = 0; x < 4; x++) {
							if (current.Shapes[current.Rotation][x, y] == 1) { Console.Write("██"); } 
							else { Console.SetCursorPosition(Console.CursorLeft + 2, Console.CursorTop); }
						}
						Console.SetCursorPosition(current.Xpos * 2, Console.CursorTop + 1); //advance cursor
					}
				}
				if (Game.Paused) { Console.SetCursorPosition(6, 8); Console.Write("[Pausat]"); } //draw the pause screen dialog
				Console.SetCursorPosition(24, 0); Console.Write($"[ Poäng: {Game.Score,8} ][ Nivå: {Game.Level,2} ][ Rader: {Game.Lines,4} ]"); //draw score
				Thread.Sleep(30); //since drawing to console takes roughly 0.25s, sleep to reduce flickering
			}
		}
	}
}