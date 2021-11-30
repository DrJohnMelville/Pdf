namespace Melville.Icc.Model.Tags;

public enum DeviceTechnology : uint
{
    FilmScanner = 0x6673636e, // fscn
    DigitalCamera = 0x6463616d, // dcam
    ReflectiveScanner = 0x72736363, // rscn
    InkjetPrinter = 0x696a6574, // ijet
    ThermalWaxPrinter = 0x74776178, // twax
    ElectrophotographicPrinter = 0x6570686f, // epho
    ElectrostaticPrinter = 0x65737461, // esta
    DyeSubliminationPrinter = 0x64737562, // dsub
    PhotographicPaperPrinter = 0x7270686f, // rpho
    FilWriter = 0x6670726e, // fprn
    VideoMonitor = 0x7669646d, // vidm
    VideoCamera =  0x76696463, // vidc
    ProjectionTelevision = 0x706a7476, // pjtv
    CathodeRayTube = 0x43525420, // CRT 
    PassiveMatrixDisplay = 0x504d4420, // PMD
    ActivmeMatrixDisplay = 0x414d4420, // AMD
    PhotoCD = 0x4b504344, // KPCD
    PhotographicImageSetter = 0x696d6773, //imgs
    Gravure = 0x67726176, // grav
    OffsetLithography = 0x6f666673, // offs
    Silkscreen = 0x73696c6b, // silk
    Flexography = 0x666c6578, //flex
    MotionPictureFilmScanner = 0x6d706673, // mpfs
    MotionPictureFilmRecorder = 0x6d706672, // mpfr
    DigitalMotionPictureCamera = 0x646d7963, //dmpc
    DigitalCinemaProjector = 0x64636a70 // dcpj
}