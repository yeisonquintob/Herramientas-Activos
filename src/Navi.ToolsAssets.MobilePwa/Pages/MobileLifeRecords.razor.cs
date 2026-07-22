using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Navi.ToolsAssets.MobilePwa.Models;

namespace Navi.ToolsAssets.MobilePwa.Pages;

public partial class MobileLifeRecords
{
    // NAVI MOBILE LIFE RECORD LIST PREVIEW V50 START

    private MobileToolDto? previewToolV50;

    private bool isToolImagePreviewOpenV50;

    private bool isToolImagePreviewLoadingV50;

    private string? toolImagePreviewUrlV50;

    private string? toolImagePreviewErrorV50;


    private MobileToolDto? PreviewToolV50
    {
        get
        {
            return previewToolV50;
        }
    }


    private bool IsToolImagePreviewOpenV50
    {
        get
        {
            return isToolImagePreviewOpenV50;
        }
    }


    private bool IsToolImagePreviewLoadingV50
    {
        get
        {
            return isToolImagePreviewLoadingV50;
        }
    }


    private string? ToolImagePreviewUrlV50
    {
        get
        {
            return toolImagePreviewUrlV50;
        }
    }


    private string? ToolImagePreviewErrorV50
    {
        get
        {
            return toolImagePreviewErrorV50;
        }
    }


    private async Task OpenToolImagePreviewV50(
        MobileToolDto tool)
    {
        previewToolV50 = tool;

        isToolImagePreviewOpenV50 = true;
        isToolImagePreviewLoadingV50 = true;

        toolImagePreviewUrlV50 = null;
        toolImagePreviewErrorV50 = null;

        try
        {
            var documents =
                await Api.GetListJsonAsync<
                    MobileDocumentDto>(
                    $"api/tools/{tool.Id}/documents");

            var latestImage =
                documents
                    .Where(IsToolPreviewImageV50)
                    .OrderByDescending(
                        document =>
                            document.UploadedAt
                            ?? DateTime.MinValue)
                    .FirstOrDefault();

            if (
                latestImage is null
                ||
                string.IsNullOrWhiteSpace(
                    latestImage.DownloadUrl))
            {
                toolImagePreviewErrorV50 =
                    "Sin imagen registrada";

                return;
            }

            toolImagePreviewUrlV50 =
                NormalizeToolPreviewUrlV50(
                    latestImage.DownloadUrl);
        }
        catch
        {
            toolImagePreviewErrorV50 =
                "No se pudo cargar la imagen";
        }
        finally
        {
            isToolImagePreviewLoadingV50 = false;
        }
    }


    private void CloseToolImagePreviewV50()
    {
        isToolImagePreviewOpenV50 = false;
        isToolImagePreviewLoadingV50 = false;

        previewToolV50 = null;

        toolImagePreviewUrlV50 = null;
        toolImagePreviewErrorV50 = null;
    }


    private static bool IsToolPreviewImageV50(
        MobileDocumentDto document)
    {
        if (
            document.ContentType?.StartsWith(
                "image/",
                StringComparison.OrdinalIgnoreCase)
            == true)
        {
            return true;
        }

        var extension =
            Path.GetExtension(
                document.FileName
                ?? string.Empty);

        return extension.Equals(
                   ".jpg",
                   StringComparison.OrdinalIgnoreCase)
               ||
               extension.Equals(
                   ".jpeg",
                   StringComparison.OrdinalIgnoreCase)
               ||
               extension.Equals(
                   ".png",
                   StringComparison.OrdinalIgnoreCase)
               ||
               extension.Equals(
                   ".webp",
                   StringComparison.OrdinalIgnoreCase);
    }


    private string NormalizeToolPreviewUrlV50(
        string downloadUrl)
    {
        var cleanUrl =
            downloadUrl.Trim();

        if (
            Uri.TryCreate(
                cleanUrl,
                UriKind.Absolute,
                out var absoluteUrl))
        {
            if (!absoluteUrl.IsFile)
            {
                return absoluteUrl.ToString();
            }

            cleanUrl =
                absoluteUrl.AbsolutePath;
        }

        cleanUrl =
            cleanUrl.TrimStart('/');

        return Api.ToApiUrl(cleanUrl);
    }

    // NAVI MOBILE LIFE RECORD LIST PREVIEW V50 END

}
