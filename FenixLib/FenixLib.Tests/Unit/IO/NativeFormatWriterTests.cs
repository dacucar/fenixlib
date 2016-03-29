﻿/*  Copyright 2016 Darío Cutillas Carrillo
*
*   Licensed under the Apache License, Version 2.0 (the "License");
*   you may not use this file except in compliance with the License.
*   You may obtain a copy of the License at
*
*       http://www.apache.org/licenses/LICENSE-2.0
*
*   Unless required by applicable law or agreed to in writing, software
*   distributed under the License is distributed on an "AS IS" BASIS,
*   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
*   See the License for the specific language governing permissions and
*   limitations under the License.
*/
using NUnit.Framework;
using Rhino.Mocks;
using System;
using System.IO;
using System.Linq;
using FenixLib.Core;
using FenixLib.IO;

namespace FenixLib.Tests.Unit.IO
{
    [TestFixture ( Category = "Unit" )]
    public class NativeFormatWriterTests
    {

        private NativeFormatWriter formatWriter;
        // Keeps track of the bytes written to the fake stream
        // NOTE: As per https://msdn.microsoft.com/en-us/library/24e33k1w%28v=vs.110%29.aspx
        // BinaryWriter implementation (superclass to fakeWriter) uses little-endian format
        private byte[] memory;

        [SetUp]
        public void SetUp ()
        {
            // Stream stub that memorizes the bytes written to the field every
            // time Write() overloads are called
            var streamStub = MockRepository.GenerateStub<Stream> ();

            streamStub.Stub ( _ => _.CanWrite ).Return ( true );

            streamStub.Stub ( _ => _.Write (
                Arg<byte[]>.Is.NotNull,
                Arg<int>.Is.GreaterThanOrEqual ( 0 ),
                Arg<int>.Is.GreaterThan ( 0 ) ) )
            .WhenCalled ( a =>
            {
                var bytes = a.Arguments[0] as byte[];
                var tmp = new byte[( int ) a.Arguments[2]];
                Array.Copy ( bytes, tmp, tmp.Length );
                ResizeMemory ( tmp );
            } );

            streamStub.Stub ( _ => _.WriteByte ( Arg<byte>.Is.Anything ) )
            .WhenCalled ( _ =>
            {
                var bytes = new byte[] { ( byte ) _.Arguments[0] };
                ResizeMemory ( bytes );
            } );

            formatWriter = new NativeFormatWriter ( streamStub );
        }

        [TearDown]
        public void TearDown()
        {
            memory = null;
        }

        [Test]
        public void Construct_NullArgument_ThrowsException ()
        {
            Assert.That ( () => new NativeFormatWriter ( null ),
                Throws.ArgumentNullException );
        }

        [Test]
        public void WriteAsciiZ_StringSorterThanMaxLength_ByteAffterCharacterIs0 ()
        {
            formatWriter.WriteAsciiZ ( "a", 2 );
            Assert.That ( memory[1], Is.EqualTo ( 0 ) );
        }

        [Test]
        public void WriteAsciiZ_NullString_ThrowsException ()
        {
            Assert.That ( () => formatWriter.WriteAsciiZ ( null, 2 ),
                Throws.ArgumentNullException );
        }

        [Test]
        public void WriteAsciiZ_NegativeMaxLength_ThrowsException ()
        {
            Assert.That ( () => formatWriter.WriteAsciiZ ( "a text", -2 ),
                Throws.InstanceOf<ArgumentOutOfRangeException> () );
        }

        [Test]
        public void WriteExtendedGlyphInfo_Test ()
        {
            var glyphInfo = new NativeFormat.GlyphInfo (
                width: 0x10, 
                height: 0x20, 
                xAdvance: 0x30, 
                yAdvance: 0x40, 
                xOffset:0x50, 
                yOffset:0x60, 
                fileOffset:0x70 );

            var expectedBytes = new byte[]
            {
                0x10, 0, 0, 0,
                0x20, 0, 0, 0,
                0x30, 0, 0, 0,
                0x40, 0, 0, 0,
                0x50, 0, 0, 0,
                0x60, 0, 0, 0,
                0x70, 0, 0, 0
            };

            formatWriter.WriteExtendedGlyphInfo ( ref glyphInfo );

            Assert.That ( memory, Is.EqualTo ( expectedBytes ) );
        }

