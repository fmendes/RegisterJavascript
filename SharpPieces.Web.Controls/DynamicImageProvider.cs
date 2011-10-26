using System;
using System.Web;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Net;
using System.Web.Caching;
using System.IO;

namespace SharpPieces.Web.Controls
{

    /// <summary>
    /// Represents the dynamic image generation service.
    /// </summary>
    public class DynamicImageProvider : IHttpHandler
    {

        // Fields

        private HttpContext context = null;
        private NameValueCollection parameters = null;
        private int clientCacheDuration = 0;
        private int serverCacheDuration = 0;
        private DynamicImageFormat imageFormat = DynamicImageFormat.Original;

        internal const string IMAGEFORMAT_KEY = "imf";
        internal const string CREATIONTYPE_KEY = "cty";
        internal const string TRANSFTYPEPREFIX_KEY = "trt";
        internal const string CLIENTCACHEDUR_KEY = "ccd";
        internal const string SERVERCACHEDUR_KEY = "scd";

        internal static IImageCreator localizedImageCreation = new LocalizedImageCreation();
        internal static IImageTransformation resizeImageTrasformation = new ResizeImageTransformation();
        internal static IImageTransformation rotateImageTrasformation = new RotateFlipImageTransformation();        
        internal static IImageTransformation grayscaleImageTrasformation = new GrayscaleImageTransformation();
        internal static IImageTransformation sepiaImageTrasformation = new SepiaImageTransformation();
        internal static IImageTransformation textImageTrasformation = new TextImageTransformation();

        // Methods

        /// <summary>
        /// Creates an image.
        /// </summary>
        /// <param name="parameters">The params.</param>
        /// <returns>A new image based on the params.</returns>
        protected virtual Image CreateImage(NameValueCollection parameters)
        {
            return DynamicImageProvider.localizedImageCreation.Get(parameters);
        }

        /// <summary>
        /// Applies image transformations.
        /// </summary>
        /// <param name="image">The image to be transfored.</param>
        /// <param name="parameters">The transformation params.</param>
        protected virtual void TransformImage(ref Image image, NameValueCollection parameters)
        {
            DynamicImageProvider.resizeImageTrasformation.Apply(ref image, parameters);

            DynamicImageProvider.textImageTrasformation.Apply(ref image, parameters);

            DynamicImageProvider.grayscaleImageTrasformation.Apply(ref image, parameters);

            DynamicImageProvider.sepiaImageTrasformation.Apply(ref image, parameters);

            DynamicImageProvider.rotateImageTrasformation.Apply(ref image, parameters);
        }

