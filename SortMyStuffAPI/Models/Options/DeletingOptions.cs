namespace SortMyStuffAPI.Models
{
    public class DeletingOptions
    {
        /// <summary>
        /// If this option is true, then the dependents of the 
        /// deleting resource will be deleted as well.
        /// </summary>
        public bool DelDependents { get; set; }
    }
}
