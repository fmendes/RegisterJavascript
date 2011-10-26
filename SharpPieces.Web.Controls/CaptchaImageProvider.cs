using System;
using System.Web;
using System.Security.Permissions;
using System.Collections.Specialized;
using System.Drawing;
using SharpPieces.Web.Controls;
using System.Drawing.Drawing2D;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.IO;

namespace SharpPieces.Web.Controls
{

    /// <summary>
    /// Represents the captcha image generation service.
    /// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal), AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class CaptchaImageProvider : DynamicImageProvider
    {

        // Fields

        internal static IImageCreator textDependentImageCreator = new TextDependentImageCreator();
        internal static IImageTransformation captchaImageTransformation = new CaptchaImageTransformation();

        private static readonly byte[] desPass = ASCIIEncoding.ASCII.GetBytes("fgfgfgfg");
        private static readonly byte[] desVector = ASCIIEncoding.ASCII.GetBytes("jgjgjgjg");

        // Methods

        /// <summary>
        /// Creates an image.
        /// </summary>
        /// <param name="parameters">The params.</param>
        /// <returns>A new image based on the params.</returns>
        protected override Image CreateImage(NameValueCollection parameters)
        {
            // change the parameters due to the crypted text

            DynamicText text;
            if (!TextImageTransformation.ParseTextParam(this.Parameters, out text))
            {
                throw new ArgumentNullException("text");
            }

            NameValueCollection newParameters = new NameValueCollection(this.Parameters);
            newParameters.Set(TextImageTransformation.TEXTVALUE_KEY, CaptchaImageProvider.DecryptText(text.Value));

            this.Parameters = newParameters;

            return CaptchaImageProvider.textDependentImageCreator.Get(parameters);
        }

        /// <summary>
        /// Applies image transformations.
        /// </summary>
        /// <param name="image">The image to be transfored.</param>
        /// <param name="parameters">The transformation params.</param>
        protected override void TransformImage(ref Image image, NameValueCollection parameters)
        {
            DynamicImageProvider.resizeImageTrasformation.Apply(ref image, parameters);

            CaptchaImageProvider.captchaImageTransformation.Apply(ref image, parameters);

            DynamicImageProvider.grayscaleImageTrasformation.Apply(ref image, parameters);

            DynamicImageProvider.sepiaImageTrasformation.Apply(ref image, parameters);

            DynamicImageProvider.rotateImageTrasformation.Apply(ref image, parameters);
        }

        // Methods

        internal static string ToQueryString(
            int width,
            int height,
            int clientCacheDuration,
            int serverCacheDuration,
            RotateFlipType rotateFlip,
            bool drawGrayscale,
            bool drawSepia,
            DynamicText text,
            DynamicImageFormat imageFormat,
            KeyValuePair<Type, IDictionary<string, string>>? imageCreator,
            Dictionary<Type, IDictionary<string, string>> imageTransformations,
            TextContainerSizeType sizeType,
            CaptchaStyle distortionStyle,
            ReadnessLevel readnessLevel,
            Color backColor)
        {
            string dynamicImageQueryString = DynamicImageProvider.ToQueryString(
                null,
                width,
                height,
                clientCacheDuration,
                serverCacheDuration,
                rotateFlip,
                drawGrayscale,
                drawSepia,
                text,
                imageFormat,
                imageCreator,
                imageTransformations);

            List<string> parameters = new List<string>();
            string parameterFormat = "{0}={1}";

            // size type
            if (TextContainerSizeType.Specified != sizeType)
            {
                parameters.Add(string.Format(parameterFormat, TextDependentImageCreator.TEXTCONTSIZETYPE_KEY, sizeType));
            }

            // distortion style
            if (CaptchaStyle.Confetti != distortionStyle)
            {
                parameters.Add(string.Format(parameterFormat, CaptchaImageTransformation.DISTSTYLE_KEY, distortionStyle));
            }

            // readness level
            if (ReadnessLevel.Normal != readnessLevel)
            {
                parameters.Add(string.Format(parameterFormat, CaptchaImageTransformation.READLEVEL_KEY, readnessLevel));
            }

            // back color
            if (Color.White != backColor)
            {
                parameters.Add(string.Format(parameterFormat, CaptchaImageTransformation.CAPTCHABACKCOLOR_KEY, backColor.ToArgb()));
            }

            return (0 < parameters.Count) ? string.Concat(dynamicImageQueryString, "&", string.Join("&", parameters.ToArray())) : dynamicImageQueryString;
        }

        /// <summary>
        /// Encrypts a text.
        /// </summary>
        /// <param name="text">The text to be encrypted.</param>
        /// <returns>The encrypted text.</returns>
        public static string EncryptText(string text)
        {
            if (null == text)
            {
                return null;
            }

            byte[] data = System.Text.ASCIIEncoding.ASCII.GetBytes(text);

            //1024-bit encryption
            using (MemoryStream memoryStream = new MemoryStream(1024))
            using (CryptoStream cryptoStream = new CryptoStream(
                memoryStream,
                new DESCryptoServiceProvider().CreateEncryptor(CaptchaImageProvider.desPass, CaptchaImageProvider.desVector),
                CryptoStreamMode.Write))
            {
                cryptoStream.Write(data, 0, data.Length);
                cryptoStream.FlushFinalBlock();

                byte[] result = new byte[(int)memoryStream.Position];
                memoryStream.Position = 0;
                memoryStream.Read(result, 0, result.Length);

                return Convert.ToBase64String(result);
            }
        }

        /// <summary>
        /// Decrypts a text.
        /// </summary>
        /// <param name="text">The encrypted text.</param>
        /// <returns>The decrypted text.</returns>
        public static string DecryptText(string text)
        {
            if (null == text)
            {
                return null;
            }

            byte[] data = System.Convert.FromBase64String(text);

            using (MemoryStream memoryStream = new MemoryStream(data.Length))
            using (CryptoStream cryptoStream = new CryptoStream(
                memoryStream,
                new DESCryptoServiceProvider().CreateDecryptor(CaptchaImageProvider.desPass, CaptchaImageProvider.desVector),
                CryptoStreamMode.Read))
            {
                memoryStream.Write(data, 0, data.Length);
                memoryStream.Position = 0;

                using (StreamReader sr = new StreamReader(cryptoStream))
                {
                    return sr.ReadToEnd();
                }
            }
        }


        // Properties

        /// <summary>
        /// Gets the server cache key.
        /// </summary>
        /// <value>The server cache key.</value>
        protected override string ServerCacheKey
        {
            get
            {
                string[] parameters = new string[this.Parameters.Count];
                this.Parameters.CopyTo(parameters, 0);

                return string.Concat(string.Join("&", this.Parameters.AllKeys), "+", string.Join("&", parameters));
            }
        }

    }

