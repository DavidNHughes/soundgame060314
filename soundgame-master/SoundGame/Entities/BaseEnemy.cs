using SFML.Graphics;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SoundEngine;
using SoundGame.Helpers;

namespace SoundGame.Entities
{
    class LongEnemy : Transformable, Drawable, IBaseEntity
    {
        const int MAX_SEGMENTS = 16;
        const float SPRITE_SIZE = 20.0f; // RADIUS of the sprite
        const float SEGMENT_SPACING = 22.0f;
        private enum State
        {
            IDLE = 0,
            DIVE
        };

        private double TimeToDive;
        private State CurrentState;
        private static Random rand = new Random();

        private double TimeAlive;
        private Vector2f DesiredPosition;
        private Vector2f LastPosition;
        private Line PositionHistory;
        private Texture EnemyTexture;
        private Sprite EnemySprite;
        public float Heading; // TODO: Make private
        private double RandomSeed;
        private int Segments;

        public LongEnemy(Vector2f vecPosition, int iSegments = MAX_SEGMENTS, List<Vector2f> vecHistory = null )
        {
            Position = vecPosition;

            EnemyTexture = ContentManager.LoadTexture("resources/chitinbit.png");
            EnemySprite = new Sprite(EnemyTexture);
            EnemySprite.Origin = new Vector2f(EnemySprite.TextureRect.Width/4, EnemySprite.TextureRect.Height/2);
            EnemySprite.Scale = new Vector2f(1.2f, 1.2f);
            CurrentState = State.IDLE;
            RandomSeed = rand.NextDouble() * 100.0d; ;
            TimeToDive = rand.NextDouble() * 5.0d;
            Heading = 0.0f;
            Segments = iSegments;

            PositionHistory = new Line(SEGMENT_SPACING * (Segments + 2), vecHistory);
        }

        public void Update(double dFrameTime)
        {
            TimeAlive += dFrameTime;

            // SUPER crude state machine bullshit for gds test, rework this...
            // AWFUL CODE BEGINNING
            if (CurrentState != State.DIVE && TimeAlive > TimeToDive)
            {
                CurrentState = State.DIVE;
                DesiredPosition = new Vector2f((float)rand.NextDouble() * SoundGame.GameSize.X, SoundGame.GameSize.Y * 0.75f);
            }

            // TEMP: Revise this
            if (CurrentState == State.IDLE)
            {
                DesiredPosition = new Vector2f((float)((Math.Sin((TimeAlive + RandomSeed) * 0.95) / 2.0d) + 0.5) * SoundGame.GameSize.X, (float)(Math.Cos((TimeAlive + RandomSeed) * 5.4) * 50) + 100);

            }
            else if ( CurrentState == State.DIVE )
            {
                // done dive?
                if ( (DesiredPosition - Position).Length() < 30.0f )
                {
                    CurrentState = State.IDLE;
                    TimeToDive = TimeAlive + (rand.NextDouble() * 10.0d) + 2.0d;
                    
                    for (int i = 0; i < SoundGame.GameState.m_entities.Count; i++)
                    {
                        WaveEntity waveEntity = (WaveEntity)SoundGame.GameState.m_entities[i];

                        // revise this, jesus christ
                        if (Position.X > waveEntity.Position.X && Position.X < waveEntity.Position.X + WaveEntity.SEGMENT_WIDTH)
                        {
                            if (Position.Y > waveEntity.Position.Y && Position.Y < waveEntity.Position.Y + 100)
                            {
                                waveEntity.Distort(0.15d);
                                break;
                            }
                        }

                    }
                }
            }

            // AWFUL CODE END
            float flMovementSpeed = (CurrentState == State.IDLE ? 350.0f : 600.0f);
            float flTurnSpeed = (CurrentState == State.IDLE ? 400.0f : 250.0f);

            float flDesiredHeading = (DesiredPosition - Position).Angle();
            Heading = (float)AngleHelper.Approach(Heading, flDesiredHeading, (float)dFrameTime * flTurnSpeed);

            Vector2f vecForward = AngleHelper.ToVector(Heading);
            Position += (vecForward) * (flMovementSpeed * (float)dFrameTime);

            // Feed new position to history
            if ( !LastPosition.Equals( Position ) )
            {
                PositionHistory.AddPosition(Position);
                LastPosition = Position;
            }

            // Mouse control
            /*Vector2f vecMouse = new Vector2f(Mouse.GetPosition(SoundGame.Game).X, Mouse.GetPosition(SoundGame.Game).Y);

            // if we've moved...
            if (vecMouse.X != Position.X || vecMouse.Y != Position.Y)
                PositionHistory.AddPosition(vecMouse);

            Position = vecMouse;*/

        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            RenderStates enemyState = new RenderStates(states);
            enemyState.Texture = EnemyTexture;

            Vector2u TEXTURE_SIZE = EnemyTexture.Size;

            // segments
            // draw in reverse
            for (int i = Segments - 1; i >= 0; i--)
            {
                const int SMOOTHING_DIST = 15;
                float segmentPos = (i + 1) * SEGMENT_SPACING;
                Vector2f positionOnLine = PositionHistory.GetLinePosition( segmentPos );
                Vector2f positionOnPre = PositionHistory.GetLinePosition( segmentPos - SMOOTHING_DIST );
                //Vector2f positionOnPost = PositionHistory.GetLinePosition( segmentPos + SMOOTHING_DIST );

                float flAngle1 = (positionOnLine - positionOnPre).Angle();
                //float flAngle2 = (positionOnPost - positionOnLine).Angle();
                //double dAverage = AngleHelper.Average(flAngle1, flAngle2);

                
                EnemySprite.Position = positionOnLine;
                EnemySprite.Rotation = (float)flAngle1;
                target.Draw(EnemySprite, enemyState);
            }

            /*
            Vector2f[] vecSegments;
            PositionHistory.GetTrailPositions( Segments, SEGMENT_SPACING, out vecSegments );
            for (int i = Segments - 1; i >= 0; i--)
            {
                Vector2f positionOnLine = vecSegments[i];

                double dAverage;
                if (i < 1)
                    dAverage = Heading + 180;
                else
                {

                    Vector2f positionPre2 = vecSegments[i - 1];
                    Vector2f positionPost2 = vecSegments[i + 1];

                    float flAngle1 = (positionOnLine - positionPre2).Angle();
                    float flAngle2 = (positionPost2 - positionOnLine).Angle();

                    dAverage = AngleHelper.Average(flAngle1, flAngle2);
                }

                EnemySprite.Position = positionOnLine;
                EnemySprite.Rotation = (float)dAverage;
                target.Draw(EnemySprite, enemyState);
            }*/
            

            // base 
            EnemySprite.Position = Position;
            target.Draw(EnemySprite, enemyState);


            //target.Draw(PositionHistory);
            //EnemySprite.Rotation
        }

