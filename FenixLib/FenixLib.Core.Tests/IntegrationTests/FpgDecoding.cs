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

using FenixLib.Core;
using FenixLib.IO;

namespace FenixLib.Tests
{
    [TestFixture, Category("Integration")]
    public class FpgDecoding
    {
        private SpriteAsset DecodeFpg ( string path, int depth )
        {
            SpriteAsset fpg = File.LoadFpg ( path );

            Assert.AreEqual ( depth, fpg.GraphicFormat );
            Assert.AreEqual ( 3, fpg.Sprites.Count );
            Assert.IsNotNull ( fpg.Sprites );

            // Validate the dimensions
            Assert.AreEqual ( 304, fpg[1].Width );
            Assert.AreEqual ( 256, fpg[100].Width );
            Assert.AreEqual ( 256, fpg[500].Width );
            Assert.AreEqual ( 315, fpg[1].Height );
            Assert.AreEqual ( 256, fpg[100].Height );
            Assert.AreEqual ( 256, fpg[500].Height );

            // Validate description
            Assert.AreEqual ( "hippo", fpg[1].Description );
            Assert.AreEqual ( "parrot", fpg[100].Description );
            Assert.AreEqual ( "penguin", fpg[500].Description );

            // Validate control points
            Assert.AreEqual ( 2, fpg[500].PivotPoints.Count );

            return fpg;
        }

        [Test]
        public void DecodeFpg_8bppUncompressed ()
        {
            var fpg = DecodeFpg ( "./Fpg/8bpp-uncompressed.fpg", 8 );
            Assert.IsNotNull ( fpg.Palette );
        }

        [Test]
        public void DecodeFpg_16bppUncompressed ()
        {
            var fpg = DecodeFpg ( "./Fpg/16bpp-uncompressed.fpg", 16 );
        }

        [Test]
        public void DecodeFpg_32bppUncompressed ()
        {
            var fpg = DecodeFpg ( "./Fpg/32bpp-uncompressed.fpg", 32 );
        }

        [Test]
        public void DecodeFpg_8bppCompressed ()
        {
            var fpg = DecodeFpg ( "./Fpg/8bpp-compressed.fpg", 8 );
            Assert.IsNotNull ( fpg.Palette );
        }

        [Test]
        public void DecodeFpg_16bppCompressed ()
        {
            var fpg = DecodeFpg ( "./Fpg/16bpp-compressed.fpg", 16 );
        }

        [Test]
        public void DecodeFpg_32bppCompressed ()
        {
            var fpg = DecodeFpg ( "./Fpg/32bpp-compressed.fpg", 32 );
        }

        [Test]
        public void DecodeFpg_1bppCompressed ()
        {
            SpriteAsset fpg = File.LoadFpg ( "./Fpg/1bpp-compressed.fpg" );
            Assert.AreEqual ( fpg[1].Width, 10 );
            Assert.AreEqual ( fpg[1].Height, 10 );
            Assert.AreEqual ( fpg.GraphicFormat, 1);
            Assert.AreEqual ( fpg.Sprites.Count, 1 );
        }
    }
}