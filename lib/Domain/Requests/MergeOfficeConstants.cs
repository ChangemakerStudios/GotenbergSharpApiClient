namespace Gotenberg.Sharp.API.Client.Domain.Requests;

public static class MergeOfficeConstants
{
    /// <summary>
    ///  Source is here: https://github.com/gotenberg/gotenberg/blob/main/pkg/modules/libreoffice/uno/uno.go
    /// </summary>
    public static readonly string[] AllowedExtensions = {
        ".bib", ".doc", ".xml", ".docx", ".fodt", ".html", ".ltx", ".txt", ".odt",
        ".ott", ".pdb", ".pdf", ".psw", ".rtf", ".sdw", ".stw", ".sxw", ".uot",
        ".vor", ".wps", ".epub", ".png", ".bmp", ".emf", ".eps", ".fodg", ".gif",
        ".jpg", ".jpeg", ".met", ".odd", ".otg", ".pbm", ".pct", ".pgm", ".ppm",
        ".ras", ".std", ".svg", ".svm", ".swf", ".sxd", ".sxw", ".tif", ".tiff",
        ".xhtml", ".xpm", ".odp", ".fodp", ".potm", ".pot", ".pptx", ".pps", ".ppt",
        ".pwp", ".sda", ".sdd", ".sti", ".sxi", ".uop", ".wmf", ".csv", ".dbf", ".dif",
        ".fods", ".ods", ".ots", ".pxl", ".sdc", ".slk", ".stc", ".sxc", ".uos", ".xls", ".xlt", ".xlsx"
    };
}