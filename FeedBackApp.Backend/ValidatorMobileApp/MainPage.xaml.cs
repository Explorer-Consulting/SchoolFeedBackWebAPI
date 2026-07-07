using ValidatorMobileApp.Rest;
using Newtonsoft.Json;

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

                try
                {
                    var code = JsonConvert.DeserializeObject<QRCodeContent>(args.Result[0].Text);
                    if (code == null)
                    {
                        label.Text = "QR code was not recognized az proper\nvalidation code.";
                        return;
                    }
                    var isValid = await RestService.ValidateFromQRCodeAsync(code.id);
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
                        label.Text = "An error occured while trying to validate.\nPlease try again.";
                    }

                }
                catch (Exception e)
                {
                    label.Text = "QR code was not recognized\nas a proper validation code.";
                }
                finally
                {
                    indicator.IsRunning = false;
                    indicator.IsVisible = false;
                    label.IsVisible = true;
                    cameraView.BarCodeDetectionEnabled = true;
                }
            });
        }
    }
}
