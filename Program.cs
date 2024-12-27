using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using Almanax2Json.Dofus;
using Almanax2Json.IO;
using HtmlAgilityPack;

const string KROSMOZ_BASE_URL = "https://www.krosmoz.com/fr/almanax";

List<Offering> offerings = new List<Offering>();
int currentYear = DateTime.Now.Year;

DateTime startDate = new DateTime(currentYear, 1, 1);
DateTime endDate = new DateTime(currentYear, 12, 31);

try 
{
    for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
    {
        offerings.Add(await ParseOfferingForDate(date));

        await Task.Delay(800);
    }
}
catch(Exception ex)
{
    Console.WriteLine(ex.Message);
    Serializer.Serialize(offerings, "data.json");
}

Console.WriteLine($"{offerings.Count} offrandes parsé.");
Serializer.Serialize(offerings, "data.json");

// ---- Méthodes auxiliaires ----
async Task<Offering> ParseOfferingForDate(DateTime date)
{
    string formattedDate = date.ToString("yyyy-MM-dd");
    string url = $"{KROSMOZ_BASE_URL}/{formattedDate}";

    int[] kamas = GetKamas("almanax.csv");

    HtmlDocument document = LoadHtmlDocument(url);

    Offering offering = new Offering().WithDate(date).WithKamas(kamas[date.Date.DayOfYear - 1]);

    offering.WithBonus(ParseBonus(document));
    offering.WithItem(ParseOfferingItem(document));
    offering.WithBoss(ParseBoss(document));
    offering.WithProtector(ParseProtector(document));
    offering.WithName(ParseName(document));

    // Télécharge les icones s'ils n'existent pas
    if (!string.IsNullOrEmpty(offering.Boss.IconUrl))
        await DownloadBossIcon(offering.Boss);

    if (!string.IsNullOrEmpty(offering.Protector.IconUrl))
        await DownloadProtectorIcon(offering.Protector);

    if (offering.Item != null)
        await DownloadItemIcon(offering.Item);

    Console.WriteLine($"Ajout de {offering.Name} : {formattedDate}");

    return offering;
}

HtmlDocument LoadHtmlDocument(string url)
{
    HtmlWeb web = new();
    return web.Load(url);
}

string ParseName(HtmlDocument document)
{
    const string bonusSelector = "//div[contains(@id, 'achievement_dofus')]//div[contains(@class, 'mid')]";
    HtmlNode bonusNode = document.DocumentNode.SelectSingleNode(bonusSelector);

    string[] bonusLines = CleanLines(bonusNode?.InnerText);

    if (bonusLines.Length > 2)
        return bonusLines[2];

    return "Calendrier Test";
}

Bonus ParseBonus(HtmlDocument document)
{
    const string bonusSelector = "//div[contains(@id, 'achievement_dofus')]//div[contains(@class, 'mid')]";
    HtmlNode bonusNode = document.DocumentNode.SelectSingleNode(bonusSelector);

    string[] bonusLines = CleanLines(bonusNode?.InnerText);
    return new Bonus()
        .WithName(bonusLines[0])
        .WithDescription(bonusLines[1]);
}

Item? ParseOfferingItem(HtmlDocument document)
{
    const string itemIconSelector = "//div[contains(@id, 'achievement_dofus')]//div[contains(@class, 'more-infos-content')]/img";
    string itemIconUrl = GetAttributeValue(document, itemIconSelector, "src");

    const string itemPattern = @"Récupérer\s+(\d+)\s+(.+?)\s+et";
    HtmlNode bonusNode = document.DocumentNode.SelectSingleNode("//div[contains(@id, 'achievement_dofus')]//div[contains(@class, 'mid')]");
    string[] bonusLines = CleanLines(bonusNode?.InnerText);

    if (bonusLines.Length > 3)
    {
        Match match = Regex.Match(bonusLines[3], itemPattern);
        if (match.Success)
        {
            return new Item()
                .WithName(match.Groups[2].Value.Replace(":", "-"))
                .WithQuantity(int.Parse(match.Groups[1].Value))
                .WithIcon(itemIconUrl);
        }
    }

    return null;
}

Boss ParseBoss(HtmlDocument document)
{
    const string bossSelector = "//div[contains(@id, 'almanax_boss')]";
    const string bossImageSelector = "//div[contains(@id, 'almanax_boss_image')]/img";

    HtmlNode bossNode = document.DocumentNode.SelectSingleNode(bossSelector);
    string bossImageUrl = GetAttributeValue(document, bossImageSelector, "src");

    string bossName = CleanLines(bossNode?.InnerText)[0];
    return new Boss().WithName(bossName).WithIcon(bossImageUrl);
}

Protector ParseProtector(HtmlDocument document)
{
    const string protectorSelector = "//div[contains(@id, 'almanax_protector')]";
    const string protectorImageSelector = "//div[contains(@id, 'almanax_protector')]/img";

    HtmlNode protectorNode = document.DocumentNode.SelectSingleNode(protectorSelector);
    string protectorImageUrl = GetAttributeValue(document, protectorImageSelector, "src");
    string protectorName = CleanLines(protectorNode?.InnerText)[0];
    return new Protector().WithName(protectorName).WithIcon(protectorImageUrl);
}

int[] GetKamas(string filename)
{
    List<int> kamasList = new();
    using (var reader = new StreamReader(filename))
    {
        while (!reader.EndOfStream)
        {
            var value = reader.ReadLine().Split(',');
            if (value.Last() == "Kamas") continue;

            kamasList.Add(int.Parse(value.Last()));
        }
    }

    return kamasList.ToArray();
}

string GetAttributeValue(HtmlDocument document, string xpath, string attribute)
{
    HtmlNode node = document.DocumentNode.SelectSingleNode(xpath);
    return node?.GetAttributeValue(attribute, string.Empty) ?? string.Empty;
}

string[] CleanLines(string? content)
{
    if (string.IsNullOrWhiteSpace(content))
        return Array.Empty<string>();

    return content
        .Split('\n')
        .Select(line => line.Trim())
        .Where(line => !string.IsNullOrEmpty(line))
        .ToArray();
}

string GetExtensionIcon(string url)
{
    return url.Split('.').Last();
}

async Task DownloadBossIcon(Boss boss)
{
    await DownloadIcon(boss.IconUrl, $"images/boss/{boss.Name}");
}

async Task DownloadProtectorIcon(Protector protector)
{
    await DownloadIcon(protector.IconUrl, $"images/protectors/{protector.Name}");
}

async Task DownloadItemIcon(Item item)
{
    await DownloadIcon(item.IconUrl, $"images/items/{item.Name}");
}

async Task DownloadIcon(string url, string output)
{
    output = $"{output}.{GetExtensionIcon(url)}";
    string[] directories = output.Split('/');

    if (!Directory.Exists(directories[0])) Directory.CreateDirectory(directories[0]);
    if (!Directory.Exists($"{directories[0]}/{directories[1]}")) Directory.CreateDirectory($"{directories[0]}/{directories[1]}");

    if (File.Exists(output)) return;

    // Délai anti cloudflare
    await Task.Delay(800);

    using HttpClient client = new HttpClient();
    HttpResponseMessage response = await client.GetAsync(url);
    response.EnsureSuccessStatusCode();

    byte[] imageData = await response.Content.ReadAsByteArrayAsync();
    await File.WriteAllBytesAsync(output, imageData);
}