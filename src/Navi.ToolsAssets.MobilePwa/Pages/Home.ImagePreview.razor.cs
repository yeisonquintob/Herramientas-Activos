using System;
using System.Linq;
using System.Threading.Tasks;
using Navi.ToolsAssets.MobilePwa.Models;

namespace Navi.ToolsAssets.MobilePwa.Pages;

public partial class Home
{
    // NAVI MOBILE HOME IMAGE PREVIEW V80 START

    private MobileToolDto? homeImagePreviewToolV80;

    private bool isHomeImagePreviewOpenV80;

    private bool isHomeImageLoadingV80;

    private string? homeImageUrlV80;

    private string? homeImageErrorV80;


    private async Task OpenHomeImagePreviewV80(
        MobileToolDto tool)
    {
        homeImagePreviewToolV80 =
            tool;

        isHomeImagePreviewOpenV80 =
            true;

        isHomeImageLoadingV80 =
            true;

        homeImageUrlV80 =
            null;

        homeImageErrorV80 =
            null;

        try
        {
            var documents =
                await Api.GetListJsonAsync<
                    MobileDocumentDto>(
                    $"api/tools/{tool.Id}/documents");

            var latestImage =
                documents
                    .Where(
                        IsHomeImageDocumentV80)
                    .OrderByDescending(
                        document =>
                            document.UploadedAt)
                    .FirstOrDefault();

            if (
                latestImage is null
                ||
                string.IsNullOrWhiteSpace(
                    latestImage.DownloadUrl))
            {
                homeImageErrorV80 =
                    "Sin imagen registrada";

                return;
            }

            homeImageUrlV80 =
                NormalizeHomeImageUrlV80(
                    latestImage.DownloadUrl);
        }
        catch (Exception exception)
        {
            homeImageErrorV80 =
                "No se pudo cargar la imagen.";

            System.Diagnostics.Debug.WriteLine(
                exception);
        }
        finally
        {
            isHomeImageLoadingV80 =
                false;
        }
    }


    private void CloseHomeImagePreviewV80()
    {
        isHomeImagePreviewOpenV80 =
            false;

        isHomeImageLoadingV80 =
            false;

        homeImagePreviewToolV80 =
            null;

        homeImageUrlV80 =
            null;

        homeImageErrorV80 =
            null;
    }


    private static bool IsHomeImageDocumentV80(
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
            System.IO.Path.GetExtension(
                document.FileName
                ??
                string.Empty);

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


    private string NormalizeHomeImageUrlV80(
        string downloadUrl)
    {
        var cleanUrl =
            downloadUrl.Trim();

        if (
            cleanUrl.StartsWith(
                "http://",
                StringComparison.OrdinalIgnoreCase)
            ||
            cleanUrl.StartsWith(
                "https://",
                StringComparison.OrdinalIgnoreCase))
        {
            return cleanUrl;
        }

        if (
            cleanUrl.StartsWith(
                "file://",
                StringComparison.OrdinalIgnoreCase)
            &&
            Uri.TryCreate(
                cleanUrl,
                UriKind.Absolute,
                out var fileUrl))
        {
            cleanUrl =
                fileUrl.AbsolutePath;
        }

        cleanUrl =
            cleanUrl.TrimStart('/');

        return Api.ToApiUrl(
            cleanUrl);
    }

    // NAVI MOBILE HOME IMAGE PREVIEW V80 END
}
