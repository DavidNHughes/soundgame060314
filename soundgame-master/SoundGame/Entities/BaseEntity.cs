using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFML.Graphics;

namespace SoundGame.Entities
{
    public interface IBaseEntity : Drawable
    {
        void Update( double dFrameTime );
    }
}
