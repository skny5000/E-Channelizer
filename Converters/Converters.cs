using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using EChannelizer.Models;

namespace EChannelizer.Converters
{
    /// <summary>
    /// Boolean to Visibility converter
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                if (parameter is string param)
                {
                    var parts = param.Split(';');
                    if (parts.Length == 2)
                    {
                        return boolValue ? parts[0] : parts[1];
                    }
                }
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }
            return false;
        }
    }

    /// <summary>
    /// Inverse Boolean to Visibility converter
    /// </summary>
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility != Visibility.Visible;
            }
            return true;
        }
    }

    /// <summary>
    /// Enum to Boolean converter (for radio buttons)
    /// </summary>
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            var checkValue = value.ToString();
            var targetValue = parameter.ToString();

            return checkValue.Equals(targetValue, StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue && parameter != null)
            {
                return Enum.Parse(targetType, parameter.ToString()!);
            }
            return Binding.DoNothing;
        }
    }

    /// <summary>
    /// Service Type to Icon converter
    /// </summary>
    public class ServiceTypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ServiceType type)
            {
                return type switch
                {
                    ServiceType.TV => "TelevisionClassic",
                    ServiceType.Radio => "Radio",
                    ServiceType.Data => "Database",
                    ServiceType.Mosaic => "ViewGrid",
                    ServiceType.AdvancedCodecHD => "TelevisionBox",
                    ServiceType.AdvancedCodecSD => "TelevisionBox",
                    ServiceType.AdvancedCodecRadio => "RadioBox",
                    ServiceType.HEVC_UHD => "TelevisionBox",
                    ServiceType.HEVC_HD => "TelevisionBox",
                    _ => "TelevisionClassic"
                };
            }
            return "TelevisionClassic";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    /// <summary>
    /// Service Flags to String converter
    /// </summary>
    public class ServiceFlagsToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ServiceFlags flags)
            {
                var parts = new System.Collections.Generic.List<string>();

                if (flags.HasFlag(ServiceFlags.Scrambled)) parts.Add("🔒");
                if (flags.HasFlag(ServiceFlags.Hidden)) parts.Add("👁");
                if (flags.HasFlag(ServiceFlags.Favorite)) parts.Add("⭐");
                if (flags.HasFlag(ServiceFlags.Locked)) parts.Add("🔐");
                if (flags.HasFlag(ServiceFlags.Skip)) parts.Add("⏭");
                if (flags.HasFlag(ServiceFlags.HD)) parts.Add("HD");
                if (flags.HasFlag(ServiceFlags.UHD)) parts.Add("4K");
                if (flags.HasFlag(ServiceFlags.HEVC)) parts.Add("H.265");
                if (flags.HasFlag(ServiceFlags.AC3)) parts.Add("AC3");
                if (flags.HasFlag(ServiceFlags.EAC3)) parts.Add("EAC3");
                if (flags.HasFlag(ServiceFlags.AAC)) parts.Add("AAC");
                if (flags.HasFlag(ServiceFlags.Subtitles)) parts.Add("📝");
                if (flags.HasFlag(ServiceFlags.Teletext)) parts.Add("📺");
                if (flags.HasFlag(ServiceFlags.MultiAudio)) parts.Add("🔊");

                return string.Join(" ", parts);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    /// <summary>
    /// Frequency to String converter (kHz to GHz/MHz)
    /// </summary>
    public class FrequencyToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is long freqKHz)
            {
                if (freqKHz >= 1000000) // >= 1 GHz
                {
                    return $"{freqKHz / 1000000.0:F3} GHz";
                }
                else if (freqKHz >= 1000) // >= 1 MHz
                {
                    return $"{freqKHz / 1000.0:F1} MHz";
                }
                else
                {
                    return $"{freqKHz} kHz";
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    /// <summary>
    /// Symbol Rate to String converter (ksym/s to Msym/s)
    /// </summary>
    public class SymbolRateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int symRate)
            {
                if (symRate >= 1000)
                {
                    return $"{symRate / 1000.0:F3} Msym/s";
                }
                else
                {
                    return $"{symRate} ksym/s";
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    /// <summary>
    /// Channel Marker to Color converter
    /// </summary>
    public class ChannelMarkerToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ChannelMarkerType marker)
            {
                return marker switch
                {
                    ChannelMarkerType.Favorite => Brushes.Gold,
                    ChannelMarkerType.Locked => Brushes.Red,
                    ChannelMarkerType.Hidden => Brushes.Gray,
                    ChannelMarkerType.Skip => Brushes.Orange,
                    ChannelMarkerType.Custom1 => Brushes.Blue,
                    ChannelMarkerType.Custom2 => Brushes.Green,
                    ChannelMarkerType.Custom3 => Brushes.Purple,
                    ChannelMarkerType.Custom4 => Brushes.Cyan,
                    ChannelMarkerType.Custom5 => Brushes.Magenta,
                    _ => Brushes.Transparent
                };
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    /// <summary>
    /// Polarization to String converter
    /// </summary>
    public class PolarizationToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Polarization pol)
            {
                return pol switch
                {
                    Polarization.Horizontal => "H",
                    Polarization.Vertical => "V",
                    Polarization.CircularLeft => "L",
                    Polarization.CircularRight => "R",
                    Polarization.Auto => "A",
                    _ => "?"
                };
            }
            return "?";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    /// <summary>
    /// Player Status to Color converter
    /// </summary>
    public class PlayerStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PlayerStatus status)
            {
                return status switch
                {
                    PlayerStatus.Playing => Brushes.Green,
                    PlayerStatus.Buffering => Brushes.Orange,
                    PlayerStatus.Paused => Brushes.Blue,
                    PlayerStatus.Error => Brushes.Red,
                    PlayerStatus.Connecting => Brushes.Yellow,
                    _ => Brushes.Gray
                };
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    /// <summary>
    /// Installation Status to String converter
    /// </summary>
    public class InstallationStatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is InstallationStatus status)
            {
                return status switch
                {
                    InstallationStatus.NotInstalled => "Not Installed",
                    InstallationStatus.Installing => "Installing...",
                    InstallationStatus.Installed => "Installed",
                    InstallationStatus.Failed => "Failed",
                    InstallationStatus.Updating => "Updating...",
                    InstallationStatus.Updated => "Updated",
                    _ => "Unknown"
                };
            }
            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    /// <summary>
    /// Bouquet Source to Icon converter
    /// </summary>
    public class BouquetSourceToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BouquetSource source)
            {
                return source switch
                {
                    BouquetSource.User => "Folder",
                    BouquetSource.Provider => "Server",
                    BouquetSource.Satellite => "Satellite",
                    BouquetSource.All => "FolderMultiple",
                    _ => "Folder"
                };
            }
            return "Folder";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    /// <summary>
    /// Bouquet Source to Color converter
    /// </summary>
    public class BouquetSourceToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BouquetSource source)
            {
                return source switch
                {
                    BouquetSource.User => Brushes.White,
                    BouquetSource.Provider => Brushes.Cyan,
                    BouquetSource.Satellite => Brushes.Orange,
                    BouquetSource.All => Brushes.Yellow,
                    _ => Brushes.White
                };
            }
            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    /// <summary>
    /// Null to Visibility converter
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isNull = value == null;
            var invert = parameter?.ToString()?.Equals("invert", StringComparison.OrdinalIgnoreCase) == true;
            return (isNull ^ invert) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    /// <summary>
    /// Count to Visibility converter
    /// </summary>
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            if (value is System.Collections.ICollection collection)
            {
                return collection.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}