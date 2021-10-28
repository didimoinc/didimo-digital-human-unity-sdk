using System;
using UnityEngine;

namespace DigitalSalmon.Extensions
{
    [Flags]
    public enum RectAnchor
    {
        Center = 1,
        Top    = 2,
        Right  = 4,
        Bottom = 8,
        Left   = 16
    }

    public static class RectExtensions
    {
        //-----------------------------------------------------------------------------------------
        // Size Methods:
        //-----------------------------------------------------------------------------------------

        public static Rect WithWidth(this Rect self, float size, RectAnchor anchor = RectAnchor.Left) => WithTrimX(self, self.width - size, anchor);
        public static Rect WithHeight(this Rect self, float size, RectAnchor anchor = RectAnchor.Top) => WithTrimY(self, self.height - size, anchor);

        public static Rect WithSize(this Rect self, float size, RectAnchor anchor = RectAnchor.Top | RectAnchor.Left) => WithTrim(self, new Vector2(self.size.x - size, self.size.y - size), anchor);

        public static Rect WithSize(this Rect self, Vector2Int size, RectAnchor anchor = RectAnchor.Top | RectAnchor.Left) => WithTrim(self, self.size - size, anchor);
        public static Rect WithSize(this Rect self, Vector2 size, RectAnchor anchor = RectAnchor.Top | RectAnchor.Left) => WithTrim(self, self.size - size, anchor);

        public static Rect WithTrimX(this Rect self, float trim, RectAnchor anchor = RectAnchor.Left) => WithTrim(self, new Vector2(trim, 0), anchor);
        public static Rect WithTrimY(this Rect self, float trim, RectAnchor anchor = RectAnchor.Top) => WithTrim(self, new Vector2(0, trim), anchor);

        public static Rect WithTrim(this Rect self, float trim, RectAnchor anchor = RectAnchor.Left) => WithTrim(self, new Vector2(trim, trim), anchor);
        public static Rect WithTrim(this Rect self, Vector2Int trim, RectAnchor anchor = RectAnchor.Top) => WithTrim(self, new Vector2(trim.x, trim.y), anchor);

        public static Rect WithTrim(this Rect self, Vector2 trim, RectAnchor anchor = RectAnchor.Center)
        {
            self.size -= trim;

            if (anchor.HasFlag(RectAnchor.Center)) { self.position += trim / 2; }

            if (anchor.HasFlag(RectAnchor.Right)) self.x = self.x + trim.x;
            if (anchor.HasFlag(RectAnchor.Bottom)) self.y = self.y + trim.y;

            return self;
        }

        //-----------------------------------------------------------------------------------------
        // Padding Methods:
        //-----------------------------------------------------------------------------------------

        public static Rect WithPadX(this Rect self, float padding, RectAnchor anchor = RectAnchor.Center) => WithPadding(self, new Vector2(padding, 0), anchor);
        public static Rect WithPadY(this Rect self, float padding, RectAnchor anchor = RectAnchor.Center) => WithPadding(self, new Vector2(0, padding), anchor);
        public static Rect WithPadding(this Rect self, float padding, RectAnchor anchor = RectAnchor.Center) => WithPadding(self, new Vector2(padding, padding), anchor);
        public static Rect WithPadding(this Rect self, Vector2Int padding, RectAnchor anchor = RectAnchor.Center) => self.WithPadding(new Vector2(padding.x, padding.y), anchor);

        public static Rect WithPadding(this Rect self, Vector2 padding, RectAnchor anchor = RectAnchor.Center)
        {
            self.size -= padding * 2;

            if (anchor.HasFlag(RectAnchor.Center)) { self.position += padding; }

            if (anchor.HasFlag(RectAnchor.Right)) self.x = self.x + padding.x * 2;
            if (anchor.HasFlag(RectAnchor.Bottom)) self.y = self.y + padding.y * 2;

            return self;
        }

        //-----------------------------------------------------------------------------------------
        // Position Methods:
        //-----------------------------------------------------------------------------------------

        public static Rect WithPositionX(this Rect self, int position, RectAnchor anchor = RectAnchor.Left)
        {
            self.position = new Vector2(position, self.position.y);

            if (anchor.HasFlag(RectAnchor.Center)) { self.position = self.position - new Vector2(0, self.size.y / 2); }

            if (anchor.HasFlag(RectAnchor.Bottom)) { self.position = self.position - new Vector2(0, self.size.y); }

            return self;
        }

