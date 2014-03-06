using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Audio;
using SFML.Graphics;
using SFML.Window;
using SoundEngine;
using SoundGame.Entities;

namespace SoundGame
{

    #region UNUSED 

    class TestStream : SoundStream
    {
        public TestStream(uint iChannels, uint iSampleRate, short[] audioData)
        {
            Initialize(iChannels, iSampleRate);
            m_iOffset = 30 * (int)iChannels * (int)iSampleRate;
            m_dBitCrushScale = 0.0;
            m_audioData = audioData;
            m_latestData = new List<short>();
        }

        protected override bool OnGetData(out short[] samples)
        {
            Random rand = new Random();

            lock (m_audioData)
            {
                samples = new short[m_iSamplesPerUpdate];

                for (int i = 0; i < m_iSamplesPerUpdate; i++)
                {
                    int iIndex = i + m_iOffset;

                    // If we're bitcrushing, we want to round our indicies to the nearest X,
                    // the higher value we round it to, the more distorted the result is.
                    if (m_dBitCrushScale > 0.0d)
                        iIndex = (int)(Math.Round((double)iIndex / m_dBitCrushScale) * m_dBitCrushScale);

                    /*short sData = m_audioData[iIndex];

                    double dData = (double)sData;
                    dData /= short.MaxValue; // Bring it into -1 to 1 range
                    samples[i] = (short)(dData * short.MaxValue);*/
                    //samples[i] = m_audioData[iIndex];
                    samples[i] = (short)rand.Next(short.MinValue, short.MaxValue);
                }

                m_iOffset += m_iSamplesPerUpdate;
                m_lastSamples = samples;

                return true;
            }
        }

        protected override void OnSeek(TimeSpan timeOffset)
        {
            // Ignore seeking for now.
        }

        public void SetBitCrushScale(double dScale)
        {
            Console.WriteLine("Bitcrush scale: " + dScale);
            m_dBitCrushScale = dScale;
        }

        public double GetBitCrushScale()
        {
            return m_dBitCrushScale;
        }

        public void FillPlot(out Vertex[] plotData)
        {
            /*const int iSampleAmount = 4410;
            int iTotalSamples = m_audioData.Length / iSampleAmount;
            plotData = new Vertex[iTotalSamples];

            for (int i = 0; i < m_audioData.Length; i += iSampleAmount)
            {
                double dSample = 0.0d;
                if (i < m_audioData.Length - iSampleAmount)
                {
                    for (int j = 0; j < iSampleAmount; j++)
                        dSample += m_audioData[i + j];
                    dSample /= iSampleAmount;
                }

                int iIndex = i / iSampleAmount;
                float flX = iIndex * 2;
                //float flY = i;
                float flY = (float)( ( (float)dSample / (float)short.MaxValue ) * 1500.0f ) + 400;

                iIndex = Math.Min(iIndex, iTotalSamples - 1);
                plotData[iIndex] = new Vertex(new Vector2f(flX, flY));
            }*/

            const int iAverage = 5;
            int iSamplesToSample = m_lastSamples.Length;
            plotData = new Vertex[iSamplesToSample / iAverage];

            const int iOffsetX = 0;
            const int iOffsetY = 400;

            for (int i = 0; i < iSamplesToSample; i += iAverage)
            {
                double dAverage = 0.0d;
                if (i + iAverage < iSamplesToSample)
                {
                    for ( int j = 0; j < iAverage; j++ )
                    {
                        dAverage += m_lastSamples[i+j];
                    }
                    dAverage /= iAverage;
                }
                else
                {
                    dAverage = m_lastSamples[iSamplesToSample-1];
                }

                int iIndex = i / iAverage;
                float flX = iOffsetX + ( iIndex * 2 );
                float flY = (float)(((float)dAverage / (float)short.MaxValue) * 150.0f) + iOffsetY;

                plotData[iIndex] = new Vertex(new Vector2f(flX, flY));
            }

        }

