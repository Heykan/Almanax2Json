namespace Almanax2Json.Dofus;

public struct Bonus {
    public string Name { get; private set; }
    public string Description { get; private set; }

    public Bonus WithName(string name) {
        Name = name;
        return this;
    }

    public Bonus WithDescription(string description) {
        Description = description;
        return this;
    }
}