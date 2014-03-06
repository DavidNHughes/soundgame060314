using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFML.Audio;

namespace SoundGame
{
    // Music controller!
    // Is passed pieces of audio to play...
    class MusicController : SoundStream
    {
        public MusicController(uint iChannels, uint iSampleRate, SoundBuffer buffer)
        {
            Initialize(iChannels, iSampleRate);
            m_buffer = buffer;
            m_audioData = buffer.Samples;
        }

        protected override bool OnGetData(out short[] samples)
        {
            // Not enough data...
            samples = new short[m_iSamplesPerUpdate];
            //Buffer.BlockCopy(m_audioData, m_iProgress, samples, 0, m_iSamplesPerUpdate);
            for (int i = 0; i < m_iSamplesPerUpdate; i++)
            {
                int iIndex = i + m_iProgress;

                // If we're bitcrushing, we want to round our indicies to the nearest X,
                // the higher value we round it to, the more distorted the result is.
                if (m_distortion > 0.0d)
                {
                    double dAmt = 130 * m_distortion;
                    iIndex = (int)(Math.Round((double)iIndex / dAmt) * dAmt);
                }

                double dData = (double)m_audioData[iIndex];
                dData /= short.MaxValue; // Bring it into -1 to 1 range
                samples[i] = (short)((dData * m_flVolume) * short.MaxValue);
            }

            // Advance progress
            m_iProgress += m_iSamplesPerUpdate;

            return true;
        }

        protected override void OnSeek(TimeSpan timeOffset)
        {
            // We don't care about seek attempts.
        }

        public double CurrentTime()
        {
            return ((double)m_iProgress / (double)(m_buffer.ChannelCount * m_buffer.SampleRate)) * 2;
        }

        public void SetDistortion(double dAmt)
        {
            m_distortion = Math.Min(dAmt, 1.0);
            m_distortion = Math.Max(m_distortion, 0.0);
        }

        private SoundBuffer m_buffer;
        private short[] m_audioData;
        private int m_iProgress;
        const int m_iSamplesPerUpdate = 4410;
        const float m_flVolume = 0.1f;
        private double m_distortion = 0.0d;

    }
}
