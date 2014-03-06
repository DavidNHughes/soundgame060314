using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SFML.Graphics;

namespace SoundGame
{
    /*
     * Game profiler!
     * This is used for measuring our FPS, time to render frames
     * keeps track of averages, min & max time for frames
     * too help us in profiling :~)
     * */ 

    class Profiler : Drawable
    {
        public Profiler()
        {
            m_fpsTimer = new Stopwatch();
            m_fpsTimer.Restart();

            m_fpsAverageTimer = new Stopwatch();
            m_fpsAverageTimer.Restart();

            TextFont = new Font("resources/sansation.ttf");

            FrameTimeBuffer = new double[FRAME_TIME_SAMPLES];
        }

        // Called when frame starts, calculates FPS using elapsed time since last frame start.
        public void OnFrameStart()
        {
            FrameInterval = m_fpsTimer.Elapsed.TotalSeconds;
            m_fpsTimer.Restart();

            // Count the number of frames, every X seconds we calcualte the average FPS for the period.
            m_iFramesDrawn++;
            if ( m_fpsAverageTimer.Elapsed.TotalSeconds >= AVERAGE_FPS_INTERVAL )
            {
                m_fpsAverageTimer.Restart();
                AverageFPS = (double)m_iFramesDrawn / AVERAGE_FPS_INTERVAL;
                m_iFramesDrawn = 0;
            }
        }

        // Called at the end of each frame, returns the total length of time it took to render this frame.
        public double OnFrameEnd()
        {
            TotalFrames++;

            double dTimeForFrame = m_fpsTimer.Elapsed.TotalMilliseconds;

            // Add to our frame time buffer history
            FrameTimeBuffer[TotalFrames % FRAME_TIME_SAMPLES] = dTimeForFrame;

            // Calc min/avg/max frametimes.
            double dTotalFrameTime = 0.0d;
            FrameTimeMin = FrameTimeBuffer[0];
            FrameTimeMax = FrameTimeBuffer[0];
            for (int i = 0; i < FRAME_TIME_SAMPLES; i++ )
            {
                dTotalFrameTime += FrameTimeBuffer[i];
                FrameTimeMin = Math.Min(FrameTimeMin, FrameTimeBuffer[i]);
                FrameTimeMax = Math.Max(FrameTimeMax, FrameTimeBuffer[i]);
            }
            FrameTimeAvg = dTotalFrameTime / FRAME_TIME_SAMPLES;

            return dTimeForFrame;
        }

        // Renders profiler info on screen
        public void Draw(RenderTarget target, RenderStates states)
        {
            String sData = "";
            sData += "FPS: " + AverageFPS + "\n";
            sData += "Frame Times:\n" + Math.Round(FrameTimeMin, 4) + "\n" + Math.Round(FrameTimeAvg, 4) + "\n" + Math.Round(FrameTimeMax, 4);

            Text profilerText = new Text(sData, TextFont, 16);
            profilerText.Draw( target, states );
        }

        public double FrameInterval = 0.0d;
        public double AverageFPS = 0.0d;
        private Stopwatch m_fpsTimer;
        private int TotalFrames = 0;

        // Stuff for average FPS
        private const double AVERAGE_FPS_INTERVAL = 0.25d;
        private long m_iFramesDrawn = 0;
        private Stopwatch m_fpsAverageTimer;

        // Frame times!
        private const int FRAME_TIME_SAMPLES = 1000;
        private double[] FrameTimeBuffer;
        private double FrameTimeMin = 1000.0d;
        private double FrameTimeMax = 0.0d;
        private double FrameTimeAvg = 0.0d;

        // Render stuff
        private Font TextFont;
    }
}
