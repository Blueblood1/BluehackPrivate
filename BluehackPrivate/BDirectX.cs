using System;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Drawing.Imaging;
using SharpDX;
using SharpDX.DirectWrite;
using SharpDX.Direct2D1;
using Factory = SharpDX.Direct2D1.Factory;
using FontFactory = SharpDX.DirectWrite.Factory;
using Format = SharpDX.DXGI.Format;
using DrawTypes = SharpDX.Mathematics.Interop;

namespace SubService
{
    internal partial class BDirectX : Form
    {
        #region Imports
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        [DllImport("dwmapi.dll")]
        public static extern void DwmExtendFrameIntoClientArea(IntPtr hWnd, ref int[] pMargins);
        #endregion

        #region Initalisers
        private IntPtr handle;
        private WindowRenderTarget device;

        private SolidColorBrush brushSolidYellow;
        private SolidColorBrush brushSolidRed;
        private SolidColorBrush brushSolidGreenYellow;
        private SolidColorBrush brushSolidGreen;
        private SolidColorBrush brushTransparentRed;

        #region Bitmaps
        private Bitmap bmKevlar;
        private Bitmap bmKevlarHelmet;
        private Bitmap bmBomb;
        private Bitmap bmDefuser;

        private Bitmap bmWeaponAK;
        private Bitmap bmWeaponM4A1;
        private Bitmap bmWeaponGlock;
        private Bitmap bmWeaponKnife;
        private Bitmap bmWeaponUSP;

        private Bitmap bmGrenadeSmoke;
        private Bitmap bmGrenadeFlashbang;
        private Bitmap bmGrenadeHE;
        private Bitmap bmGrenadeDecoy;
        private Bitmap bmGrenadeMolotov;
        #endregion
        #endregion

        #region Setup
        public BDirectX()
        {
            this.handle = Handle;
            SetWindowLong(this.Handle, -20, GetWindowLong(this.Handle, -20) | 0x80000 | 0x20);
            SetWindowPos(this.Handle, (IntPtr)(-1), 0, 0, 0, 0, 0x0001 | 0x0002);
            OnResize(null);

            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;

            InitializeComponent();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80;
                return cp;
            }
        }

        private void BDirectX_Load(object sender, EventArgs e)
        {
            this.ShowInTaskbar = false;

            this.Width = 1280;
            this.Height = 1024;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.Opaque | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);

            HwndRenderTargetProperties renderProperties = new HwndRenderTargetProperties()
            {
                Hwnd = this.Handle,
                PixelSize = new Size2(1280, 1024),
                PresentOptions = PresentOptions.None
            };

