using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Almanax2Json.IO;

public class Serializer {
    public static void Serialize(object obj, string output) {
        var options = new JsonSerializerOptions {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement, UnicodeRanges.LatinExtendedA)
        };

        string json = JsonSerializer.Serialize(obj, options);
        File.WriteAllText(output, json);

        Console.WriteLine("Données enregistrées avec succès.");
    }
}