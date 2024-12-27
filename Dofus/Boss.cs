namespace Almanax2Json.Dofus;

public class Boss {
    public string Name { get; private set; }
    public string IconUrl { get; private set; }

    public Boss WithIcon(string url) {
        IconUrl = url;
        return this;
    }

    public Boss WithName(string name) {
        Name = name;
        return this;
    }
}