using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Abstracts;
using MonoGame.GameManager.Enums;
using System;
using System.Linq;
using System.Text;

namespace MonoGame.GameManager.Controls
{
    public class MultiLineLabel : ScalableControlAbstract<MultiLineLabel>
    {
        private Panel container;

        private string text;
        public string Text
        {
            get => text;
            set
            {
                text = value;
                MarkAsDirty();
            }
        }

        private SpriteFont spriteFont;
        public SpriteFont SpriteFont
        {
            get => spriteFont;
            set
            {
                spriteFont = value;
                MarkAsDirty();
            }
        }

        private TextAlign textAlign;
        public TextAlign TextAlign
        {
            get => textAlign;
            set
            {
                textAlign = value;
                MarkAsDirty();
            }
        }

        private int textBoxWidth;
        public int TextBoxWidth
        {
            get => textBoxWidth;
            set
            {
                textBoxWidth = value;
                MarkAsDirty();
            }
        }

        private Color color;
        public override Color Color
        {
            get => color;
            set
            {
                color = value;
                MarkAsDirty();
            }
        }

        public MultiLineLabel(SpriteFont spriteFont, string text, Vector2 position, Color color, int textBoxWidth)
        {
            SpriteFont = spriteFont;
            Text = text;
            SetPosition(position);
            Color = color;
            TextBoxWidth = textBoxWidth;

            container = new Panel();
        }

        protected override void UpdateDestinationRects()
        {
            // TODO improve multi-text label to only re-create labels if it is necessary
            IsDirty = false;
            CreateLabels();
            MarkSizeAsDirty();
            base.UpdateDestinationRects();

            container.Size = Size;
            container.SetPosition(GetPosition());
            container.OnBeforeDraw();
        }

        public MultiLineLabel SetTextAlign(TextAlign textAlign)
        {
            TextAlign = textAlign;
            return this;
        }

        private void CreateLabels()
        {
            var textRows = WrapText(Text).Split('\n').ToList();

            container.ClearChildren();
            textRows.ForEach(textRow => container.AddChild(CreateLabel(textRow)));

            CalculateLabelsPositions();
        }

        private Label CreateLabel(string textRow)
        {
            Anchor labelAnchor;
            switch (TextAlign) {
                case TextAlign.Center:
                    labelAnchor = Anchor.TopCenter;
                    break;
                case TextAlign.Right:
                    labelAnchor = Anchor.TopRight;
                    break;
                default:
                    labelAnchor = Anchor.TopLeft;
                    break;
            }
            return new Label(spriteFont, textRow, Vector2.One, Color)
                .SetScale(NestedScale)
                .SetAnchor(labelAnchor);
        }

        private void CalculateLabelsPositions()
        {
            var posY = 0f;
            var parentPos = Parent.DestinationRectangle.Location;

            container.IterateChildren(child =>
            {
                var label = child as Label;
                label.SetPosition(new Vector2(0, posY));
                posY += label.Size.Y;
            }, false);
        }

        protected override Vector2 CalculateSize()
        {
            var size = new Vector2(0);

            container.IterateChildren(child =>
            {
                // do not apply scale in the child size
                var childSize = child.Size / NestedScale;

                size.X = Math.Max(size.X, childSize.X);
                size.Y += childSize.Y;
            });

            return size;
        }

        private string WrapText(string text)
        {
            var wrapTextOutput = new StringBuilder();
            var charSpaceWidth = spriteFont.MeasureString(" ").X * NestedScale.X;

            var textRows = text.Replace("\\n", "\n").Replace("\\N", "\n").Split(new string[] { "\n" }, StringSplitOptions.None);

            var textBoxWidthNestedScale = TextBoxWidth * Parent.NestedScale.X;

            for (int i = 0; i < textRows.Count(); i++)
            {
                var textRow = textRows[i];

                var words = textRow.Split(' ').ToList();
                var rowWidth = 0f;

                var isFristWord = true;
                words.ForEach(word =>
                {
                    var wordSize = spriteFont.MeasureString(word) * NestedScale;

                    // check with this word makes the row larger than the text box width
                    if (rowWidth + wordSize.X < textBoxWidthNestedScale)
                    {
                        // append the 
                        rowWidth += wordSize.X + charSpaceWidth;
                    }
                    else
                    {
                        if (!isFristWord)
                            wrapTextOutput.Append("\n");

                        rowWidth = wordSize.X + charSpaceWidth;
                    }

                    wrapTextOutput.Append(word + " ");
                    isFristWord = false;
                });

                if (i + 1 < textRows.Count())
                    wrapTextOutput.Append("\n");
            }

            // remove spaces at the end of the rows
            var textOuput = wrapTextOutput.ToString();
            textOuput = textOuput.Replace(" \n", "\n").Trim();
            return textOuput;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            container.Draw(spriteBatch);
        }
    }
}