        [Test]
        public void WriteLegacyFntGlyphInfo_ValidGlyph_Works ()
        {
            var glyphInfo = new NativeFormat.GlyphInfo (
                0x10, 0x20, 0x30, 0x40);
            var expectedBytes = new byte[]
            {
                0x10, 0, 0, 0,
                0x20, 0, 0, 0,
                0x30, 0, 0, 0,
                0x40, 0, 0, 0
            };


            formatWriter.WriteLegacyGlyphInfo ( ref glyphInfo );

            Assert.That ( memory, Is.EqualTo ( expectedBytes ) );
        }

        [Test]
        public void WritePalette_NullPalette_Throws ()
        {
            Assert.That ( () => formatWriter.Write ( null as Palette ),
                Throws.ArgumentNullException );
        }

        [Test]
        public void WritePalette_ValidPalette_Works ()
        {
            var paletteStub = MockRepository.GenerateStub<Palette> ();
            var paletteColors = new PaletteColor[256];
            var expectedEncodedBytes = new byte[256 * 3];

            for ( int i = 0 ; i < 256 ; i++ )
            {
                // A sample color that will be in the stub palette
                var color = new PaletteColor ( i, i / 2, i / 3 );
                paletteColors[i] = color;
                paletteStub[i] = color;

                // Color components are to be encoded in 6bits
                expectedEncodedBytes[i * 3 + 0] = ( byte ) ( ( i ) >> 2 );
                expectedEncodedBytes[i * 3 + 1] = ( byte ) ( ( i / 2 ) >> 2 );
                expectedEncodedBytes[i * 3 + 2] = ( byte ) ( ( i / 3 ) >> 2 );
            }
            // Stub the Colors property
            paletteStub.Stub ( _ => _.Colors ).Return ( paletteColors );

            formatWriter.Write ( paletteStub );

            Assert.That ( memory, Is.EqualTo ( expectedEncodedBytes ) );
        }

        [Test]
        public void WritePivotPoints_ValidPivotPoint_Works ()
        {
            var pivotPoint = new PivotPoint ( 0, 0x01AA, 0x02BB );
            var expectedEncodedBytes = new byte[] { 0xAA, 0x01, 0xBB, 0x02 };

            formatWriter.Write ( pivotPoint );

            Assert.That ( () => memory, Is.EqualTo ( expectedEncodedBytes ) );
        }

        [Test]
        public void WriteReservedPaletteGammaSection_Works ()
        {
            formatWriter.WriteReservedPaletteGammaSection ();
            
            // There are 16 gamma sections and they occupy 36bytes each.
            Assert.That ( memory.Length, Is.EqualTo ( 16 * 36 ) );
            // Every 16th byte needs to be 8, 16 or 32 for the format to be 
            // compatible with DIV games studio
            Assert.That ( memory.Where ( ( x, i ) => i % 36 == 0 ), 
                Is.All.EqualTo(8).Or.EqualTo(16).Or.EqualTo(32) );
        }

        // Resizes currentMemory to hold bytes and copies the contents
        // of bytes to it
        private void ResizeMemory ( byte[] bytes )
        {
            int destIndex;

            if ( memory == null )
            {
                destIndex = 0;
                memory = new byte[bytes.Length];
            }
            else
            {
                destIndex = memory.Length;
                var oldMemory = memory;
                memory = new byte[memory.Length + bytes.Length];
                Array.Copy ( oldMemory, memory, oldMemory.Length );
            }

            Array.Copy ( bytes, 0, memory, destIndex, bytes.Length );
        }
    }
}
