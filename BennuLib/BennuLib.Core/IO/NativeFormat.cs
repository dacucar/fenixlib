using System;
using static System.Collections.StructuralComparisons;

namespace BennuLib.IO
{
	public static class NativeFormat
	{
		public static readonly byte[] Terminator = {
			0x1a,
			0xd,
			0xa,
			0x0
		};
	    
        /// <summary>
        /// The size of the color palette area, in bytes
        /// </summary>
		public const int PaletteBytesSize = 768;

        /// <summary>
        /// The size of the gamma color area, in bytes
        /// </summary>
		public const int ReservedBytesSize = 576;
		
        /// <summary>
        /// Bit mask used to separate the number of pivot points from the animation flags
        /// </summary>
		public const int PivotPointsNumberBitMask = 0xfff;
		
        /// <summary>
        /// Bit mask used to get the animation bit flag. Animation is not supported.
        /// </summary>
		public const int AnimationFlagBitMask = 0x1000;


        /// <summary>
        /// Represents the header of the native formats, i.e. a section describing type
        /// type of file (graphic, graphic collection, font or palettes), and the depth
        /// of the graphic information (1, 8, 16 or 32bpp).
        /// </summary>
		public sealed class Header
		{

            private string _magic;
			public string Magic { get { return _magic; } }

            private int _lastByte;
			public int LastByte { get { return _lastByte; } }

			private readonly byte[] _terminator = new byte[5];
            public byte[] Terminator { get { return _terminator;  } }
			
			public Header(string magic, byte[] terminator, int lastByte)
			{
				_magic = magic.ToLower();
				_lastByte = lastByte;
				_terminator = terminator;
			}

			public bool IsTerminatorValid()
            {
                    return StructuralEqualityComparer.Equals(NativeFormat.Terminator, Terminator);
			}

            /// <summary>
            /// All native format's magic follow the pattern 'aXY' where XY indicates, for
            /// non 8bpp formats, the depth (01, 16 or 32). For 8bpp formats, XY ar two 
            /// characters.
            /// </summary>
			public int Depth
            {
                // TODO: Perhaps it is wiser to have a ParseDepth instead of a property...
                // TODO: Definitely, as the Depth does not apply for the Fnt FNX format, for instance
                get
                {
                    int depth;

                    if (int.TryParse(Magic.Substring(1, 2), out depth))
                        return depth;
                    else
                        return 8;
                }
            }

		}

		public struct GlyphInfo
		{
            /// <summary>
            /// The width of the character's glyph
            /// </summary>
			public int Width { get; }
            /// <summary>
            /// The height of the characters's glyph
            /// </summary>
			public int Height { get; }
            /// <summary>
            /// Displacement in the x-axis from the left side
            /// </summary>
            public int XOffset { get; }
            /// <summary>
            /// Displacement in the Y-axis from the top side
            /// </summary>
			public int YOffset { get; }
            /// <summary>
            /// 
            /// </summary>
            public int XAdvance { get; }
            /// <summary>
            /// 
            /// </summary>
            public int YAdvance { get; }
            /// <summary>
            /// The byte-location of the glyph's graphic data (pixels) in the
            /// file.
            /// </summary>
            public int FileOffset { get; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="width"></param>
            /// <param name="height"></param>
            /// <param name="yOffset"></param>
            /// <param name="fileOffset"></param>
            public GlyphInfo(int width, int height, int yOffset, int fileOffset)
			{
                Width = width;
                Height = height;
                XOffset = 0;
                YOffset = yOffset;
                XAdvance = width;
                YAdvance = height + yOffset;
                FileOffset = fileOffset;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="width"></param>
            /// <param name="height"></param>
            /// <param name="xOffset"></param>
            /// <param name="yOffset"></param>
            /// <param name="xAdvance"></param>
            /// <param name="yAdvance"></param>
            /// <param name="fileOffset"></param>
            public GlyphInfo(int width, int height, int xOffset, 
                int yOffset, int xAdvance, int yAdvance, int fileOffset)
            {
                Width = width;
                Height = height;
                XOffset = xOffset;
                YOffset = yOffset;
                XAdvance = xAdvance;
                YAdvance = yAdvance; 
                FileOffset = fileOffset;
            }
		}
	}
}
