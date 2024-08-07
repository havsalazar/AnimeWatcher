﻿using AnimeWatcher.Core.Models;
using AnimeWatcher.Core.Helpers;
namespace AnimeWatcher.Core.Services;
public class SelectSourceService
{
    private readonly ClassReflectionHelper _classReflectionHelper = new();  
    internal static List<T> MoveToFirst<T>(List<T> list, T item)
    {
        List<T> newList = new List<T>(list);
        if (newList.Remove(item))
        {
            newList.Insert(0, item);
        }
        return newList;
    }

    public async Task<string> SelectSourceAsync(VideoSource[] videoSources, string byDefault = "")
    {
        var streamUrl = "";
        /*logic to get the default source here*/
        var item = videoSources.FirstOrDefault(e => e.Server == byDefault) ?? videoSources[0];
        var orderedSources = MoveToFirst(videoSources.ToList(), item);

        foreach (var source in orderedSources)
        {
            var tempUrl="";
            //var tempUrl = source.server switch
            //{
            //    "Okru" => await okruExtractor.GetStreamAsync(source.checkedUrl),
            //    "Streamwish" => await streamWishExtractor.GetStreamAsync(source.checkedUrl),
            //    "YourUpload" => await yourUploadExtractor.GetStreamAsync(source.checkedUrl),
            //    _ => ""
            //};
             
            var reflex = _classReflectionHelper.GetMethodFromVideoSource(source);
            var method = reflex.Item1;
            var instance = reflex.Item2;
            tempUrl = await (Task<string>)method.Invoke(instance, new object[]{source.CheckedUrl});


            if (!string.IsNullOrEmpty(tempUrl))
            {
                streamUrl = tempUrl;
                break;
            }
        }


        return streamUrl;
    }


}
