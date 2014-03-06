using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using SFML.Audio;
using SFML.Window;
using SFML.Graphics;
using System.Diagnostics;
using System.Threading;

namespace SoundGame
{
    class SoundGame
    {
        static void Start()
        {
            ContextSettings windowSettings = new ContextSettings(0, 0, 0);
            // Create the main window
            Game = new RenderWindow(new VideoMode(1280, 720, 32), "Sound Game", Styles.Default, windowSettings);
            GameSize = Game.GetView().Size;

            Game.Closed += new EventHandler(OnClosed);
            Game.Resized += new EventHandler<SizeEventArgs>(OnReize);
            Game.SetKeyRepeatEnabled(false);

            // Init states...
            GameState = new GameState(Game);
            GameState.Init();

            Stopwatch frameTimer = new Stopwatch();
            Profiler gameProfiler = new Profiler();

            while (Game.IsOpen())
            {
                // Update our profiler first.
                gameProfiler.OnFrameStart();

                // Required by SFML to pass through events that have happened
                Game.DispatchEvents();

                Game.Clear();

                // Pass updates & draws to our states.
                switch( currentState )
                {
                    case GameStates.STATE_GAME:
                        GameState.Update(gameProfiler.FrameInterval);
                        Game.Draw(GameState);
                        break;

                    default:
                        Console.WriteLine("Unknown game state?");
                        break;
                }

                Game.Draw(gameProfiler);
                Game.Display();

                double dMSForFrame = gameProfiler.OnFrameEnd();

                // So... we're capping the FPS to 200, this kinda seems
                // counter-intuitive to limit it but it makes sense, I swear.
                // Desired sleep time should be = 1/FPS * 1000
                /*
                if (dMSForFrame <= 5.0d)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(5.0d - dMSForFrame));
                }
                else
                {
                    // No sleep, we want another frame ASAP
                    Console.WriteLine("Warning, frame took > 5ms to render");
                }
                */
            }
        }

        static void OnClosed(object sender, EventArgs e)
        {
            Window window = (Window)sender;
            window.Close();
        }

        static void OnReize(object sender, EventArgs e)
        {
            SizeEventArgs data = (SizeEventArgs)e;
            //Game.SetView(new View(new FloatRect(0, 0, data.Width, data.Height)));
        }


        // Main entry point.
        static void Main(string[] args)
        {
            SoundGame.Start();
        }

        // 
        enum GameStates
        {
            STATE_GAME = 0
        };

        static GameStates currentState = GameStates.STATE_GAME;
        public static RenderWindow Game;
        public static GameState GameState;
        public static Vector2f GameSize; // todo, figure out the whole screensize system, it's complicated

    }

}
