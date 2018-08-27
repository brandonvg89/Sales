namespace Sales.ViewModels
{
    using System.Collections.ObjectModel;
    using System.Windows.Input;
    using GalaSoft.MvvmLight.Command;
    using Helpers;
    using Sales.Common.Models;
    using Services;
    using Xamarin.Forms;

    public class AddProductViewModel : BaseViewModel
    {
        #region Attributes
        private ApiService apiService;
        //private ObservableCollection<Product> products;
        //private ObservableCollection<Product> Products;
        private bool isRunning;
        private bool isEnabled;
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
        #endregion

        #region Constructors
        public AddProductViewModel()
        {
            this.apiService = new ApiService();
            this.IsEnabled = true;
        }
        #endregion

        #region Methods

        #endregion

        #region Commands
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

            var product = new Product
            {
                Description = this.Description,
                Price = price,
                Remarks = this.Remarks,
            };


            var url = Application.Current.Resources["UrlAPI"].ToString();
            var prefix = Application.Current.Resources["UrlPrefix"].ToString();
            var controller = Application.Current.Resources["UrlProductsController"].ToString();
            var response = await this.apiService.Post(url, prefix, controller, product);

            if (!response.IsSuccess)
            {
                this.isEnabled = true;
                this.isRunning = false;
                await Application.Current.MainPage.DisplayAlert(Languages.Error, response.Message, Languages.Accept);
                return;
            }
            var newProduct = (Product)response.Result;
            var viewModel = ProductsViewModel.GetInstance();
            viewModel.Products.Add(newProduct);


            this.isEnabled = true;
            this.isRunning = false;
            await Application.Current.MainPage.Navigation.PopAsync();
        }
        #endregion
    }
}
        


