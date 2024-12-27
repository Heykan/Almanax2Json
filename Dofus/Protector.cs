namespace Almanax2Json.Dofus;

public class Protector {
    public string Name { get; private set; }
    public string IconUrl { get; private set; }

    public Protector WithIcon(string url) {
        IconUrl = url;
        return this;
    }

    public Protector WithName(string name) {
        Name = name;
        return this;
    }
}