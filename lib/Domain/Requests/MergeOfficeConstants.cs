// Copyright 2019-2025 Chris Mohan, Jaben Cargman
//  and GotenbergSharpApiClient Contributors
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

namespace Gotenberg.Sharp.API.Client.Domain.Requests;

public static class MergeOfficeConstants
{
    /// <summary>
    ///  Docs are here: https://gotenberg.dev/docs/routes#office-documents-into-pdfs-route
    /// Last updated 2/12/2025
    /// </summary>
    public static readonly string[] AllowedExtensions =
    [
        ".123", ".602", ".abw", ".bib", ".bmp", ".cdr", ".cgm", ".cmx", ".csv", ".cwk",
        ".dbf", ".dif", ".doc", ".docm", ".docx", ".dot", ".dotm", ".dotx", ".dxf", ".emf",
        ".eps", ".epub", ".fodg", ".fodp", ".fods", ".fodt", ".fopd", ".gif", ".htm", ".html",
        ".hwp", ".jpeg", ".jpg", ".key", ".ltx", ".lwp", ".mcw", ".met", ".mml", ".mw",
        ".numbers", ".odd", ".odg", ".odm", ".odp", ".ods", ".odt", ".otg", ".oth", ".otp",
        ".ots", ".ott", ".pages", ".pbm", ".pcd", ".pct", ".pcx", ".pdb", ".pdf", ".pgm",
        ".png", ".pot", ".potm", ".potx", ".ppm", ".pps", ".ppt", ".pptm", ".pptx", ".psd",
        ".psw", ".pub", ".pwp", ".pxl", ".ras", ".rtf", ".sda", ".sdc", ".sdd", ".sdp",
        ".sdw", ".sgl", ".slk", ".smf", ".stc", ".std", ".sti", ".stw", ".svg", ".svm",
        ".swf", ".sxc", ".sxd", ".sxg", ".sxi", ".sxm", ".sxw", ".tga", ".tif", ".tiff",
        ".txt", ".uof", ".uop", ".uos", ".uot", ".vdx", ".vor", ".vsd", ".vsdm", ".vsdx",
        ".wb2", ".wk1", ".wks", ".wmf", ".wpd", ".wpg", ".wps", ".xbm", ".xhtml", ".xls",
        ".xlsb", ".xlsm", ".xlsx", ".xlt", ".xltm", ".xltx", ".xlw", ".xml", ".xpm", ".zabw"
    ];
}