using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoundGame
{
    public static class ContentManager
    {
        public static Texture LoadTexture( String sTexture )
        {
            // If our loaded materials dictionary has this texture, return it instead.
            if (LoadedMaterials.ContainsKey(sTexture))
            {
                return LoadedMaterials[sTexture];
            }

            // Not loaded, load it from disk.
            Texture newTexture = new Texture(sTexture);
            LoadedMaterials.Add(sTexture, newTexture);
            return newTexture;
        }

        private static IDictionary<string, Texture> LoadedMaterials = new Dictionary<string, Texture>();
    }
}
