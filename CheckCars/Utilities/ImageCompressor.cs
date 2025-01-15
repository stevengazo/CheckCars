using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCars.Utilities
{
    public static class ImageCompressor
    {
        public async static Task CompressImageAsync(string InputImage, int Quality)
        {
            using( Bitmap mybitmab = new Bitmap(@InputImage))
            {
                ImageCodecInfo jpgEncoder = GetEncoder(System.Drawing.Imaging.ImageFormat.Jpeg);
                System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
                EncoderParameters myEncoderParameters = new EncoderParameters(1);
                EncoderParameter parameter = new EncoderParameter(myEncoder,Quality);

                myEncoderParameters.Param[0] = parameter;
               
                mybitmab.Save(InputImage,jpgEncoder,myEncoderParameters);

            }
        }
        private static ImageCodecInfo GetEncoder(System.Drawing.Imaging.ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if(codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}
