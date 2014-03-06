using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;
using SFML.Window;
using SoundGame.Entities;

namespace SoundGame
{
    class WaveEntity : Transformable, IBaseEntity
    {
        public WaveEntity( GameState game, int iSegment, short[] audioData )
        {
            m_parent = game;
            m_shape = new RectangleShape(new Vector2f(SEGMENT_WIDTH-1, 100));

            m_iSegment = iSegment;

            m_bDeleteMe = false;
            m_audioData = audioData;

            // Set up waveform
            CalcWave();
        }

        public void CalcWave()
        {
            int iTotalSamples = 400;
            
            int iSamplesPerPixel = (m_audioData.Length / 2) / iTotalSamples;
            m_waveVerts = new VertexArray(PrimitiveType.LinesStrip, (uint)iTotalSamples);
            for (int i = 0; i < iTotalSamples; i++)
            {
                // Old average
                /*double dTotal = 0.0d;
                int iTotalTaken = 0;
                for (int j = i * iSamplesPerPixel; j < (i + 1) * iSamplesPerPixel; j++)
                {
                    iTotalTaken++;

                    int iIndex = j;

                    // If we're bitcrushing, we want to round our indicies to the nearest X,
                    // the higher value we round it to, the more distorted the result is.
                    if (m_dDistortion > 0.0d)
                    {
                        double dAmt = 550 * m_dDistortion;
                        iIndex = (int)(Math.Round((double)iIndex / dAmt) * dAmt);
                    }

                    dTotal += m_audioData[iIndex];
                }
                dTotal /= iTotalTaken;*/

                // New peak per sample area
                short maxPeak = short.MinValue;
                for (int j = i * iSamplesPerPixel; j < (i + 1) * iSamplesPerPixel; j++)
                {
                    int iIndex = j;

                    // If we're bitcrushing, we want to round our indicies to the nearest X,
                    // the higher value we round it to, the more distorted the result is.
                    if (m_dDistortion > 0.0d)
                    {
                        double dAmt = 550 * m_dDistortion;
                        iIndex = (int)(Math.Round((double)iIndex / dAmt) * dAmt);
                    }

                    maxPeak = Math.Max(maxPeak, m_audioData[iIndex]);
                }

                float flVertX = (float)i * ((float)SEGMENT_WIDTH / (float)iTotalSamples);
                float flVertY = ((float)maxPeak / (float)short.MaxValue) * 50.0f;
                m_waveVerts[(uint)i] = new Vertex(new Vector2f(flVertX, flVertY + 50));
            }

        }

        public void Update( double dFrameTime )
        {

        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            if (Position.X < -SEGMENT_WIDTH)
                m_bDeleteMe = true;

            // TEMP
            if (Position.X > 1280 || Position.X < -SEGMENT_WIDTH)
                return;

            // bg color
            if (m_dDistortion > 0.0d)
                m_shape.FillColor = new Color(255, 0, 0, (byte)(m_dDistortion * 255));
            else
                m_shape.FillColor = Color.Transparent;

            states.Transform *= Transform;
            target.Draw(m_shape, states);
            target.Draw(m_waveVerts, states);
        }

        public int Segment()
        {
            return m_iSegment;
        }

        public void OnClick()
        {
            Distort(0.1d);
        }
        public void Distort( double dAmt )
        {
            m_dDistortion += dAmt;
            m_dDistortion = Math.Min(m_dDistortion, 1.0d);
            CalcWave();
        }

        private GameState m_parent;
        private RectangleShape m_shape;

        private short[] m_audioData;
        private int m_iSegment;
        public bool m_bDeleteMe;

        public double m_dDistortion;

        private VertexArray m_waveVerts;
        public const double SEGMENT_SECONDS = 2.0d;
        public const int SEGMENT_WIDTH = 200;

    }
}
