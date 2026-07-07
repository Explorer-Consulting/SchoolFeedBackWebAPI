namespace ValidatorMobileApp
{
    public partial class MainPage : ContentPage
    {
        public static readonly BindableProperty IsLoadingProperty =
            BindableProperty.Create(nameof(IsLoading), typeof(bool), typeof(MainPage), false);

        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

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

        private void cameraView_BarcodeDetected(object sender, Camera.MAUI.ZXingHelper.BarcodeEventArgs args)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                cameraView.BarCodeDetectionEnabled = false;
                label.IsVisible = false;
                indicator.IsVisible = true;
                indicator.IsRunning = true;

                var code = args.Result[0].Text;
                var isValid = await RestService.ValidateFromQRCodeAsync(code);
                if (isValid.Equals("success"))
                {
                    label.Text = "Validation Successful!";
                }
                else if (isValid.Equals("fail"))
                {
                    label.Text = "Validation failed!";
                } 
                else
                {
                    label.Text = "An error occured while trying to validate.\n Please try again.";
                }

                indicator.IsRunning = false;
                indicator.IsVisible = false;
                label.IsVisible = true;
                cameraView.BarCodeDetectionEnabled = true;
            });
        }
    }
}