    /// <summary>
    /// Implements a captcha image transformation.
    /// </summary>
    public sealed class CaptchaImageTransformation : IImageTransformation
    {

        // Fields

        internal const string DISTSTYLE_KEY = "dst";
        internal const string READLEVEL_KEY = "rlv";
        internal const string CAPTCHABACKCOLOR_KEY = "cbk";

        // Methods

        internal static bool ParseDistorsionParam(NameValueCollection parameters, out CaptchaStyle distStyle)
        {
            if ((null != parameters) && !string.IsNullOrEmpty(parameters[CaptchaImageTransformation.DISTSTYLE_KEY]))
            {
                distStyle = (CaptchaStyle)Enum.Parse(typeof(CaptchaStyle), parameters[CaptchaImageTransformation.DISTSTYLE_KEY]);
                if (CaptchaStyle.Random == distStyle)
                {
                    distStyle = (CaptchaStyle)new Random(DateTime.Now.Millisecond).Next(3);
                }
                return true;
            }
            else
            {
                distStyle = CaptchaStyle.Confetti;
                return false;
            }
        }

        internal static bool ParseReadnessParam(NameValueCollection parameters, out ReadnessLevel readnessLevel)
        {
            if ((null != parameters) && !string.IsNullOrEmpty(parameters[CaptchaImageTransformation.READLEVEL_KEY]))
            {
                readnessLevel = (ReadnessLevel)Enum.Parse(typeof(ReadnessLevel), parameters[CaptchaImageTransformation.READLEVEL_KEY]);
                return true;
            }
            else
            {
                readnessLevel = ReadnessLevel.Normal;
                return false;
            }
        }

