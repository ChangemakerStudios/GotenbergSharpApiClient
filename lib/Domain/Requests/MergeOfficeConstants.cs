//  Copyright 2019-2022 Chris Mohan, Jaben Cargman
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
    ///  Source is here: https://github.com/gotenberg/gotenberg/blob/main/pkg/modules/libreoffice/uno/uno.go
    /// </summary>
    public static readonly string[] AllowedExtensions =
    {
        ".bib", ".doc", ".xml", ".docx", ".fodt", ".html", ".ltx", ".txt", ".odt",
        ".ott", ".pdb", ".pdf", ".psw", ".rtf", ".sdw", ".stw", ".sxw", ".uot",
        ".vor", ".wps", ".epub", ".png", ".bmp", ".emf", ".eps", ".fodg", ".gif",
        ".jpg", ".jpeg", ".met", ".odd", ".otg", ".pbm", ".pct", ".pgm", ".ppm",
        ".ras", ".std", ".svg", ".svm", ".swf", ".sxd", ".sxw", ".tif", ".tiff",
        ".xhtml", ".xpm", ".odp", ".fodp", ".potm", ".pot", ".pptx", ".pps", ".ppt",
        ".pwp", ".sda", ".sdd", ".sti", ".sxi", ".uop", ".wmf", ".csv", ".dbf", ".dif",
        ".fods", ".ods", ".ots", ".pxl", ".sdc", ".slk", ".stc", ".sxc", ".uos", ".xls", ".xlt",
        ".xlsx"
    };
}