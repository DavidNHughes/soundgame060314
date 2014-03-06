using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFML.Graphics;
using SFML.Window;
using SoundEngine;

namespace SoundGame.Entities
{
    class KamikazeEnemy : Transformable, Drawable, IBaseEntity
    {
        private double TimeToDive;
        private static Random rand = new Random();
        private double TimeAlive;
        private State CurrentState;
        private Vector2f DesiredPosition;
        private float speed = 0.2f;
        private enum State
        {
            IDLE = 0,
            DIVE
        };
        private Texture EnemyTexture;
        private Sprite EnemySprite;

        public KamikazeEnemy(Vector2f vecPosition)
        {
            Position = vecPosition;
            EnemyTexture = ContentManager.LoadTexture("resources/greenfly.png");
            EnemySprite = new Sprite(EnemyTexture);
            EnemySprite.Origin = new Vector2f(EnemySprite.TextureRect.Width / 4, EnemySprite.TextureRect.Height / 2);
            EnemySprite.Scale = new Vector2f(0.6f, 0.6f);
            CurrentState = State.DIVE;
            TimeToDive = rand.NextDouble() * 5.0d;
        }

        public void Update(double dFrameTime)
        {
            TimeAlive += dFrameTime;

            // SUPER crude state machine bullshit for gds test, rework this...
            // ill need to change this aswell ^^
            if (CurrentState == State.DIVE && TimeAlive < TimeToDive)
            {
                DesiredPosition.Y = speed;
            }

            if (CurrentState == State.DIVE && Position.Y > 25.0f)
            {
                DesiredPosition.Y = 0.0f;
                CurrentState = State.IDLE;
            }

            if(CurrentState == State.IDLE && TimeAlive > TimeToDive)
            {
                CurrentState = State.DIVE;
                
            }

            if (CurrentState == State.DIVE && TimeAlive > TimeToDive)
            {
                speed += 0.02f;
                DesiredPosition.Y += speed;

                for (int i = 0; i < SoundGame.GameState.m_entities.Count; i++)
                {
                    WaveEntity waveEntity = (WaveEntity)SoundGame.GameState.m_entities[i];

                    // revise this, jesus christ
                    if (Position.X > waveEntity.Position.X && Position.X < waveEntity.Position.X + WaveEntity.SEGMENT_WIDTH)
                    {
                        if (Position.Y > waveEntity.Position.Y && Position.Y < waveEntity.Position.Y + 100)
                        {
                            waveEntity.Distort(0.15d);
                            SoundGame.GameState.m_kamikaze_enemies.Remove(this);
                            break;
                        }
                    }
                }
            }
            

            // AWFUL CODE END

                Position += DesiredPosition;
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            RenderStates enemyState = new RenderStates(states);
            enemyState.Texture = EnemyTexture;

            Vector2u TEXTURE_SIZE = EnemyTexture.Size;          

            EnemySprite.Position = Position;
            target.Draw(EnemySprite, enemyState);
        }

        public void IsClicked(float x, float y)
        {
            // is mouse co-ords in the sprite?
            if (EnemySprite.GetGlobalBounds().Contains(x, y))
            {
                Kill();
            }
        }

        public void Kill()
        {
            SoundGame.GameState.m_kamikaze_enemies.Remove(this);
        }
    }


}
