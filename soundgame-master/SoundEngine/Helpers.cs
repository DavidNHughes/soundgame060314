using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using SFML.Audio;
using SFML.Graphics;
using SFML.Window;

namespace SoundEngine
{
    public static class VectorExtension
    {
        public static float LengthSq(this Vector2f v)
        {
            return v.X * v.X + v.Y * v.Y;
        }

        public static float Length(this Vector2f v)
        {
            return (float)Math.Sqrt( v.LengthSq() );
        }

        public static Vector2f Normalize(this Vector2f v)
        {
            var length = v.Length();
            if (length == 0) return new Vector2f();
            return v / length;
        }

        public static float Angle(this Vector2f v, bool radians = false)
        {
            var value = (float)Math.Atan2(v.Y, v.X);
            if (!radians) value *= 180 / (float)Math.PI;
            return value;
        }
    }

    public static class AngleHelper
    {
        public static double Average(double angle1, double angle2)
        {
            double x_part = 0, y_part = 0;

            x_part += Math.Cos(angle1 * Math.PI / 180);
            y_part += Math.Sin(angle1 * Math.PI / 180);

            x_part += Math.Cos(angle2 * Math.PI / 180);
            y_part += Math.Sin(angle2 * Math.PI / 180);

            return Math.Atan2(y_part / 2, x_part / 2) * 180 / Math.PI;
        }

        public static Vector2f ToVector( float flAng )
        {
            return new Vector2f((float)Math.Cos(flAng * Math.PI / 180), (float)Math.Sin(flAng * Math.PI / 180));
        }

        public static double Approach(float value, float target, float speed )
        {
            float delta = target - value;

            // Speed is assumed to be positive
            if (speed < 0)
                speed = -speed;

            if (delta < -180)
                delta += 360;
            else if (delta > 180)
                delta -= 360;

            if (delta > speed)
                value += speed;
            else if (delta < -speed)
                value -= speed;
            else
                value = target;

            return value;
        }
    }

    
    public static class SoundMath
    {
        public static float Sqrt2Approx( float z )
        {
            if (z == 0) return 0;
            FloatIntUnion u;
            u.tmp = 0;
            u.f = z;
            u.tmp -= 1 << 23; /* Subtract 2^m. */
            u.tmp >>= 1; /* Divide by 2. */
            u.tmp += 1 << 29; /* Add ((b + 1) / 2) * 2^m. */
            return u.f;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct FloatIntUnion
        {
            [FieldOffset(0)]
            public float f;

            [FieldOffset(0)]
            public int tmp;
        }
    }
}
