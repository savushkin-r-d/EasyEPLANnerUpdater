using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Updater;

public static class Extensions
{
    public static async Task DownloadFile(this HttpClient client, string address, string filename)
    {
        using (var response = await client.GetAsync(address))
        using (var stream = await response.Content.ReadAsStreamAsync())
        using (var file = File.OpenWrite(filename))
        {
            stream.CopyTo(file);
        }
    }

}
