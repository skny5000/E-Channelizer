namespace EChannelizer.Models
{
    /// <summary>
    /// DVB Standards supported
    /// </summary>
    public enum DvbStandard
    {
        DVBS = 0,     // Satellite
        DVBS2 = 1,    // Satellite Second Generation
        DVBC = 2,     // Cable
        DVBC2 = 3,    // Cable Second Generation
        DVBT = 4,     // Terrestrial
        DVBT2 = 5,    // Terrestrial Second Generation
        ATSC = 6,     // ATSC (North America)
        ISDBT = 7,    // ISDB-T (Japan/Brazil)
        DTMB = 8      // DTMB (China)
    }

    /// <summary>
    /// Polarization for satellite signals
    /// </summary>
    public enum Polarization
    {
        Horizontal = 0,
        Vertical = 1,
        CircularLeft = 2,
        CircularRight = 3,
        Auto = 4
    }

    /// <summary>
    /// Satellite direction
    /// </summary>
    public enum SatelliteDirection
    {
        East = 0,
        West = 1
    }

    /// <summary>
    /// Service types
    /// </summary>
    public enum ServiceType
    {
        Unknown = 0,
        TV = 1,
        Radio = 2,
        Data = 3,
        Mosaic = 4,
        NVODReference = 5,
        NVODTimeShifted = 6,
        AdvancedCodecHD = 19,
        AdvancedCodecSD = 20,
        AdvancedCodecRadio = 21,
        HEVC_UHD = 25,
        HEVC_HD = 26
    }

    /// <summary>
    /// Service flags
    /// </summary>
    [Flags]
    public enum ServiceFlags : uint
    {
        None = 0,
        Scrambled = 1 << 0,
        Hidden = 1 << 1,
        Skip = 1 << 2,
        Favorite = 1 << 3,
        Locked = 1 << 4,
        New = 1 << 5,
        Updated = 1 << 6,
        Deleted = 1 << 7,
        Marker = 1 << 8,
        HD = 1 << 9,
        UHD = 1 << 10,
        HDR = 1 << 11,
        HEVC = 1 << 12,
        AC3 = 1 << 13,
        EAC3 = 1 << 14,
        AAC = 1 << 15,
        Subtitles = 1 << 16,
        Teletext = 1 << 17,
        MultiAudio = 1 << 18
    }

    /// <summary>
    /// Channel sort options
    /// </summary>
    public enum ChannelSort
    {
        None = 0,
        Name = 1,
        Number = 2,
        Frequency = 3,
        ServiceId = 4,
        Type = 5,
        Provider = 6,
        Satellite = 7,
        Favorites = 8
    }

    /// <summary>
    /// Stream TS type
    /// </summary>
    public enum StreamTsType
    {
        TS = 0,
        M3U = 1,
        M3U8 = 2,
        HLS = 3,
        DASH = 4,
        RTMP = 5,
        RTSP = 6,
        HTTP = 7,
        UDP = 8,
        RTP = 9
    }

    /// <summary>
    /// Stream buffering mode
    /// </summary>
    public enum StreamBuffering
    {
        None = 0,
        Low = 1,
        Medium = 2,
        High = 3,
        Custom = 4
    }

    /// <summary>
    /// EPG source types
    /// </summary>
    public enum EpgSourceType
    {
        None = 0,
        XMLTV = 1,
        DVB_EIT = 2,
        MHW = 3,
        EPGDAT = 4,
        Custom = 5
    }

    /// <summary>
    /// Bouquet source
    /// </summary>
    public enum BouquetSource
    {
        User = 0,
        Provider = 1,
        Satellite = 2,
        All = 3
    }

    /// <summary>
    /// Layout components visibility
    /// </summary>
    [Flags]
    public enum LayoutComponents
    {
        None = 0,
        Toolbar = 1 << 0,
        StatusBar = 1 << 1,
        ChannelList = 1 << 2,
        GroupTree = 1 << 3,
        TransponderList = 1 << 4,
        ServiceList = 1 << 5,
        Favorites = 1 << 6,
        LogPanel = 1 << 7,
        All = Toolbar | StatusBar | ChannelList | GroupTree | TransponderList | ServiceList | Favorites | LogPanel
    }

    /// <summary>
    /// Import options for satellites
    /// </summary>
    [Flags]
    public enum ImportSatellitesOptions
    {
        None = 0,
        Transponders = 1 << 0,
        Channels = 1 << 1,
        Services = 1 << 2,
        Logos = 1 << 3,
        EPG = 1 << 4,
        All = Transponders | Channels | Services | Logos | EPG
    }

    /// <summary>
    /// Import DVB options
    /// </summary>
    [Flags]
    public enum ImportDvbOptions
    {
        None = 0,
        TV = 1 << 0,
        Radio = 1 << 1,
        Data = 1 << 2,
        HD = 1 << 3,
        UHD = 1 << 4,
        Scrambled = 1 << 5,
        FTA = 1 << 6,
        All = TV | Radio | Data | HD | UHD | Scrambled | FTA
    }

    /// <summary>
    /// Player status
    /// </summary>
    public enum PlayerStatus
    {
        Stopped = 0,
        Buffering = 1,
        Playing = 2,
        Paused = 3,
        Error = 4,
        Connecting = 5
    }

    /// <summary>
    /// Installation status
    </summary>
    public enum InstallationStatus
    {
        NotInstalled = 0,
        Installing = 1,
        Installed = 2,
        Failed = 3,
        Updating = 4,
        Updated = 5
    }

    /// <summary>
    /// Update status
    </summary>
    public enum UpdateStatus
    {
        UpToDate = 0,
        UpdateAvailable = 1,
        Checking = 2,
        Downloading = 3,
        Installing = 4,
        Error = 5
    }

    /// <summary>
    /// Channel marker types
    /// </summary>
    public enum ChannelMarkerType
    {
        None = 0,
        Favorite = 1,
        Locked = 2,
        Hidden = 3,
        Skip = 4,
        Custom1 = 5,
        Custom2 = 6,
        Custom3 = 7,
        Custom4 = 8,
        Custom5 = 9
    }

    /// <summary>
    /// Service short type for display
    /// </summary>
    public enum ServiceShortType
    {
        TV = 0,
        Radio = 1,
        Data = 2,
        Unknown = 3
    }

    /// <summary>
    /// Service long type for detailed display
    /// </summary>
    public enum ServiceLongType
    {
        DigitalTV = 0,
        DigitalRadio = 1,
        Teletext = 2,
        NVODReference = 3,
        NVODTimeShifted = 4,
        Mosaic = 5,
        DataBroadcast = 6,
        AdvancedCodecHD = 7,
        AdvancedCodecSD = 8,
        AdvancedCodecRadio = 9,
        HEVC_UHD = 10,
        HEVC_HD = 11
    }

    /// <summary>
    /// Quick hints for UI
    /// </summary>
    [Flags]
    public enum QuickHints
    {
        None = 0,
        ShowTooltips = 1 << 0,
        ShowKeyboardShortcuts = 1 << 1,
        ShowContextHelp = 1 << 2,
        ShowWelcomeScreen = 1 << 3,
        All = ShowTooltips | ShowKeyboardShortcuts | ShowContextHelp | ShowWelcomeScreen
    }

    /// <summary>
    /// Channel options
    /// </summary>
    [Flags]
    public enum ChannelOptions
    {
        None = 0,
        ShowNumbers = 1 << 0,
        ShowIcons = 1 << 1,
        ShowFlags = 1 << 2,
        ShowProvider = 1 << 3,
        ShowSatellite = 1 << 4,
        ShowFrequency = 1 << 5,
        ShowPolarization = 1 << 6,
        ShowSymbolRate = 1 << 7,
        ShowFEC = 1 << 8,
        ShowModulation = 1 << 9,
        ShowServiceId = 1 << 10,
        ShowNetworkId = 1 << 11,
        ShowTransportId = 1 << 12,
        All = ShowNumbers | ShowIcons | ShowFlags | ShowProvider | ShowSatellite | ShowFrequency | ShowPolarization | ShowSymbolRate | ShowFEC | ShowModulation | ShowServiceId | ShowNetworkId | ShowTransportId
    }

    /// <summary>
    /// Devices options
    /// </summary>
    [Flags]
    public enum DevicesOptions
    {
        None = 0,
        ShowIP = 1 << 0,
        ShowMAC = 1 << 1,
        ShowModel = 1 << 2,
        ShowFirmware = 1 << 3,
        ShowStatus = 1 << 4,
        All = ShowIP | ShowMAC | ShowModel | ShowFirmware | ShowStatus
    }

    /// <summary>
    /// Services sort order
    /// </summary>
    public enum ServicesSort
    {
        Name = 0,
        ServiceId = 1,
        Type = 2,
        Provider = 3,
        Frequency = 4,
        Number = 5
    }
}