        private short[] m_lastSamples;
        private short[] m_audioData;
        public List<short> m_latestData;
        public double m_dBitCrushScale;
        public int m_iOffset;
        const int m_iSamplesPerUpdate = 4410;
    }

    #endregion

    class GameState : Drawable
    {
        public GameState( RenderWindow window )
        {
            m_audioBuffer = new SoundBuffer("resources/leaders.ogg");
            Console.WriteLine("SoundBuffer loaded ("+m_audioBuffer+")");

            // Create our controller, pass rate, channels
            m_musicController = new MusicController(m_audioBuffer.ChannelCount, m_audioBuffer.SampleRate, m_audioBuffer );

            m_window = window;
            m_entities = new List<IBaseEntity>();
            m_enemies = new List<IBaseEntity>();
            m_kamikaze_enemies = new List<IBaseEntity>();
            gameTimer = new Stopwatch();
            gameTimer.Start();
        }

        public void Init()
        {
            m_window.KeyPressed += new EventHandler<KeyEventArgs>(KeyPressed);
            m_window.MouseButtonPressed += new EventHandler<MouseButtonEventArgs>(MouseButtonPressed);

            // Handle entity spawning...
            //if (!gameTimer.IsRunning || gameTimer.ElapsedMilliseconds >= SAMPLES_PER_ENTITY * 1000)
            double dDuration = (double)m_audioBuffer.Samples.Length / (double)(m_audioBuffer.SampleRate);
            Console.WriteLine(dDuration);
           
            // each entity represents 1 second
            // each entity is 100 pixels wide
            // 100 pixels per second

            Console.WriteLine("Starting entity spawn: " + Math.Ceiling(dDuration));
            short[] baseData = m_audioBuffer.Samples;
            uint iSamplesPerBlock = (uint)( WaveEntity.SEGMENT_SECONDS * m_audioBuffer.ChannelCount * m_audioBuffer.SampleRate );
            m_iTotalEntites = (int)Math.Ceiling(dDuration / WaveEntity.SEGMENT_SECONDS);
            for (int i = 0; i < m_iTotalEntites - 1; i++)
            {
                short[] data = new short[iSamplesPerBlock];
                Buffer.BlockCopy(baseData, (int)iSamplesPerBlock * i, data, 0, (int)iSamplesPerBlock);

                WaveEntity newWave = new WaveEntity(this, i, data);
                m_entities.Add(newWave);
            }
            Console.WriteLine("Ending entity spawn");

            m_musicController.Play();
        }

        public void Update( double dFrameTime )
        {

            WaveEntity waveEntPlaying = null;
            double dTimeThroughSong = m_musicController.CurrentTime();
            double dOffset = ( dTimeThroughSong / WaveEntity.SEGMENT_SECONDS) * WaveEntity.SEGMENT_WIDTH;
            //double dOffset = 0;

            // Render our entities!
            for (int i = 0; i < m_entities.Count; i++)
            {
                // Todo revise, this will break when we add new entities
                WaveEntity waveEntity = (WaveEntity)m_entities[i];

                waveEntity.Position = new Vector2f((WaveEntity.SEGMENT_WIDTH * waveEntity.Segment()) - (float)(dOffset), 500);

                if (waveEntity.Position.X < 0)
                    waveEntPlaying = waveEntity;

                m_entities[i].Update(dFrameTime);
            }

            for (int i = m_entities.Count - 1; i >= 0; i--)
            {
                // Todo revise, this will break when we add new entities
                WaveEntity waveEntity = (WaveEntity)m_entities[i];
                if (waveEntity.m_bDeleteMe)
                    m_entities.RemoveAt(i);
            }

            // Enemy updating
            for (int i = 0; i < m_enemies.Count; i++)
            {
                m_enemies[i].Update(dFrameTime);
            }

            for (int i = 0; i < m_kamikaze_enemies.Count; i++)
            {
                m_kamikaze_enemies[i].Update(dFrameTime);
            }

            m_dCurrentDistortion -= 0.3 * dFrameTime;

            if ( waveEntPlaying != null )
            {
                m_dCurrentDistortion = Math.Max( m_dCurrentDistortion, waveEntPlaying.m_dDistortion );
            }

            m_musicController.SetDistortion(m_dCurrentDistortion);
        }

