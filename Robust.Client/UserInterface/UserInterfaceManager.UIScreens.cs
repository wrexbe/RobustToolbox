﻿using System;
using System.Collections.Generic;
using Robust.Client.UserInterface.Controls;

namespace Robust.Client.UserInterface;

internal partial class UserInterfaceManager
{
    private UIScreen? _activeScreen;
    public UIScreen? ActiveScreen {
        get => _activeScreen;
        private set
        {
            if (_activeScreen == value) return;
            _activeScreen?.OnRemoved();
            _activeScreen = value;
            _activeScreen?.OnAdded();
        }
    }
    private readonly Dictionary<Type, UIScreen> _screens = new();
    private void _initializeScreens()
    {
        foreach (var screenType in _reflectionManager.GetAllChildren<UIScreen>())
        {
            if (screenType.IsAbstract) continue;
            _screens.Add(screenType, (UIScreen)_dynamicTypeFactory.CreateInstance(screenType));
        }
    }
    public void LoadScreen<T>() where T:UIScreen, new()
    {
        ((IUserInterfaceManager)this).LoadScreenInternal(typeof(T));
    }

    public T? GetActiveUIWidgetOrNull<T>() where T : UIWidget, new()
    {
        if (_activeScreen == null) return null;
        return (T?)_activeScreen.GetWidget<T>();
    }

    public T GetActiveUIWidget<T>() where T : UIWidget, new()
    {
        if (_activeScreen == null) throw new Exception("No screen is currently active");
        var widget = _activeScreen.GetWidget<T>();
        if (widget == null) throw new Exception("No widget of type found in active screen");
        return (T)widget;
    }

    void IUserInterfaceManager.LoadScreenInternal(Type type)
    {
        ActiveScreen = _screens[type];
    }

    public void UnloadScreen()
    {
        _activeScreen = null;
    }
}
