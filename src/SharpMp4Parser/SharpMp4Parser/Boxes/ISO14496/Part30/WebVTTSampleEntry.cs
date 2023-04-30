﻿namespace SharpMp4Parser.Boxes.ISO14496.Part30
{
    /**
     * Sample Entry for WebVTT subtitles.
     * <pre>
     * class WVTTSampleEntry() extends PlainTextSampleEntry (‘wvtt’){
     *   WebVTTConfigurationBox config;
     *   WebVTTSourceLabelBox label; // recommended
     *   MPEG4BitRateBox (); // optional
     * }
     * </pre>
     */
    public class WebVTTSampleEntry : AbstractSampleEntry
    {
        public const string TYPE = "wvtt";

        public WebVTTSampleEntry() : base(TYPE)
        { }

        public override void parse(ReadableByteChannel dataSource, ByteBuffer header, long contentSize, BoxParser boxParser)
        {
            initContainer(dataSource, contentSize, boxParser);
        }

        public override void getBox(WritableByteChannel writableByteChannel)
        {
            writableByteChannel.write(getHeader());
            writeContainer(writableByteChannel);
        }

        public WebVTTConfigurationBox getConfig()
        {
            return Path.getPath(this, "vttC");
        }

        public WebVTTSourceLabelBox getSourceLabel()
        {
            return Path.getPath(this, "vlab");
        }
    }
}