        public void Draw( RenderTarget target, RenderStates states)
        {
            // SHADER TEST
            //Sprite bg = new Sprite(ContentManager.LoadTexture("resources/background.jpg"));
            //target.Draw(bg, states);

            // Render our entities!
            for (int i = 0; i < m_entities.Count; i++)
                target.Draw(m_entities[i]);

            // Render enemies
            for (int i = 0; i < m_enemies.Count; i++)
                target.Draw(m_enemies[i]);

            for (int i = 0; i < m_kamikaze_enemies.Count; i++)
                target.Draw(m_kamikaze_enemies[i]);
        }

        void KeyPressed(object sender, EventArgs e)
        {
            Random rand = new Random();
            KeyEventArgs ke = (KeyEventArgs)e;

            switch( ke.Code )
            {
                case Keyboard.Key.Space:
                    m_enemies.Add(new LongEnemy(new Vector2f((float)rand.NextDouble() * SoundGame.GameSize.X, -50)));
                    break;
                case Keyboard.Key.S:
                    m_enemies.Add(new LongEnemy(new Vector2f(500, -500), 250));
                    break;
                case Keyboard.Key.F:
                    m_enemies.Add(new LongEnemy(new Vector2f(500, 500), 3));
                    break;
                case Keyboard.Key.K:
                    //random number since we dont have screen width sorted?
                    m_kamikaze_enemies.Add(new KamikazeEnemy(new Vector2f(rand.Next(1200) , -50)));
                    break;
            }
        }
        void MouseButtonPressed(object sender, EventArgs e)
        {
            MouseButtonEventArgs ke = (MouseButtonEventArgs)e;

            Vector2i vecMouse = new Vector2i( ke.X, ke.Y );
            Vector2f vecWorld = m_window.MapPixelToCoords(vecMouse);

            // click on wave to distort
            /*for (int i = 0; i < m_entities.Count; i++)
            {
                WaveEntity waveEntity = (WaveEntity)m_entities[i];

                // revise this, jesus christ
                if (vecWorld.X > waveEntity.Position.X && vecWorld.X < waveEntity.Position.X + WaveEntity.SEGMENT_WIDTH)
                {
                    if (vecWorld.Y > waveEntity.Position.Y && vecWorld.Y < waveEntity.Position.Y + 100)
                    {
                        waveEntity.OnClick();
                        break;
                    }
                }
                
            }*/

            // click on enemy to split
            for (int i = 0; i < m_enemies.Count; i++)
            {
                if (!(m_enemies[i] is LongEnemy))
                    continue;

                // TODO: interface this shit out to the base entity for collision stuff probably
                LongEnemy enemy = (LongEnemy)m_enemies[i];
                int iSegment = enemy.GetSegmentInPosition(vecWorld.X, vecWorld.Y);

                if (iSegment != -1)
                {
                    enemy.Split(iSegment);
                    return;
                }
            }

            // click on enemy to kill
            for (int i = 0; i < m_kamikaze_enemies.Count; i++)
            {
                KamikazeEnemy enemy = (KamikazeEnemy)m_kamikaze_enemies[i];
                enemy.IsClicked(vecWorld.X, vecWorld.Y);               
            }


        }

        public void EntitySpawned( )
        {
        }

        private SoundBuffer m_audioBuffer;
        private RenderWindow m_window;
        public List<IBaseEntity> m_entities; // todo: make private
        public List<IBaseEntity> m_enemies; // todo: make private
        public List<IBaseEntity> m_kamikaze_enemies; // todo: make private

        private MusicController m_musicController;
        private int m_iTotalEntites;
        private Stopwatch gameTimer;

        private double m_dCurrentDistortion = 0.0d;

        private const int SAMPLES_PER_ENTITY = 1;
    }
}
