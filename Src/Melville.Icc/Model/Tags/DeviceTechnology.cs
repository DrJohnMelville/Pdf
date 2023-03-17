namespace Melville.Icc.Model.Tags;

/// <summary>
/// Represents different hardware devices which can be described with an ICC profile.
/// </summary>
public enum DeviceTechnology : uint
{
    /// <summary>
    /// DeviceTechnology for FilmScanner = 'fscn'
    /// </summary>
    FilmScanner = 0x6673636e,
    /// <summary>
    /// DeviceTechnology for DigitalCamera = 'dcam'
    /// </summary>
    DigitalCamera = 0x6463616d,
    /// <summary>
    /// DeviceTechnology for ReflectiveScanner = 'rscn'
    /// </summary>
    ReflectiveScanner = 0x72736363,
    /// <summary>
    /// DeviceTechnology for InkjetPrinter = 'ijet'
    /// </summary>
    InkjetPrinter = 0x696a6574,
    /// <summary>
    /// DeviceTechnology for ThermalWaxPrinter = 'twax'
    /// </summary>
    ThermalWaxPrinter = 0x74776178,
    /// <summary>
    /// DeviceTechnology for ElectrophotographicPrinter = 'epho'
    /// </summary>
    ElectrophotographicPrinter = 0x6570686f,
    /// <summary>
    /// DeviceTechnology for ElectrostaticPrinter = 'esta'
    /// </summary>
    ElectrostaticPrinter = 0x65737461,
    /// <summary>
    /// DeviceTechnology for DyeSubliminationPrinter = 'dsub'
    /// </summary>
    DyeSubliminationPrinter = 0x64737562,
    /// <summary>
    /// DeviceTechnology for PhotographicPaperPrinter = 'rpho'
    /// </summary>
    PhotographicPaperPrinter = 0x7270686f,
    /// <summary>
    /// DeviceTechnology for FilWriter = 'fprn'
    /// </summary>
    FilWriter = 0x6670726e,
    /// <summary>
    /// DeviceTechnology for VideoMonitor = 'vidm'
    /// </summary>
    VideoMonitor = 0x7669646d,
    /// <summary>
    /// DeviceTechnology for Video Camera = 'vidc'
    /// </summary>
    VideoCamera = 0x76696463, // vidc
    /// <summary>
    /// DeviceTechnology for ProjectionTelevision = 'pjtv'
    /// </summary>
    ProjectionTelevision = 0x706a7476,
    /// <summary>
    /// DeviceTechnology for CathodeRayTube = 'CRT'
    /// </summary>
    CathodeRayTube = 0x43525420,
    /// <summary>
    /// DeviceTechnology for PassiveMatrixDisplay = 'PMD'
    /// </summary>
    PassiveMatrixDisplay = 0x504d4420,
    /// <summary>
    /// DeviceTechnology for ActivmeMatrixDisplay = 'AMD'
    /// </summary>
    ActivmeMatrixDisplay = 0x414d4420,
    /// <summary>
    /// DeviceTechnology for PhotoCD = 'KPCD'
    /// </summary>
    PhotoCD = 0x4b504344,
    /// <summary>
    /// DeviceTechnology for Photographic Image Setter = 'imgs'
    /// </summary>
    PhotographicImageSetter = 0x696d6773, //imgs
    /// <summary>
    /// DeviceTechnology for Gravure = 'grav'
    /// </summary>
    Gravure = 0x67726176,
    /// <summary>
    /// DeviceTechnology for OffsetLithography = 'offs'
    /// </summary>
    OffsetLithography = 0x6f666673,
    /// <summary>
    /// DeviceTechnology for Silkscreen = 'silk'
    /// </summary>
    Silkscreen = 0x73696c6b,
    /// <summary>
    /// DeviceTechnology for Flexography = 'flex'
    /// </summary>
    Flexography = 0x666c6578, //flex
    /// <summary>
    /// DeviceTechnology for MotionPictureFilmScanner = 'mpfs'
    /// </summary>
    MotionPictureFilmScanner = 0x6d706673,
    /// <summary>
    /// DeviceTechnology for MotionPictureFilmRecorder = 'mpfr'
    /// </summary>
    MotionPictureFilmRecorder = 0x6d706672,
    /// <summary>
    /// DeviceTechnology for Digital Motion Picture Camera= 'DMPC'
    /// </summary>
    DigitalMotionPictureCamera = 0x646d7963, //dmpc
    /// <summary>
    /// DeviceTechnology for DigitalCinemaProjector = 'dcpj'
    /// </summary>
    DigitalCinemaProjector = 0x64636a70,
}