namespace MonoGame.GameManager.Controls.Sprites
{
    public class SpriteAnimationCycle
    {
        public const string DefaultCycleName = "default";
        public string Name { get; set; }
        public SpriteAnimationFrame[] Frames { get; set; }

        public SpriteAnimationCycle(string name, SpriteAnimationFrame[] frames)
        {
            Name = name;
            Frames = frames;
        }
    }
}
