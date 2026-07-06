namespace ValidatorMobileApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            cameraView.BarCodeOptions = new()
            {
                TryInverted=true,
                AutoRotate = true,
                TryHarder = true,
                PossibleFormats = { Camera.MAUI.BarcodeFormat.QR_CODE }
            };
            cameraView.BarCodeDecoder = new Camera.MAUI.ZXing.ZXingBarcodeDecoder();
            cameraView.BarcodeDetected += cameraView_BarcodeDetected;
        }

       
        private void cameraView_CamerasLoaded(object sender, EventArgs e)
        {
            Console.WriteLine("Camera loaded");

            if (cameraView.NumCamerasDetected <= 0)
            {
                Console.WriteLine("No active camerase");
                return;
            }

            cameraView.Camera = cameraView.Cameras.FirstOrDefault();
            MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await cameraView.StopCameraAsync();
                await cameraView.StartCameraAsync();
            });
        }

        private void buttonToClick_Clicked(object sender, EventArgs e)
        {
            barcodeResult.Text = "Click";
        }

        private void cameraView_BarcodeDetected(object sender, Camera.MAUI.ZXingHelper.BarcodeEventArgs args)
        {
            Console.WriteLine("asd");
            MainThread.BeginInvokeOnMainThread(() =>
            {
                barcodeResult.Text = args.Result[0].Text;
            });
        }
    }
}
