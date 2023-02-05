using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AllUp.Helpers
{
    public static class Extensions
    {
        public static bool IsImage(this IFormFile file)
        {
            return file.ContentType.Contains("image/");
        }
        public static bool OlderOneMb(this IFormFile file)
        {
            return file.Length/1024>3072;
        }
        public static async Task<string> SaveFileAsync(this IFormFile file,string path)
        {
            string fileName=Guid.NewGuid() + file.FileName;
            string fullPath = Path.Combine(path,fileName);

            using (FileStream fileStream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return fileName;
        }
    }

}
