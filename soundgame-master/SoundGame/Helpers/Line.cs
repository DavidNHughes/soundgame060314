using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFML.Graphics;
using SFML.Window;
using SoundEngine;

namespace SoundGame.Helpers
{

    // Helper class for some cool line shit, i'm proud of this, good target for optimization should we need to
    class Line : Drawable
    {
        public Line(float maxLength, List<Vector2f> vecHistory = null)
        {
            MaxLength = maxLength;

            /*Positions = new List<Vector2f>();

            if ( vecHistory != null )
            {
                for (int i = 0; i < vecHistory.Count; i++)
                    AddPosition(vecHistory[i]);
            }*/
            if (vecHistory == null)
                Positions = new List<Vector2f>();
            else
                Positions = new List<Vector2f>(vecHistory);
        }

        public void AddPosition(Vector2f pos)
        {
            Positions.Add(pos);
            CalculateLength();
            //PruneLine();
        }

        public void AddPosition(float x, float y)
        {
            AddPosition(new Vector2f(x, y));
        }

        // Returns a screen position X pixels away from the end of the line
        public Vector2f GetLinePosition(float flGoal)
        {
            if (Positions.Count <= 0)
                return new Vector2f();

            // If we're wanting a distance further than our total length, 
            // bail early and just return the end position
            if (flGoal > Length)
            {
                return Positions[0];
            }

            // this could be improved by searching through the position array backwards
            if (Positions.Count > 1)
            {
                Vector2f vecLastPos = Positions[Positions.Count - 1];
                float flDistanceProgress = 0.0f;

                for (int i = Positions.Count - 2; i >= 0; i--)
                {
                    float flDistanceLastSegment = (Positions[i] - vecLastPos).Length();
                    float flDistanceNextSegment = (Positions[Math.Max(i - 1, 0)] - Positions[i]).Length();

                    flDistanceProgress += flDistanceLastSegment;

                    if (flGoal > flDistanceProgress && flGoal <= flDistanceProgress + flDistanceNextSegment)
                    {
                        //Console.WriteLine("Found position after " + ((Positions.Count - 2) - i));
                        float flFractionOnSegment = (flGoal - flDistanceProgress) / flDistanceNextSegment;
                        return Positions[i] + ((Positions[Math.Max(i - 1, 0)] - Positions[i]) * flFractionOnSegment);
                    }

                    vecLastPos = Positions[i];
                }
            }

            // Should never reach here
            return Positions[0];
        }
        
        public void GetTrailPositions( int iTotalPositions, float flSpacing, out Vector2f[] trailPositions )
        {
            iTotalPositions += 1;
            trailPositions = new Vector2f[iTotalPositions];

            if (Positions.Count <= 0)
                return;

            float flCurrentPos = 0.0f;
            float flNextAddPos = flSpacing;
            int iAddedPositions = 0;
            Vector2f vecLastPos = Positions[Positions.Count-1];

            for (int i = 0; i < Positions.Count; i++)
            {
                int iIndex = Positions.Count - 1 - i;

                if (iAddedPositions >= iTotalPositions)
                    return;

                //float flDistanceLastSegment = (Positions[i] - vecLastPos).Length();
                Vector2f vecThisToNext = Positions[Math.Max(iIndex - 1, 0)] - Positions[iIndex];
                float flDistanceNextSegment = vecThisToNext.Length();
                flCurrentPos += flDistanceNextSegment;

                while ( flNextAddPos < flCurrentPos )
                {
                    if (iAddedPositions >= iTotalPositions)
                        return;

                    float flFractionOnSegment = (flCurrentPos - flNextAddPos) / flDistanceNextSegment;
                    //Console.WriteLine(flFractionOnSegment);
                    trailPositions[iAddedPositions++] = Positions[iIndex] + (vecThisToNext * flFractionOnSegment);
                    flNextAddPos += flSpacing;
                }

            }
        }
        
        // Debug rendering
        public void Draw(RenderTarget target, RenderStates states)
        {
            VertexArray line = new VertexArray(PrimitiveType.LinesStrip, (uint)Positions.Count);

            for (int i = 0; i < Positions.Count; i++)
            {
                float flAlpha = Math.Max( 50, 255.0f * ((float)i / (float)Positions.Count) );
                line[(uint)i] = new Vertex(Positions[i], new Color(255, 255, 255, (byte)flAlpha));

            }

            target.Draw(line, states);
        }

        // PRIVATE methods
        private void CalculateLength()
        {
            Length = 0.0f;

            // Only valid length if we have > 1 point
            if (Positions.Count > 1)
            {
                Vector2f vecLastPos = Positions[0];

                for (int i = 1; i < Positions.Count; i++)
                {
                    Length += (Positions[i] - vecLastPos).Length();
                    vecLastPos = Positions[i];
                }
            }
        }
        private void PruneLine()
        {
            // No need if we're below maxlength
            if (Length <= MaxLength)
                return;

            float postSnipLength = Length;
            int iLinesToRemove = 0;
            Vector2f vecLastPos = Positions[0];

            for (int i = 1; i < Positions.Count; i++)
            {
                iLinesToRemove++;
                postSnipLength -= (Positions[i] - vecLastPos).Length();

                // Check if we're now satisfied with the length
                if (postSnipLength < MaxLength)
                    break;
            }

            Positions.RemoveRange(0, iLinesToRemove);
            CalculateLength();
        }

        private float MaxLength; // Will start removing entries if length is above this
        private float Length; // Length of the line
        public List<Vector2f> Positions; // Position archive

    }
}
