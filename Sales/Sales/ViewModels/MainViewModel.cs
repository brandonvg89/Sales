namespace Sales.ViewModels
{
    using System.Windows.Input;
    using GalaSoft.MvvmLight.Command;
    using Views;
    using Xamarin.Forms;

    public class MainViewModel
    {

        #region Properties
        public ProductsViewModel Products { get; set; }
        public AddProductViewModel AddProduct { get; set; }
        #endregion

        #region Constructors
        public MainViewModel()
        {
            this.Products = new ProductsViewModel();
        }
        #endregion

        #region Commands
        public ICommand AddProductCommand
        {
            get
            {
                return new RelayCommand(GoToAddProduct);
            }
        }
        #endregion

        private async void GoToAddProduct()
        {
            this.AddProduct = new AddProductViewModel();
            await Application.Current.MainPage.Navigation.PushAsync(new AddProductPage());
        }
    }
}
