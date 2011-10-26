using System;
using System.Web;
using System.Security.Permissions;
using System.Drawing;
using System.Collections.Specialized;
using System.IO;
using System.Drawing.Imaging;
using SharpPieces.Web.Controls;
using System.Drawing.Drawing2D;
using System.Collections.Generic;

namespace SharpPieces.Web.Controls
{

    /// <summary>
    /// Represents the gradient button generation service.
    /// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal), AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class GradientButtonProvider : DynamicImageProvider
    {

        // Fields

        internal static IImageCreator textDependentImageCreator = new TextDependentImageCreator();
        internal static IImageTransformation gradientButtonImageTransformation = new GradientButtonImageTransformation();

        // Methods

        /// <summary>
        /// Creates an image.
        /// </summary>
        /// <param name="parameters">The params.</param>
        /// <returns>A new image based on the params.</returns>
        protected override Image CreateImage(NameValueCollection parameters)
        {
            return GradientButtonProvider.textDependentImageCreator.Get(parameters);
        }

        /// <summary>
        /// Applies image transformations.
        /// </summary>
        /// <param name="image">The image to be transfored.</param>
        /// <param name="parameters">The transformation params.</param>
        protected override void TransformImage(ref Image image, NameValueCollection parameters)
        {
            GradientButtonProvider.gradientButtonImageTransformation.Apply(ref image, parameters);

            base.TransformImage(ref image, parameters);
        }

        // Methods

        

        /// <summary>
        /// Utility method that returns a query string representation based on the provided parameter values.
        /// </summary>
        /// <param name="source">The image source.</param>
        /// <param name="width">The image width.</param>
        /// <param name="height">The image height.</param>
        /// <param name="clientCacheDuration">The image client cache duration.</param>
        /// <param name="serverCacheDuration">The image server cache duration.</param>
        /// <param name="rotateFlip">The image rotate flip type.</param>
        /// <param name="drawGrayscale">The image draw grayscale type.</param>
        /// <param name="drawSepia">The image draw sepia type.</param>
        /// <param name="text">The image dynamic text.</param>
        /// <param name="gradientBackground">The button gradient background.</param>
        /// <param name="sizeType">The button size type.</param>
        /// <param name="textRenderTime">The time when the text is rendered.</param>
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
            GradientBackground gradientBackground,
            TextContainerSizeType sizeType)
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

            if (null != gradientBackground)
            {
                // gradient border color
                if (Color.Gray != gradientBackground.BorderColor)
                {
                    parameters.Add(string.Format(parameterFormat, GradientButtonImageTransformation.GRADIENTBKGCOLOR_KEY, gradientBackground.BorderColor.ToArgb()));
                }

                // gradient start color
                if (Color.Brown != gradientBackground.GradientStartColor)
                {
                    parameters.Add(string.Format(parameterFormat, GradientButtonImageTransformation.GRADIENTBKGSTARTCOLOR_KEY, gradientBackground.GradientStartColor.ToArgb()));
                }

                // gradient end color
                if (Color.White != gradientBackground.GradientEndColor)
                {
                    parameters.Add(string.Format(parameterFormat, GradientButtonImageTransformation.GRADIENTBKGENDCOLOR_KEY, gradientBackground.GradientEndColor.ToArgb()));
                }

                // gradient round corner radius
                if (0 <= gradientBackground.RoundCornerRadius)
                {
                    parameters.Add(string.Format(parameterFormat, GradientButtonImageTransformation.GRADIENTBKGRCORNER_KEY, gradientBackground.RoundCornerRadius));
                }

                // gradient border width
                if (0 <= gradientBackground.BorderWidth)
                {
                    parameters.Add(string.Format(parameterFormat, GradientButtonImageTransformation.GRADIENTBKGBWIDTH_KEY, gradientBackground.BorderWidth));
                }

                // gradient type
                if (GradientType.BackwardDiagonal != gradientBackground.Type)
                {
                    parameters.Add(string.Format(parameterFormat, GradientButtonImageTransformation.GRADIENTBKGTYPE_KEY, gradientBackground.Type));
                }

                // gradient inner border color
                if (Color.Gray != gradientBackground.InnerBorderColor)
                {
                    parameters.Add(string.Format(parameterFormat, GradientButtonImageTransformation.GRADIENTBKGIBCOLOR_KEY, gradientBackground.InnerBorderColor.ToArgb()));
                }

                // gradient inner border width
                if (0 <= gradientBackground.InnerBorderWidth)
                {
                    parameters.Add(string.Format(parameterFormat, GradientButtonImageTransformation.GRADIENTBKGIBWIDTH_KEY, gradientBackground.InnerBorderWidth));
                }
            }