        internal static string ToQueryString(
            string source,
            int? width,
            int? height,
            int clientCacheDuration,
            int serverCacheDuration,
            RotateFlipType rotateFlip,
            bool drawGrayscale,
            bool drawSepia,
            DynamicText text,
            DynamicImageFormat imageFormat,
            KeyValuePair<Type, IDictionary<string, string>>? imageCreator,
            Dictionary<Type, IDictionary<string, string>> imageTransformations)
        {
            HttpServerUtility server = HttpContext.Current.Server;

            string parameterFormat = "{0}={1}";

            Dictionary<string, string> parameters = new Dictionary<string, string>();

            // width

            if (width.HasValue && (0 < width.Value))
            {
                parameters.Add(
                    ResizeImageTransformation.SIZEW_KEY,
                    string.Format(parameterFormat, ResizeImageTransformation.SIZEW_KEY, width.Value));
            }

            // height

            if (height.HasValue && (0 < height.Value))
            {
                parameters.Add(
                    ResizeImageTransformation.SIZEH_KEY,
                    string.Format(parameterFormat, ResizeImageTransformation.SIZEH_KEY, height.Value));
            }

            // client cache duration

            if (0 < clientCacheDuration)
            {
                parameters.Add(
                    DynamicImageProvider.CLIENTCACHEDUR_KEY,
                    string.Format(parameterFormat, DynamicImageProvider.CLIENTCACHEDUR_KEY, clientCacheDuration));
            }

            // server cache duration

            if (0 < serverCacheDuration)
            {
                parameters.Add(
                    DynamicImageProvider.SERVERCACHEDUR_KEY,
                    string.Format(parameterFormat, DynamicImageProvider.SERVERCACHEDUR_KEY, serverCacheDuration));
            }

            // rotate flip

            if (RotateFlipType.RotateNoneFlipNone != rotateFlip)
            {
                parameters.Add(
                    RotateFlipImageTransformation.ROTATETYPE_KEY,
                    string.Format(parameterFormat, RotateFlipImageTransformation.ROTATETYPE_KEY, rotateFlip));
            }

            // grayscale

            if (drawGrayscale)
            {
                parameters.Add(
                    GrayscaleImageTransformation.GRAYSCALE_KEY,
                    string.Format(parameterFormat, GrayscaleImageTransformation.GRAYSCALE_KEY, drawGrayscale));
            }

            // sepia

            if (drawSepia)
            {
                parameters.Add(
                    SepiaImageTransformation.SEPIA_KEY,
                    string.Format(parameterFormat, SepiaImageTransformation.SEPIA_KEY, drawSepia));
            }

            if ((null != text) && !string.IsNullOrEmpty(text.Value))
            {
                // text

                parameters.Add(
                    TextImageTransformation.TEXTVALUE_KEY,
                    string.Format(parameterFormat, TextImageTransformation.TEXTVALUE_KEY, server.UrlEncode(text.Value)));

                // text font

                if (null != text.Font)
                {
                    string stringFont = new FontConverter().ConvertToString(text.Font);
                    if ("Arial;10.0f" != stringFont)
                    {
                        parameters.Add(
                            TextImageTransformation.TEXTFONT_KEY,
                            string.Format(parameterFormat, TextImageTransformation.TEXTFONT_KEY, server.UrlEncode(stringFont)));
                    }
                }

                // text color

                if (Color.Black != text.Color)
                {
                    parameters.Add(
                        TextImageTransformation.TEXTCOLOR_KEY,
                        string.Format(parameterFormat, TextImageTransformation.TEXTCOLOR_KEY, text.Color.ToArgb()));
                }

                // text halignment

                if (StringAlignment.Far != text.HorizontalAlign)
                {
                    parameters.Add(
                        TextImageTransformation.TEXTHEIGHTALIGN_KEY,
                        string.Format(parameterFormat, TextImageTransformation.TEXTHEIGHTALIGN_KEY, text.HorizontalAlign));
                }

                // text valignment

                if (StringAlignment.Far != text.VerticalAlign)
                {
                    parameters.Add(
                        TextImageTransformation.TEXTWIDTHALIGN_KEY,
                        string.Format(parameterFormat, TextImageTransformation.TEXTWIDTHALIGN_KEY, text.VerticalAlign));
                }
            }

            // image format

            if (DynamicImageFormat.Original != imageFormat)
            {
                parameters.Add(
                    DynamicImageProvider.IMAGEFORMAT_KEY,
                    string.Format(parameterFormat, DynamicImageProvider.IMAGEFORMAT_KEY, imageFormat));
            }

            if ((null == imageCreator) || (null == imageCreator.Value.Key))
            {
                // source

                if (!string.IsNullOrEmpty(source))
                {
                    parameters.Add(
                        LocalizedImageCreation.SOURCE_KEY,
                        string.Format(parameterFormat, LocalizedImageCreation.SOURCE_KEY, server.UrlEncode(source)));
                }
            }

            // set for last the custom creators and transformations 
            // to be able to yield duplicate keys

            // image creation type

            if ((null != imageCreator) && (null != imageCreator.Value.Key))
            {
                parameters.Add(
                    DynamicImageProvider.CREATIONTYPE_KEY,
                    string.Format(parameterFormat, DynamicImageProvider.CREATIONTYPE_KEY, server.UrlEncode(imageCreator.Value.Key.AssemblyQualifiedName)));

                if ((null != imageCreator.Value.Value) && (0 < imageCreator.Value.Value.Count))
                {
                    foreach (KeyValuePair<string, string> param in imageCreator.Value.Value)
                    {
                        if (!string.IsNullOrEmpty(param.Key))
                        {
                            if (parameters.ContainsKey(param.Key) || param.Key.StartsWith(DynamicImageProvider.TRANSFTYPEPREFIX_KEY, StringComparison.InvariantCultureIgnoreCase))
                            {
                                throw new InvalidOperationException("Duplicate parameter key in query string (creation type).");
                            }

                            parameters.Add(param.Key, string.Format(parameterFormat, server.UrlEncode(param.Key), server.UrlEncode(param.Value)));
                        }
                    }
                }
            }

            // image tranformations types
            if ((null != imageTransformations) && (0 < imageTransformations.Count))
            {
                int i = 0;
                foreach (Type t in imageTransformations.Keys)
                {
                    string typeKey = string.Format("{0}{1}", DynamicImageProvider.TRANSFTYPEPREFIX_KEY, i++);

                    parameters.Add(
                        typeKey,
                        string.Format(parameterFormat, typeKey, server.UrlEncode(t.AssemblyQualifiedName)));

                    if ((null != imageTransformations[t]) && (0 < imageTransformations[t].Count))
                    {
                        foreach (KeyValuePair<string, string> param in imageTransformations[t])
                        {
                            if (parameters.ContainsKey(param.Key) || param.Key.StartsWith(DynamicImageProvider.TRANSFTYPEPREFIX_KEY, StringComparison.InvariantCultureIgnoreCase))
                            {
                                throw new InvalidOperationException("Duplicate parameter key in query string (creation type).");
                            }

                            parameters.Add(param.Key, string.Format(parameterFormat, server.UrlEncode(param.Key), server.UrlEncode(param.Value)));
                        }
                    }
                }
            }

            return (parameters.Count > 0) ? string.Join("&", new List<string>(parameters.Values).ToArray()) : string.Empty;
        }

