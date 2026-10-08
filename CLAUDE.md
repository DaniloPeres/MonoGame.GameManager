# MonoGame.GameManager

## Text rendering: FontStashSharp only

- Never use `Microsoft.Xna.Framework.Graphics.SpriteFont`, `.spritefont` files, `LoadSpriteFont` or `SpriteBatch.DrawString(SpriteFont, ...)`, in the library or in the samples.
- Text uses `FontStashSharp.MonoGame`: controls take a `FontStashSharp.SpriteFontBase` (property `Font`, setter `SetFont`).
- Fonts are TrueType/OpenType files copied to the content folder (`/copy:Fonts/<file>.ttf` in the `.mgcb`) and loaded with `IContentLoader.LoadFontSystem(...)` or `IContentLoader.LoadFont(path, size)`.
- Outlines go through `TextOutline.Draw` (colored round stroke rasterized by `TextOutline.GlyphRenderer`, set on the font systems of `LoadFontSystem`); never use FontStashSharp's built-in `FontSystemEffect.Stroked` renderer directly (its stroke is always black and made of 4 offset copies).
- Controls draw text with `FontScaling.Resolve(font, ref scale, ref origin)` followed by `font.DrawText(...)`, so scaled text is rasterized at its final size. Measure with `font.MeasureString` and use `font.LineHeight` for the line height.
- The samples use `Roboto-Regular.ttf` (Apache 2.0, license next to the file). The demos also use 17 display fonts (OFL or Apache 2.0, licenses next to them), loaded by `ContentHandler.Fonts` with Roboto as fallback.

## Conventions

- Code, comments and docs are written in English.