        internal static bool ParseBackColorParam(NameValueCollection parameters, out Color backColor)
        {
            if ((null != parameters) && !string.IsNullOrEmpty(parameters[CaptchaImageTransformation.CAPTCHABACKCOLOR_KEY]))
            {
                backColor = Color.FromArgb(int.Parse(parameters[CaptchaImageTransformation.CAPTCHABACKCOLOR_KEY]));
                return true;
            }
            else
            {
                backColor = Color.White;
                return false;
            }
        }

        #region IImageTransformation Members

        /// <summary>
        /// Applies the specified image transformation.
        /// </summary>
        /// <param name="image">The image to be transfored.</param>
        /// <param name="parameters">The transformation params.</param>
        public void Apply(ref Image image, NameValueCollection parameters)
        {
            DynamicText text;
            if (!TextImageTransformation.ParseTextParam(parameters, out text))
            {
                throw new ArgumentNullException("text");
            }

            CaptchaStyle distStyle;
            if (!CaptchaImageTransformation.ParseDistorsionParam(parameters, out distStyle))
            {
                distStyle = CaptchaStyle.Confetti;
            }

            ReadnessLevel readnessLevel;
            if (!CaptchaImageTransformation.ParseReadnessParam(parameters, out readnessLevel))
            {
                readnessLevel = ReadnessLevel.Normal;
            }

            Color backColor;
            if (!CaptchaImageTransformation.ParseBackColorParam(parameters, out backColor))
            {
                backColor = Color.White;
            }

            switch (distStyle)
            {
                case CaptchaStyle.Confetti:
                    {
                        using (Graphics g = Graphics.FromImage(image))
                        {
                            Color middleColor = Color.FromArgb(
                                text.Color.R + (backColor.R - text.Color.R) / 2,
                                text.Color.G + (backColor.G - text.Color.G) / 2,
                                text.Color.B + (backColor.B - text.Color.B) / 2);

                            Rectangle rectangle = new Rectangle(Point.Empty, image.Size);

                            // fill in the background
                            using (Brush brush = new HatchBrush(HatchStyle.SmallConfetti, middleColor, backColor))
                            {
                                g.FillRectangle(brush, rectangle);
                            }

                            Random rand = new Random(DateTime.Now.Millisecond);

                            using (GraphicsPath path = new GraphicsPath())
                            {
                                using (StringFormat format = new StringFormat())
                                {
                                    format.LineAlignment = text.HorizontalAlign;
                                    format.Alignment = text.VerticalAlign;

                                    path.AddString(text.Value, text.Font.FontFamily, (int)text.Font.Style, text.Font.Size, rectangle, format);
                                }

                                float mix = 4;
                                PointF[] points =
                                  {
                                    new PointF(rand.Next(rectangle.Width) / mix, rand.Next(rectangle.Height) / mix),
                                    new PointF(rectangle.Width - rand.Next(rectangle.Width) / mix, rand.Next(rectangle.Height) / mix),
                                    new PointF(rand.Next(rectangle.Width) / mix, rectangle.Height - rand.Next(rectangle.Height) / mix),
                                    new PointF(rectangle.Width - rand.Next(rectangle.Width) / mix, rectangle.Height - rand.Next(rectangle.Height) / mix)
                                  };

                                path.Warp(points, rectangle, new Matrix(), WarpMode.Perspective, 0f);

                                // draw the text
                                using (HatchBrush brush = new HatchBrush(HatchStyle.LargeConfetti, middleColor, text.Color))
                                {
                                    g.FillPath(brush, path);
                                }
                            }

                            // add some random noise                            
                            int level;
                            switch (readnessLevel)
                            {
                                case ReadnessLevel.Normal:
                                    {
                                        level = 50;
                                        break;
                                    }
                                case ReadnessLevel.Hard:
                                    {
                                        level = 30;
                                        break;
                                    }
                                case ReadnessLevel.AlmostImpossible:
                                    {
                                        level = 20;
                                        break;
                                    }
                                default:
                                    {
                                        throw new NotImplementedException();
                                    }
                            }

                            int m = Math.Max(rectangle.Width, rectangle.Height);
                            for (int i = 0; i < (int)(rectangle.Width * rectangle.Height / 30); i++)
                            {
                                using (HatchBrush brush = new HatchBrush(HatchStyle.LargeConfetti, middleColor, text.Color))
                                {
                                    g.FillEllipse(brush, rand.Next(rectangle.Width), rand.Next(rectangle.Height), rand.Next(m / level), rand.Next(m / level));
                                }
                            }
                        }

                        break;
                    }
                case CaptchaStyle.Gradient:
                    {
                        using (Graphics g = Graphics.FromImage(image))
                        {
                            g.SmoothingMode = SmoothingMode.AntiAlias;

                            Rectangle rectangle = new Rectangle(Point.Empty, image.Size);

                            Random rand = new Random(DateTime.Now.Millisecond);

                            // draw the background as a random linear gradient
                            using (LinearGradientBrush brush = new LinearGradientBrush(
                                rectangle,
                                Color.FromArgb(Color.White.R - backColor.R, Color.White.G - backColor.G, Color.White.B - backColor.B),
                                backColor,
                                (float)rand.NextDouble() * 360,
                                false))
                            {
                                g.FillRectangle(brush, rectangle);
                            }

                            // draw the string into a clone image, distort the text and 
                            // copy into the original image
                            Size textSize = g.MeasureString(text.Value, text.Font).ToSize();
                            Size maxDistortion = new Size((rectangle.Width - textSize.Width) / 2 - 1, (rectangle.Height - textSize.Height) / 2 - 1);
                            Size distortion;
                            if (!Size.Empty.Equals(maxDistortion))
                            {
                                switch (readnessLevel)
                                {
                                    case ReadnessLevel.Normal:
                                        {
                                            distortion = new Size((maxDistortion.Width / 3) * ((rand.Next(2) == 1) ? 1 : -1), (maxDistortion.Height / 3) * ((rand.Next(2) == 1) ? 1 : -1));
                                            break;
                                        }
                                    case ReadnessLevel.Hard:
                                        {
                                            distortion = new Size((maxDistortion.Width / 2) * ((rand.Next(2) == 1) ? 1 : -1), (maxDistortion.Height / 2) * ((rand.Next(2) == 1) ? 1 : -1));
                                            break;
                                        }
                                    case ReadnessLevel.AlmostImpossible:
                                        {
                                            distortion = new Size(maxDistortion.Width * ((rand.Next(2) == 1) ? 1 : -1), maxDistortion.Height * ((rand.Next(2) == 1) ? 1 : -1));
                                            break;
                                        }
                                    default:
                                        {
                                            throw new NotImplementedException();
                                        }
                                }

                                using (GraphicsPath textPath = new GraphicsPath())
                                using (StringFormat format = new StringFormat())
                                {
                                    format.LineAlignment = text.HorizontalAlign;
                                    format.Alignment = text.VerticalAlign;

                                    textPath.AddString(text.Value, text.Font.FontFamily, (int)text.Font.Style, text.Font.Size, rectangle, format);

                                    PointF[] points = textPath.PathPoints;
                                    PointF[] distortedPoints = new PointF[textPath.PointCount];
                                    for (int i = 0; i < textPath.PointCount; i++)
                                    {
                                        distortedPoints[i] = new PointF(
                                            points[i].X,
                                            //(float)(textPath.PathPoints[i].X + (distortion.Width * Math.Sin(Math.PI * textPath.PathPoints[i].Y / 48.0))),
                                            (float)(points[i].Y + (distortion.Height * Math.Cos(Math.PI * points[i].X / 48.0))));
                                    }

                                    Color endColor = Color.FromArgb(Color.White.R - text.Color.R, Color.White.G - text.Color.G, Color.White.B - text.Color.B);

                                    using (GraphicsPath distortedTextPath = new GraphicsPath(distortedPoints, textPath.PathTypes))
                                    using (LinearGradientBrush brush = new LinearGradientBrush(
                                        new Rectangle(0, 0, rectangle.Width / 2, rectangle.Height / 2),
                                        text.Color,
                                        endColor,
                                        (float)rand.NextDouble() * 360,
                                        false))
                                    {
                                        g.FillPath(brush, distortedTextPath);
                                    }
                                }
                            }
                        }

                        break;
                    }
                case CaptchaStyle.Holes:
                    {
                        using (Graphics g = Graphics.FromImage(image))
                        {
                            g.Clear(backColor);

                            Random rand = new Random(DateTime.Now.Millisecond);

                            int noCircles = 0;
                            switch (readnessLevel)
                            {
                                case ReadnessLevel.Normal:
                                    {
                                        noCircles = 1 * text.Value.Length + rand.Next(text.Value.Length);
                                        break;
                                    }
                                case ReadnessLevel.Hard:
                                    {
                                        noCircles = 2 * text.Value.Length + rand.Next(text.Value.Length);
                                        break;
                                    }
                                case ReadnessLevel.AlmostImpossible:
                                    {
                                        noCircles = 4 * text.Value.Length + rand.Next(text.Value.Length);
                                        break;
                                    }
                                default:
                                    {
                                        throw new NotImplementedException();
                                    }
                            }

                            Rectangle rectangle = new Rectangle(Point.Empty, image.Size);

                            using (GraphicsPath textPath = new GraphicsPath())
                            {
                                using (Brush brush = new SolidBrush(text.Color))
                                {
                                    using (StringFormat format = new StringFormat())
                                    {
                                        format.LineAlignment = text.HorizontalAlign;
                                        format.Alignment = text.VerticalAlign;

                                        textPath.AddString(text.Value, text.Font.FontFamily, (int)text.Font.Style, text.Font.Size, rectangle, format);
                                    }

                                    int mix = 6;
                                    PointF[] points =
                                  {
                                    new PointF(rand.Next(rectangle.Width) / mix, rand.Next(rectangle.Height) / mix),
                                    new PointF(rectangle.Width - rand.Next(rectangle.Width) / mix, rand.Next(rectangle.Height) / mix),
                                    new PointF(rand.Next(rectangle.Width) / mix, rectangle.Height - rand.Next(rectangle.Height) / mix),
                                    new PointF(rectangle.Width - rand.Next(rectangle.Width) / mix, rectangle.Height - rand.Next(rectangle.Height) / mix)
                                  };
                                    textPath.Warp(points, rectangle, new Matrix(), WarpMode.Perspective, 0);

                                    g.FillPath(brush, textPath);
                                }

                                float circleHalfWidth = text.Font.Size / 10;
                                using (GraphicsPath circlePath = new GraphicsPath())
                                {
                                    for (int i = 0; i < noCircles; i++)
                                    {
                                        PointF textPathPoint = textPath.PathPoints[rand.Next(textPath.PathPoints.Length)];
                                        circlePath.AddEllipse(new RectangleF(new PointF(textPathPoint.X - circleHalfWidth, textPathPoint.Y - circleHalfWidth), new SizeF(circleHalfWidth * 2, circleHalfWidth * 2)));
                                    }

                                    using (Brush brush = new SolidBrush(backColor))
                                    {
                                        g.FillPath(brush, circlePath);
                                    }
                                }
                            }
                        }

                        break;
                    }
                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }

        #endregion
    }

}

