namespace BlockSmash
{
    using CahtFramework;
    using System.Collections.Generic;
    using UnityEngine;

    public class ThemeColor : IdentifiedObject
    {
        [SerializeField] private List<Sprite> sprites = new();

        public IReadOnlyList<Sprite> Sprites => this.sprites;

        public Sprite GetRandomSprite()
        {
            if (this.sprites == null || this.sprites.Count == 0) return null;
            return this.sprites[Random.Range(0, this.sprites.Count)];
        }
    }
}