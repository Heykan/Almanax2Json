namespace Almanax2Json.Dofus;

public class Item {
    public string Name { get; private set; }
    public int Quantity { get; private set; }
    public string IconUrl { get; private set; }

    public Item WithIcon(string url) {
        IconUrl = url;
        return this;
    }

    public Item WithName(string name) {
        Name = name;
        return this;
    }

    public Item WithQuantity(int quantity) {
        Quantity = quantity;
        return this;
    }
}