        public static Rect WithPositionY(this Rect self, int position, RectAnchor anchor = RectAnchor.Left)
        {
            self.position = new Vector2(self.position.x, position);

            if (anchor.HasFlag(RectAnchor.Center)) { self.position = self.position - new Vector2(self.size.x / 2, 0); }

            if (anchor.HasFlag(RectAnchor.Right)) { self.position = self.position - new Vector2(self.size.x, 0); }

            return self;
        }

        public static Rect WithPosition(this Rect self, Vector2 position, RectAnchor anchor = RectAnchor.Left | RectAnchor.Top)
        {
            self.position = position;

            if (anchor.HasFlag(RectAnchor.Center)) { self.position -= self.size / 2; }

            if (anchor.HasFlag(RectAnchor.Right)) self.x = self.x - self.width;
            if (anchor.HasFlag(RectAnchor.Bottom)) self.y = self.y - self.height;

            return self;
        }

        /// <summary>
        /// Offsets the rect by 'offset'.
        /// </summary>
        public static Rect WithOffset(this Rect self, Vector2 offset) => WithOffset(self, new Vector2Int((int) offset.x, (int) offset.y));

        public static Rect WithOffset(this Rect self, Vector2Int offset)
        {
            self.position += offset;
            return self;
        }

        /// <summary>
        /// Offsets the x position of the rect by 'offset' (anchored left).
        /// </summary>
        public static Rect WithOffsetX(this Rect self, float offset) => WithOffsetX(self, (int) offset);

        public static Rect WithOffsetX(this Rect self, int offset)
        {
            self.x += offset;
            return self;
        }

        /// <summary>
        /// Offsets the y position of the rect by 'offset' (anchored top).
        /// </summary>
        public static Rect WithOffsetY(this Rect self, float offset) => WithOffsetY(self, (int) offset);

        public static Rect WithOffsetY(this Rect self, int offset)
        {
            self.y += offset;
            return self;
        }

        //-----------------------------------------------------------------------------------------
        // Helper Methods:
        //-----------------------------------------------------------------------------------------

        /// <summary>
        /// Split the rect into 'count' columns and return the column rect for column[index].
        /// </summary>
        public static Rect SplitX(this Rect self, int count, int index)
        {
            int width = (int) (self.width / count);
            self.width = width;
            self.x += width * index;
            return self;
        }

        /// <summary>
        /// Split the rect into 'count' rows and return the row rect for row[index].
        /// </summary>
        public static Rect SplitY(this Rect self, int count, int index)
        {
            int height = (int) (self.height / count);
            self.height = height;
            self.y += height * index;
            return self;
        }

        /// <summary>
        /// Returns a rect with a given 'aspectRatio' which will fit within this rect.
        /// </summary>
        public static Rect FitAspect(this Rect self, float aspectRatio)
        {
            Rect source = self;
            float newWidth = source.width * aspectRatio;
            float newHeight = newWidth / aspectRatio;

            if (newWidth > source.width)
            {
                newWidth = source.width;
                newHeight = newWidth / aspectRatio;
            }

            if (newHeight > source.height)
            {
                newHeight = source.height;
                newWidth = newHeight * aspectRatio;
            }

            return self.WithSize(new Vector2(newWidth, newHeight), RectAnchor.Center);
        }

        /// <summary>
        /// Returns a rect with a given 'aspectRatio' which will fill (Overflow) this rect.
        /// </summary>
        public static Rect FillAspect(this Rect self, float aspectRatio)
        {
            if (aspectRatio > 1)
            {
                float widthDelta = self.width - self.height * aspectRatio;
                self = self.WithTrimX(widthDelta).WithOffsetX(widthDelta / 2);
            }
            else
            {
                float heightDelta = self.height - self.width / aspectRatio;
                self = self.WithTrimY(heightDelta).WithOffsetY(heightDelta / 2);
            }

            return self;
        }

        /// <summary>
        /// Returns an identical looking rect with a strictly positive width and height.
        /// </summary>
        public static Rect Absolute(this Rect self)
        {
            if (self.width < 0)
            {
                self.x += self.width;
                self.width *= -1;
            }

            if (self.height < 0)
            {
                self.y += self.height;
                self.height *= -1;
            }

            return self;
        }

        // Zeroing

        public static Rect WithZeroPosition(this Rect self)
        {
            self.position = Vector2.zero;
            return self;
        }

        public static Rect WithZeroSize(this Rect self)
        {
            self.size = Vector2.zero;
            return self;
        }
    }
}