namespace RichHudFramework
{
    public enum MsgTypes : int
    {
        RegistrationRequest = 1,
        RegistrationSuccessful = 2,
        RegistrationFailed = 3,
    }

    public enum ApiModuleTypes : int
    {
        BindManager = 1,
        HudMain = 2,
        FontManager = 3,
        SettingsMenu = 4,
        BillBoardUtils = 5
    }

    public enum ClientDataAccessors : int
    {   
        GetVersionID = 1,
        GetSubtype = 2
    }

    public enum ClientSubtypes : int
    {
        Full = 1,
        NoLib = 2,
        Terminal = 3,
        FontManager = 4,
        BindManager = 5
    }
}