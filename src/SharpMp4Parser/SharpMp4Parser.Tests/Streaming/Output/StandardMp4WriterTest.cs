﻿using SharpMp4Parser.Java;
using SharpMp4Parser.Streaming;
using SharpMp4Parser.Streaming.Input.H264;
using SharpMp4Parser.Streaming.Output.MP4;

namespace SharpMp4Parser.Tests.Streaming.Output
{
    [TestClass]
    public class StandardMp4WriterTest
    {
        [TestMethod]
        public async Task testMuxing()
        {
            using (MemoryStream h264Ms = new MemoryStream())
            {
                FileStream h264Fis = File.OpenRead("tos.h264");
                h264Fis.CopyTo(h264Ms);
                h264Ms.Position = 0;

                var h264Stream = new ByteStream(h264Ms.ToArray());
                H264AnnexBTrack b = new H264AnnexBTrack(h264Stream);
                ByteStream baos = new ByteStream();
                StandardMp4Writer writer = new StandardMp4Writer(new List<StreamingTrack>() { b }, Channels.newChannel(baos));
                //FragmentedMp4Writer writer = new FragmentedMp4Writer(new List<StreamingTrack> { b }, Channels.newChannel(baos));
                await Task.Run(() => b.call());
                writer.close();

                //using (var file = File.Create("test.mp4"))
                //{
                //    baos.position(0);
                //    baos.CopyTo(file);
                //}

                //Walk.through(isoFile);
                //List<Sample> s = new Mp4SampleList(1, isoFile, new InMemRandomAccessSourceImpl(baos.toByteArray()));
                //for (Sample sample : s) {
                //            System.err.println("s: " + sample.getSize());
                //          sample.asByteBuffer();
                //    }

                h264Fis.Close();
            }
        }
    }
}