        /// <summary>
        /// Gets the MIME type from the image format.
        /// </summary>
        /// <param name="format">The image format.</param>
        /// <returns>the right MIME type.</returns>
        internal string ImageFormatToMimeType(ImageFormat format)
        {
            if (null == format)
            {
                throw new ArgumentNullException("format");
            }

            if (format.Equals(ImageFormat.Bmp) || format.Equals(ImageFormat.MemoryBmp))
            {
                return "image/bmp";
            }
            else if (format.Equals(ImageFormat.Emf))
            {
                return "image/x-emf";
            }
            else if (format.Equals(ImageFormat.Gif))
            {
                return "image/gif";
            }
            else if (format.Equals(ImageFormat.Jpeg))
            {
                return "image/jpeg";
            }
            else if (format.Equals(ImageFormat.Png))
            {
                return "image/x-png";
            }
            else if (format.Equals(ImageFormat.Tiff))
            {
                return "image/tiff";
            }
            else if (format.Equals(ImageFormat.Wmf))
            {
                return "image/x-wmf";
            }

            throw new NotSupportedException();
        }

        // Properties

        /// <summary>
        /// Gets the current context.
        /// </summary>
        /// <value>The current context.</value>
        protected HttpContext Context
        {
            get
            {
                if (null == this.context)
                {
                    this.context = HttpContext.Current;
                }
                return this.context;
            }
        }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        /// <value>The parameters.</value>
        protected virtual NameValueCollection Parameters
        {
            get { return this.parameters; }
            set { this.parameters = value; }
        }

        /// <summary>
        /// Gets the server cache key.
        /// </summary>
        /// <value>The server cache key.</value>
        protected virtual string ServerCacheKey
        {
            get { return this.Context.Request.QueryString.ToString(); }
        }

