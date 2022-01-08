using MonoGame.GameManager.Enums;
using System;

namespace MonoGame.GameManager.Models
{
    public class ContentAsset
    {
        public object Asset { get; set; }
        public DateTime TimeUsed { get; set; }
        public ContentAssetState State { get; set; }
    }
}
