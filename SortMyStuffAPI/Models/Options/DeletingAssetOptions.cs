namespace SortMyStuffAPI.Models
{
    public class DeletingAssetOptions
    {
        /// <summary>
        /// Set it to true if the deletion only applies to the asset itself 
        /// but not to the contents/children. If true, the content assets will be moved 
        /// to the container of the deleted asset.
        /// </summary>
        public bool OnlySelf { get; set; }
    }
}