            Factory factory = new Factory();
            device = new WindowRenderTarget(factory, new RenderTargetProperties(new SharpDX.Direct2D1.PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)), renderProperties);
            DrawingInit();

            Thread BDirectX = new Thread(new ParameterizedThreadStart(ThreadDirectX));
            BDirectX.Priority = ThreadPriority.Highest;
            BDirectX.IsBackground = true;
            BDirectX.Start();
        }

        private SharpDX.Direct2D1.Bitmap LoadFromFile(RenderTarget device, System.Drawing.Bitmap res)
        {
            using (var bitmap = new System.Drawing.Bitmap(res))
            {
                var sourceArea = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
                var bitmapProperties = new BitmapProperties(new SharpDX.Direct2D1.PixelFormat(Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied));
                var size = new Size2(bitmap.Width, bitmap.Height);

                int stride = bitmap.Width * sizeof(int);
                using (var tempStream = new DataStream(bitmap.Height * stride, true, true))
                {
                    var bitmapData = bitmap.LockBits(sourceArea, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        int offset = bitmapData.Stride * y;
                        for (int x = 0; x < bitmap.Width; x++)
                        {
                            byte B = Marshal.ReadByte(bitmapData.Scan0, offset++);
                            byte G = Marshal.ReadByte(bitmapData.Scan0, offset++);
                            byte R = Marshal.ReadByte(bitmapData.Scan0, offset++);
                            byte A = Marshal.ReadByte(bitmapData.Scan0, offset++);
                            int rgba = R | (G << 8) | (B << 16) | (A << 24);
                            tempStream.Write(rgba);
                        }

                    }
                    bitmap.UnlockBits(bitmapData);
                    tempStream.Position = 0;

                    return new SharpDX.Direct2D1.Bitmap(device, size, tempStream, stride, bitmapProperties);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            int[] pMargins = new int[] { 0, 0, Width, Height };
            DwmExtendFrameIntoClientArea(this.Handle, ref pMargins);
            this.SetDesktopLocation(0, 0);
            this.TopMost = true;
        }
        #endregion

        private void DrawingInit()
        {
            brushSolidRed = new SolidColorBrush(device, Color.Red);
            brushSolidYellow = new SolidColorBrush(device, Color.Yellow);
            brushSolidGreenYellow = new SolidColorBrush(device, Color.GreenYellow);
            brushSolidGreen = new SolidColorBrush(device, Color.Green);
            brushTransparentRed = new SolidColorBrush(device, new DrawTypes.RawColor4(0.7F, 0.1F, 0.26F, 0.55F));

            bmGrenadeSmoke = LoadFromFile(device, BResource.GrenadeSmoke);
            bmGrenadeFlashbang = LoadFromFile(device, BResource.GrenadeFlashbang);
            bmGrenadeHE = LoadFromFile(device, BResource.GrenadeHE);
            bmGrenadeDecoy = LoadFromFile(device, BResource.GrenadeDecoy);
            bmGrenadeMolotov = LoadFromFile(device, BResource.GrenadeMolotov);
        }

        #region Player Draws
        private void DrawPlayerHealth(float x, float y, float distance, float value)
        {
            float width = 2000 / distance;
            float height = 55000 / distance;

            float offsetX = 5000 / distance;
            float offsetY = offsetX / 2;

            x += (20000 / distance) / 2;
            y -= 4500 / distance;
            x += offsetX;
            y += offsetY;

            DrawTypes.RawRectangleF espHealthRectRed = new DrawTypes.RawRectangleF(x, y, x + width, y + height);
            device.FillRectangle(espHealthRectRed, brushSolidRed);

            DrawTypes.RawRectangleF espHealthRectGreen = new DrawTypes.RawRectangleF(x, y, x + width, y + ((height / 100) * value));
            device.FillRectangle(espHealthRectGreen, brushSolidGreen);
        }

        private void DrawPlayerHead(float x, float y, float distance)
        {
            float radius = 4000 / distance;

            DrawTypes.RawVector2 HeadPos = new DrawTypes.RawVector2(x, y);

            Ellipse HeadCricle = new Ellipse(HeadPos, radius, radius);
            device.DrawEllipse(HeadCricle, brushSolidGreenYellow);
        }

        private void DrawPlayerBones(float[] bones)
        {
            for (int i = 0; i < bones.Length; i++)
            {
                if (i % 4 == 0)
                {
                    DrawTypes.RawVector2 BoneStart = new DrawTypes.RawVector2(bones[i], bones[i + 1]);
                    DrawTypes.RawVector2 BoneEnd = new DrawTypes.RawVector2(bones[i + 2], bones[i + 3]);
                    device.DrawLine(BoneStart, BoneEnd, brushTransparentRed);
                }
            }
        }

        private void DrawPlayerCrosshair(float yOffset, float xOffset)
        {
            float x = 1280 / 2;
            float y = 1024 / 2;
            float dy = 1024 / 90;
            float dx = 1280 / 90;
            x -= (dx * (xOffset));
            y += (dy * (yOffset));

            DrawTypes.RawVector2 CrosshairHStart = new DrawTypes.RawVector2(x - 11, y);
            DrawTypes.RawVector2 CrosshairHEnd = new DrawTypes.RawVector2(x - 3, y);
            device.DrawLine(CrosshairHStart, CrosshairHEnd, brushSolidYellow, 2);

            DrawTypes.RawVector2 CrosshairVStart = new DrawTypes.RawVector2(x, y - 11);
            DrawTypes.RawVector2 CrosshairVEnd = new DrawTypes.RawVector2(x, y - 3);
            device.DrawLine(CrosshairVStart, CrosshairVEnd, brushSolidYellow, 2);

            DrawTypes.RawVector2 CrosshairH1Start = new DrawTypes.RawVector2(x + 11, y);
            DrawTypes.RawVector2 CrosshairH1End = new DrawTypes.RawVector2(x + 3, y);
            device.DrawLine(CrosshairH1Start, CrosshairH1End, brushSolidYellow, 2);

            DrawTypes.RawVector2 CrosshairV1Start = new DrawTypes.RawVector2(x, y + 11);
            DrawTypes.RawVector2 CrosshairV1End = new DrawTypes.RawVector2(x, y + 3);
            device.DrawLine(CrosshairV1Start, CrosshairV1End, brushSolidYellow, 2);
        }
        #endregion

        #region Entity Draws
        private void DrawEntityGrenade(float x, float y, float distance, int ClassID, string Model)
        {
            float width = 20;
            float height = 30;

            x -= width / 2;
            y -= 4500 / distance;

            DrawTypes.RawRectangleF ImageArea = new DrawTypes.RawRectangleF(x, y, x + width, y + height);
            switch (ClassID)
            {
                case (int)BHelper.AcceptedGrenadeIDS.CSmokeGrenadeProjectile:
                    device.DrawBitmap(bmGrenadeSmoke, ImageArea, 1.0f, BitmapInterpolationMode.Linear);
                    break;
                case (int)BHelper.AcceptedGrenadeIDS.CBaseCSGrenadeProjectile:
                    if (Model.Contains("flash"))
                        device.DrawBitmap(bmGrenadeFlashbang, ImageArea, 1.0f, BitmapInterpolationMode.Linear);
                    else
                        device.DrawBitmap(bmGrenadeHE, ImageArea, 1.0f, BitmapInterpolationMode.Linear);
                    break;
                case (int)BHelper.AcceptedGrenadeIDS.CDecoyProjectile:
                    device.DrawBitmap(bmGrenadeDecoy, ImageArea, 1.0f, BitmapInterpolationMode.Linear);
                    break;
                case (int)BHelper.AcceptedGrenadeIDS.CMolotovProjectile:
                    device.DrawBitmap(bmGrenadeMolotov, ImageArea, 1.0f, BitmapInterpolationMode.Linear);
                    break;
            }
        }
        #endregion

        private void ThreadDirectX(object sender)
        {
            while (true)
            {
                device.BeginDraw();
                device.Clear(Color.Transparent);
                device.TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Aliased;

                List<BHelper.PlayerDraw> PlayerDataBuffer = Bluehack.PlayerDraws;
                for (int i = 0; i < PlayerDataBuffer.Count; i++)
                {
                    BHelper.PlayerDraw PlayerData = PlayerDataBuffer[i];

                    DrawPlayerHealth(PlayerData.espX, PlayerData.espY, PlayerData.espDistance, PlayerData.PlayerHealth);
                    DrawPlayerHead(PlayerData.espX, PlayerData.espY, PlayerData.espDistance);
                    DrawPlayerBones(PlayerData.Bones);
                }

                List<BHelper.EntityDraw> EntityDataBuffer = Bluehack.EntityDraws;
                for (int i = 0; i < EntityDataBuffer.Count; i++)
                {
                    BHelper.EntityDraw EntityData = EntityDataBuffer[i];
                    if (Enum.IsDefined(typeof(BHelper.AcceptedGrenadeIDS), EntityData.EntityClassID))
                    {
                        DrawEntityGrenade(EntityData.espX, EntityData.espY, EntityData.espDistance, EntityData.EntityClassID, EntityData.EntityModel);
                    }
                }

                float[] Punch = BHelper.LocalPlayer.GetPunch();
                DrawPlayerCrosshair(Punch[0], Punch[1]);

                device.EndDraw();
                Thread.Sleep(10);
            }
        }
    }
}