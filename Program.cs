// Zadanie nr2 

using System.Text;

const string sourceFilePath = @"napisy do filmu.srt";
const string shiftedFilePath = @"napisy do filmu - shifted.srt";
const string zeroSecondsFilePath = @"napisy do filmu - zero seconds.srt";
const string nonZeroSecondsFilePath = @"napisy do filmu - non zero seconds.srt";
const string renumberedZeroSecondsFilePath = @"napisy do filmu - zero seconds renumbered.srt";
const string renumberedNonZeroSecondsFilePath = @"napisy do filmu - non zero seconds renumbered.srt";
TimeSpan shift = new TimeSpan(0, 0, 0, 0, 5880);    //5s 880ms do przodu


/// Konwersja znacznika czasu do TimeOnly
TimeOnly StringToTime(string param)
{
    if (!TimeOnly.TryParseExact(param, "hh:mm:ss,fff", out TimeOnly value))
    {
        Console.WriteLine($"Błąd formatu czasu: {param}");
        Environment.Exit(-1);
    }
    return value;
}


/// Przesunięcie napisów o podaną ilość ms
string[] TitleShift(string[] input, TimeSpan diff)
{
    List<string> output = new List<string>();

    int sequenceRow = 0;
    foreach (string line in input)
    {
        if (sequenceRow == 1){
            TimeOnly start = StringToTime(line.Substring(0, 12));
            TimeOnly final = StringToTime(line.Substring(17, 12));
            string newline = $"{start.Add(diff).ToString("HH:mm:ss,fff")} --> {final.Add(diff).ToString("HH:mm:ss,fff")}";
            output.Add(newline);
        }
        else
        {
            output.Add(line);
        }
        
        if (string.IsNullOrWhiteSpace(line))
        {
            sequenceRow = 0;
        }
        else
        {
            sequenceRow += 1;
        }
    }
    return output.ToArray();
}


/// Selekcja sekwencji startujących od xx:xx:00,xxx
string[] GetStartingAtZeroSeconds(string[] input, bool inverse = false)
{
    List<string> output = new List<string>();

    int sequenceRow = 0;
    string row0 = String.Empty;
    bool includeSequence = false;

    foreach (string line in input)
    {
        if (sequenceRow == 0){
            row0 = line;
        }
        else if (!inverse && sequenceRow == 1 && line.Substring(6,2).Equals("00") ){            
            includeSequence = true;
            output.Add(row0);
            output.Add(line);
        }
        else if (inverse && sequenceRow == 1 && !line.Substring(6,2).Equals("00") ){            
            includeSequence = true;
            output.Add(row0);
            output.Add(line);
        }
        else if (includeSequence)
        {
            output.Add(line);
        }
        
        if (string.IsNullOrWhiteSpace(line))
        {
            sequenceRow = 0;
            includeSequence = false;
        }
        else
        {
            sequenceRow += 1;
        }
    }
    return output.ToArray();
}

/// Renumeracja sekwencji
string[] Renumber(string[] input)
{
    List<string> output = new List<string>();

    int sequenceRow = 0;
    int counter = 1;

    foreach (string line in input)
    {
        if (sequenceRow == 0){
            output.Add(counter.ToString());
            counter += 1;
        }
        else
        {
            output.Add(line);
        }
        
        if (string.IsNullOrWhiteSpace(line))
        {
            sequenceRow = 0;
        }
        else
        {
            sequenceRow += 1;
        }
    }
    return output.ToArray();
}


/// Main program
Console.WriteLine("antheap2 - Konwerter SRT, Tomasz Król");

/// Check file exists
if (!File.Exists(sourceFilePath))
{
    Console.WriteLine($" Nie można otworzyć pliku źródłowego {sourceFilePath}, wychodzę!");
    Environment.Exit(-1);
}

/// Get source file
string[] source = File.ReadAllLines(sourceFilePath, Encoding.UTF8);

/// Process file by shifting times
string[] shifted = TitleShift(source, shift);
File.WriteAllLines(shiftedFilePath, shifted);
Console.WriteLine($"- Zapisano plik z przesunięciem czasu: {shifted.Length} wierszy");

/// Process shifted data by choosing sequences starting at equal seconds
string[] zeroed = GetStartingAtZeroSeconds(shifted);
File.WriteAllLines(zeroSecondsFilePath, zeroed);
Console.WriteLine($"- Zapisano plik z napisami rozpoczynającymi się o równych sekundach: {zeroed.Length} wierszy");

/// Process shifted data by skipping sequences starting at equal seconds
string[] nonzeroed = GetStartingAtZeroSeconds(shifted, true);
File.WriteAllLines(nonZeroSecondsFilePath, nonzeroed);
Console.WriteLine($"- Zapisano plik z napisami NIE rozpoczynającymi się o równych sekundach: {nonzeroed.Length} wierszy");

/// Renumber shifted data with sequences starting at equal seconds
string[] zeroedRenumbered = Renumber(zeroed);
File.WriteAllLines(renumberedZeroSecondsFilePath, zeroedRenumbered);
Console.WriteLine($"- Zapisano plik z przenumerowanymi napisami rozpoczynającymi się o równych sekundach: {zeroedRenumbered.Length} wierszy");

/// Renumber shifted data with sequences starting at equal seconds
string[] nonZeroedRenumbered = Renumber(nonzeroed);
File.WriteAllLines(renumberedNonZeroSecondsFilePath, nonZeroedRenumbered);
Console.WriteLine($"- Zapisano plik z przenumerowanymi napisami NIE rozpoczynającymi się o równych sekundach: {nonZeroedRenumbered.Length} wierszy");


Console.WriteLine("Zakończono.");

/// Z opisu zadania nie wynika dokładnie czy pośrednie pliki są pożądane czy nie, stąd program je tworzy
/// Jeśli nie byłyby potrzebne, to można zoptymalizować kod do mniejszej ilości iteracji na danych