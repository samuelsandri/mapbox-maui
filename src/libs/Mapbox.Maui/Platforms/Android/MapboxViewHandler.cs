﻿namespace Mapbox.Maui;

using PlatformView = AndroidX.Fragment.App.FragmentContainerView;
using MapboxMapsStyle = Com.Mapbox.Maps.Style;
using Com.Mapbox.Maps;
using Com.Mapbox.Maps.Plugin.Scalebar;
using Microsoft.Maui.Platform;
using Android.Content;
using System;

public partial class MapboxViewHandler
{
    MapboxFragment mapboxFragment;

    private static void HandleLightChanged(MapboxViewHandler handler, IMapboxView view)
    {
        var mapView = handler.GetMapView();
        if (mapView == null) return;

        if (view.Light == null) return;

        mapView.MapboxMap.Style.SetStyleLight(
            view.Light.ToPlatformValue(true)
        );
    }

    private static void HandleLayersChanged(MapboxViewHandler handler, IMapboxView view)
    {
        var mapView = handler.GetMapView();
        if (mapView == null) return;

        if (view.Layers == null) return;

        foreach (var layer in view.Layers)
        {
            var value = layer.ToPlatformValue(true);

            if (mapView.MapboxMap.Style.StyleLayerExists(layer.Id))
            {
                mapView.MapboxMap.Style.SetStyleLayerProperties(layer.Id, value);

                continue;
            }

            mapView.MapboxMap.Style.AddStyleLayer(
                value,
                layer.LayerPosition.ToPlatformValue()
            );
        }
    }

    private static void HandleTerrainChanged(MapboxViewHandler handler, IMapboxView view)
    {
        var mapView = handler.GetMapView();
        if (mapView == null) return;

        if (view.Terrain == null) return;

        var platformValue = view.Terrain.ToPlatformValue();
        platformValue.BindTo(mapView.MapboxMap.Style);
    }

    private static void HandleSourcesChanged(MapboxViewHandler handler, IMapboxView view)
    {
        var mapView = handler.GetMapView();
        if (mapView == null) return;

        if (view.Sources == null) return;

        foreach (var source in view.Sources)
        {
            mapView.MapboxMap.Style.AddStyleSource(
                source.Id,
                source.ToPlatformValue()
            );

            if (!source.VolatileProperties.Any()) continue;

            mapView.MapboxMap.Style.SetStyleSourceProperties(
                source.Id,
                source.GetVolatileProperties()
            );
        }
    }

    private static void HandleCameraOptionsChanged(MapboxViewHandler handler, IMapboxView view)
    {
        var cameraOptions = view.CameraOptions.ToNative();

        handler.GetMapView()?.MapboxMap.SetCamera(cameraOptions);
    }

    private static void HandleDebugOptionsChanged(MapboxViewHandler handler, IMapboxView view)
    {
        if (view.DebugOptions == null) return;

        var mapView = handler.GetMapView();
        if (mapView == null) return;

        var debugOptions = mapView.MapboxMap.Debug;
        handler.GetMapView().MapboxMap.SetDebug(debugOptions, false);

        debugOptions = view.DebugOptions.ToNative();
        handler.GetMapView().MapboxMap.SetDebug(debugOptions, true);
    }

    private static void HandleScaleBarVisibilityChanged(MapboxViewHandler handler, IMapboxView view)
    {
        var mapView = handler.GetMapView();
        if (mapView?.MapboxMap.Style?.IsStyleLoaded != true) return;

        var scaleBarPlugin = ScaleBarUtils.GetScaleBar(mapView);
        if (view.ScaleBarVisibility == OrnamentVisibility.Hidden)
        {
            scaleBarPlugin.Enabled = false;
            return;
        }

        scaleBarPlugin.Enabled = true;
        
    }

    private static void HandleMapboxStyleChanged(MapboxViewHandler handler, IMapboxView view)
    {
        var styleUri = view.MapboxStyle.ToNative();

        if (string.IsNullOrWhiteSpace(styleUri))
        {
            styleUri = MapboxMapsStyle.MapboxStreets;
        }

        handler.GetMapView()?.MapboxMap.LoadStyleUri(styleUri);
    }

    protected override PlatformView CreatePlatformView()
    {
        var mainActivity = (MauiAppCompatActivity)Context.GetActivity();

        var fragmentContainerView = new PlatformView(Context)
        {
            Id = Android.Views.View.GenerateViewId(),
        };
        mapboxFragment = new MapboxFragment();
        mapboxFragment.MapViewReady += HandleMapViewReady;
        mapboxFragment.StyleLoaded += HandleStyleLoaded;
        mapboxFragment.MapLoaded += HandleMapLoaded;

        var fragmentTransaction = mainActivity.SupportFragmentManager.BeginTransaction();
        fragmentTransaction.Replace(fragmentContainerView.Id, mapboxFragment, $"mapbox-maui-{fragmentContainerView.Id}");
        fragmentTransaction.CommitAllowingStateLoss();
        return fragmentContainerView;
    }

    protected override void DisconnectHandler(PlatformView platformView)
    {
        if (mapboxFragment != null)
        {
            mapboxFragment.MapViewReady -= HandleMapViewReady;
            mapboxFragment.StyleLoaded -= HandleStyleLoaded;
            mapboxFragment.MapLoaded -= HandleMapLoaded;
            mapboxFragment.Dispose();
            mapboxFragment = null;
        }
        base.DisconnectHandler(platformView);
    }

    private void HandleMapViewReady(MapView view)
        => (VirtualView as MapboxView)?.InvokeMapReady();

    private void HandleMapLoaded(MapView view)
        => (VirtualView as MapboxView)?.InvokeMapLoaded();

    private void HandleStyleLoaded(MapView view)
        => (VirtualView as MapboxView)?.InvokeStyleLoaded();
}

