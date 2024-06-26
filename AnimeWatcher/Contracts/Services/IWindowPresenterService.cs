﻿namespace AnimeWatcher.Contracts.Services;
public interface IWindowPresenterService
{
    event EventHandler WindowPresenterChanged;

    bool IsFullScreen
    {
        get;
    }

    void ToggleFullScreen();
}
