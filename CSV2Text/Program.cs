// See https://aka.ms/new-console-template for more information
using System.Runtime.CompilerServices;

string sDirt = "C:\\Users\\Surendra_Sharma1\\Downloads\\Vikings (1)";
List<string> files = Directory.GetFiles(sDirt).Where(a=>a.EndsWith(".en[cc].vtt")).ToList();

foreach (string file in files)
{
    string hi_file_path = file.Replace(".en[cc].vtt", "_hi.txt");
    string en_file_path = file.Replace(".en[cc].vtt", "_en.txt");
    string file_path = file.Replace(".en[cc].vtt", ".txt");

    List<StoreText> file_text = new List<StoreText>();
    List<StoreText> en_text = GetFile(file);
    List<StoreText> hi_text = GetFile(file.Replace(".en[cc].vtt", ".hi.vtt"));

    foreach (var item in hi_text)
    {
        TimeSpan startTime = item.StartTime.Add(new TimeSpan(0, 0, 0, 0, -500));
        TimeSpan endTime = item.StartTime.Add(new TimeSpan(0, 0, 0, 0, 500));
        var messageList = en_text.Where(w => w.StartTime >= startTime && w.StartTime <= endTime).Select(x => x.Message).ToList();
        if (messageList.Any())
        {
            file_text.Add(new StoreText() { StartTime = item.StartTime, Message = $"{string.Join(" ", messageList)} ({item.Message})" });
        }
        else
        {
            startTime = item.StartTime.Add(new TimeSpan(0, 0, 0, 0, -1500));
            endTime = item.StartTime.Add(new TimeSpan(0, 0, 0, 0, 1500));
            messageList = en_text.Where(w => w.StartTime >= startTime && w.StartTime <= endTime).Select(x => x.Message).ToList();
            file_text.Add(new StoreText() { StartTime = item.StartTime, Message = $"{string.Join(" ", messageList)} ({item.Message})" });
        }


    }

    await File.WriteAllLinesAsync(hi_file_path, hi_text.Select(x => $"{x.StartTime} => {x.Message}"));
    await File.WriteAllLinesAsync(en_file_path, en_text.Select(x => $"{x.StartTime} => {x.Message}"));
    await File.WriteAllLinesAsync(file_path, file_text.Select(x => $"{x.StartTime} => {x.Message}"));
}



Console.WriteLine("Written all three fiels");
Console.ReadKey();


List<StoreText> GetFile(string path)
{
    List<StoreText> lines = new List<StoreText>();

    using (StreamReader file = new StreamReader(path))
    {
        int counter = 0;
        string ln;
        bool start = false;
        int sectionCount = 0;
        string showText = string.Empty;
        StoreText obj = new StoreText();
        while ((ln = file.ReadLine()) != null)
        {
            if (start)
            {
                string text = ln;
                if (sectionCount == 1)
                {
                    // showText+=text.Split("-->")[0];
                    obj.StartTime = TimeSpan.Parse(text.Split("-->")[0]);
                    // obj.StartTime = obj.StartTime.Add(new TimeSpan(0, 0, 0, 0, 51));
                }
                else if (sectionCount == 2 || (sectionCount == 3 && text != ""))
                {
                    showText += " " + (text
                        .Replace("<c.white><c.mono_sans>", "").Replace("</c.mono_sans></c.white>", "")
                        .Replace("<c.bg_transparent>", "").Replace("</c.bg_transparent>", ""));

                }


                sectionCount++;
                if ((sectionCount == 4 && text == "") || (sectionCount == 5 && text == ""))
                {
                    obj.Message = showText.Trim();
                    if (!(obj.Message.StartsWith("(") && obj.Message.EndsWith(")")))
                    {
                        lines.Add(obj);
                    }

                    obj = new StoreText();
                    showText = string.Empty;
                    sectionCount = 0;
                }
            }
            else
            {
                try
                {
                    start = ln == "1";
                    if (start)
                        sectionCount++;
                }
                catch (Exception ex)
                {


                }
            }
            counter++;
        }
        file.Close();

    }


    lines = lines.GroupBy(a => a.StartTime).Select(x => new StoreText() { StartTime = x.Key, Message = string.Join(" ", x.Select(j => j.Message).ToList()) }).ToList();
    return lines;

}
public class StoreText
{
    public TimeSpan StartTime { get; set; }
    public string Message { get; set; }
}