        public int GetSegmentInPosition( float x, float y )
        {
            // main head check
            if (EnemySprite.GetGlobalBounds().Contains(x, y))
                return 0;
            
            Vector2f[] vecSegments;
            PositionHistory.GetTrailPositions(Segments, SEGMENT_SPACING, out vecSegments);
            for (int i = Segments - 1; i >= 0; i--)
            {
                Vector2f positionOnLine = vecSegments[i];
                EnemySprite.Position = positionOnLine;
                if (EnemySprite.GetGlobalBounds().Contains(x, y))
                {
                    return i;
                }
            }

            return -1;
        }

        // Splits the enemy at the given segment, removing the segment and creating two enemies either side.
        public void Split( int iSegment )
        {
            Vector2f[] vecSegments;
            PositionHistory.GetTrailPositions(Segments, SEGMENT_SPACING, out vecSegments);

            int iFrontSize = iSegment;
            int iBackSize = Segments - iSegment - 1;

            if (iFrontSize > 0)
            {
                LongEnemy newEnemy = new LongEnemy(Position, iFrontSize, PositionHistory.Positions);
                newEnemy.Heading = Heading;
                SoundGame.GameState.m_enemies.Add(newEnemy);
            }

            if (iBackSize > 0)
            {
                List<Vector2f> backHistory = new List<Vector2f>();
                //for (int i = 0; i < iBackSize; i++ )
                for (int i = iBackSize - 1; i >= 0; i--)
                {
                    backHistory.Add(vecSegments[iFrontSize + i + 1]);
                }

                LongEnemy newEnemy = new LongEnemy(vecSegments[iFrontSize], iBackSize, backHistory);
                newEnemy.Heading = Heading;
                SoundGame.GameState.m_enemies.Add(newEnemy );
            }

            SoundGame.GameState.m_enemies.Remove(this);
        }
    }
}
