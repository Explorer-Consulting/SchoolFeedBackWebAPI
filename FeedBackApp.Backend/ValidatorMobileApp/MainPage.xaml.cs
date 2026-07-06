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
            MainThread.BeginInvokeOnMainThread(() =>
            {
                IsLoading = !IsLoading;
            });
        }
    }
}
