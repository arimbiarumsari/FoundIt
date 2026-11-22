using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace FoundIt.Desktop.Forms;

internal static class ImageHelper {
    private const int MaxSide = 1024;

    // Baca file gambar, perkecil, lalu kembalikan sebagai JPEG (byte[]).
    public static byte[] PrepareForUpload(string path) {
        using var original = Image.FromFile(path);
        var scale = Math.Min(1.0, (double)MaxSide / Math.Max(original.Width, original.Height));
        var width = Math.Max(1, (int)(original.Width * scale));
        var height = Math.Max(1, (int)(original.Height * scale));

        using var resized = new Bitmap(width, height);
        using (var graphics = Graphics.FromImage(resized)) {
            graphics.Clear(Color.White);                       // untuk PNG transparan
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.DrawImage(original, 0, 0, width, height);
        }

        var jpeg = ImageCodecInfo.GetImageEncoders().First(e => e.FormatID == ImageFormat.Jpeg.Guid);
        using var parameters = new EncoderParameters(1);
        parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 80L);
        using var stream = new MemoryStream();
        resized.Save(stream, jpeg, parameters);
        return stream.ToArray();
    }

    // Ubah byte[] dari database menjadi Image untuk PictureBox (null kalau tidak ada gambar).
    public static Image? FromBytes(byte[]? data) {
        if (data is not { Length: > 0 }) return null;
        using var stream = new MemoryStream(data);
        using var image = Image.FromStream(stream);
        return new Bitmap(image);                              // salinan, agar stream boleh ditutup
    }
}