            // size type
            if (TextContainerSizeType.Specified != sizeType)
            {
                parameters.Add(string.Format(parameterFormat, TextDependentImageCreator.TEXTCONTSIZETYPE_KEY, sizeType));
            }

            return (0 < parameters.Count) ? string.Concat(dynamicImageQueryString, "&", string.Join("&", parameters.ToArray())) : dynamicImageQueryString;
        }
    }


    /// <summary>
    /// Represents an image creator for a text dependent image.
    /// </summary>
    public sealed class TextDependentImageCreator : IImageCreator
    {

        // Fields

        internal const string TEXTCONTSIZETYPE_KEY = "tcs";
        internal static readonly Size padding = new Size(5, 5);

        // Methods

        internal static bool ParseTextSizeParam(NameValueCollection parameters, out TextContainerSizeType textSizeType)
        {
            if ((null != parameters) && !string.IsNullOrEmpty(parameters[TextDependentImageCreator.TEXTCONTSIZETYPE_KEY]))
            {
                textSizeType = (TextContainerSizeType)Enum.Parse(typeof(TextContainerSizeType), parameters[TextDependentImageCreator.TEXTCONTSIZETYPE_KEY]);
                return true;
            }
            else
            {
                textSizeType = TextContainerSizeType.Specified;
                return false;
            }
        }

        #region IImageCreator Members

        /// <summary>
        /// Creates a gradient button image according to the specified params.
        /// </summary>
        /// <param name="parameters">The creator params.</param>
        /// <returns>A new image based on the params.</returns>
        public Image Get(NameValueCollection parameters)
        {
            TextContainerSizeType textSizeType;
            if (!TextDependentImageCreator.ParseTextSizeParam(parameters, out textSizeType))
            {
                textSizeType = TextContainerSizeType.Specified;
            }

            // the size is required

            Size size;
            if (!ResizeImageTransformation.ParseSizeParam(parameters, out size))
            {
                throw new ArgumentNullException("size");
            }

            Image image;

            switch (textSizeType)
            {
                case TextContainerSizeType.Specified:
                    {
                        using (Bitmap temp = new Bitmap(size.Width, size.Height))
                        {
                            MemoryStream ms = new MemoryStream();
                            temp.Save(ms, ImageFormat.Png);
                            image = Image.FromStream(ms);
                        }

                        break;
                    }
                case TextContainerSizeType.StrechToText:
                    {
                        // fit the size on the text

                        DynamicText text;
                        if (!TextImageTransformation.ParseTextParam(parameters, out text))
                        {
                            text = new DynamicText();
                            text.Value = "Gradient button";
                        }

                        Size textSize;
                        using (Bitmap temp = new Bitmap(size.Width, size.Height))
                        {
                            using (Graphics g = Graphics.FromImage(temp))
                            {
                                textSize = g.MeasureString(text.Value, text.Font).ToSize();
                            }
                        }

                        using (Bitmap temp = new Bitmap(textSize.Width + TextDependentImageCreator.padding.Width, textSize.Height + TextDependentImageCreator.padding.Height))
                        {
                            MemoryStream ms = new MemoryStream();
                            temp.Save(ms, ImageFormat.Png);
                            image = Image.FromStream(ms);
                        }

                        break;
                    }
                default:
                    {
                        throw new NotSupportedException("textSizeType");
                    }
            }

            // add background image
            using (Graphics g = Graphics.FromImage(image))
            {
                // add transparent background
                using (Pen pen = new Pen(Color.Transparent))
                {
                    g.DrawRectangle(pen, new Rectangle(Point.Empty, new Size(image.Width, image.Height)));
                }
            }

            return image;
        }

        #endregion

    }


    /// <summary>
    /// Implements a gradient button transformation.
    /// </summary>
    public sealed class GradientButtonImageTransformation : IImageTransformation
    {

        // Fields

        internal const string GRADIENTBKGCOLOR_KEY = "gbc";
        internal const string GRADIENTBKGSTARTCOLOR_KEY = "gsc";
        internal const string GRADIENTBKGENDCOLOR_KEY = "gec";
        internal const string GRADIENTBKGRCORNER_KEY = "grc";
        internal const string GRADIENTBKGBWIDTH_KEY = "gbw";
        internal const string GRADIENTBKGTYPE_KEY = "gty";
        internal const string GRADIENTBKGIBCOLOR_KEY = "gibc";
        internal const string GRADIENTBKGIBWIDTH_KEY = "gibw";

        // Methods

        internal static bool ParseGradientBackground(NameValueCollection parameters, out GradientBackground gradientBackground)
        {
            if (null != parameters)
            {
                gradientBackground = new GradientBackground();
                if (!string.IsNullOrEmpty(parameters[GradientButtonImageTransformation.GRADIENTBKGCOLOR_KEY]))
                {
                    gradientBackground.BorderColor = Color.FromArgb(int.Parse(parameters[GradientButtonImageTransformation.GRADIENTBKGCOLOR_KEY]));
                }
                if (!string.IsNullOrEmpty(parameters[GradientButtonImageTransformation.GRADIENTBKGSTARTCOLOR_KEY]))
                {
                    gradientBackground.GradientStartColor = Color.FromArgb(int.Parse(parameters[GradientButtonImageTransformation.GRADIENTBKGSTARTCOLOR_KEY]));
                }
                if (!string.IsNullOrEmpty(parameters[GradientButtonImageTransformation.GRADIENTBKGENDCOLOR_KEY]))
                {
                    gradientBackground.GradientEndColor = Color.FromArgb(int.Parse(parameters[GradientButtonImageTransformation.GRADIENTBKGENDCOLOR_KEY]));
                }
                if (!string.IsNullOrEmpty(parameters[GradientButtonImageTransformation.GRADIENTBKGRCORNER_KEY]))
                {
                    gradientBackground.RoundCornerRadius = int.Parse(parameters[GradientButtonImageTransformation.GRADIENTBKGRCORNER_KEY]);
                }
                if (!string.IsNullOrEmpty(parameters[GradientButtonImageTransformation.GRADIENTBKGBWIDTH_KEY]))
                {
                    gradientBackground.BorderWidth = int.Parse(parameters[GradientButtonImageTransformation.GRADIENTBKGBWIDTH_KEY]);
                }
                if (!string.IsNullOrEmpty(parameters[GradientButtonImageTransformation.GRADIENTBKGTYPE_KEY]))
                {
                    gradientBackground.Type = (GradientType)Enum.Parse(typeof(GradientType), parameters[GradientButtonImageTransformation.GRADIENTBKGTYPE_KEY]);
                }
                if (!string.IsNullOrEmpty(parameters[GradientButtonImageTransformation.GRADIENTBKGIBCOLOR_KEY]))
                {
                    gradientBackground.InnerBorderColor = Color.FromArgb(int.Parse(parameters[GradientButtonImageTransformation.GRADIENTBKGIBCOLOR_KEY]));
                }
                if (!string.IsNullOrEmpty(parameters[GradientButtonImageTransformation.GRADIENTBKGIBWIDTH_KEY]))
                {
                    gradientBackground.InnerBorderWidth = int.Parse(parameters[GradientButtonImageTransformation.GRADIENTBKGIBWIDTH_KEY]);
                }
                return true;
            }
            else
            {
                gradientBackground = null;
                return false;
            }
        }

        /// <summary>
        /// Gets the round path for a rectangle and a depth.
        /// </summary>
        /// <param name="r">The rectangle.</param>
        /// <param name="depth">The depth.</param>
        /// <returns>The rounded path.</returns>
        internal static GraphicsPath GetRoundPath(Rectangle r, int depth)
        {
            GraphicsPath graphPath = new GraphicsPath();

            if (0 < depth)
            {
                graphPath.AddArc(r.X, r.Y, 2 * depth, 2 * depth, 180, 90);
                graphPath.AddLine(new Point(r.X + depth, r.Y), new Point(r.X + r.Width - depth, r.Y));
                graphPath.AddArc(r.X + r.Width - 2 * depth, r.Y, 2 * depth, 2 * depth, 270, 90);
                graphPath.AddLine(new Point(r.X + r.Width, r.Y + depth), new Point(r.X + r.Width, r.Y + r.Height - depth));
                graphPath.AddArc(r.X + r.Width - 2 * depth, r.Y + r.Height - 2 * depth, 2 * depth, 2 * depth, 0, 90);
                graphPath.AddLine(new Point(r.X + r.Width - depth, r.Y + r.Height), new Point(r.X + depth, r.Y + r.Height));
                graphPath.AddArc(r.X, r.Y + r.Height - 2 * depth, 2 * depth, 2 * depth, 90, 90);
                graphPath.AddLine(r.X, r.Y + r.Height - depth, r.X, r.Y + depth);
            }
            else
            {
                // 0 or less gets a rectangle
                graphPath.AddRectangle(r);
            }

            return graphPath;
        }

        #region IImageTransformation Members

        /// <summary>
        /// Applies the specified image transformation.
        /// </summary>
        /// <param name="image">The image to be transfored.</param>
        /// <param name="parameters">The transformation params.</param>
        public void Apply(ref Image image, NameValueCollection parameters)
        {
            GradientBackground gradientBackground;
            if (GradientButtonImageTransformation.ParseGradientBackground(parameters, out gradientBackground))
            {
                // add background image
                using (Graphics g = Graphics.FromImage(image))
                {
                    // add smooth lines support
                    g.SmoothingMode = SmoothingMode.AntiAlias;

                    // add gradient
                    Rectangle rect = new Rectangle(
                        gradientBackground.BorderWidth / 2,
                        gradientBackground.BorderWidth / 2,
                        image.Width - gradientBackground.BorderWidth,
                        image.Height - gradientBackground.BorderWidth);

                    // prevent excedent of round corner radius
                    GraphicsPath graphPath = GradientButtonImageTransformation.GetRoundPath(rect,
                        Math.Min(gradientBackground.RoundCornerRadius, Math.Min(rect.Width / 2, rect.Height / 2)));

                    Brush brush;

                    switch (gradientBackground.Type)
                    {
                        case GradientType.BackwardDiagonal:
                            {
                                brush = new LinearGradientBrush(rect, gradientBackground.GradientStartColor, gradientBackground.GradientEndColor, LinearGradientMode.BackwardDiagonal);
                                break;
                            }
                        case GradientType.ForwardDiagonal:
                            {
                                brush = new LinearGradientBrush(rect, gradientBackground.GradientStartColor, gradientBackground.GradientEndColor, LinearGradientMode.ForwardDiagonal);
                                break;
                            }
                        case GradientType.Horizontal:
                            {
                                brush = new LinearGradientBrush(rect, gradientBackground.GradientStartColor, gradientBackground.GradientEndColor, LinearGradientMode.Horizontal);
                                break;
                            }
                        case GradientType.Vertical:
                            {
                                brush = new LinearGradientBrush(rect, gradientBackground.GradientStartColor, gradientBackground.GradientEndColor, LinearGradientMode.Vertical);
                                break;
                            }
                        case GradientType.BlendingIn:
                            {
                                brush = new PathGradientBrush(graphPath);
                                ((PathGradientBrush)brush).CenterColor = gradientBackground.GradientStartColor;
                                ((PathGradientBrush)brush).SurroundColors = new Color[] { gradientBackground.GradientEndColor };
                                ((PathGradientBrush)brush).FocusScales = new PointF(0.7f, 0.5f);
                                break;
                            }
                        case GradientType.VerticalSuddenFalloff:
                            {
                                Blend blend = new Blend();
                                blend.Positions = new float[] { 0, 0.49f, 0.51f, 1f };
                                blend.Factors = new float[] { 0f, 0f, 1f, 1f };
                                brush = new LinearGradientBrush(rect, gradientBackground.GradientStartColor, gradientBackground.GradientEndColor, LinearGradientMode.Vertical);
                                ((LinearGradientBrush)brush).Blend = blend;
                                break;
                            }
                        default:
                            {
                                throw new NotSupportedException();
                            }
                    }

                    g.FillPath(brush, graphPath);

                    // add border
                    if (0 < gradientBackground.BorderWidth)
                    {
                        using (Pen pen = new Pen(Color.FromArgb(180, gradientBackground.BorderColor),
                            gradientBackground.BorderWidth))
                        {
                            g.DrawPath(pen, graphPath);
                        }
                    }

                    if (0 < gradientBackground.InnerBorderWidth)
                    {
                        Rectangle rect2 = new Rectangle(
                            gradientBackground.BorderWidth + gradientBackground.InnerBorderWidth / 2,
                            gradientBackground.BorderWidth + gradientBackground.InnerBorderWidth / 2,
                            image.Width - 2 * gradientBackground.BorderWidth - gradientBackground.InnerBorderWidth,
                            image.Height - 2 * gradientBackground.BorderWidth - gradientBackground.InnerBorderWidth);

                        GraphicsPath graphPath2 = GradientButtonImageTransformation.GetRoundPath(rect2,
                            Math.Min(gradientBackground.RoundCornerRadius, Math.Min(rect2.Width / 2, rect2.Height / 2)));

                        using (Pen pen = new Pen(Color.FromArgb(180, gradientBackground.InnerBorderColor),
                            gradientBackground.InnerBorderWidth))
                        {
                            g.DrawPath(pen, graphPath2);
                        }

                        graphPath.AddPath(graphPath2, false);
                    }

                    // draw Image
                    g.DrawImageUnscaled(image, Point.Empty);
                }
            }
        }

        #endregion
    }

}
