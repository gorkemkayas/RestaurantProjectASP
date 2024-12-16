namespace deneme.Models
{
    public class MenuItem
    {
        public int MenuItemId { get; set; }
        public int MenuId { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public string Description { get; set; }
        public string MenuName { get; internal set; }
    }
}
