using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Abstracts;
using MonoGame.GameManager.Controls.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoGame.GameManager.Controls
{
    public class Panel : ContainerAbstract<Panel>
    {

        public Panel() { }

        public Panel(Rectangle destinationRectangle) : this(destinationRectangle.Location.ToVector2(), destinationRectangle.Size.ToVector2()) { }

        public Panel(Vector2 position, Vector2 size)
            : base(position, size) { }
    }
}
