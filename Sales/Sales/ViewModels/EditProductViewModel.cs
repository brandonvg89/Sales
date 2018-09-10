namespace Sales.ViewModels
{
    using Common.Models;

    public class EditProductViewModel
    {
        #region Attributes
        private Product product; 
        #endregion

        #region Constructors
        public EditProductViewModel(Product product)
        {
            this.product = product;
        } 
        #endregion
    }
}