        #region IHttpHandler Members

        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler"/> instance.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Web.IHttpHandler"/> instance is reusable; otherwise, false.</returns>
        public bool IsReusable
        {
            get { return false; }
        }

        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler"/> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext"/> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        public void ProcessRequest(HttpContext context)
        {
            this.context = context;

            // native params

            this.Parameters = this.Context.Request.QueryString;

            if (!string.IsNullOrEmpty(this.Parameters[DynamicImageProvider.CLIENTCACHEDUR_KEY]))
            {
                this.clientCacheDuration = int.Parse(this.Parameters[DynamicImageProvider.CLIENTCACHEDUR_KEY]);
            }
            if (!string.IsNullOrEmpty(this.Parameters[DynamicImageProvider.SERVERCACHEDUR_KEY]))
            {
                this.serverCacheDuration = int.Parse(this.Parameters[DynamicImageProvider.SERVERCACHEDUR_KEY]);
            }
            if (!string.IsNullOrEmpty(this.Parameters[DynamicImageProvider.IMAGEFORMAT_KEY]))
            {
                this.imageFormat = (DynamicImageFormat)Enum.Parse(typeof(DynamicImageFormat), this.Parameters[DynamicImageProvider.IMAGEFORMAT_KEY]);
            }

            // begin the image processing

            Image image = null;
            ImageFormat imageFormat = null;

            // try to load it from server cache

            if (0 < this.serverCacheDuration)
            {
                // try to get it from the server cache;
                // the source can be pointless sometimes (in child classes), so the same source value for different images
                // the only thing that uniquely identifies an image si the query string of the image request;
                // this approach comes with a risk: the parameters in the query string should be in an exact order
                // so everything will be ok as long as the query string will be implemented in DynamicImage.ToQueryString();
                // a querystring approach strategy is required when the image request will have support from the client side

                DynamicImageCacheItem cacheItem = this.Context.Cache[this.ServerCacheKey] as DynamicImageCacheItem;
                if (null != cacheItem)
                {
                    // an identic one cached, no processing needed

                    image = cacheItem.Image;
                    imageFormat = cacheItem.ImageFormat;
                }
            }

            // the image is not cached so create and transform it

            if (null == image)
            {
                // an image creator is specified 

                if (!string.IsNullOrEmpty(this.Parameters[DynamicImageProvider.CREATIONTYPE_KEY]))
                {
                    Type t = Type.GetType(this.Parameters[DynamicImageProvider.CREATIONTYPE_KEY]);
                    IImageCreator imageCreator = Activator.CreateInstance(t) as IImageCreator;
                    image = imageCreator.Get(this.Parameters);
                }
                else
                {
                    // create the image and get the original format

                    image = this.CreateImage(this.Parameters);

                    if (null == image)
                    {
                        this.Context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        this.Context.Response.End();
                        return;
                    }
                }

                // handle the original or requested image format

                if (DynamicImageFormat.Original == this.imageFormat)
                {
                    imageFormat = image.RawFormat;
                }
                else
                {
                    switch (this.imageFormat)
                    {
                        case DynamicImageFormat.Bmp:
                            {
                                imageFormat = ImageFormat.Bmp;
                                break;
                            }
                        case DynamicImageFormat.Gif:
                            {
                                imageFormat = ImageFormat.Gif;
                                break;
                            }
                        case DynamicImageFormat.Jpeg:
                            {
                                imageFormat = ImageFormat.Jpeg;
                                break;
                            }
                        case DynamicImageFormat.Png:
                            {
                                imageFormat = ImageFormat.Png;
                                break;
                            }
                        default:
                            {
                                throw new NotSupportedException("imageFormat");
                            }
                    }
                }
                

                // transform the image (with native transformations)

                this.TransformImage(ref image, this.Parameters);

                // transform the image (with custom transformations)

                foreach (string keyParam in this.Parameters.AllKeys)
                {
                    if ((null != keyParam) && (keyParam.StartsWith(DynamicImageProvider.TRANSFTYPEPREFIX_KEY, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        Type t = Type.GetType(this.Parameters[keyParam]);
                        IImageTransformation imageTransformation = Activator.CreateInstance(t) as IImageTransformation;
                        imageTransformation.Apply(ref image, this.Parameters);
                    }
                }

                // makes sense to add it to server cache if not already

                if (0 < serverCacheDuration)
                {
                    DynamicImageCacheItem cacheItem = new DynamicImageCacheItem(image, imageFormat);
                    this.Context.Cache.Add(
                        this.ServerCacheKey,
                        cacheItem,
                        null,
                        DateTime.Now.AddMinutes(serverCacheDuration),
                        TimeSpan.Zero,
                        CacheItemPriority.Normal,
                        null);
                }
            }

            // resolve the client cache

            if (0 < clientCacheDuration)
            {
                this.Context.Response.CacheControl = "private";
                this.Context.Response.Expires = clientCacheDuration;
            }

            // finalize the request
            
            this.Context.Response.ContentType = this.ImageFormatToMimeType(imageFormat);

            // write image, avoid seekable stream issues by using an intermediate stream

            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, imageFormat);
                ms.WriteTo(this.Context.Response.OutputStream);
            }

            this.Context.Response.End();
        }

        #endregion

    }


    /// <summary>
    /// The dynamic image class to store in cache.
    /// </summary>
    public class DynamicImageCacheItem
    {

        // Fields

