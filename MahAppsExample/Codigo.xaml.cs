using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using QRCoder;
using System.Drawing;
using System.IO;

namespace HS5
{
    /// <summary>
    /// Interaction logic for Codigo.xaml
    /// </summary>
    public partial class Codigo : Window
    {
        string id_rem;
        string icon = "";
        public Codigo(string Id_Remedio)
        {
            InitializeComponent();
            id_rem = Id_Remedio; //Pasa id del remedio para generar QR
            RenderQrCode();
        }

        private void RenderQrCode()
        {
            //Levels (L, M, Q, H) - Default - L
            string level = "L";
            QRCodeGenerator.ECCLevel eccLevel = (QRCodeGenerator.ECCLevel)(level == "L" ? 0 : level == "M" ? 1 : level == "Q" ? 2 : 3);
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(id_rem, eccLevel))
                {
                    using (QRCode qrCode = new QRCode(qrCodeData))
                    {
                        image.Source = Convert(qrCode.GetGraphic(20, System.Drawing.Color.Black, System.Drawing.Color.White,
                            GetIconBitmap(), 10));
                    }
                }
            }
        }

        public BitmapImage Convert(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            ((System.Drawing.Bitmap)src).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }

        private Bitmap GetIconBitmap()
        {
            Bitmap img = null;
            if (icon.Length > 0)
            {
                try
                {
                    img = new Bitmap(icon);
                }
                catch (Exception)
                {
                }
            }
            return img;
        }
    }
}
