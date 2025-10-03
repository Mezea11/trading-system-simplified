class Item
{
    public int Id;
    public string Owner;     // username
    public string Name;
    public string Description;

    public Item(int id, string owner, string name, string description)
    {
        Id = id; Owner = owner; Name = name; Description = description;
    }
}