        private Image image;
        private ImageFormat imageFormat;


        // Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicImageCacheItem"/> struct.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="unprocessedImageFormat">The unprocessed image format.</param>
        public DynamicImageCacheItem(Image image, ImageFormat imageFormat)
        {
            if (null == image)
            {
                throw new ArgumentNullException("image");
            }
            this.image = image;
            this.imageFormat = imageFormat ?? image.RawFormat;
        }

        // Properties

        /// <summary>
        /// Gets the image.
        /// </summary>
        /// <value>The image.</value>
        public Image Image
        {
            get { return this.image; }
        }

        /// <summary>
        /// Gets the original image format.
        /// </summary>
        /// <value>The unprocessed image format.</value>
        public ImageFormat ImageFormat
        {
            get { return this.imageFormat; }
        }

    }


    /// <summary>
    /// Defines an image creator.
    /// </summary>
    public interface IImageCreator
    {

        /// <summary>
        /// Creates an image according to the specified params.
        /// </summary>
        /// <param name="parameters">The creator params.</param>
        /// <returns>A new image based on the params.</returns>
        Image Get(NameValueCollection parameters);

    }


    /// <summary>
    /// Represents an image creator for a localized image.
    /// </summary>
    public sealed class LocalizedImageCreation : IImageCreator
    {

        internal const string SOURCE_KEY = "src";

        #region IImageCreator Members

        /// <summary>
        /// Performs an localized image creation according to the specified params.
        /// </summary>
        /// <param name="parameters">The creation params.</param>
        /// <returns>
        /// A new image based on the creation params.
        /// </returns>
        public Image Get(NameValueCollection parameters)
        {
            if (null == parameters)
            {
                throw new ArgumentNullException("parameters");
            }
            if (string.IsNullOrEmpty(parameters[LocalizedImageCreation.SOURCE_KEY]))
            {
                throw new ArgumentException("parameters should contain the image source.");
            }

            string source = parameters[LocalizedImageCreation.SOURCE_KEY];

            if (source.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
            {
                using (WebClient client = new WebClient())
                {
                    return Image.FromStream(client.OpenRead(source));
                }
            }
            else
            {
                return Image.FromFile(HttpContext.Current.Server.MapPath(source));
            }
        }

        #endregion

    }


    /// <summary>
    /// Defines an image transformation.
    /// </summary>
    public interface IImageTransformation
    {

        /// <summary>
        /// Applies the specified image transformation.
        /// </summary>
        /// <param name="image">The image to be transfored.</param>
        /// <param name="parameters">The transformation params.</param>
        void Apply(ref Image image, NameValueCollection parameters);

    }


    /// <summary>
    /// Implements a resize image transformation.
    /// </summary>
    public sealed class ResizeImageTransformation : IImageTransformation
    {

        // Fields

        internal const string SIZEW_KEY = "rsw";
        internal const string SIZEH_KEY = "rsh";

        // Methods

        internal static bool ParseSizeParam(NameValueCollection parameters, out Size size)
        {
            if (null != parameters)
            {
                int width = !string.IsNullOrEmpty(parameters[ResizeImageTransformation.SIZEW_KEY]) ? int.Parse(parameters[ResizeImageTransformation.SIZEW_KEY]) : 0;
                int height = !string.IsNullOrEmpty(parameters[ResizeImageTransformation.SIZEH_KEY]) ? int.Parse(parameters[ResizeImageTransformation.SIZEH_KEY]) : 0;

                if ((0 < width) || (0 < height))
                {
                    size = new Size(width, height);
                }
                else
                {
                    size = Size.Empty;
                }
            }
            else
            {
                size = Size.Empty;
            }

            return Size.Empty != size;
        }

        #region IImageTransformation Members

        /// <summary>
        /// Applies the specified image transformation.
        /// </summary>
        /// <param name="image">The image to be transfored.</param>
        /// <param name="parameters">The transformation params.</param>
        public void Apply(ref Image image, NameValueCollection parameters)
        {
            if (null == image)
            {
                throw new ArgumentNullException("image");
            }

            Size size;
            if (ResizeImageTransformation.ParseSizeParam(parameters, out size) && (size != image.Size))
            {
                if (0 == size.Height)
                {
                    image = image.GetThumbnailImage(size.Width, (int)Math.Round((decimal)image.Height * size.Width / image.Width), delegate() { return false; }, IntPtr.Zero);
                }
                else if (0 == size.Width)
                {
                    image = image.GetThumbnailImage((int)Math.Round((decimal)image.Width * size.Height / image.Height), size.Height, delegate() { return false; }, IntPtr.Zero);
                }
                else
                {
                    image = image.GetThumbnailImage(size.Width, size.Height, delegate() { return false; }, IntPtr.Zero);
                }
            }
        }

