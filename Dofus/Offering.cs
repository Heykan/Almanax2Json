namespace Almanax2Json.Dofus;

public class Offering {
    public string Name { get; private set; }
    public Bonus Bonus {get; private set; }
    public Item Item { get; private set; }
    public Protector Protector { get; private set; }
    public Boss Boss { get; private set; }
    public DateTimeOffset Date { get; private set; }

    public Offering WithName(string name) {
        Name = name;
        return this;
    }

    public Offering WithBonus(Bonus bonus) {
        Bonus = bonus;
        return this;
    }

    public Offering WithItem(Item item) {
        Item = item;
        return this;
    }

    public Offering WithProtector(Protector protector) {
        Protector = protector;
        return this;
    }

    public Offering WithBoss(Boss boss) {
        Boss = boss;
        return this;
    }

    public Offering WithDate(DateTimeOffset date) {
        Date = date;
        return this;
    }
}