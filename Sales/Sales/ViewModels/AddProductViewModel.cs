namespace Sales.ViewModels
{
    using System.Windows.Input;
    using Common.Models;
    using GalaSoft.MvvmLight.Command;
    using Helpers;
    using Plugin.Media.Abstractions;
    using Services;
    using Xamarin.Forms;

    public class AddProductViewModel : BaseViewModel
    {
        #region Attributes
        private ApiService apiService;
        private bool isRunning;
        private bool isEnabled;
        private ImageSource imageSource;
        private MediaFile file;
        #endregion

        #region Properties

        public string Description { get; set; }
        public string Price { get; set; }
        public string Remarks { get; set; }
        public bool IsRunning {
            get
            {
                return this.isRunning;
            }
            set
            {
                this.SetValue(ref this.isRunning, value);
            }
        }
        public bool IsEnabled
        {
            get { return this.isEnabled; }
            set
            {
                this.SetValue(ref this.isEnabled, value);
            }
        }

        public ImageSource ImageSource
        {
            get { return this.imageSource; }
            set
            {
                this.SetValue(ref this.imageSource, value);
            }
        }
        #endregion

        #region Constructors
        public AddProductViewModel()
        {
            this.apiService = new ApiService();
            this.IsEnabled = true;
            this.imageSource = "noproduct";
        }
        #endregion
        
        #region Commands
        public ICommand ChangeImageCommand
        {
            get
            {
                return new RelayCommand(ChangeImage);
            }
        }

        private async void ChangeImage()
        {
            await Plugin.Media.CrossMedia.Current.Initialize();
            var source = await Application.Current.MainPage.DisplayActionSheet(
                Languages.ImageSource,
                Languages.Cancel,
                null,
                Languages.FromGallery,
                Languages.NewPicture);
            if (source == Languages.Cancel)
            {
                this.file = null;
                return;
            }
            if (source == Languages.NewPicture)
            {
                this.file = await Plugin.Media.CrossMedia.Current.TakePhotoAsync(
                    new StoreCameraMediaOptions
                    {
                        Directory = "Sample",
                        Name = "test.jpg",
                        PhotoSize = PhotoSize.Small,
                    });
            }
            else
            {
                this.file = await Plugin.Media.CrossMedia.Current.PickPhotoAsync();
            }
            if (this.file != null)
            {
                this.ImageSource = ImageSource.FromStream(() =>
                { var stream = file.GetStream(); return stream; });
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                return new RelayCommand(Save);
            }
        }

        private async void Save()
        {
            if (string.IsNullOrEmpty(this.Description))
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "You must enter a description",
                    "Accept");
                return;
            }

            if (string.IsNullOrEmpty(this.Price))
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "You must enter a price",
                    "Accept");
                return;
            }

            var price = decimal.Parse(this.Price);
            if (price <= 0)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "You must enter a price greater than zero",
                    "Accept");
                return;
            }

            this.isEnabled = false;
            this.isRunning = true;

            var connection = await this.apiService.CheckConnection();

            if (!connection.IsSuccess)
            {
                this.isEnabled = true;
                this.isRunning = false;
                await Application.Current.MainPage.DisplayAlert(Languages.Error, connection.Message, Languages.Accept);
                return;
            }

            byte[] imageArray = null;
            if (this.file != null) { imageArray = FilesHelper.ReadFully(this.file.GetStream()); }
            var product = new Product{
                Description = this.Description,
                Price = price,
                Remarks = this.Remarks,
                ImageArray = imageArray,
            };

            //var product = new Product
            //{
            //    Description = this.Description,
            //    Price = price,
            //    Remarks = this.Remarks,
            //};


            var url = Application.Current.Resources["UrlAPI"].ToString();
            var prefix = Application.Current.Resources["UrlPrefix"].ToString();
            var controller = Application.Current.Resources["UrlProductsController"].ToString();
            var response = await this.apiService.Post(url, prefix, controller, product);

            if (!response.IsSuccess)
            {
                this.isEnabled = true;
                this.isRunning = false;
                await Application.Current.MainPage.DisplayAlert(
                    Languages.Error,
                    response.Message, 
                    Languages.Accept);
                return;
            }
            var newProduct = (Product)response.Result;
            var productsViewModel = ProductsViewModel.GetInstance();
            productsViewModel.MyProducts.Add(newProduct);
            productsViewModel.RefresList();

            this.isEnabled = true;
            this.isRunning = false;
            await Application.Current.MainPage.Navigation.PopAsync();
        }
        #endregion
    }
}
        