        #endregion

    }


    /// <summary>
    /// Implements a rotate-flip image transformation.
    /// </summary>
    public sealed class RotateFlipImageTransformation : IImageTransformation
    {

        // Fields

        internal const string ROTATETYPE_KEY = "rft";

        // Methods

        internal static bool ParseRotateParam(NameValueCollection parameters, out RotateFlipType rotateType)
        {
            if ((null != parameters) && !string.IsNullOrEmpty(parameters[RotateFlipImageTransformation.ROTATETYPE_KEY]))
            {
                rotateType = (RotateFlipType)Enum.Parse(typeof(RotateFlipType), parameters[RotateFlipImageTransformation.ROTATETYPE_KEY]);
                return true;
            }
            else
            {
                rotateType = RotateFlipType.RotateNoneFlipNone;
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
            if (null == image)
            {
                throw new ArgumentNullException("image");
            }

            RotateFlipType rotateType;
            if (RotateFlipImageTransformation.ParseRotateParam(parameters, out rotateType))
            {
                image.RotateFlip(rotateType);
            }
        }

        #endregion

    }


    /// <summary>
    /// Implements a grayscale image transformation.
    /// </summary>
    public sealed class GrayscaleImageTransformation : IImageTransformation
    {

        // Fields

        internal const string GRAYSCALE_KEY = "gys";

        private static readonly ColorMatrix grayMatrix = new ColorMatrix(new float[][] {
            new float[] {0.3f, 0.3f, 0.3f, 0f, 0f},
            new float[] {0.59f, 0.59f, 0.59f, 0f, 0f},
            new float[] {0.11f, 0.11f, 0.11f, 0f, 0f},
            new float[] {0f, 0f, 0f, 1f, 0f},
            new float[] {0f, 0f, 0f, 0f, 1f} });

        // Methods

        internal static bool ParseGrayscaleParam(NameValueCollection parameters, out bool drawGrayscale)
        {
            if ((null != parameters) && !string.IsNullOrEmpty(parameters[GrayscaleImageTransformation.GRAYSCALE_KEY]))
            {
                drawGrayscale = bool.Parse(parameters[GrayscaleImageTransformation.GRAYSCALE_KEY]);
                return true;
            }
            else
            {
                drawGrayscale = false;
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
            if (null == image)
            {
                throw new ArgumentNullException("image");
            }

            bool drawGrayscale;
            if (GrayscaleImageTransformation.ParseGrayscaleParam(parameters, out drawGrayscale) && drawGrayscale)
            {
                using (Graphics graphics = Graphics.FromImage(image))
                {
                    using (ImageAttributes attributes = new ImageAttributes())
                    {
                        attributes.SetColorMatrix(GrayscaleImageTransformation.grayMatrix);
                        graphics.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
                    }
                }
            }
        }

        #endregion

    }


    /// <summary>
    /// Implements a sepia image transformation.
    /// </summary>
    public sealed class SepiaImageTransformation : IImageTransformation
    {

        // Fields

        internal const string SEPIA_KEY = "sep";

        private static readonly ColorMatrix sepiaMatrix = new ColorMatrix(new float[][] {
                new float[] {0.39f, 0.35f, 0.27f, 0f, 0f},
                new float[] {0.77f, 0.68f, 0.53f, 0f, 0f},
                new float[] {0.19f, 0.17f, 0.13f, 0f, 0f},
                new float[] {0f, 0f, 0f, 1f, 0f},
                new float[] {0f, 0f, 0f, 0f, 1f} });

        // Methods

