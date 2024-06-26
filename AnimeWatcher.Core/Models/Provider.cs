﻿using SQLite;

namespace AnimeWatcher.Core.Models;
public class Provider
{
    [PrimaryKey]
    public int Id
    {
        get; set;
    }
    public string Name
    {
        get; set;
    }
    public string Url
    {
        get; set;
    }
    //this property means that the url data changes over time, only applies to animepache
    public bool Persistent
    {
        get; set;
    }
    public string Type
    {
        get; set;
    }

    [Ignore]
    public Anime[]? Animes
    {
        get; set;
    }
}
