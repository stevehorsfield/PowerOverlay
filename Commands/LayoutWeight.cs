using System;
using System.ComponentModel;
using System.Windows;

namespace PowerOverlay.Commands;

public class LayoutWeight : DependencyObject
{
    public static readonly DependencyProperty WeightProperty;
    public static readonly DependencyProperty GridLengthProperty;

    public int Weight
    {
        get { return (int)GetValue(WeightProperty); }
        set { SetValue(WeightProperty, value); }
    }
    public GridLength GridLength
    {
        get { return (GridLength)GetValue(GridLengthProperty); }
        set { SetValue(GridLengthProperty, value); }
    }

    private const int minWeight = 1;
    private const int maxWeight = 1000;

    static LayoutWeight()
    {
        GridLengthProperty = DependencyProperty.Register(nameof(GridLength), typeof(GridLength), typeof(LayoutWeight),
            new PropertyMetadata(new GridLength(1, GridUnitType.Star), OnGridLengthChanged, CoerceGridLength));
        WeightProperty = DependencyProperty.Register(nameof(Weight), typeof(int), typeof(LayoutWeight),
            new PropertyMetadata(1, OnWeightChanged, CoerceWeight));
    }

    private static object CoerceWeight(DependencyObject d, object baseValue)
    {
        var newVal = (int)baseValue;
        if (newVal < minWeight) return minWeight;
        if (newVal > maxWeight) return maxWeight;
        return newVal;
    }

    private static void OnWeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var lw = (LayoutWeight)d;
        if (lw.GridLength.Value != lw.Weight) lw.GridLength = new GridLength(lw.Weight, GridUnitType.Star);
    }

    private static object CoerceGridLength(DependencyObject d, object baseValue)
    {
        var gl = (GridLength)baseValue;
        var newVal = (int)gl.Value;
        if (newVal < minWeight) return new GridLength(minWeight, GridUnitType.Star);
        if (newVal > maxWeight) return new GridLength(maxWeight, GridUnitType.Star);
        return new GridLength(newVal, GridUnitType.Star);
    }

    private static void OnGridLengthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var lw = (LayoutWeight)d;
        if (lw.GridLength.Value != lw.Weight) lw.Weight = (int) lw.GridLength.Value;
    }
}