        internal static bool ParseSepiaParam(NameValueCollection parameters, out bool drawSepia)
        {
            if ((null != parameters) && !string.IsNullOrEmpty(parameters[SepiaImageTransformation.SEPIA_KEY]))
            {
                drawSepia = bool.Parse(parameters[SepiaImageTransformation.SEPIA_KEY]);
                return true;
            }
            else
            {
                drawSepia = false;
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
            if (null == image)
            {
                throw new ArgumentNullException("image");
            }

            bool drawSepia;
            if (SepiaImageTransformation.ParseSepiaParam(parameters, out drawSepia) && drawSepia)
            {
                using (Graphics graphics = Graphics.FromImage(image))
                {
                    using (ImageAttributes attributes = new ImageAttributes())
                    {
                        attributes.SetColorMatrix(SepiaImageTransformation.sepiaMatrix);
                        graphics.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
                    }
                }
            }
        }

        #endregion

    }


    /// <summary>
    /// Implements a text image transformation.
    /// </summary>
    public sealed class TextImageTransformation : IImageTransformation
    {

        // Fields

        internal const string TEXTVALUE_KEY = "txv";
        internal const string TEXTFONT_KEY = "txf";
        internal const string TEXTCOLOR_KEY = "txc";
        internal const string TEXTWIDTHALIGN_KEY = "txw";
        internal const string TEXTHEIGHTALIGN_KEY = "txh";

        // Methods

        internal static bool ParseTextParam(NameValueCollection parameters, out DynamicText text)
        {
            if ((null != parameters) && !string.IsNullOrEmpty(parameters[TextImageTransformation.TEXTVALUE_KEY]))
            {
                text = new DynamicText();
                text.Value = parameters[TextImageTransformation.TEXTVALUE_KEY];
                if (!string.IsNullOrEmpty(parameters[TextImageTransformation.TEXTFONT_KEY]))
                {
                    text.Font = (Font)new FontConverter().ConvertFromString(parameters[TextImageTransformation.TEXTFONT_KEY]);
                }
                if (!string.IsNullOrEmpty(parameters[TextImageTransformation.TEXTCOLOR_KEY]))
                {
                    text.Color = Color.FromArgb(int.Parse(parameters[TextImageTransformation.TEXTCOLOR_KEY]));
                }
                if (!string.IsNullOrEmpty(parameters[TextImageTransformation.TEXTHEIGHTALIGN_KEY]))
                {
                    text.HorizontalAlign = (StringAlignment)Enum.Parse(typeof(StringAlignment), parameters[TextImageTransformation.TEXTHEIGHTALIGN_KEY]);
                }
                if (!string.IsNullOrEmpty(parameters[TextImageTransformation.TEXTWIDTHALIGN_KEY]))
                {
                    text.VerticalAlign = (StringAlignment)Enum.Parse(typeof(StringAlignment), parameters[TextImageTransformation.TEXTWIDTHALIGN_KEY]);
                }
                return true;
            }
            else
            {
                text = null;
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
            if (null == image)
            {
                throw new ArgumentNullException("image");
            }

            DynamicText text;
            if (TextImageTransformation.ParseTextParam(parameters, out text))
            {
                using (Graphics g = Graphics.FromImage(image))
                {
                    SizeF size = g.MeasureString(text.Value, text.Font);

                    PointF pt = new PointF();
                    switch (text.HorizontalAlign)
                    {
                        case StringAlignment.Near:
                            {
                                pt.X = 0;
                                break;
                            }
                        case StringAlignment.Center:
                            {
                                pt.X = (image.Width - size.Width) / 2;
                                break;
                            }
                        case StringAlignment.Far:
                            {
                                pt.X = image.Width - size.Width;
                                break;
                            }
                        default:
                            {
                                pt.X = image.Width - size.Width;
                                break;
                            }
                    }

                    switch (text.VerticalAlign)
                    {
                        case StringAlignment.Near:
                            {
                                pt.Y = 0;
                                break;
                            }
                        case StringAlignment.Center:
                            {
                                pt.Y = (image.Height - size.Height) / 2;
                                break;
                            }
                        case StringAlignment.Far:
                            {
                                pt.Y = image.Height - size.Height;
                                break;
                            }
                        default:
                            {
                                pt.Y = image.Height - size.Height;
                                break;
                            }
                    }

                    g.TextRenderingHint = TextRenderingHint.AntiAlias;
                    using (Brush brush = new SolidBrush(text.Color))
                    {
                        g.DrawString(text.Value, text.Font, brush, pt);
                    }
                }
            }
        }

        #endregion

    }

}
